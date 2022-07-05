using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Gui
{
    internal static class TabTerritory
    {
        static uint SelectedKey = uint.MaxValue;
        static bool OnlyModded = false;
        static string Filter = string.Empty;
        internal static void Draw()
        {
            ImGui.Checkbox("Global enable overrides local settings", ref P.config.GlobalOverridesLocal);
            ImGuiEx.TextWrapped("If this checkbox is checked, when enabling plugin with /at command per-territory settings will become irrelevant " +
                "and global settings will be used.\nOtherwise per-territory settings will always be used, regardless of plugin's global state.");
            ImGuiEx.Text("Current plugin state: globally ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.Enabled ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed, P.Enabled ? "enabled" : "disabled");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(", locally ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.IsTerritoryEnabled() ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed, P.IsTerritoryEnabled() ? "enabled" : "disabled");
            ImGuiEx.SetNextItemFullWidth();
            if (ImGui.BeginCombo("##terrselect", P.TerritoryNames.TryGetValue(SelectedKey, out var selected) ? selected : "Select a territory..."))
            {
                ImGui.SetNextItemWidth(100f);
                ImGui.InputTextWithHint("##selectflts", "Filter", ref Filter, 50);
                ImGui.SameLine();
                ImGui.Checkbox("Only modified", ref OnlyModded);
                if (P.TerritoryNames.TryGetValue(Svc.ClientState.TerritoryType, out var current) && ImGui.Selectable($"Current: {current}"))
                {
                    SelectedKey = Svc.ClientState.TerritoryType;
                }
                ImGui.Separator();
                foreach (var x in P.TerritoryNames)
                {
                    if (Filter != string.Empty && !x.Value.Contains(Filter, StringComparison.OrdinalIgnoreCase)) continue;
                    if (OnlyModded && !P.config.TerritoryConditions.ContainsKey(x.Key)) continue;
                    if (ImGui.Selectable(x.Value))
                    {
                        SelectedKey = x.Key;
                    }
                    if(ImGui.IsWindowAppearing() && SelectedKey == x.Key)
                    {
                        ImGui.SetScrollHereY();
                    }
                }
            }
            if(P.TerritoryNames.TryGetValue(SelectedKey, out var terr))
            {
                if(P.config.TerritoryConditions.TryGetValue(SelectedKey, out var settings))
                {
                    if(ImGui.Button("Remove custom settings"))
                    {
                        P.config.TerritoryConditions.Remove(SelectedKey);
                    }
                    ImGui.Checkbox("Automatic quest accept", ref settings.EnableQuestAccept);
                    ImGui.Checkbox("Automatic quest complete", ref settings.EnableQuestComplete);
                    ImGui.Checkbox("Automatic talk skip", ref settings.EnableTalkSkip);
                    ImGui.Checkbox("Semi-automatic request handin", ref settings.EnableRequestHandin);
                    ImGui.Checkbox("Automatic ESC press during cutscene", ref settings.EnableCutsceneEsc);
                    ImGui.Checkbox("Automatic cutscene skip confirmation", ref settings.EnableCutsceneSkipConfirm);
                }
                else
                {
                    ImGuiEx.Text("No custom settings are present for this area.");
                    if (ImGui.Button("Create custom settings"))
                    {
                        P.config.TerritoryConditions[SelectedKey] = new();
                    }
                }
            }
        }
    }
}
