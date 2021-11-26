using ClickLib;
using ClickLib.Clicks;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Internal;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static TextAdvance.ClickManager;

// some code has been copied from https://github.com/daemitus/ClickLib
namespace TextAdvance
{
    unsafe class TextAdvance : IDalamudPlugin
    {
        internal ClickManager clickManager;
        internal bool InCutscene = false;
        internal bool WasInCutscene = false;
        internal bool Enabled = false;
        bool CanPressEsc = false;
        static string[] AcceptStr = { "Accept", "接受", "Annehmen" };
        static string[] SkipCutsceneStr = { "Skip cutscene?", "要跳过这段过场动画吗？", "Videosequenz überspringen?" };
        static string[] YesStr = { "Yes.", "是", "Ja" };
        static string[] CompleteStr = { "Complete", "完成", "Abschließen" };
        static string[] HandOverStr = { "Hand Over" };
        internal Config config;
        internal ConfigGui configGui;
        bool loggedIn = false;

        public string Name => "TextAdvance";

        public void Dispose()
        {
            Svc.Framework.Update -= Tick;
            Svc.ClientState.Logout -= Logout;
            Svc.ClientState.Login -= Login;
            Svc.Commands.RemoveHandler("/at");
        }

        public TextAdvance(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Svc>();
            config = Svc.PluginInterface.GetPluginConfig() as Config ?? new Config();
            clickManager = new ClickManager(this);
            Svc.Framework.Update += Tick;
            Svc.ClientState.Logout += Logout;
            Svc.ClientState.Login += Login;
            configGui = new ConfigGui(this);
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.IsOpen = true; };
            Svc.Commands.AddHandler("/at", new CommandInfo(HandleCommand)
            {
                ShowInHelp = true,
                HelpMessage = "toggles TextAdvance plugin. Note: you MUST enable it every time you are logging in for it to work. Every time you log out, plugin will disable itself." +
                "\nHold shift when plugin is on to temporarily pause skipping. Hold alt to temporarily enable skipping while plugin is disabled."
            });
            if (Svc.ClientState.IsLoggedIn)
            {
                loggedIn = true;
            }
            //Click.Initialize();
        }

        private void Logout(object sender, EventArgs e)
        {
            Enabled = false;
        }

        private void Login(object sender, EventArgs e)
        {
            loggedIn = true;
        }

        private void HandleCommand(string command, string arguments)
        {
            if(arguments == "test")
            {
                try
                {
                    Svc.Chat.Print("test");
                    //ClickRequest.Using(Svc.GameGui.GetAddonByName("Request", 1)).RightClickItem(0);
                }
                catch(Exception e)
                {
                    Svc.Chat.Print($"{e.Message}\n{e.StackTrace ?? ""}");
                }
                return;
            }
            Enabled = !Enabled;
            Svc.Toasts.ShowQuest("Auto advance " + (Enabled ? "Enabled" : "Disabled"),
                new QuestToastOptions() { PlaySound = true, DisplayCheckmark = true });
        }


