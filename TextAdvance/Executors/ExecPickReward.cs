using Dalamud.Game.Addon.Events;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using ECommons.Automation.NeoTaskManager;
using ECommons.ChatMethods;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Executors
{
    internal static unsafe class ExecPickReward
    {
        internal static bool IsEnabled = false;
        internal static readonly uint[] CofferIcons = [26557, 26509, 26558, 26559, 26560, 26561, 26562, 25916, 26564, 26565, 26566, 26567,];
        internal static readonly uint[] GilIcons = [26001];
        internal static Random Random = new();

        internal static void Init()
        {
            Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "JournalResult", OnJournalResultSetup);
        }

        private static void OnJournalResultSetup(AddonEvent type, AddonArgs args)
        {
            var addon = (AtkUnitBase*)args.Addon;
            var canvas = ((AtkComponentNode*)addon->UldManager.NodeList[7])->Component;
            PluginLog.Information($"Component: {(nint)canvas:X16}");
            if (IsEnabled)
            {
                var r = new ReaderJournalResult(addon);
                if (r.OptionalRewards.Count > 0)
                {
                    PluginLog.Information($"Preparing to select optional reward item. Candidates: ({r.OptionalRewards.Count})\n{r.OptionalRewards.Select(x => $"ID:{x.ItemID} / Icon:{x.IconID} / Amount:{x.Amount} / Name:{x.Name} ").Print("\n")}");
                    foreach (var x in r.OptionalRewards)
                    {
                        if (Svc.Data.GetExcelSheet<Item>().GetRowOrDefault(x.ItemID) == null)
                        {
                            DuoLog.Warning($"Encountered unknown item id: {x.ItemID}. Selecting cancelled. Please report this error with logs and screenshot.");
                            return;
                        }
                    }
                    foreach (var x in P.config.PickRewardOrder)
                    {
                        {
                            if (x == PickRewardMethod.Gil_sacks && TrySelectGil(r.OptionalRewards, out var index))
                            {
                                PluginLog.Information($"Selecting {index} = {r.OptionalRewards[index].Name} because it's gil sack");
                                if (!P.config.PickRewardSilent) ChatPrinter.Green($"[TextAdvance] Auto-selected optional reward {index + 1}/{r.OptionalRewards.Count}: {r.OptionalRewards[index].Name} (gil)");
                                P.Memory.PickRewardItemUnsafe((nint)canvas, index);
                                return;
                            }
                        }
                        {
                            if (x == PickRewardMethod.Highest_vendor_value && TrySelectHighestVendorValue(r.OptionalRewards, out var index))
                            {
                                PluginLog.Information($"Selecting {index} = {r.OptionalRewards[index].Name} because it's highest vendor value");
                                if (!P.config.PickRewardSilent) ChatPrinter.Green($"[TextAdvance] Auto-selected optional reward {index + 1}/{r.OptionalRewards.Count}: {r.OptionalRewards[index].Name} (highest value)");
                                P.Memory.PickRewardItemUnsafe((nint)canvas, index);
                                return;
                            }
                        }
                        {
                            if (x == PickRewardMethod.Gear_coffer && TrySelectCoffer(r.OptionalRewards, out var index))
                            {
                                PluginLog.Information($"Selecting {index} = {r.OptionalRewards[index].Name} because it's coffer");
                                if (!P.config.PickRewardSilent) ChatPrinter.Green($"[TextAdvance] Auto-selected optional reward {index + 1}/{r.OptionalRewards.Count}: {r.OptionalRewards[index].Name} (coffer)");
                                P.Memory.PickRewardItemUnsafe((nint)canvas, index);
                                return;
                            }
                        }
                        {
                            if (x == PickRewardMethod.Equipable_item_for_current_job && TrySelectCurrentJobItem(r.OptionalRewards, out var index))
                            {
                                PluginLog.Information($"Selecting {index} = {r.OptionalRewards[index].Name} because it's current job item");
                                if (!P.config.PickRewardSilent) ChatPrinter.Green($"[TextAdvance] Auto-selected optional reward {index + 1}/{r.OptionalRewards.Count}: {r.OptionalRewards[index].Name} (equipable)");
                                P.Memory.PickRewardItemUnsafe((nint)canvas, index);
                                return;
                            }
                        }
                        {
                            if (x == PickRewardMethod.High_quality_gear && TrySelectHighQualityGear(r.OptionalRewards, out var index))
                            {
                                PluginLog.Information($"Selecting {index} = {r.OptionalRewards[index].Name} because it's high quality gear item");
                                if (!P.config.PickRewardSilent) ChatPrinter.Green($"[TextAdvance] Auto-selected optional reward {index + 1}/{r.OptionalRewards.Count}: {r.OptionalRewards[index].Name} (HQ gear item)");
                                P.Memory.PickRewardItemUnsafe((nint)canvas, index);
                                return;
                            }
                        }
                    }
                    var rand = Random.Next(r.OptionalRewards.Count);
                    PluginLog.Information($"Selecting random reward: {rand} - {r.OptionalRewards[rand].Name}");
                    if (!P.config.PickRewardSilent) ChatPrinter.Green($"[TextAdvance] Auto-selected optional reward {rand + 1}/{r.OptionalRewards.Count}: {r.OptionalRewards[rand].Name} (random)");
                    P.Memory.PickRewardItemUnsafe((nint)canvas, rand);
                    return;
                }
            }
        }

        internal static void Shutdown()
        {
            Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "JournalResult", OnJournalResultSetup);
            Random = null;
        }

        internal static string[] DetermineGearCoffer()
        {
            if (Player.Object.GetRole() == CombatRole.Tank) return Lang.CofferOfFending;
            if (Player.Object.GetRole() == CombatRole.Healer) return Lang.CofferOfHealing;
            if (Player.Job.GetUpgradedJob().EqualsAny(Job.BRD, Job.DNC, Job.MCH)) return Lang.CofferOfAiming;
            if (Player.Job.GetUpgradedJob().EqualsAny(Job.DRG, Job.RPR)) return Lang.CofferOfMaiming;
            if (Player.Job.GetUpgradedJob().EqualsAny(Job.NIN, Job.VPR)) return Lang.CofferOfScouting;
            if (Player.Object.ClassJob.Value.Role == 2) return Lang.CofferOfSlaying; //other melees
            if (Player.Object.GetRole() == CombatRole.DPS) return Lang.CofferOfCasting; //only casters left
            return null; //doh/dol
        }

        internal static string[] DetermineAccessoryCoffer()
        {
            if (Player.Object.GetRole() == CombatRole.Tank) return Lang.CofferOfFending;
            if (Player.Object.GetRole() == CombatRole.Healer) return Lang.CofferOfHealing;
            if (Player.Job.GetUpgradedJob().EqualsAny(Job.BRD, Job.DNC, Job.MCH, Job.NIN, Job.VPR)) return Lang.CofferOfAiming;
            if (Player.Object.ClassJob.Value.Role == 3) return Lang.CofferOfCasting; //phys rangeds are excluded before
            if (Player.Object.GetRole() == CombatRole.DPS) return Lang.CofferOfStriking; //only non-aiming melee left
            return null; //doh/dol
        }

        internal static bool TrySelectCoffer(List<ReaderJournalResult.OptionalReward> data, out int index)
        {
            List<int> possible = [];
            var accessoryString = DetermineAccessoryCoffer();
            var gearString = DetermineGearCoffer();
            for (var i = 0; i < data.Count; i++)
            {
                if (gearString != null && data[i].Name.ContainsAny(StringComparison.OrdinalIgnoreCase, gearString))
                {
                    PluginLog.Information($"Gear string was {gearString.Print()}");
                    index = i;
                    return true;
                }
                if (accessoryString != null && data[i].Name.ContainsAny(StringComparison.OrdinalIgnoreCase, gearString))
                {
                    PluginLog.Information($"Accessory string was {gearString.Print()}");
                    index = i;
                    return true;
                }
            }
            for (var i = 0; i < data.Count; i++)
            {
                var d = data[i];
                if (CofferIcons.Contains(d.IconID))
                {
                    possible.Add(i);
                }
            }
            if (possible.Count > 0)
            {
                index = possible[Random.Next(possible.Count)];
                return true;
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
                var item = Svc.Data.GetExcelSheet<Item>().GetRowOrDefault(d.ItemID);
                if (item != null && item.Value.PriceLow * d.Amount > value)
                {
                    index = i;
                    value = item.Value.PriceLow * d.Amount;
                }
            }
            return value > 0;
        }

        internal static bool TrySelectCurrentJobItem(List<ReaderJournalResult.OptionalReward> data, out int index)
        {
            List<int> possible = [];
            if (Player.Available)
            {
                for (var i = 0; i < data.Count; i++)
                {
                    var d = data[i];
                    var item = Svc.Data.GetExcelSheet<Item>().GetRowOrDefault(d.ItemID);
                    if (item != null && item.Value.ClassJobCategory.ValueNullable != null && item.Value.ClassJobCategory.Value.IsJobInCategory((Job)Player.Object.ClassJob.RowId))
                    {
                        possible.Add(i);
                    }
                }
            }
            if (possible.Count > 0)
            {
                index = possible[Random.Next(possible.Count)];
                return true;
            }
            index = default;
            return false;
        }

        internal static readonly uint[] GearCats = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 40, 41, 42, 43, 84, 87, 88, 89, 96, 97, 98, 99, 105, 106, 107, 108, 109];
        internal static bool TrySelectHighQualityGear(List<ReaderJournalResult.OptionalReward> data, out int index)
        {
            List<int> possible = [];
            for (var i = 0; i < data.Count; i++)
            {
                var d = data[i];
                var item = Svc.Data.GetExcelSheet<Item>().GetRowOrDefault(d.ItemID);
                if (d.IsHQ && item != null && item.Value.ItemUICategory.ValueNullable?.RowId.EqualsAny(GearCats) == true)
                {
                    possible.Add(i);
                }
            }
            if (possible.Count > 0)
            {
                index = possible[Random.Next(possible.Count)];
                return true;
            }
            index = default;
            return false;
        }
    }
}
