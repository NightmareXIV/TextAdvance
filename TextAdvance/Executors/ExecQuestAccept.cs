using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Executors
{
    internal unsafe static class ExecQuestAccept
    {
        internal static void Tick()
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
            if (!Lang.AcceptStr.Contains(Marshal.PtrToStringUTF8((IntPtr)textComponent->NodeText.StringPtr))) return;
            if (textComponent->AtkResNode.Color.A != 255) return;
            if (EzThrottler.Throttle("Accept"))
            {
                PluginLog.Debug("Accepting quest");
                ClickJournalAccept.Using(addon).Accept((AtkComponentButton*)buttonNode);
            }
        }
    }
}
