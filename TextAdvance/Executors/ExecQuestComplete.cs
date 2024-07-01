using ClickLib.Clicks;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ECommons.Automation.UIInput;

namespace TextAdvance.Executors;

internal unsafe static class ExecQuestComplete
{
    internal static void Tick()
    {
        if(TryGetAddonByName<AtkUnitBase>("JournalResult", out var addon) && IsAddonReady(addon))
        {
            var button = addon->GetButtonNodeById(37);
            if (button->IsEnabled)
            {
                if (EzThrottler.Throttle("JournalResultComplete"))
                {
                    PluginLog.Debug("Completing quest");
                    button->ClickAddonButton(addon);
                }
            }
        }
    }
}
