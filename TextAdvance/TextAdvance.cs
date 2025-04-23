using Dalamud.Game.Command;
using Dalamud.Game.Gui.Toast;
using ECommons.Automation;
using ECommons.Automation.LegacyTaskManager;
using ECommons.Configuration;
using ECommons.Events;
using ECommons.EzEventManager;
using ECommons.EzIpcManager;
using ECommons.SimpleGui;
using ECommons.Singletons;
using ECommons.SplatoonAPI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using TextAdvance.Executors;
using TextAdvance.Navmesh;
using TextAdvance.Services;

namespace TextAdvance;

public unsafe class TextAdvance : IDalamudPlugin
{
    public static Config C => P.Config;

    internal bool InCutscene = false;
    internal bool WasInCutscene = false;
    internal bool Enabled = false;
    private bool CanPressEsc = false;
    //static string[] HandOverStr = { "Hand Over" };
    private Config Config;
    private bool LoggedIn = false;
    internal static TextAdvance P;
    internal Dictionary<uint, string> TerritoryNames = [];

    internal const string BlockListNamespace = "TextAdvance.StopRequests";
    internal HashSet<string> BlockList;
    internal TaskManager TaskManager;
    internal SplatoonHandler SplatoonHandler;
    public NavmeshManager NavmeshManager;
    public Queue<Element> QueuedSplatoonElements = [];

    public string Name => "TextAdvance";

    public void Dispose()
    {
        Svc.Commands.RemoveHandler("/at");
        Safe(ExecSkipTalk.Shutdown);
        Safe(ExecPickReward.Shutdown);
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
            this.Config = EzConfig.Init<Config>();
            new EzFrameworkUpdate(this.Tick);
            new EzLogout(this.Logout);
            ProperOnLogin.RegisterAvailable(this.Login);
            this.SplatoonHandler = new();
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
                this.LoggedIn = true;
            }

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
            this.NavmeshManager = new();
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
        return !P.Locked && (!P.IsDisableButtonHeld() || !P.IsEnabled()) && P.IsEnabled() && C.GetEnableCutsceneEsc();
    }

    private void Logout()
    {
        this.SplatoonHandler.Reset();
        if (!this.Config.DontAutoDisable)
        {
            this.Enabled = false;
        }
    }

    private void Login()
    {
        this.SplatoonHandler.Reset();
        this.LoggedIn = true;
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
            EzConfigGui.Open();
        }
        else if (arguments.EqualsIgnoreCaseAny("g", "gui"))
        {
            this.Config.QTIEnabled = !this.Config.QTIEnabled;
            if (this.Config.QTIEnabled)
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
            S.EntityOverlay.TaskManager.Abort();
            if (C.Navmesh) P.NavmeshManager.Stop();
        }
        else if (arguments.EqualsIgnoreCase("d"))
        {
            if (Svc.Targets.Target != null) Copy(new ObjectDescriptor(Svc.Targets.Target, true).AsCtorString());
        }
        else
        {
            this.Enabled = arguments.EqualsIgnoreCaseAny("enable", "e", "yes", "y") || (!arguments.EqualsIgnoreCaseAny("disable", "d", "no", "n") && !this.Enabled);
            if (!C.NotifyDisableManualState)
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
        return C.TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var cfg) && cfg.IsEnabled();
    }

    private void Tick()
    {
        try
        {
            while (QueuedSplatoonElements.TryDequeue(out var element))
            {
                if (Splatoon.IsConnected() && element.IsValid())
                {
                    Splatoon.DisplayOnce(element);
                }
            }
            ExecSkipTalk.IsEnabled = false;
            ExecPickReward.IsEnabled = false;
            ExecAutoSnipe.IsEnabled = false;
            if (this.LoggedIn && Svc.ClientState.LocalPlayer != null)
            {
                this.LoggedIn = false;
                if (this.Config.AutoEnableNames.Contains(Svc.ClientState.LocalPlayer.Name.ToString() + "@" + Svc.ClientState.LocalPlayer.HomeWorld.ValueNullable?.Name.ToString()))
                {
                    this.Enabled = true;
                    if (!C.NotifyDisableOnLogin)
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
                        if (this.Config.GetEnableCutsceneSkipConfirm() && this.InCutscene)
                        {
                            ExecConfirmCutsceneSkip.Tick();
                        }
                        if (this.Config.GetEnableAutoInteract()) ExecAutoInteract.Tick();
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
                        if (this.Config.GetEnableTalkSkip()) ExecSkipTalk.IsEnabled = true;
                        if (this.Config.GetEnableQuestComplete()) ExecQuestComplete.Tick();
                        if (this.Config.GetEnableQuestAccept()) ExecQuestAccept.Tick();
                        if (this.Config.GetEnableRequestHandin()) ExecRequestComplete.Tick();
                        if (this.Config.GetEnableRequestFill()) ExecRequestFill.Tick();
                        if (this.Config.GetEnableRewardPick()) ExecPickReward.IsEnabled = true;
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
        if (this.Config.TempDisableButton == Button.ALT && ImGui.GetIO().KeyAlt) return !CSFramework.Instance()->WindowInactive;
        if (this.Config.TempDisableButton == Button.CTRL && ImGui.GetIO().KeyCtrl) return !CSFramework.Instance()->WindowInactive;
        if (this.Config.TempDisableButton == Button.SHIFT && ImGui.GetIO().KeyShift) return !CSFramework.Instance()->WindowInactive;
        return false;
    }

    private bool IsEnableButtonHeld()
    {
        if (this.Config.TempEnableButton == Button.ALT && ImGui.GetIO().KeyAlt) return !CSFramework.Instance()->WindowInactive;
        if (this.Config.TempEnableButton == Button.CTRL && ImGui.GetIO().KeyCtrl) return !CSFramework.Instance()->WindowInactive;
        if (this.Config.TempEnableButton == Button.SHIFT && ImGui.GetIO().KeyShift) return !CSFramework.Instance()->WindowInactive;
        return false;
    }
}
