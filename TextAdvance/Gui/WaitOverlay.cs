using Dalamud.Interface.Utility;

namespace TextAdvance.Gui;

internal class WaitOverlay : Window
{
    public WaitOverlay() : base("TAWaitOverlay", ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse, true)
    {
        this.IsOpen = true;
        this.Position = Vector2.Zero;
        this.RespectCloseHotkey = false;
    }

    internal long StartTime = 0;
    internal int Frame = 0;

    public override bool DrawConditions()
    {
        return P.TaskManager.IsBusy;
    }

    public override void PreDraw()
    {
        ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, 0x00000088u.Vector4FromRGBA());
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor();
    }

    public override void Draw()
    {
        ImGui.SetWindowFocus();
        if (ImGui.GetFrameCount() - Frame > 1) StartTime = Environment.TickCount64;
        Frame = ImGui.GetFrameCount();
        CImGui.igBringWindowToDisplayFront(CImGui.igGetCurrentWindow());
        ImGui.Dummy(new(ImGuiHelpers.MainViewport.Size.X, ImGuiHelpers.MainViewport.Size.Y / 3));
        ImGuiEx.ImGuiLineCentered("Waitoverlay1", () => ImGuiEx.Text($"Filling in request."));
        ImGuiEx.ImGuiLineCentered("Waitoverlay2", () => ImGuiEx.Text($"This can take couple seconds. If this process is stuck, please click the button below."));
        ImGuiEx.Text("");
        var span = TimeSpan.FromMilliseconds(Environment.TickCount64 - StartTime);
        ImGuiEx.ImGuiLineCentered("Waitoverlay4", () => ImGuiEx.Text($"{span.Minutes:D2}:{span.Seconds:D2}"));
        ImGuiEx.Text("");
        ImGuiEx.Text("");
        ImGuiEx.ImGuiLineCentered("Waitoverlay3", () =>
        {
            if (ImGui.Button("Cancel"))
            {
                P.TaskManager.Abort();
            }
        });
    }
}
