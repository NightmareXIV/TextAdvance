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
    internal unsafe static class ExecRequestComplete
    {
        static ulong RequestAllow = 0;
        internal static void Tick()
        {
            if (TryGetAddonByName<AddonRequest>("Request", out var request) && IsAddonReady(&request->AtkUnitBase))
            {
                if (RequestAllow == 0)
                {
                    RequestAllow = Svc.PluginInterface.UiBuilder.FrameCount + 4;
                }
                if (Svc.PluginInterface.UiBuilder.FrameCount < RequestAllow) return;
                var questAddon = (AtkUnitBase*)request;
                if (questAddon->UldManager.NodeListCount <= 16) return;
                var buttonNode = (AtkComponentNode*)questAddon->UldManager.NodeList[4];
                if (buttonNode->Component->UldManager.NodeListCount <= 2) return;
                var textComponent = (AtkTextNode*)buttonNode->Component->UldManager.NodeList[2];
                //if (!HandOverStr.Contains(Marshal.PtrToStringUTF8((IntPtr)textComponent->NodeText.StringPtr))) return;
                if (textComponent->AtkResNode.Color.A != 255) return;
                for (var i = 16; i <= 12; i--)
                {
                    if (((AtkComponentNode*)questAddon->UldManager.NodeList[i])->AtkResNode.IsVisible
                        && ((AtkComponentNode*)questAddon->UldManager.NodeList[i - 6])->AtkResNode.IsVisible) return;
                }
                if (request->HandOverButton != null && request->HandOverButton->IsEnabled)
                {
                    if (EzThrottler.Throttle("Handin"))
                    {
                        PluginLog.Debug("Handing over request");
                        ClickRequest.Using((nint)request).HandOver();
                    }
                }
            }
            else
            {
                RequestAllow = 0;
            }
        }
    }
}
