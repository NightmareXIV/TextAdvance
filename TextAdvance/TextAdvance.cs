using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Game.Internal.Gui.Toast;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static TextAdvance.ClickManager;

namespace TextAdvance
{
    unsafe class TextAdvance : IDalamudPlugin
    {
        internal DalamudPluginInterface pi;
        internal ClickManager clickManager;
        internal bool InCutscene = false;
        internal bool WasInCutscene = false;
        internal bool Enabled = false;
        bool CanPressEsc = false;

        public string Name => "TextAdvance";

        public void Dispose()
        {
            pi.Framework.OnUpdateEvent -= Tick;
            pi.ClientState.OnLogout -= Logout;
            pi.CommandManager.RemoveHandler("/at");
            pi.Dispose();
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;
            clickManager = new ClickManager(this);
            pi.Framework.OnUpdateEvent += Tick;
            pi.ClientState.OnLogout += Logout;
            pi.CommandManager.AddHandler("/at", new CommandInfo(HandleCommand)
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
            pi.Framework.Gui.Toast.ShowQuest("Auto advance " + (Enabled ? "Enabled" : "Disabled"),
                new QuestToastOptions() { PlaySound = true, DisplayCheckmark = true });
        }


        [HandleProcessCorruptedStateExceptions]
        private void Tick(Framework framework)
        {
            try
            {
                if (!Enabled) return;
                InCutscene = pi.ClientState.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                    || pi.ClientState.Condition[ConditionFlag.WatchingCutscene78];
                var nLoading = pi.Framework.Gui.GetUiObjectByName("NowLoading", 1);
                var skip = true;
                var addon = pi.Framework.Gui.GetUiObjectByName("SelectString", 1);
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
                if (pi.ClientState.Condition[ConditionFlag.OccupiedInQuestEvent] ||
                    pi.ClientState.Condition[ConditionFlag.Occupied33] || InCutscene)
                {
                    TickTalk();
                    TickQuestComplete();
                    TickQuestAccept();
                }
                WasInCutscene = InCutscene;
            }
            catch (Exception e)
            {
                pi.Framework.Gui.Chat.Print(e.Message + "" + e.StackTrace);
            }
        }

        uint ticksQuestCompleteVisible = 0;
        void TickQuestComplete()
        {
            var addon = pi.Framework.Gui.GetUiObjectByName("JournalResult", 1);
            if (addon == IntPtr.Zero)
            {
                ticksQuestCompleteVisible = 0;
                return;
            }
            ticksQuestCompleteVisible++;
            if (ticksQuestCompleteVisible < 2) return;
            var questAddon = (AtkUnitBase*)addon;
            if (questAddon->UldManager.NodeListCount <= 4) return;
            var buttonNode = (AtkComponentNode*)questAddon->UldManager.NodeList[4];
            if (buttonNode->Component->UldManager.NodeListCount <= 2) return;
            var textComponent = (AtkTextNode*)buttonNode->Component->UldManager.NodeList[2];
            if ("Complete" != Marshal.PtrToStringAnsi((IntPtr)textComponent->NodeText.StringPtr)) return;
            if (textComponent->AtkResNode.Color.A != 255) return;
            //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Pass");
            if(!((AddonJournalResult*)addon)->CompleteButton->IsEnabled) return;
            clickManager.SendClickThrottled(addon, EventType.CHANGE, 1, ((AddonJournalResult*)addon)->CompleteButton->AtkComponentBase.OwnerNode);
        }

        uint ticksQuestAcceptVisible = 0;
        void TickQuestAccept()
        {
            if (ImGui.GetIO().KeyShift) return;
            var addon = pi.Framework.Gui.GetUiObjectByName("JournalAccept", 1);
            if (addon == IntPtr.Zero)
            {
                ticksQuestAcceptVisible = 0;
                return;
            }
            ticksQuestAcceptVisible++;
            if (ticksQuestAcceptVisible < 2) return;
            var questAddon = (AtkUnitBase*)addon;
            if (questAddon->UldManager.NodeListCount <= 6) return;
            var buttonNode = (AtkComponentNode*)questAddon->UldManager.NodeList[6];
            if (buttonNode->Component->UldManager.NodeListCount <= 2) return;
            var textComponent = (AtkTextNode*)buttonNode->Component->UldManager.NodeList[2];
            if ("Accept" != Marshal.PtrToStringAnsi((IntPtr)textComponent->NodeText.StringPtr)) return;
            if (textComponent->AtkResNode.Color.A != 255) return;
            //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Pass");
            clickManager.SendClickThrottled(addon, EventType.CHANGE, 1, buttonNode);
        }

        void TickTalk()
        {
            var addon = pi.Framework.Gui.GetUiObjectByName("Talk", 1);
            if (addon == IntPtr.Zero) return;
            var talkAddon = (AtkUnitBase*)addon;
            if (!talkAddon->IsVisible/* || !talkAddon->UldManager.NodeList[14]->IsVisible*/) return;
            //var imageNode = (AtkImageNode*)talkAddon->UldManager.NodeList[14];
            //if (imageNode->PartsList->Parts[imageNode->PartId].U != 288) return;
            clickManager.SendClick(addon, ClickManager.EventType.MOUSE_CLICK, 0, ((AddonTalk*)talkAddon)->AtkStage);
        }

        void TickSelectSkip()
        {
            var addon = pi.Framework.Gui.GetUiObjectByName("SelectString", 1);
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
            if ("Skip cutscene?" != Marshal.PtrToStringAnsi((IntPtr)txt->NodeText.StringPtr)) return;
            if (a->Component->UldManager.NodeListCount <= 2) return;
            var b = (AtkComponentNode*)a->Component->UldManager.NodeList[1];
            if (b->Component->UldManager.NodeListCount <= 3) return;
            var c = (AtkTextNode*)b->Component->UldManager.NodeList[3];
            if ("Yes." != Marshal.PtrToStringAnsi((IntPtr)c->NodeText.StringPtr)) return;
            clickManager.SelectStringClick(addon, 0);
        }
    }
}
