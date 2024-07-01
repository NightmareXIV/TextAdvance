using ClickLib.Clicks;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ECommons.Automation.UIInput;
using ECommons.UIHelpers.AddonMasterImplementations;

namespace TextAdvance.Executors;

internal unsafe static class ExecRequestComplete
{
    static ulong RequestAllow = 0;
    internal static void Tick()
    {
        if (TryGetAddonByName<AtkUnitBase>("Request", out var addon) && IsAddonReady(addon))
        {
            if (RequestAllow == 0)
            {
                RequestAllow = Svc.PluginInterface.UiBuilder.FrameCount + 4;
            }
            if (Svc.PluginInterface.UiBuilder.FrameCount < RequestAllow) return;
            var m = new RequestMaster(addon);
            if (m.IsHandOverEnabled && m.IsFilled)
            {
                if (EzThrottler.Throttle("Handin"))
                {
                    PluginLog.Debug("Handing over request");
                    m.HandOver();
                }
            }
        }
        else
        {
            RequestAllow = 0;
        }
    }
}