        [HandleProcessCorruptedStateExceptions]
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
                if (!IsDisableButtonHeld() || !Enabled)
                {
                    if (Enabled)
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
                            if (!selectStrAddon->IsVisible) skip = false;
                        }
                        if (InCutscene && !skip)
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
                                    if (CanPressEsc)
                                    {
                                        Native.Keypress.SendKeycode(Process.GetCurrentProcess().MainWindowHandle, Native.Keypress.Escape);
                                        CanPressEsc = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            CanPressEsc = true;
                        }
                        if (InCutscene)
                        {
                            TickSelectSkip();
                        }
                    }
                    if ((Enabled || (IsEnableButtonHeld() && Native.ApplicationIsActivated())) &&
                        (Svc.Condition[ConditionFlag.OccupiedInQuestEvent] ||
                        Svc.Condition[ConditionFlag.Occupied33] ||
                        Svc.Condition[ConditionFlag.OccupiedInEvent] ||
                        Svc.Condition[ConditionFlag.Occupied30] ||
                        Svc.Condition[ConditionFlag.Occupied38] ||
                        Svc.Condition[ConditionFlag.Occupied39] ||
                        Svc.Condition[ConditionFlag.OccupiedSummoningBell] ||
                        Svc.Condition[ConditionFlag.WatchingCutscene] ||
                        InCutscene))
                    {
                        TickTalk();
                        TickQuestComplete();
                        TickQuestAccept();
                        TickRequestComplete();
                    }
                }
                WasInCutscene = InCutscene;
            }
            catch(NullReferenceException e)
            {
                PluginLog.Debug(e.Message + "" + e.StackTrace);
                Svc.Chat.Print(e.Message + "" + e.StackTrace);
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
            if (!questAddon->IsVisible) return;
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
                    Svc.PluginInterface.UiBuilder.AddNotification("Clicking");
                    //ClickRequest.Using(addon).HandOver();
                }, 500);
            }
        }

        uint ticksQuestCompleteVisible = 0;
        void TickQuestComplete()
        {
            var addon = Svc.GameGui.GetAddonByName("JournalResult", 1);
            if (addon == IntPtr.Zero)
            {
                ticksQuestCompleteVisible = 0;
                return;
            }
            ticksQuestCompleteVisible++;
            if (ticksQuestCompleteVisible < 5) return;
            var questAddon = (AtkUnitBase*)addon;
            if (!questAddon->IsVisible) return;
            if (questAddon->UldManager.NodeListCount <= 4) return;
            var buttonNode = (AtkComponentNode*)questAddon->UldManager.NodeList[4];
            if (buttonNode->Component->UldManager.NodeListCount <= 2) return;
            var textComponent = (AtkTextNode*)buttonNode->Component->UldManager.NodeList[2];
            if (!CompleteStr.Contains(Marshal.PtrToStringUTF8((IntPtr)textComponent->NodeText.StringPtr))) return;
            if (textComponent->AtkResNode.Color.A != 255) return;
            //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Pass");
            if(!((AddonJournalResult*)addon)->CompleteButton->IsEnabled) return;
            clickManager.SendClickThrottled(addon, EventType.CHANGE, 1, ((AddonJournalResult*)addon)->CompleteButton->AtkComponentBase.OwnerNode);
        }

        uint ticksQuestAcceptVisible = 0;
        void TickQuestAccept()
        {
            var addon = Svc.GameGui.GetAddonByName("JournalAccept", 1);
            if (addon == IntPtr.Zero)
            {
                ticksQuestAcceptVisible = 0;
                return;
            }
            ticksQuestAcceptVisible++;
            if (ticksQuestAcceptVisible < 5) return;
            var questAddon = (AtkUnitBase*)addon;
            if (!questAddon->IsVisible) return;
            if (questAddon->UldManager.NodeListCount <= 6) return;
            var buttonNode = (AtkComponentNode*)questAddon->UldManager.NodeList[6];
            if (buttonNode->Component->UldManager.NodeListCount <= 2) return;
            var textComponent = (AtkTextNode*)buttonNode->Component->UldManager.NodeList[2];
            if (!AcceptStr.Contains(Marshal.PtrToStringUTF8((IntPtr)textComponent->NodeText.StringPtr))) return;
            if (textComponent->AtkResNode.Color.A != 255) return;
            //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Pass");
            clickManager.SendClickThrottled(addon, EventType.CHANGE, 1, buttonNode);
        }

        void TickTalk()
        {
            var addon = Svc.GameGui.GetAddonByName("Talk", 1);
            if (addon == IntPtr.Zero) return;
            var talkAddon = (AtkUnitBase*)addon;
            if (!talkAddon->IsVisible/* || !talkAddon->UldManager.NodeList[14]->IsVisible*/) return;
            //var imageNode = (AtkImageNode*)talkAddon->UldManager.NodeList[14];
            //if (imageNode->PartsList->Parts[imageNode->PartId].U != 288) return;
            clickManager.SendClick(addon, EventType.MOUSE_CLICK, 0, ((AddonTalk*)talkAddon)->AtkStage);
        }

        void TickSelectSkip()
        {
            var addon = Svc.GameGui.GetAddonByName("SelectString", 1);
            if (addon == IntPtr.Zero) return;
            var selectStrAddon = (AtkUnitBase*)addon;
            if (!selectStrAddon->IsVisible)
            {
                //NextClick = Environment.TickCount + 500;
                return;
            }
            if (selectStrAddon->UldManager.NodeListCount <= 3) return;
            var a = (AtkComponentNode*)selectStrAddon->UldManager.NodeList[2];
            var txt = (AtkTextNode*)selectStrAddon->UldManager.NodeList[3];
            if (!SkipCutsceneStr.Contains(Marshal.PtrToStringUTF8((IntPtr)txt->NodeText.StringPtr))) return;
            if (a->Component->UldManager.NodeListCount <= 2) return;
            var b = (AtkComponentNode*)a->Component->UldManager.NodeList[1];
            if (b->Component->UldManager.NodeListCount <= 3) return;
            var c = (AtkTextNode*)b->Component->UldManager.NodeList[3];
            if (!YesStr.Contains(Marshal.PtrToStringUTF8((IntPtr)c->NodeText.StringPtr))) return;
            clickManager.SelectStringClick(addon, 0);
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
}
