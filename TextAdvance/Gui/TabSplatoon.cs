using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Gui
{
    public static class TabSplatoon
    {
        public static void Draw()
        {
            ImGuiEx.TextWrapped("These functions require Splatoon plugin installed and enabled.");
            if(Svc.PluginInterface.InstalledPlugins.TryGetFirst(x => x.InternalName == "Splatoon", out var plugin))
            {
                if (plugin.IsLoaded)
                {
                    ImGuiEx.TextWrapped(EColor.Green, $"You have Splatoon v{plugin.Version} installed and enabled.");
                }
                else
                {
                    ImGuiEx.TextWrapped(EColor.Red, $"You have Splatoon v{plugin.Version} installed but not enabled.");
                }
            }
            else
            {
                ImGuiEx.TextWrapped(EColor.Red, $"You do not have Splatoon installed.");
                if (ImGui.Button("Get Splatoon")) ShellStart("https://puni.sh/plugin/Splatoon");
            }
            ImGui.Checkbox($"Display quest target indicators", ref P.config.MainConfig.QTIQuestEnabled);
            ImGui.ColorEdit4($"Quest target indicator color", ref P.config.MainConfig.QTIQuestColor, ImGuiColorEditFlags.NoInputs);
            ImGui.Checkbox($"Quest target indicator tether", ref P.config.MainConfig.QTIQuestTether);
            ImGuiEx.SetNextItemWidthScaled(60f);
            ImGui.DragFloat($"Quest target indicator thickness", ref P.config.MainConfig.QTIQuestThickness, 0.02f, 1f, 10f);
            ImGui.Separator();
            ImGui.Checkbox($"Enable event object finder", ref P.config.EObjFinder);
            ImGui.Checkbox($"Enable event NPC finder", ref P.config.ENpcFinder);
            ImGuiEx.SetNextItemWidthScaled(150f);
            ImGuiEx.EnumCombo("Display only while holding key", ref P.config.FinderKey);
        }
    }
}
