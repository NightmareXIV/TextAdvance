﻿using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility;
using ECommons.Automation.LegacyTaskManager;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Interop;
using ECommons.SplatoonAPI;
using static TextAdvance.SplatoonHandler;

namespace TextAdvance.Gui;
public sealed unsafe class EntityOverlay : IDisposable
{
    public TaskManager TaskManager;
    public ulong AutoFrame;
    public EntityOverlay()
    {
        Svc.PluginInterface.UiBuilder.Draw += this.Draw;
        this.TaskManager = new()
        {
            AbortOnTimeout = true,
        };
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= this.Draw;
    }

    public void Draw()
    {
        var navmeshAvail = C.Navmesh && P.NavmeshManager.IsReady();
        if (Utils.ShouldHideUI()) return;
        var qtaEnabled = (C.QTAEnabledWhenTADisable || P.IsEnabled()) && C.GetQTAQuestEnabled();
        var finderEnabled = (C.QTAFinderEnabledWhenTADisable || P.IsEnabled());
        foreach (var x in Svc.Objects)
        {
            var id = x.Struct()->NamePlateIconId;
            if (qtaEnabled && (Markers.MSQ.Contains(id) || Markers.ImportantSideProgress.Contains(id) || Markers.SideProgress.Contains(id)))
            {
                this.DrawButtonAndPath(x, navmeshAvail);
            }
            else if (qtaEnabled && x.ObjectKind == ObjectKind.EventObj && x.IsTargetable && (Markers.EventObjWhitelist.Contains(x.DataId) || Markers.EventObjNameWhitelist.ContainsIgnoreCase(x.Name.ToString())))
            {
                this.DrawButtonAndPath(x, navmeshAvail);
            }
            else if (x.IsTargetable && finderEnabled)
            {
                var display = false;
                if (x.ObjectKind == ObjectKind.EventObj && C.EObjFinder)
                {
                    display = C.FinderKey == LimitedKeys.None || IsKeyPressed(C.FinderKey);
                }
                if (x.ObjectKind == ObjectKind.EventNpc && C.ENpcFinder)
                {
                    display = C.FinderKey == LimitedKeys.None || IsKeyPressed(C.FinderKey);
                }
                if (display)
                {
                    this.DrawButtonAndPath(x, navmeshAvail);
                }
            }
        }
    }

    private void DrawButtonAndPath(IGameObject obj, bool drawButton)
    {
        if (Vector3.Distance(obj.Position, Player.Object.Position) < 3f) return;
        if (Splatoon.IsConnected() && C.GetQTAQuestTether())
        {
            var e = P.SplatoonHandler.GetFreeElement(obj.Position);
            P.QueuedSplatoonElements.Enqueue(e);
        }
        if (drawButton)
        {
            if (CSFramework.Instance()->FrameCounter == this.AutoFrame)
            {
                Move();
                this.AutoFrame = 0;
            }
            if (Svc.GameGui.WorldToScreen(obj.Position, out var pos))
            {
                var size = ImGuiHelpers.GetButtonSize(FontAwesomeIcon.PersonWalkingArrowRight.ToIconString());
                ImGuiHelpers.ForceNextWindowMainViewport();
                ImGuiHelpers.SetNextWindowPosRelativeMainViewport((pos - size - ImGuiHelpers.MainViewport.Pos));
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                if (ImGui.Begin($"##TextAdvanceButton-{obj.Address}", ImGuiEx.OverlayFlags & ~ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.AlwaysUseWindowPadding))
                {
                    ImGui.PopStyleVar();
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGui.SetWindowFontScale(2f);
                    if (ImGui.Button(FontAwesomeIcon.PersonWalkingArrowRight.ToIconString() + $"##{obj.Address}"))
                    {
                        try
                        {
                            Move();
                        }
                        catch (Exception e)
                        {
                            e.Log();
                        }
                    }
                    ImGui.PopFont();
                    ImGui.SetWindowFontScale(1f);
                    ImGuiEx.Tooltip($"{obj.Name}");
                }
                else
                {
                    ImGui.PopStyleVar();
                }
                ImGui.End();
            }
        }

        void Move()
        {
            S.EntityOverlay.TaskManager.Abort();
            S.MoveManager.EnqueueMoveAndInteract(new(obj.Position, obj.DataId, false), 3f);
        }
    }
}