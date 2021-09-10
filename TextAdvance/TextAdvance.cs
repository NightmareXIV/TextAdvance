using Dalamud.Game.ClientState;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    unsafe class TextAdvance : IDalamudPlugin
    {
        internal DalamudPluginInterface pi;
        internal ClickManager clickManager;
        internal bool InCutscene = false;
        internal bool WasInCutscene = false;

        public string Name => "TextAdvance";

        public void Dispose()
        {
            pi.Framework.OnUpdateEvent -= Tick;
            pi.Dispose();
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;
            clickManager = new ClickManager(this);
            pi.Framework.OnUpdateEvent += Tick;
        }

        private void Tick(Framework framework)
        {
            InCutscene = pi.ClientState.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                || pi.ClientState.Condition[ConditionFlag.WatchingCutscene78];
            TickTalk();
            TickSelectSkip();
            WasInCutscene = InCutscene;
        }

        void TickTalk()
        {
            if (!pi.ClientState.Condition[ConditionFlag.OccupiedInQuestEvent]) return;
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
            if (!InCutscene) return;
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
