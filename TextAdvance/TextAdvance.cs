using ClickLib.Clicks;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.Toast;
using Dalamud.Interface.Internal.Notifications;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using TextAdvance.Gui;
using static TextAdvance.Native;

namespace TextAdvance;

unsafe class TextAdvance : IDalamudPlugin
{
    internal bool InCutscene = false;
    internal bool WasInCutscene = false;
    internal bool Enabled = false;
    bool CanPressEsc = false;
    static string[] AcceptStr = { "Accept", "接受", "Annehmen", "Accepter", "受注" };
    static string[] SkipCutsceneStr = { "Skip cutscene?", "要跳过这段过场动画吗？", "要跳過這段過場動畫嗎？", "Videosequenz überspringen?", "Passer la scène cinématique ?", "このカットシーンをスキップしますか？" };
    static string[] YesStr = { "Yes.", "是", "Ja", "Oui", "はい" };
    static string[] CompleteStr = { "Complete", "完成", "Abschließen", "Accepter", "コンプリート" };
    //static string[] HandOverStr = { "Hand Over" };
    internal Config config;
    internal ConfigGui configGui;
    bool loggedIn = false;
    internal static TextAdvance P;
    internal Dictionary<uint, string> TerritoryNames = new();
    Overlay overlay;

    public string Name => "TextAdvance";

    public void Dispose()
    {
        Svc.Framework.Update -= Tick;
        Svc.ClientState.Logout -= Logout;
        Svc.ClientState.Login -= Login;
        Svc.Commands.RemoveHandler("/at");
        ECommonsMain.Dispose();
        P = null;
    }

