using Dalamud.Game.Command;
using Dalamud.Game.Gui.Toast;
using ECommons;
using ECommons.Automation;
using ECommons.Automation.LegacyTaskManager;
using ECommons.Configuration;
using ECommons.Events;
using ECommons.EzEventManager;
using ECommons.EzIpcManager;
using ECommons.Singletons;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System.Security.Principal;
using TextAdvance.Executors;
using TextAdvance.Gui;
using TextAdvance.Navmesh;
using TextAdvance.Services;

namespace TextAdvance;

public unsafe class TextAdvance : IDalamudPlugin
{
    public static Config C => P.config;

    internal bool InCutscene = false;
    internal bool WasInCutscene = false;
    internal bool Enabled = false;
    private bool CanPressEsc = false;
    //static string[] HandOverStr = { "Hand Over" };
    internal Config config;
    internal ConfigGui configGui;
    private bool loggedIn = false;
    internal static TextAdvance P;
    internal Dictionary<uint, string> TerritoryNames = [];
    private Overlay overlay;

    internal const string BlockListNamespace = "TextAdvance.StopRequests";
    internal HashSet<string> BlockList;
    internal TaskManager TaskManager;
    internal WaitOverlay WaitOverlay;
    internal SplatoonHandler SplatoonHandler;
    internal Memory Memory;
    public EntityOverlay EntityOverlay;
    public ProgressOverlay ProgressOverlay;
    public NavmeshManager NavmeshManager;

    public string Name => "TextAdvance";

    public void Dispose()
    {
        Svc.Commands.RemoveHandler("/at");
        Safe(ExecSkipTalk.Shutdown);
        Safe(ExecPickReward.Shutdown);
        Safe(() => this.EntityOverlay?.Dispose());
        ECommonsMain.Dispose();
        P = null;
    }

