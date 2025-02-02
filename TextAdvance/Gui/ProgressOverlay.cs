using Dalamud.Interface.Utility;
using ECommons.SimpleGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Gui;
public class ProgressOverlay : EzOverlayWindow
{
    public ProgressOverlay() : base("TextAdvance progress overlay", HorizontalPosition.Left, VerticalPosition.Bottom)
    {
        P.configGui.ws.AddWindow(this);
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
                P.EntityOverlay.TaskManager.Abort();
                if (C.Navmesh) P.NavmeshManager.Stop();
            }
        }
        var percent = 1f - (float)P.EntityOverlay.TaskManager.NumQueuedTasks / (float)P.EntityOverlay.TaskManager.MaxTasks;
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, EColor.Red);
        ImGui.ProgressBar(percent, new(ImGui.GetContentRegionAvail().X, 20));
        ImGui.PopStyleColor();
    }

    public override bool DrawConditions()
    {
        return P.EntityOverlay.TaskManager.IsBusy;
    }
}
