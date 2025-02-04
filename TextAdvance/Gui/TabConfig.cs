using Dalamud.Interface.Components;
using ECommons.SplatoonAPI;
using Lumina.Excel.Sheets;
using NightmareUI.PrimaryUI;

namespace TextAdvance.Gui;

internal static class TabConfig
{
    public static string Filter = "";
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

        if (S.IPCProvider.IsInExternalControl())
        {
            ImGuiEx.TextWrapped(EColor.RedBright, $"TextAdvance is externally controlled by {S.IPCProvider.Requester}. All your settings are being ignored.");
            if (ImGui.SmallButton("Cancel external control"))
            {
                S.IPCProvider.Requester = null;
                S.IPCProvider.ExternalConfig = null;
            }
        }

        new NuiBuilder()
        .Section("Plugin operation")
        .Widget(() =>
        {
            ImGui.Checkbox("Enable plugin (non-persistent)", ref P.Enabled);
            ImGui.Checkbox($"Don't auto-disable plugin on logout", ref C.DontAutoDisable);
            ImGui.Checkbox($"Enable quest target indicators (global, persistent)", ref C.QTIEnabled);
            ImGui.Indent();
            ImGui.Checkbox("Enable indicators when plugin is disabled", ref C.QTAEnabledWhenTADisable);
            ImGui.Checkbox("Enable finder when plugin is disabled", ref C.QTAFinderEnabledWhenTADisable);
            if (C.QTIEnabled)
            {
                if (!Splatoon.IsConnected())
                {
                    ImGuiEx.TextWrapped(EColor.PurpleBright, "You need to have Splatoon plugin installed and enabled for quest target indicators to work");
                }
            }
            ImGui.Unindent();
            ImGui.SetNextItemWidth(150f);
            ImGui.SliderFloat("Auto-interact max radius", ref C.AutoInteractMaxRadius, 3f, 10f);
            ImGuiEx.HelpMarker("Lowering this radius may cause issues", ImGuiColors.DalamudOrange, FontAwesomeIcon.ExclamationTriangle.ToIconString());
        })

        .Section("Keybinds")
        .Widget(() =>
        {
            ImGui.Text("Button to hold to temporarily disable plugin when active:");
            ImGui.SetNextItemWidth(200f);
            ImGuiEx.EnumCombo("##HoldDisable", ref C.TempDisableButton);
            ImGui.Text("Button to hold to temporarily enable plugin when inactive:");
            ImGui.SetNextItemWidth(200f);
            ImGuiEx.EnumCombo("##HoldEnable", ref C.TempEnableButton);
        })

        .Section("Functions")
        .Widget(() =>
        {
            ImGui.Checkbox("Automatic quest accept (QA)", ref C.MainConfig.EnableQuestAccept);
            ImGuiComponents.HelpMarker("Automatically accepts new quests when talking to quest initiating NPC");
            ImGui.Checkbox("Automatic quest complete (QC)", ref C.MainConfig.EnableQuestComplete);
            ImGuiComponents.HelpMarker("Automatically completes quests when talking to NPC that completes quest");
            ImGui.Checkbox("Automatic reward pick (RP)", ref C.MainConfig.EnableRewardPick);
            ImGuiComponents.HelpMarker("Automatically picks quest completion reward based on simple rules that are configured below");
            ImGui.Checkbox("Automatic talk skip (TS)", ref C.MainConfig.EnableTalkSkip);
            ImGuiComponents.HelpMarker("Automatically advances most of the subtitles. Some subtitles may only be advanced manually still.");
            ImGui.Checkbox("Auto-confirm request handins (RH)", ref C.MainConfig.EnableRequestHandin);
            ImGuiComponents.HelpMarker("Automatically confirms most of item requests once filled. Some requests may not be automatically confirmed.");
            ImGui.Checkbox("Automatic request fill (RF)", ref C.MainConfig.EnableRequestFill);
            ImGuiComponents.HelpMarker("Automatically fills item request window. Some requests may not be automatically filled.");
            ImGui.Checkbox("Automatic ESC press during cutscene (CS)", ref C.MainConfig.EnableCutsceneEsc);
            ImGuiComponents.HelpMarker("Automatically presses ESC key during cutscene when cutscene is skippable. Does not skips normally unskippable cutscenes.");
            ImGui.Checkbox("Automatic cutscene skip confirmation (CC)", ref C.MainConfig.EnableCutsceneSkipConfirm);
            ImGuiComponents.HelpMarker("Automatically confirms cutscene skips upon pressing ESC.");
            ImGui.Checkbox("Automatic interaction with quest-related object (IN)", ref C.MainConfig.EnableAutoInteract);
            ImGuiComponents.HelpMarker("Automatically interacts with nearby quest-related NPCs and objects.");
        })

        .Section("Overlay")
        .Widget(() =>
        {
            ImGui.Checkbox("Enable overlay when plugin is enabled", ref C.EnableOverlay);
            if (C.EnableOverlay)
            {
                ImGui.SetNextItemWidth(100f);
                ImGui.DragFloat2("Overlay offset", ref C.OverlayOffset);
            }
        })

        .Section("Notifications")
        .Widget(() =>
        {
            ImGui.Checkbox($"Disable manual plugin state change notification", ref C.NotifyDisableManualState);
            ImGui.Checkbox($"Disable notification upon login if character has auto-enable on", ref C.NotifyDisableOnLogin);
            ImGui.Checkbox($"Disable reward pick chat notification", ref C.PickRewardSilent);
        })

        .Section("Reward picking order")
        .Widget(() =>
        {
            ImGuiEx.TextWrapped($"Please note: precision is not guaranteed. ");
            ImGuiEx.EnumOrderer("", C.PickRewardOrder);
        })

        .Section("Navigation")
        .Widget(() =>
        {
            ImGui.Checkbox("Enabled##autointeract", ref C.Navmesh);
            ImGuiEx.PluginAvailabilityIndicator([new("vnavmesh")]);
            ImGui.Checkbox($"Auto-interact upon arrival", ref C.NavmeshAutoInteract);
            ImGui.SetNextItemWidth(200f);
            var current = C.Mount == -1 ? "Use no mount" : (C.Mount == 0 ? "Mount roulette" : $"{Utils.GetMountName(C.Mount)}");
            if (ImGui.BeginCombo($"##mount", current))
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
            ImGui.Checkbox("Use sprint and peloton if possible", ref C.UseSprintPeloton);
            ImGui.Checkbox($"Print navigation status into chat", ref C.NavStatusChat);
            ImGui.Checkbox("(very experimental) Allow flight", ref C.EnableFlight);
            //ImGui.Checkbox($"Allow teleporting to the nearest Aetheryte to a flag", ref C.EnableTeleportToFlag);
        })

        .Draw();
    }
}
