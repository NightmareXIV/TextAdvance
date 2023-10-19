using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace TextAdvance.Executors;

internal unsafe static class ExecSkipTalk
{
    internal static void Tick()
    {
        var addon = Svc.GameGui.GetAddonByName("Talk", 1);
        if (addon == IntPtr.Zero) return;
        var talkAddon = (AtkUnitBase*)addon;
        if (!IsAddonReady(talkAddon)) return;
        ClickTalk.Using(addon).Click();
    }
}
