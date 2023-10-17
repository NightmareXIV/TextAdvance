using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    //by Taurenkay https://github.com/PunishXIV/PandorasBox/blob/24a4352f5b01751767c7ca7f1d4b48369be98711/PandorasBox/Features/UI/AutoSelectTurnin.cs
    internal unsafe static class RequestFill
    {
        static bool active = false;
        static List<int> SlotsFilled { get; set; } = new();
        static TaskManager TaskManager => P.TaskManager;
        internal static void Tick()
        {
            if (TryGetAddonByName<AddonRequest>("Request", out var addon))
            {
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
                active = false;
                SlotsFilled.Clear();
                TaskManager.Abort();
            }
        }

        static bool? TryClickItem(AddonRequest* addon, int i)
        {
            if (SlotsFilled.Contains(i)) return true;

            var contextMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("ContextIconMenu", 1);

            if (contextMenu is null || !contextMenu->IsVisible)
            {
                var slot = i - 1;
                var unk = (44 * i) + (i - 1);

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


    }
}
