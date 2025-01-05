using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility;
using ECommons.Automation;
using ECommons.Automation.LegacyTaskManager;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Interop;
using ECommons.SimpleGui;
using ECommons.SplatoonAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using TextAdvance.Navmesh;
using static TextAdvance.SplatoonHandler;

namespace TextAdvance.Gui;
public unsafe sealed class EntityOverlay : IDisposable
{
    public TaskManager TaskManager;
    public ulong AutoFrame;
    public EntityOverlay()
    {
        Svc.PluginInterface.UiBuilder.Draw += Draw;
        TaskManager = new()
        {
            AbortOnTimeout = true,
        };
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= Draw;
    }

    public void Draw()
    {
        if (!(C.Navmesh && P.NavmeshManager.IsReady())) return;
        if (Utils.ShouldHideUI()) return;
        foreach (var x in Svc.Objects)
        {
            var id = x.Struct()->NamePlateIconId;
            if (Markers.MSQ.Contains(id) || Markers.ImportantSideProgress.Contains(id) || Markers.SideProgress.Contains(id))
            {
                DrawButton(x);
            }
            else if (x.ObjectKind == ObjectKind.EventObj && x.IsTargetable && (Markers.EventObjWhitelist.Contains(x.DataId) || Markers.EventObjNameWhitelist.ContainsIgnoreCase(x.Name.ToString())))
            {
                DrawButton(x);
            }
            else if (x.IsTargetable)
            {
                var display = false;
                if (x.ObjectKind == ObjectKind.EventObj && P.config.EObjFinder)
                {
                    display = P.config.FinderKey == LimitedKeys.None || IsKeyPressed(P.config.FinderKey);
                }
                if (x.ObjectKind == ObjectKind.EventNpc && P.config.ENpcFinder)
                {
                    display = P.config.FinderKey == LimitedKeys.None || IsKeyPressed(P.config.FinderKey);
                }
                if (display)
                {
                    DrawButton(x);
                }
            }
        }
    }

    void DrawButton(IGameObject obj)
    {
        if (Vector3.Distance(obj.Position, Player.Object.Position) < 3f) return;
        if(CSFramework.Instance()->FrameCounter == this.AutoFrame)
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
                    catch(Exception e)
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

        void Move()
        {
            P.EntityOverlay.TaskManager.Abort();
            S.MoveManager.EnqueueMoveAndInteract(new(obj.Position, obj.DataId, false), 3f);
        }
    }
}