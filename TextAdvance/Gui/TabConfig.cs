using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Gui
{
    internal static class TabConfig
    {
        internal static void Draw()
        {
            ImGui.Checkbox("Enable plugin (non-persistent)", ref P.Enabled);
            ImGui.Separator();
            ImGui.Text("Button to hold to temporarily disable plugin when active:");
            ImGui.SetNextItemWidth(200f);
            ImGuiEx.EnumCombo("##HoldDisable", ref P.config.TempDisableButton);
            ImGui.Text("Button to hold to temporarily enable plugin when inactive:");
            ImGui.SetNextItemWidth(200f);
            ImGuiEx.EnumCombo("##HoldEnable", ref P.config.TempEnableButton);
            ImGui.Separator();
            ImGui.Text("Functions: ");
            ImGui.Checkbox("Automatic quest accept", ref P.config.MainConfig.EnableQuestAccept);
            ImGui.Checkbox("Automatic quest complete", ref P.config.MainConfig.EnableQuestComplete);
            ImGui.Checkbox("Automatic talk skip", ref P.config.MainConfig.EnableTalkSkip);
            ImGui.Checkbox("Semi-automatic request handin", ref P.config.MainConfig.EnableRequestHandin);
            ImGui.Checkbox("Automatic ESC press during cutscene", ref P.config.MainConfig.EnableCutsceneEsc);
            ImGui.Checkbox("Automatic cutscene skip confirmation", ref P.config.MainConfig.EnableCutsceneSkipConfirm);
        }
    }
}
