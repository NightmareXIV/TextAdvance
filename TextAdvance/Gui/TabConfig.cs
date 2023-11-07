﻿using ECommons.SplatoonAPI;

namespace TextAdvance.Gui;

internal static class TabConfig
{
    internal static void Draw()
    {
        if (P.BlockList.Count > 0)
        {
            ImGuiEx.TextWrapped(EColor.RedBright, $"TextAdvance is stopped by these plugins: {string.Join(",", P.BlockList)}");
            if (ImGui.SmallButton("Remove all locks"))
            {
                P.BlockList.Clear();
            }
        }
        ImGui.Checkbox("Enable plugin (non-persistent)", ref P.Enabled);
        ImGui.Checkbox($"Enable quest target indicators (global, persistent)", ref P.config.QTIEnabled);
        if(P.config.QTIEnabled)
        {
            if (!Splatoon.IsConnected())
            {
                ImGuiEx.TextWrapped(EColor.PurpleBright, "You need to have Splatoon plugin installed and enabled for quest target indicators to work");
            }
        }
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
        ImGui.Checkbox("Automatic reward pick (RP) (BETA)", ref P.config.MainConfig.EnableRewardPick);
        ImGui.Checkbox("Automatic talk skip (TS)", ref P.config.MainConfig.EnableTalkSkip);
        ImGui.Checkbox("Semi-automatic request handin (RH)", ref P.config.MainConfig.EnableRequestHandin);
        ImGui.Checkbox("Automatic request fill (RF) (NEW!)", ref P.config.MainConfig.EnableRequestFill);
        ImGui.Checkbox("Automatic ESC press during cutscene (CS)", ref P.config.MainConfig.EnableCutsceneEsc);
        ImGui.Checkbox("Automatic cutscene skip confirmation (CC)", ref P.config.MainConfig.EnableCutsceneSkipConfirm);
        ImGui.Separator();
        ImGui.Checkbox($"Display quest target indicators", ref P.config.MainConfig.QTIQuestEnabled);
        ImGui.ColorEdit4($"Quest target indicator color", ref P.config.MainConfig.QTIQuestColor, ImGuiColorEditFlags.NoInputs);
        ImGui.Checkbox($"Quest target indicator tether", ref P.config.MainConfig.QTIQuestTether);
        ImGui.SetNextItemWidth(60f);
        ImGui.DragFloat($"Quest target indicator thickness", ref P.config.MainConfig.QTIQuestThickness, 0.02f, 1f, 10f);
        ImGui.Separator();
        ImGui.Checkbox("Enable overlay when plugin is enabled", ref P.config.EnableOverlay);
        if (P.config.EnableOverlay)
        {
            ImGui.SetNextItemWidth(100f);
            ImGui.DragFloat2("Overlay offset", ref P.config.OverlayOffset);
        }

        ImGui.Separator();
        ImGuiEx.Text($"Notifications: ");
        ImGui.Checkbox($"Disable manual plugin state change notification", ref P.config.NotifyDisableManualState);
        ImGui.Checkbox($"Disable notification upon login if character has auto-enable on", ref P.config.NotifyDisableOnLogin);
        ImGui.Separator();
        ImGuiEx.TextWrapped($"Reward pick item prioritization order. Please note: precision is not guaranteed. ");
        ImGuiEx.EnumOrderer("", P.config.PickRewardOrder);
    }
}