    public TextAdvance(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this, Module.SplatoonAPI);
        new TickScheduler(delegate
        {
            EzConfig.Migrate<Config>();
            this.config = EzConfig.Init<Config>();
            new EzFrameworkUpdate(this.Tick);
            new EzLogout(this.Logout);
            ProperOnLogin.RegisterAvailable(this.Login);
            this.configGui = new ConfigGui(this);
            this.overlay = new();
            this.SplatoonHandler = new();
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { this.configGui.IsOpen = true; };
            Svc.Commands.AddHandler("/at", new CommandInfo(this.HandleCommand)
            {
                ShowInHelp = true,
                HelpMessage = """
                toggles TextAdvance plugin.\n/at y|yes|e|enable - turns on TextAdvance.
                /at n|no|d|disable - turns off TextAdvance.
                /at c|config|s|settings - opens TextAdvance settings.
                /at g - toggles visual quest target markers
                /at mtq - move to the first available quest location, if present (requires navmesh integration to be enabled)
                /at mtqstop - cancel all pending movement tasks
                /at mtf - move to flag
                """
            });
            if (Svc.ClientState.IsLoggedIn)
            {
                this.loggedIn = true;
            }
            this.overlay = new();
            this.configGui.ws.AddWindow(this.overlay);
            this.WaitOverlay = new();
            this.configGui.ws.AddWindow(this.WaitOverlay);

            this.TerritoryNames = Svc.Data.GetExcelSheet<TerritoryType>().Where(x => x.PlaceName.ValueNullable?.Name.ToString().Length > 0)
            .ToDictionary(
                x => x.RowId,
                x => $"{x.RowId} | {x.PlaceName.ValueNullable?.Name}{(x.ContentFinderCondition.ValueNullable?.Name.ToString().Length > 0 ? $" ({x.ContentFinderCondition.ValueNullable?.Name})" : string.Empty)}");
            this.BlockList = Svc.PluginInterface.GetOrCreateData<HashSet<string>>(BlockListNamespace, () => []);
            this.BlockList.Clear();
            AutoCutsceneSkipper.Init(this.CutsceneSkipHandler);
            this.TaskManager = new()
            {
                AbortOnTimeout = true,
            };
            new EzTerritoryChanged(this.ClientState_TerritoryChanged);
            ExecSkipTalk.Init();
            ExecPickReward.Init();
            this.Memory = new();
            this.EntityOverlay = new();
            this.ProgressOverlay = new();
            this.NavmeshManager = new();
            //ExecAutoSnipe.Init(); // must init after memory
            SingletonServiceManager.Initialize(typeof(ServiceManager));
            EzIPC.OnSafeInvocationException += this.EzIPC_OnSafeInvocationException;
        });
    }

    private void EzIPC_OnSafeInvocationException(Exception obj)
    {
        InternalLog.Error($"IPC error: {obj}");
    }

    private void ClientState_TerritoryChanged(ushort obj)
    {
        this.SplatoonHandler.Reset();
        ExecAutoInteract.InteractedObjects.Clear();
    }

    private bool CutsceneSkipHandler(nint ptr)
    {
        if (Svc.ClientState.TerritoryType.EqualsAny<ushort>(670))
        {
            if (TryGetAddonByName<AtkUnitBase>("FadeMiddle", out var addon) && addon->IsVisible) return false;
        }
        return !P.Locked && (!P.IsDisableButtonHeld() || !P.IsEnabled()) && P.IsEnabled() && P.config.GetEnableCutsceneEsc();
    }

    private void Logout()
    {
        this.SplatoonHandler.Reset();
        if (!this.config.DontAutoDisable)
        {
            this.Enabled = false;
        }
    }

    private void Login()
    {
        this.SplatoonHandler.Reset();
        this.loggedIn = true;
    }

    private void HandleCommand(string command, string arguments)
    {
        if (arguments == "test")
        {
            try
            {
                //getRefValue((int)VirtualKey.ESCAPE) = 3;
            }
            catch (Exception e)
            {
                PluginLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            }
            return;
        }
        else if (arguments.EqualsIgnoreCaseAny("s", "settings", "c", "config"))
        {
            this.configGui.IsOpen = true;
        }
        else if (arguments.EqualsIgnoreCaseAny("g", "gui"))
        {
            this.config.QTIEnabled = !this.config.QTIEnabled;
            if (this.config.QTIEnabled)
            {
                Notify.Info($"Quest target indicators enabled");
            }
            else
            {
                Notify.Info($"Quest target indicators disabled");
            }
        }
        else if (arguments.EqualsIgnoreCaseAny("mtq"))
        {
            S.MoveManager.MoveToQuest();
        }
        else if (arguments.EqualsIgnoreCaseAny("mtf"))
        {
            S.MoveManager.MoveToFlag();
        }
        else if (arguments.EqualsIgnoreCaseAny("mtqstop"))
        {
            P.EntityOverlay.TaskManager.Abort();
            if (C.Navmesh) P.NavmeshManager.Stop();
        }
        else if (arguments.EqualsIgnoreCase("d"))
        {
            if (Svc.Targets.Target != null) Copy(new ObjectDescriptor(Svc.Targets.Target, true).AsCtorString());
        }
        else
        {
            this.Enabled = arguments.EqualsIgnoreCaseAny("enable", "e", "yes", "y") || (!arguments.EqualsIgnoreCaseAny("disable", "d", "no", "n") && !this.Enabled);
            if (!P.config.NotifyDisableManualState)
            {
                Svc.Toasts.ShowQuest("Auto advance " + (this.Enabled ? "globally enabled" : "disabled (except custom territories)"),
                    new QuestToastOptions() { PlaySound = true, DisplayCheckmark = true });
            }
        }
    }

    internal bool Locked => this.BlockList.Count != 0;

    internal bool IsEnabled(bool pure = false)
    {
        return (this.Enabled || S.IPCProvider.IsInExternalControl() || this.IsTerritoryEnabled() || this.IsEnableButtonHeld()) && (!this.Locked || pure);
    }

    internal bool IsTerritoryEnabled()
    {
        if (S.IPCProvider.IsInExternalControl())
        {
            return false;
        }
        return P.config.TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var cfg) && cfg.IsEnabled();
    }

    private void Tick()
    {
        try
        {
            ExecSkipTalk.IsEnabled = false;
            ExecPickReward.IsEnabled = false;
            ExecAutoSnipe.IsEnabled = false;
            if (this.loggedIn && Svc.ClientState.LocalPlayer != null)
            {
                this.loggedIn = false;
                if (this.config.AutoEnableNames.Contains(Svc.ClientState.LocalPlayer.Name.ToString() + "@" + Svc.ClientState.LocalPlayer.HomeWorld.ValueNullable?.Name.ToString()))
                {
                    this.Enabled = true;
                    if (!P.config.NotifyDisableOnLogin)
                    {
                        Notify.Success("Auto text advance has been automatically enabled on this character");
                    }
                }
            }
            this.InCutscene = Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                || Svc.Condition[ConditionFlag.WatchingCutscene78];
            if (!this.Locked)
            {
                if (!this.IsDisableButtonHeld() || !this.IsEnabled())
                {
                    if (this.IsEnabled())
                    {
                        if (this.config.GetEnableCutsceneSkipConfirm() && this.InCutscene)
                        {
                            ExecConfirmCutsceneSkip.Tick();
                        }
                        if (this.config.GetEnableAutoInteract()) ExecAutoInteract.Tick();
                    }
                    if (this.IsEnabled() &&
                        (Svc.Condition[ConditionFlag.OccupiedInQuestEvent] ||
                        Svc.Condition[ConditionFlag.Occupied33] ||
                        Svc.Condition[ConditionFlag.OccupiedInEvent] ||
                        Svc.Condition[ConditionFlag.Occupied30] ||
                        Svc.Condition[ConditionFlag.Occupied38] ||
                        Svc.Condition[ConditionFlag.Occupied39] ||
                        Svc.Condition[ConditionFlag.OccupiedSummoningBell] ||
                        Svc.Condition[ConditionFlag.WatchingCutscene] ||
                        Svc.Condition[ConditionFlag.Mounting71] ||
                        Svc.Condition[ConditionFlag.CarryingObject] ||
                        Svc.Condition[ConditionFlag.CarryingItem] ||
                        this.InCutscene))
                    {
                        if (this.config.GetEnableTalkSkip()) ExecSkipTalk.IsEnabled = true;
                        if (this.config.GetEnableQuestComplete()) ExecQuestComplete.Tick();
                        if (this.config.GetEnableQuestAccept()) ExecQuestAccept.Tick();
                        if (this.config.GetEnableRequestHandin()) ExecRequestComplete.Tick();
                        if (this.config.GetEnableRequestFill()) ExecRequestFill.Tick();
                        if (this.config.GetEnableRewardPick()) ExecPickReward.IsEnabled = true;
                        if (this.config.GetEnableAutoSnipe()) ExecAutoSnipe.IsEnabled = true;
                    }
                }
            }
            this.WasInCutscene = this.InCutscene;
        }
        catch (NullReferenceException e)
        {
            PluginLog.Debug(e.Message + "" + e.StackTrace);
            //Svc.Chat.Print(e.Message + "" + e.StackTrace);
        }
        catch (Exception e)
        {
            Svc.Chat.Print(e.Message + "" + e.StackTrace);
        }
    }

    internal bool IsDisableButtonHeld()
    {
        if (this.config.TempDisableButton == Button.ALT && ImGui.GetIO().KeyAlt) return !CSFramework.Instance()->WindowInactive;
        if (this.config.TempDisableButton == Button.CTRL && ImGui.GetIO().KeyCtrl) return !CSFramework.Instance()->WindowInactive;
        if (this.config.TempDisableButton == Button.SHIFT && ImGui.GetIO().KeyShift) return !CSFramework.Instance()->WindowInactive;
        return false;
    }

    private bool IsEnableButtonHeld()
    {
        if (this.config.TempEnableButton == Button.ALT && ImGui.GetIO().KeyAlt) return !CSFramework.Instance()->WindowInactive;
        if (this.config.TempEnableButton == Button.CTRL && ImGui.GetIO().KeyCtrl) return !CSFramework.Instance()->WindowInactive;
        if (this.config.TempEnableButton == Button.SHIFT && ImGui.GetIO().KeyShift) return !CSFramework.Instance()->WindowInactive;
        return false;
    }
}
