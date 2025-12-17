using ECommons.Automation;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Callback = ECommons.Automation.Callback;

namespace TextAdvance.Executors;

internal static unsafe class ExecConfirmCutsceneSkip
{
    internal static void Tick()
    {
        if(TryGetAddonMaster<AddonMaster.SelectString>(out var m) && m.IsAddonReady)
        {
            foreach(var x in m.Entries)
            {
                if (Lang.SkipCutsceneStr.Contains(x.Text))
                {
                    if (EzThrottler.Throttle("SkipCutsceneConfirm"))
                    {
                        x.Select();
                    }
                    return;
                }
            }
        }
    }
}
