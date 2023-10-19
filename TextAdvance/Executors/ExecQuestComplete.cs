using ClickLib.Clicks;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Executors
{
    internal unsafe static class ExecQuestComplete
    {
        internal static void Tick()
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
            if (!Lang.CompleteStr.Contains(Marshal.PtrToStringUTF8((IntPtr)textComponent->NodeText.StringPtr))) return;
            if (textComponent->AtkResNode.Color.A != 255) return;
            //pi.Framework.Gui.Chat.Print(Environment.TickCount + " Pass");
            if (!((AddonJournalResult*)addon)->CompleteButton->IsEnabled) return;
            if (EzThrottler.Throttle("Complete"))
            {
                PluginLog.Debug("Completing quest");
                ClickJournalResult.Using(addon).Complete();
            }
        }
    }
}