    public TextAdvance(DalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this);
        new TickScheduler(delegate
        {
            config = Svc.PluginInterface.GetPluginConfig() as Config ?? new Config();
            Svc.Framework.Update += Tick;
            Svc.ClientState.Logout += Logout;
            Svc.ClientState.Login += Login;
            configGui = new ConfigGui(this);
            overlay = new();
            
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.IsOpen = true; };
            Svc.Commands.AddHandler("/at", new CommandInfo(HandleCommand)
            {
                ShowInHelp = true,
                HelpMessage = "toggles TextAdvance plugin.\n/at y|yes|e|enable - turns on TextAdvance.\n/at n|no|d|disable - turns off TextAdvance.\n" +
                "/at c|config|s|settings - opens TextAdvance settings."
            });
            if (Svc.ClientState.IsLoggedIn)
            {
                loggedIn = true;
                PrintNotice();
            }
            overlay = new();
            configGui.ws.AddWindow(overlay);
            TerritoryNames = Svc.Data.GetExcelSheet<TerritoryType>().Where(x => x.PlaceName?.Value?.Name?.ToString().Length > 0)
            .ToDictionary(
                x => x.RowId, 
                x => $"{x.RowId} | {x.PlaceName?.Value?.Name}{(x.ContentFinderCondition?.Value?.Name?.ToString().Length > 0 ? $" ({x.ContentFinderCondition?.Value?.Name})" : string.Empty)}");
        });
    }

    private void Logout(object sender, EventArgs e)
    {
        Enabled = false;
    }

    private void Login(object sender, EventArgs e)
    {
        loggedIn = true;
        PrintNotice();
    }

    void PrintNotice()
    {
        /*
        Svc.Chat.Print("[TextAdvance] Thank you for using TextAdvance! This plugin wasn't much tested" +
            " in Endwalker and using it can possibly lead to game crashing. Should you be uncomfortable with it - " +
            "please uninstall it for now (or simply don't enable) and wait some time until testing is complete.");
        */
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
        else
        {
            Enabled = arguments.EqualsIgnoreCaseAny("enable", "e", "yes", "y") || (!arguments.EqualsIgnoreCaseAny("disable", "d", "no", "n") && !Enabled);
            Svc.Toasts.ShowQuest("Auto advance " + (Enabled ? "globally enabled" : "disabled (except custom territories)"),
                new QuestToastOptions() { PlaySound = true, DisplayCheckmark = true });
        }
    }

    internal bool IsEnabled()
    {
        return Enabled || IsTerritoryEnabled();
    }

    internal bool IsTerritoryEnabled()
    {
        return P.config.TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var cfg) && cfg.IsEnabled();
    }

    private void Tick(Framework framework)
    {
        try
        {
            if(loggedIn && Svc.ClientState.LocalPlayer != null)
            {
                loggedIn = false;
                if(config.AutoEnableNames.Contains(Svc.ClientState.LocalPlayer.Name.ToString() + "@" + Svc.ClientState.LocalPlayer.HomeWorld.GameData.Name))
                {
                    Enabled = true;
                    Svc.PluginInterface.UiBuilder.AddNotification("Auto text advance has been automatically enabled on this character",
                        "TextAdvance", NotificationType.Info, 10000);
                }
            }
            InCutscene = Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                || Svc.Condition[ConditionFlag.WatchingCutscene78];
            if (!IsDisableButtonHeld() || !IsEnabled())
            {
                if (IsEnabled())
                {
                    if (config.GetEnableCutsceneEsc())
                    {
                        var nLoading = Svc.GameGui.GetAddonByName("NowLoading", 1);
                        var skip = true;
                        var addon = Svc.GameGui.GetAddonByName("SelectString", 1);
                        if (addon == IntPtr.Zero)
                        {
                            skip = false;
                        }
                        else
                        {
                            var selectStrAddon = (AtkUnitBase*)addon;
                            if (!IsAddonReady(selectStrAddon)) skip = false;
                        }
                        if (InCutscene)
                        {
                            if (!skip)
                            {
                                if (nLoading != IntPtr.Zero)
                                {
                                    var nowLoading = (AtkUnitBase*)nLoading;
                                    if (nowLoading->IsVisible)
                                    {
                                        //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Now loading visible");
                                    }
                                    else
                                    {
                                        //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Now loading not visible");
                                        if (CanPressEsc && TryFindGameWindow(out var hwnd))
                                        {
                                            //getRefValue((int)VirtualKey.ESCAPE) = 3;
                                            Keypress.SendKeycode(hwnd, Keypress.Escape);
                                            PluginLog.Debug("Pressing Esc");
                                            CanPressEsc = false;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            CanPressEsc = true;
                        }
                    }
                    if (config.GetEnableCutsceneSkipConfirm() && InCutscene)
                    {
                        TickSelectSkip();
                    }
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
                    InCutscene))
                {
                    if(config.GetEnableTalkSkip()) TickTalk();
                    if(config.GetEnableQuestComplete()) TickQuestComplete();
                    if(config.GetEnableQuestAccept()) TickQuestAccept();
                    if(config.GetEnableRequestHandin()) TickRequestComplete();
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

    long requestAllow = 0;
    void TickRequestComplete()
    {
        var addon = Svc.GameGui.GetAddonByName("Request", 1);
        if (addon == IntPtr.Zero)
        {
            requestAllow = 0;
            return;
        }
        if(requestAllow == 0)
        {
            requestAllow = Environment.TickCount64 + 500;
        }
        if (Environment.TickCount64 < requestAllow) return;
        var request = (AddonRequest*)addon;
        var questAddon = (AtkUnitBase*)addon;
        if (!IsAddonReady(questAddon)) return;
        if (questAddon->UldManager.NodeListCount <= 16) return;
        var buttonNode = (AtkComponentNode*)questAddon->UldManager.NodeList[4];
        if (buttonNode->Component->UldManager.NodeListCount <= 2) return;
        var textComponent = (AtkTextNode*)buttonNode->Component->UldManager.NodeList[2];
        //if (!HandOverStr.Contains(Marshal.PtrToStringUTF8((IntPtr)textComponent->NodeText.StringPtr))) return;
        if (textComponent->AtkResNode.Color.A != 255) return;
        for(var i = 16; i <= 12; i--)
        {
            if (((AtkComponentNode*)questAddon->UldManager.NodeList[i])->AtkResNode.IsVisible
                && ((AtkComponentNode*)questAddon->UldManager.NodeList[i - 6])->AtkResNode.IsVisible) return;
        }
        if (request->HandOverButton != null && request->HandOverButton->IsEnabled)
        {
            ThrottleManager.Throttle(delegate
            {
                PluginLog.Debug("Handing over request");
                ClickRequest.Using(addon).HandOver();
            }, 500);
        }
    }

    void TickQuestComplete()
    {
        var addon = Svc.GameGui.GetAddonByName("JournalResult", 1);
        if (addon == IntPtr.Zero)
        {
            return;
        }
        var questAddon = (AtkUnitBase*)addon;
        if (!IsAddonReady(questAddon)) return;
        if (questAddon->UldManager.NodeListCount <= 4) return;
        var buttonNode = (AtkComponentNode*)questAddon->UldManager.NodeList[4];
        if (buttonNode->Component->UldManager.NodeListCount <= 2) return;
        var textComponent = (AtkTextNode*)buttonNode->Component->UldManager.NodeList[2];
        if (!CompleteStr.Contains(Marshal.PtrToStringUTF8((IntPtr)textComponent->NodeText.StringPtr))) return;
        if (textComponent->AtkResNode.Color.A != 255) return;
        //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Pass");
        if(!((AddonJournalResult*)addon)->CompleteButton->IsEnabled) return;
        ThrottleManager.Throttle(delegate
        {
            PluginLog.Debug("Completing quest");
            ClickJournalResult.Using(addon).Complete();
        }, 500);
    }

    void TickQuestAccept()
    {
        var addon = Svc.GameGui.GetAddonByName("JournalAccept", 1);
        if (addon == IntPtr.Zero)
        {
            return;
        }
        var questAddon = (AtkUnitBase*)addon;
        if (!IsAddonReady(questAddon)) return;
        if (questAddon->UldManager.NodeListCount <= 6) return;
        var buttonNode = (AtkComponentNode*)questAddon->UldManager.NodeList[6];
        if (buttonNode->Component->UldManager.NodeListCount <= 2) return;
        var textComponent = (AtkTextNode*)buttonNode->Component->UldManager.NodeList[2];
        if (!AcceptStr.Contains(Marshal.PtrToStringUTF8((IntPtr)textComponent->NodeText.StringPtr))) return;
        if (textComponent->AtkResNode.Color.A != 255) return;
        //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Pass");
        ThrottleManager.Throttle(delegate
        {
            PluginLog.Debug("Accepting quest");
            ClickJournalAccept.Using(addon).Accept((AtkComponentButton*)buttonNode);
        }, 500);
    }

    void TickTalk()
    {
        var addon = Svc.GameGui.GetAddonByName("Talk", 1);
        if (addon == IntPtr.Zero) return;
        var talkAddon = (AtkUnitBase*)addon;
        if (!IsAddonReady(talkAddon)) return;
        ClickTalk.Using(addon).Click();
    }

    void TickSelectSkip()
    {
        var addon = Svc.GameGui.GetAddonByName("SelectString", 1);
        if (addon == IntPtr.Zero) return;
        var selectStrAddon = (AddonSelectString*)addon;
        if (!IsAddonReady(&selectStrAddon->AtkUnitBase))
        {
            return;
        }
        PluginLog.Debug($"1: {selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString()}");
        if (!SkipCutsceneStr.Contains(selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString())) return;
        ThrottleManager.Throttle(delegate
        {
            PluginLog.Debug("Selecting cutscene skipping");
            ClickSelectString.Using(addon).SelectItem(0);
        }, 500);
    }

    bool IsDisableButtonHeld()
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
