using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Internal;
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

namespace TextAdvance
{
    unsafe class TextAdvance : IDalamudPlugin
    {
        internal ClickManager clickManager;
        internal bool InCutscene = false;
        internal bool WasInCutscene = false;
        internal bool Enabled = false;
        bool CanPressEsc = false;
        static string[] AcceptStr = { "Accept", "接受" };
        static string[] SkipCutsceneStr = { "Skip cutscene?", "要跳过这段过场动画吗？" };
        static string[] YesStr = { "Yes.", "是" };
        static string[] CompleteStr = { "Complete", "完成" };

        public string Name => "TextAdvance";

        public void Dispose()
        {
            Svc.Framework.Update -= Tick;
            Svc.ClientState.Logout -= Logout;
            Svc.Commands.RemoveHandler("/at");
        }

        public TextAdvance(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Svc>();
            clickManager = new ClickManager(this);
            Svc.Framework.Update += Tick;
            Svc.ClientState.Logout += Logout;
            Svc.Commands.AddHandler("/at", new CommandInfo(HandleCommand)
            {
                ShowInHelp = true,
                HelpMessage = "toggles TextAdvance plugin. Note: you MUST enable it every time you are logging in for it to work. Every time you log out, plugin will disable itself."
            });
        }

        private void Logout(object sender, EventArgs e)
        {
            Enabled = false;
        }

        private void HandleCommand(string command, string arguments)
        {
            Enabled = !Enabled;
            Svc.Toasts.ShowQuest("Auto advance " + (Enabled ? "Enabled" : "Disabled"),
                new QuestToastOptions() { PlaySound = true, DisplayCheckmark = true });
        }


        [HandleProcessCorruptedStateExceptions]
        private void Tick(Framework framework)
        {
            try
            {
                if (!Enabled) return;
                InCutscene = Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                    || Svc.Condition[ConditionFlag.WatchingCutscene78];
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
                if (Svc.Condition[ConditionFlag.OccupiedInQuestEvent] ||
                    Svc.Condition[ConditionFlag.Occupied33] ||
                    Svc.Condition[ConditionFlag.OccupiedInEvent] ||
                    Svc.Condition[ConditionFlag.Occupied30] ||
                    Svc.Condition[ConditionFlag.Occupied38] ||
                    Svc.Condition[ConditionFlag.Occupied39] ||
                    Svc.Condition[ConditionFlag.OccupiedSummoningBell] ||
                    InCutscene)
                {
                    TickTalk();
                    TickQuestComplete();
                    TickQuestAccept();
                }
                WasInCutscene = InCutscene;
            }
            catch (Exception e)
            {
                Svc.Chat.Print(e.Message + "" + e.StackTrace);
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
            if (questAddon->UldManager.NodeListCount <= 4) return;
            var buttonNode = (AtkComponentNode*)questAddon->UldManager.NodeList[4];
            if (buttonNode->Component->UldManager.NodeListCount <= 2) return;
            var textComponent = (AtkTextNode*)buttonNode->Component->UldManager.NodeList[2];
            if (!CompleteStr.Contains(Marshal.PtrToStringAnsi((IntPtr)textComponent->NodeText.StringPtr))) return;
            if (textComponent->AtkResNode.Color.A != 255) return;
            //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Pass");
            if(!((AddonJournalResult*)addon)->CompleteButton->IsEnabled) return;
            clickManager.SendClickThrottled(addon, EventType.CHANGE, 1, ((AddonJournalResult*)addon)->CompleteButton->AtkComponentBase.OwnerNode);
        }

        uint ticksQuestAcceptVisible = 0;
        void TickQuestAccept()
        {
            if (ImGui.GetIO().KeyShift) return;
            var addon = Svc.GameGui.GetAddonByName("JournalAccept", 1);
            if (addon == IntPtr.Zero)
            {
                ticksQuestAcceptVisible = 0;
                return;
            }
            ticksQuestAcceptVisible++;
            if (ticksQuestAcceptVisible < 5) return;
            var questAddon = (AtkUnitBase*)addon;
            if (questAddon->UldManager.NodeListCount <= 6) return;
            var buttonNode = (AtkComponentNode*)questAddon->UldManager.NodeList[6];
            if (buttonNode->Component->UldManager.NodeListCount <= 2) return;
            var textComponent = (AtkTextNode*)buttonNode->Component->UldManager.NodeList[2];
            if (!AcceptStr.Contains(Marshal.PtrToStringAnsi((IntPtr)textComponent->NodeText.StringPtr))) return;
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
            clickManager.SendClick(addon, ClickManager.EventType.MOUSE_CLICK, 0, ((AddonTalk*)talkAddon)->AtkStage);
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
            if (!SkipCutsceneStr.Contains(Marshal.PtrToStringAnsi((IntPtr)txt->NodeText.StringPtr))) return;
            if (a->Component->UldManager.NodeListCount <= 2) return;
            var b = (AtkComponentNode*)a->Component->UldManager.NodeList[1];
            if (b->Component->UldManager.NodeListCount <= 3) return;
            var c = (AtkTextNode*)b->Component->UldManager.NodeList[3];
            if (!YesStr.Contains(Marshal.PtrToStringAnsi((IntPtr)c->NodeText.StringPtr))) return;
            clickManager.SelectStringClick(addon, 0);
        }
    }
}
