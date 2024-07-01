using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ECommons.Automation.UIInput;

namespace TextAdvance.Executors;

internal unsafe static class ExecQuestAccept
{
    internal static void Tick()
    {
        if(TryGetAddonByName<AtkUnitBase>("JournalAccept", out var addon) && IsAddonReady(addon))
        {
            var button = addon->GetButtonNodeById(44);
            if (button->IsEnabled)
            {
                if (EzThrottler.Throttle("JournalAcceptAccept"))
                {
                    PluginLog.Debug("Accepting quest");
                    button->ClickAddonButton(addon);
                }
            }
        }
    }
}
