using ClickLib.Clicks;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace TextAdvance.Executors;

internal unsafe static class ExecConfirmCutsceneSkip
{
    internal static void Tick()
    {
        var addon = Svc.GameGui.GetAddonByName("SelectString", 1);
        if (addon == IntPtr.Zero) return;
        var selectStrAddon = (AddonSelectString*)addon;
        if (!IsAddonReady(&selectStrAddon->AtkUnitBase))
        {
            return;
        }
        PluginLog.Debug($"1: {selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString()}");
        if (!Lang.SkipCutsceneStr.Contains(selectStrAddon->AtkUnitBase.UldManager.NodeList[3]->GetAsAtkTextNode()->NodeText.ToString())) return;
        if (EzThrottler.Throttle("SkipCutsceneConfirm"))
        {
            PluginLog.Debug("Selecting cutscene skipping");
            ClickSelectString.Using(addon).SelectItem(0);
        }
    }
}
