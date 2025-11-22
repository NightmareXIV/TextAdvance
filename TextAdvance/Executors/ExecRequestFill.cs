using ECommons.Automation;
using ECommons.Automation.LegacyTaskManager;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Callback = ECommons.Automation.Callback;

namespace TextAdvance.Executors;

//by Taurenkey https://github.com/PunishXIV/PandorasBox/blob/24a4352f5b01751767c7ca7f1d4b48369be98711/PandorasBox/Features/UI/AutoSelectTurnin.cs
internal static unsafe class ExecRequestFill
{
    private static bool active = false;
    private static List<int> SlotsFilled { get; set; } = [];
    private static TaskManager TaskManager => P.TaskManager;
    public static bool DontFillThisWindow = false;
    
    internal static void Tick()
    {
        if (TryGetAddonByName<AddonRequest>("Request", out var addon) && IsAddonReady((AtkUnitBase*)addon))
        {
            if (DontFillThisWindow) return;

            for (var i = 1; i <= addon->EntryCount; i++)
            {
                active = true;
                if (SlotsFilled.Contains(addon->EntryCount))
                {
                    P.TaskManager.Abort();
                    return;
                }
                if (SlotsFilled.Contains(i)) return;
                var val = i;
                TaskManager.Enqueue(() => TryClickItem(addon, val));
            }
        }
        else
        {
            DontFillThisWindow = false;
            active = false;
            SlotsFilled.Clear();
            TaskManager.Abort();
        }
    }

    private static bool? TryClickItem(AddonRequest* addon, int i)
    {
        if (SlotsFilled.Contains(i)) return true;

        var contextMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("ContextIconMenu", 1).Address;

        if (contextMenu is null || !contextMenu->IsVisible)
        {
            var slot = i - 1;

            Callback.Fire(&addon->AtkUnitBase, false, 2, slot, 0, 0);
            return false;
        }
        else
        {
            var contextIconMenu = (AddonContextIconMenu*)contextMenu;
            var entryCount = contextIconMenu->EntryCount;
            
            // Determine which option to select based on quality preference
            var qualityPref = C.GetRequestFillQualityPreference();
            int selectedIndex = 0; // Default to first option
            
            if (entryCount > 1 && qualityPref == RequestFillQualityPreference.HQ)
            {
                // When both NQ and HQ exist, game lists HQ first (index 0), NQ second (index 1)
                selectedIndex = 0; // Select HQ
                PluginLog.Debug($"Slot {i}: {entryCount} qualities, selecting HQ (index {selectedIndex})");
            }
            else if (entryCount > 1 && qualityPref == RequestFillQualityPreference.NQ)
            {
                selectedIndex = 1; // Select NQ (second option when both available)
                PluginLog.Debug($"Slot {i}: {entryCount} qualities, selecting NQ (index {selectedIndex})");
            }
            else
            {
                // Any quality or only one option available - use first
                selectedIndex = 0;
                PluginLog.Debug($"Slot {i}: Using first available option (index {selectedIndex})");
            }
            
            // Fire callback to select item from context menu.
            Callback.Fire(contextMenu, false, 0, selectedIndex, 1021003, 0, 0);
            SlotsFilled.Add(i);
            return true;
        }
    }

    internal static List<uint> GetRequestedItemList()
    {
        var ret = new List<uint>();
        if (TryGetAddonByName<AddonRequest>("Request", out var addon) && IsAddonReady((AtkUnitBase*)addon))
        {
            var invman = InventoryManager.Instance();
        }
        return ret;
    }
}
