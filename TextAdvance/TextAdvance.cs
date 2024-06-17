using Dalamud.Game.Command;
using Dalamud.Game.Gui.Toast;
using Dalamud.Interface.Internal.Notifications;
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
using Lumina.Excel.GeneratedSheets;
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
    bool CanPressEsc = false;
    //static string[] HandOverStr = { "Hand Over" };
    internal Config config;
    internal ConfigGui configGui;
    bool loggedIn = false;
    internal static TextAdvance P;
    internal Dictionary<uint, string> TerritoryNames = [];
    Overlay overlay;

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
        Safe(() => EntityOverlay?.Dispose());
        ECommonsMain.Dispose();
        P = null;
    }

    public TextAdvance(DalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this, Module.SplatoonAPI);
        new TickScheduler(delegate
        {
            EzConfig.Migrate<Config>();
            config = EzConfig.Init<Config>();
            new EzFrameworkUpdate(Tick);
            new EzLogout(Logout);
            ProperOnLogin.RegisterAvailable(Login);
            configGui = new ConfigGui(this);
            overlay = new();
            SplatoonHandler = new();
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.IsOpen = true; };
            Svc.Commands.AddHandler("/at", new CommandInfo(HandleCommand)
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
                loggedIn = true;
            }
            overlay = new();
            configGui.ws.AddWindow(overlay);
            WaitOverlay = new();
            configGui.ws.AddWindow(WaitOverlay);
            
            TerritoryNames = Svc.Data.GetExcelSheet<TerritoryType>().Where(x => x.PlaceName?.Value?.Name?.ToString().Length > 0)
            .ToDictionary(
                x => x.RowId, 
                x => $"{x.RowId} | {x.PlaceName?.Value?.Name}{(x.ContentFinderCondition?.Value?.Name?.ToString().Length > 0 ? $" ({x.ContentFinderCondition?.Value?.Name})" : string.Empty)}");
            BlockList = Svc.PluginInterface.GetOrCreateData<HashSet<string>>(BlockListNamespace, () => new());
            BlockList.Clear(); 
            AutoCutsceneSkipper.Init(CutsceneSkipHandler);
            TaskManager = new()
            {
                AbortOnTimeout = true,
            };
            new EzTerritoryChanged(ClientState_TerritoryChanged);
            ExecSkipTalk.Init();
            ExecPickReward.Init();
            Memory = new();
            EntityOverlay = new();
            ProgressOverlay = new();
            NavmeshManager = new();
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
        SplatoonHandler.Reset();
        ExecAutoInteract.InteractedObjects.Clear();
    }

    bool CutsceneSkipHandler(nint ptr)
    {
        if (Svc.ClientState.TerritoryType.EqualsAny<ushort>(670))
        {
            if (TryGetAddonByName<AtkUnitBase>("FadeMiddle", out var addon) && addon->IsVisible) return false;
        }
        return !P.Locked && (!P.IsDisableButtonHeld() || !P.IsEnabled()) && P.IsEnabled() && P.config.GetEnableCutsceneEsc();
    }

    private void Logout()
    {
        SplatoonHandler.Reset();
        if (!config.DontAutoDisable)
        {
            Enabled = false;
        }
    }

    private void Login()
    {
        SplatoonHandler.Reset();
        loggedIn = true;
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
            configGui.IsOpen = true;
        }
        else if (arguments.EqualsIgnoreCaseAny("g", "gui"))
        {
            config.QTIEnabled = !config.QTIEnabled;
            if (config.QTIEnabled)
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
            Enabled = arguments.EqualsIgnoreCaseAny("enable", "e", "yes", "y") || (!arguments.EqualsIgnoreCaseAny("disable", "d", "no", "n") && !Enabled);
            if (!P.config.NotifyDisableManualState)
            {
                Svc.Toasts.ShowQuest("Auto advance " + (Enabled ? "globally enabled" : "disabled (except custom territories)"),
                    new QuestToastOptions() { PlaySound = true, DisplayCheckmark = true });
            }
        }
    }

    internal bool Locked => BlockList.Count != 0;

    internal bool IsEnabled(bool pure = false)
    {
        return (Enabled || IsTerritoryEnabled()) && (!Locked || pure);
    }

    internal bool IsTerritoryEnabled()
    {
        if (S.IPCProvider.IsInExternalControl())
        {
            return S.IPCProvider.Config.IsEnabled();
        }
        return P.config.TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var cfg) && cfg.IsEnabled();
    }

    private void Tick()
    {
        try
        {
            ExecSkipTalk.IsEnabled = false;
            ExecPickReward.IsEnabled = false;
            if (loggedIn && Svc.ClientState.LocalPlayer != null)
            {
                loggedIn = false;
                if(config.AutoEnableNames.Contains(Svc.ClientState.LocalPlayer.Name.ToString() + "@" + Svc.ClientState.LocalPlayer.HomeWorld.GameData.Name))
                {
                    Enabled = true;
                    if (!P.config.NotifyDisableOnLogin)
                    {
                        Svc.PluginInterface.UiBuilder.AddNotification("Auto text advance has been automatically enabled on this character",
                            "TextAdvance", NotificationType.Info, 10000);
                    }
                }
            }
            InCutscene = Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                || Svc.Condition[ConditionFlag.WatchingCutscene78];
            if(config.QTIEnabled && IsEnabled() && !InCutscene)
            {
                SplatoonHandler.Tick();
            }
            if (!Locked)
            {
                if (!IsDisableButtonHeld() || !IsEnabled())
                {
                    if (IsEnabled())
                    {
                        if (config.GetEnableCutsceneSkipConfirm() && InCutscene)
                        {
                            ExecConfirmCutsceneSkip.Tick();
                        }
                        if (config.GetEnableAutoInteract()) ExecAutoInteract.Tick();
                    }
                    if ((IsEnabled() || (IsEnableButtonHeld() && Native.ApplicationIsActivated())) &&
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
                        InCutscene))
                    {
                        if (config.GetEnableTalkSkip()) ExecSkipTalk.IsEnabled = true;
                        if (config.GetEnableQuestComplete()) ExecQuestComplete.Tick();
                        if (config.GetEnableQuestAccept()) ExecQuestAccept.Tick();
                        if (config.GetEnableRequestHandin()) ExecRequestComplete.Tick();
                        if (config.GetEnableRequestFill()) ExecRequestFill.Tick();
                        if (config.GetEnableRewardPick()) ExecPickReward.IsEnabled = true;
                    }
                }
            }
            WasInCutscene = InCutscene;
        }
        catch(NullReferenceException e)
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
        if (config.TempDisableButton == Button.ALT && ImGui.GetIO().KeyAlt) return true;
        if (config.TempDisableButton == Button.CTRL && ImGui.GetIO().KeyCtrl) return true;
        if (config.TempDisableButton == Button.SHIFT && ImGui.GetIO().KeyShift) return true;
        return false;
    }

    bool IsEnableButtonHeld()
    {
        if (config.TempEnableButton == Button.ALT && ImGui.GetIO().KeyAlt) return true;
        if (config.TempEnableButton == Button.CTRL && ImGui.GetIO().KeyCtrl) return true;
        if (config.TempEnableButton == Button.SHIFT && ImGui.GetIO().KeyShift) return true;
        return false;
    }
}
