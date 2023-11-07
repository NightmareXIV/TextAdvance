using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Executors
{
    internal unsafe static class ExecPickReward
    {
        internal static bool IsEnabled = false;
        internal static readonly uint[] CofferIcons = [26557,26509,26558,26559,26560,26561,26562,25916,26564,26565,26566,26567,];
        internal static readonly uint[] GilIcons = [26001];
        internal static Random Random = new();

        internal static void Init()
        {
            Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "JournalResult", OnJournalResultSetup);
        }

        private static void OnJournalResultSetup(AddonEvent type, AddonArgs args)
        {
            if (IsEnabled)
            {
                var canvas = (nint)((AtkUnitBase*)args.Addon)->UldManager.NodeList[7]->GetComponent();
                var r = new ReaderJournalResult((AtkUnitBase*)args.Addon);
                if(r.OptionalRewards.Count > 0)
                {
                    PluginLog.Debug($"Preparing to select optional reward item. Candidates: ({r.OptionalRewards.Count})\n{r.OptionalRewards.Select(x => $"ID:{x.ItemID} / Icon:{x.IconID} / Amount:{x.Amount} / Name:{x.Name} ").Print("\n")}");
                    foreach(var x in r.OptionalRewards)
                    {
                        if(Svc.Data.GetExcelSheet<Item>().GetRow(x.ItemID) == null)
                        {
                            DuoLog.Warning($"Encountered unknown item id: {x.ItemID}. Selecting cancelled. Please report this error with logs.");
                            return;
                        }
                    }
                    foreach(var x in P.config.PickRewardOrder)
                    {
                        {
                            if (x == PickRewardMethod.Gil_sacks && TrySelectGil(r.OptionalRewards, out var index))
                            {
                                PluginLog.Debug($"Selecting {index} = {r.OptionalRewards[index].Name} because it's gil sack");
                                P.Memory.PickRewardItemUnsafe(canvas, index);
                                return;
                            }
                        }
                        {
                            if (x == PickRewardMethod.Highest_vendor_value && TrySelectHighestVendorValue(r.OptionalRewards, out var index))
                            {
                                PluginLog.Debug($"Selecting {index} = {r.OptionalRewards[index].Name} because it's highest vendor value");
                                P.Memory.PickRewardItemUnsafe(canvas, index);
                                return;
                            }
                        }
                        {
                            if (x == PickRewardMethod.Gear_coffer && TrySelectCoffer(r.OptionalRewards, out var index))
                            {
                                PluginLog.Debug($"Selecting {index} = {r.OptionalRewards[index].Name} because it's coffer");
                                P.Memory.PickRewardItemUnsafe(canvas, index);
                                return;
                            }
                        }
                        {
                            if (x == PickRewardMethod.Suitable_item_for_current_job && TrySelectCurrentJobItem(r.OptionalRewards, out var index))
                            {
                                PluginLog.Debug($"Selecting {index} = {r.OptionalRewards[index].Name} because it's current job item");
                                P.Memory.PickRewardItemUnsafe(canvas, index);
                                return;
                            }
                        }
                    }
                    var rand = Random.Next(0, r.OptionalRewards.Count);
                    PluginLog.Debug($"Selecting random reward: {rand}");
                    P.Memory.PickRewardItemUnsafe(canvas, rand);
                    return;
                }
            }
        }

        internal static void Shutdown()
        {
            Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "JournalResult", OnJournalResultSetup);
            Random = null;
        }

        internal static bool TrySelectCoffer(List<ReaderJournalResult.OptionalReward> data, out int index)
        {
            for (var i = 0; i < data.Count; i++)
            {
                var d = data[i];
                if (CofferIcons.Contains(d.IconID))
                {
                    index = i;
                    return true;
                }
            }
            index = default;
            return false;
        }

        internal static bool TrySelectGil(List<ReaderJournalResult.OptionalReward> data, out int index)
        {
            for (var i = 0; i < data.Count; i++)
            {
                var d = data[i];
                if (GilIcons.Contains(d.IconID))
                {
                    index = i;
                    return true;
                }
            }
            index = default;
            return false;
        }

        internal static bool TrySelectHighestVendorValue(List<ReaderJournalResult.OptionalReward> data, out int index)
        {
            var value = 0u;
            index = 0;
            for (var i = 0; i < data.Count; i++)
            {
                var d = data[i];
                var item = Svc.Data.GetExcelSheet<Item>().GetRow(d.ItemID);
                if(item != null && item.PriceLow * d.Amount > value)
                {
                    index = i;
                    value = item.PriceLow * d.Amount;
                }
            }
            return value > 0;
        }

        internal static bool TrySelectCurrentJobItem(List<ReaderJournalResult.OptionalReward> data, out int index)
        {
            if (Player.Available)
            {
                for (var i = 0; i < data.Count; i++)
                {
                    var d = data[i];
                    var item = Svc.Data.GetExcelSheet<Item>().GetRow(d.ItemID);
                    if (item != null && item.ClassJobCategory.Value != null && item.ClassJobCategory.Value.IsJobInCategory((Job)Player.Object.ClassJob.Id))
                    {
                        index = i;
                        return true;
                    }
                }
            }
            index = default;
            return false;
        }
    }
}
