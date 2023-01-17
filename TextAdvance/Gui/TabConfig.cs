namespace TextAdvance.Gui;

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
        ImGui.Checkbox("Automatic quest accept (QA)", ref P.config.MainConfig.EnableQuestAccept);
        ImGui.Checkbox("Automatic quest complete (QC)", ref P.config.MainConfig.EnableQuestComplete);
        ImGui.Checkbox("Automatic talk skip (TS)", ref P.config.MainConfig.EnableTalkSkip);
        ImGui.Checkbox("Semi-automatic request handin (RH)", ref P.config.MainConfig.EnableRequestHandin);
        ImGui.Checkbox("Automatic ESC press during cutscene (CS)", ref P.config.MainConfig.EnableCutsceneEsc);
        ImGui.Checkbox("Automatic cutscene skip confirmation (CC)", ref P.config.MainConfig.EnableCutsceneSkipConfirm);
        ImGui.Separator();
        ImGui.Checkbox("Enable overlay when plugin is enabled", ref P.config.EnableOverlay);
        if (P.config.EnableOverlay)
        {
            ImGui.SetNextItemWidth(100f);
            ImGui.DragFloat2("Overlay offset", ref P.config.OverlayOffset);
        }
    }
}
