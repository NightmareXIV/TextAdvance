using Dalamud.Interface.Utility;

namespace TextAdvance.Gui;

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
        return P.config.EnableOverlay && P.IsEnabled(true);
    }

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
    }

    public override void Draw()
    {
        var blocked = P.BlockList.Count != 0;
        List<string> l = [];
        var col = blocked ? ImGuiColors.DalamudRed : ImGuiColors.DalamudOrange;
        if(S.IPCProvider.IsInExternalControl() && !blocked)
        {
            col = EColor.GreenBright;
        }
        if (P.IsDisableButtonHeld())
        {
            col = ImGuiColors.DalamudRed;
        }
        ImGuiEx.Text(col, "TextAdvance: ");
        ImGui.SameLine(0, 0);
        if (!blocked || Environment.TickCount64 % 2000 > 1000)
        {
            ImGuiEx.Text(P.config.GetEnableQuestAccept() ? col : ImGuiColors.DalamudGrey2, "QA");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableQuestComplete() ? col : ImGuiColors.DalamudGrey2, "QC");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableRewardPick() ? col : ImGuiColors.DalamudGrey2, "RP");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableCutsceneEsc() ? col : ImGuiColors.DalamudGrey2, "CS");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableCutsceneSkipConfirm() ? col : ImGuiColors.DalamudGrey2, "CC");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableRequestHandin() ? col : ImGuiColors.DalamudGrey2, "RH");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableRequestFill() ? col : ImGuiColors.DalamudGrey2, "RF");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableTalkSkip() ? col : ImGuiColors.DalamudGrey2, "TS");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableAutoInteract() ? col : ImGuiColors.DalamudGrey2, "IN");
            /*ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(P.config.GetEnableAutoSnipe() ? col : ImGuiColors.DalamudGrey2, "SN");*/
        }
        else
        {
            ImGuiEx.Text(ImGuiColors.DalamudRed, $"paused externally");
        }

        this.Position = ImGuiHelpers.MainViewport.Pos + new Vector2(ImGuiHelpers.MainViewport.Size.X / 2 - ImGui.GetWindowSize().X / 2, 0) - P.config.OverlayOffset;
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar();
    }
}
