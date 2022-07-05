using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Gui
{
    internal class Overlay : Window
    {
        public Overlay() : base("TextAdvance overlay", 
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.AlwaysAutoResize)
        {
            this.IsOpen = true;
            this.RespectCloseHotkey = false;
        }

        public override bool DrawConditions()
        {
            return P.config.EnableOverlay && P.IsEnabled();
        }

        public override void PreDraw()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        }

        public override void Draw()
        {
            List<string> l = new();
            ImGuiEx.Text(ImGuiColors.DalamudOrange, "TextAdvance: ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableQuestAccept() ? ImGuiColors.DalamudOrange : ImGuiColors.DalamudGrey2, "QA");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableQuestComplete() ? ImGuiColors.DalamudOrange : ImGuiColors.DalamudGrey2, "QC");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableCutsceneEsc() ? ImGuiColors.DalamudOrange : ImGuiColors.DalamudGrey2, "CS");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableCutsceneSkipConfirm() ? ImGuiColors.DalamudOrange : ImGuiColors.DalamudGrey2, "CC");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableRequestHandin() ? ImGuiColors.DalamudOrange : ImGuiColors.DalamudGrey2, "RH");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableTalkSkip() ? ImGuiColors.DalamudOrange : ImGuiColors.DalamudGrey2, "TS");

            this.Position = new Vector2(ImGuiHelpers.MainViewport.Size.X / 2 - ImGui.GetWindowSize().X / 2, 0) - P.config.OverlayOffset;
        }

        public override void PostDraw()
        {
            ImGui.PopStyleVar();
        }
    }
}
