using ECommons.Automation;
using ECommons.Automation.LegacyTaskManager;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

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

        var contextMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("ContextIconMenu", 1);

        if (contextMenu is null || !contextMenu->IsVisible)
        {
            var slot = i - 1;
            var unk = 44 * i + (i - 1);

            Callback.Fire(&addon->AtkUnitBase, false, 2, slot, 0, 0);

            return false;
        }
        else
        {
            Callback.Fire(contextMenu, false, 0, 0, 1021003, 0, 0);
            PluginLog.Debug($"Filled slot {i}");
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
