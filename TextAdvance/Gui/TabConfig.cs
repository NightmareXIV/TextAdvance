using Dalamud.Interface.Components;
using ECommons.SplatoonAPI;

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
        ImGui.Checkbox($"Don't auto-disable plugin on logout", ref P.config.DontAutoDisable);
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
        ImGuiComponents.HelpMarker("Automatically accepts new quests when talking to quest initiating NPC");
        ImGui.Checkbox("Automatic quest complete (QC)", ref P.config.MainConfig.EnableQuestComplete);
        ImGuiComponents.HelpMarker("Automatically completes quests when talking to NPC that completes quest");
        ImGui.Checkbox("Automatic reward pick (RP)", ref P.config.MainConfig.EnableRewardPick);
        ImGuiComponents.HelpMarker("Automatically picks quest completion reward based on simple rules that are configured below");
        ImGui.Checkbox("Automatic talk skip (TS)", ref P.config.MainConfig.EnableTalkSkip);
        ImGuiComponents.HelpMarker("Automatically advances most of the subtitles. Some subtitles may only be advanced manually still.");
        ImGui.Checkbox("Auto-confirm request handins (RH)", ref P.config.MainConfig.EnableRequestHandin);
        ImGuiComponents.HelpMarker("Automatically confirms most of item requests once filled. Some requests may not be automatically confirmed.");
        ImGui.Checkbox("Automatic request fill (RF)", ref P.config.MainConfig.EnableRequestFill);
        ImGuiComponents.HelpMarker("Automatically fills item request window. Some requests may not be automatically filled.");
        ImGui.Checkbox("Automatic ESC press during cutscene (CS)", ref P.config.MainConfig.EnableCutsceneEsc);
        ImGuiComponents.HelpMarker("Automatically presses ESC key during cutscene when cutscene is skippable. Does not skips normally unskippable cutscenes.");
        ImGui.Checkbox("Automatic cutscene skip confirmation (CC)", ref P.config.MainConfig.EnableCutsceneSkipConfirm);
        ImGuiComponents.HelpMarker("Automatically confirms cutscene skips upon pressing ESC.");
        ImGui.Checkbox("Automatic interaction with quest-related object (IN)", ref P.config.MainConfig.EnableAutoInteract);
        ImGuiComponents.HelpMarker("Automatically interacts with nearby quest-related NPCs and objects.");
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
        ImGui.Checkbox($"Disable reward pick chat notification", ref P.config.PickRewardSilent);
        ImGui.Separator();
        ImGuiEx.TextWrapped($"Reward pick item prioritization order. Please note: precision is not guaranteed. ");
        ImGuiEx.EnumOrderer("", P.config.PickRewardOrder);
    }
}
