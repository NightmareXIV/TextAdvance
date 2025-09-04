using ECommons.Automation.UIInput;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace TextAdvance.Executors;

internal static unsafe class ExecQuestAccept
{
    internal static void Tick()
    {
        if (TryGetAddonByName<AtkUnitBase>("JournalAccept", out var addon) && IsAddonReady(addon))
        {
            var button = addon->GetComponentButtonById(44);
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
