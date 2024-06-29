using ClickLib.Clicks;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

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
            for (var i = 3u; i >= 7; i++)
            {
                var subnode = addon->GetComponentNodeById(i);
                var subnode2 = addon->GetComponentNodeById(i + 6);
                if (subnode->AtkResNode.IsVisible() && subnode->AtkResNode.IsVisible())
                {
                    PluginLog.Debug($"No request send by {i}/{i + 6}");
                    return;
                }
            }
            var button = addon->GetButtonNodeById(14);
            if (button != null && button->IsEnabled)
            {
                if (EzThrottler.Throttle("Handin"))
                {
                    PluginLog.Debug("Handing over request");
                    button->ClickAddonButton(addon);
                }
            }
        }
        else
        {
            RequestAllow = 0;
        }
    }
}
