using Dalamud.Interface.Utility;

namespace TextAdvance.Gui;
public class ProgressOverlay : EzOverlayWindow
{
    public ProgressOverlay() : base("TextAdvance progress overlay", HorizontalPosition.Left, VerticalPosition.Bottom)
    {
        this.Flags &= ~(ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoBackground);
    }

    public override void PreDrawAction()
    {
        var v = ImGuiHelpers.MainViewport.Size.X;
        this.SetSizeConstraints(new(v, 0), new(v, float.MaxValue));
    }

    public override void DrawAction()
    {
        CImGui.igBringWindowToDisplayFront(CImGui.igGetCurrentWindow());
        if (ImGui.IsWindowHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            ImGui.SetTooltip("Right click to stop all tasks");
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                S.EntityOverlay.TaskManager.Abort();
                if (C.Navmesh) P.NavmeshManager.Stop();
            }
        }
        var percent = 1f - (float)S.EntityOverlay.TaskManager.NumQueuedTasks / (float)S.EntityOverlay.TaskManager.MaxTasks;
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, EColor.Red);
        ImGui.ProgressBar(percent, sizeArg: new(ImGui.GetContentRegionAvail().X, 20));
        ImGui.PopStyleColor();
    }

    public override bool DrawConditions()
    {
        return S.EntityOverlay.TaskManager.IsBusy;
    }
}
