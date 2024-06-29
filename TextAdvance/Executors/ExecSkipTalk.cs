using ClickLib.Clicks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using ECommons.Configuration;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace TextAdvance.Executors;

internal unsafe static class ExecSkipTalk
{
    internal static bool IsEnabled = false;

    internal static void Init()
    {
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "Talk", Click);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "Talk", Click);
    }

    internal static void Shutdown()
    {
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "Talk", Click);
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostUpdate, "Talk", Click);
    }

    private static void Click(AddonEvent type, AddonArgs args)
    {
        if (IsEnabled && IsAddonReady((AtkUnitBase*)args.Addon))
        {
            Callback.Fire((AtkUnitBase*)args.Addon, true);
        }
    }
}
