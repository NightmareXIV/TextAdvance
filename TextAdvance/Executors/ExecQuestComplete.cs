﻿
using ECommons.Automation.UIInput;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace TextAdvance.Executors;

internal static unsafe class ExecQuestComplete
{
    internal static void Tick()
    {
        if (TryGetAddonByName<AtkUnitBase>("JournalResult", out var addon) && IsAddonReady(addon))
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
