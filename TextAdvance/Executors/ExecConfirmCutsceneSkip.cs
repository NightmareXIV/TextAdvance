using ECommons.Automation;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Callback = ECommons.Automation.Callback;

namespace TextAdvance.Executors;

internal static unsafe class ExecConfirmCutsceneSkip
{
    internal static void Tick()
    {
        var addon = Svc.GameGui.GetAddonByName("SelectString", 1);
        if (addon == IntPtr.Zero) return;
        var selectStrAddon = (AddonSelectString*)addon.Address;
        if (!IsAddonReady(&selectStrAddon->AtkUnitBase))
        {
            return;
        }
        PluginLog.Debug($"1: {selectStrAddon->GetTextNodeById(2)->NodeText.ToString()}");
        if (!Lang.SkipCutsceneStr.Contains(selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString())) return;
        if (EzThrottler.Throttle("SkipCutsceneConfirm"))
        {
            PluginLog.Debug("Selecting cutscene skipping");
            Callback.Fire((AtkUnitBase*)addon.Address, true, 0);
        }
    }
}
