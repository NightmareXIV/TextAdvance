using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Gui;
public static class TabNavmesh
{
    static string Filter = "";
    public unsafe static void Draw()
    {
        ImGui.Checkbox("Enabled", ref C.Navmesh);
        ImGui.Checkbox($"Auto-interact upon arrival", ref C.NavmeshAutoInteract);
        //ImGui.Checkbox($"Allow flight", ref C.EnableFlight);
        ImGui.SetNextItemWidth(200f);
        var current = C.Mount == -1 ? "Use no mount" : (C.Mount == 0 ? "Mount roulette" : $"{Utils.GetMountName(C.Mount)}");
        if(ImGui.BeginCombo($"##mount", current))
        {
            ImGui.InputTextWithHint($"##fltr", "Search", ref Filter, 50);
            if (ImGui.Selectable("Use no mount", C.Mount == -1)) C.Mount = -1;
            if (ImGui.Selectable("Mount roulette", C.Mount == 0)) C.Mount = 0;
            foreach (var x in Svc.Data.GetExcelSheet<Mount>())
            {
                var n = x.Singular.ExtractText();
                if (n == "") continue;
                if (Filter != "" && !n.Contains(Filter, StringComparison.OrdinalIgnoreCase)) continue;
                if (C.Mount == x.RowId && ImGui.IsWindowAppearing()) ImGui.SetScrollHereY(); 
                if (ImGui.Selectable($"{n}##{x.RowId}", C.Mount == x.RowId))
                {
                    C.Mount = (int)x.RowId;
                }
            }
            ImGui.EndCombo();
        }
    }
}
