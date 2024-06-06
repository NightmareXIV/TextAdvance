using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation;
using ECommons.ChatMethods;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Interop;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TextAdvance.SplatoonHandler;

namespace TextAdvance.Navmesh;
public unsafe class MoveManager
{
    private MoveManager() { }

    private void Log(string message)
    {
        PluginLog.Debug($"[MoveManager] {message}");
        if (P.config.NavStatusChat)
        {
            ChatPrinter.PrintColored(UIColor.Pink, $"[TextAdvance] {message}");
        }
    }

    public void MoveToQuest()
    {
        if (!Player.Available) return;
        //P.EntityOverlay.AutoFrame = CSFramework.Instance()->FrameCounter + 1;
        var obj = GetNearestMTQObject();
        if(obj != null)
        {
            EnqueueMoveAndInteract(new(obj.Position, obj.DataId));
            Log($"Precise nav: {obj.Name}/{obj.DataId:X8}");
        }
        else
        {
            var markers = Utils.GetEligibleMapMarkerLocations();
            if(markers.Count > 0)
            {
                var marker = markers.OrderBy(x => Vector3.Distance(x, Player.Object.Position)).First();
                EnqueueMoveAndInteract(new(marker, 0));
                Log($"Non-precise nav: {marker}");
            }
        }
    }

    private GameObject GetNearestMTQObject(Vector3? reference = null, float? maxDistance = null)
    {
        if (!Player.Available) return null;
        if (!(C.Navmesh && P.NavmeshManager.IsReady())) return null;
        reference ??= Player.Object.Position;
        foreach (var x in Svc.Objects.OrderBy(z => Vector3.Distance(reference.Value, z.Position)))
        {
            if (maxDistance != null && Vector3.Distance(reference.Value, x.Position) > maxDistance) continue;
            if (x.IsMTQ()) return x;
        }
        return null;
    }

    public void EnqueueMoveAndInteract(MoveData data)
    {
        P.EntityOverlay.TaskManager.Abort();
        if (Svc.Condition[ConditionFlag.InFlight])
        {
            Svc.Toasts.ShowError("[TextAdvance] Flying pathfinding is not supported");
            return;
        }
        if (Vector3.Distance(data.Position, Player.Object.Position) > 20f)
        {
            P.EntityOverlay.TaskManager.Enqueue(MountIfCan);
        }
        P.EntityOverlay.TaskManager.Enqueue(FlyIfCan);
        P.EntityOverlay.TaskManager.Enqueue(() => MoveToPosition(data, 3f));
        P.EntityOverlay.TaskManager.Enqueue(() => WaitUntilArrival(data, 3f), 10 * 60 * 1000);
        P.EntityOverlay.TaskManager.Enqueue(P.NavmeshManager.Stop);
        if (C.NavmeshAutoInteract)
        {
            P.EntityOverlay.TaskManager.Enqueue(() =>
            {
                var obj = data.GetGameObject();
                if(obj != null)
                {
                    P.EntityOverlay.TaskManager.Insert(() => InteractWithDataID(obj.DataId));
                }
            });
        }
    }

    public bool? FlyIfCan()
    {
        if (Utils.CanFly())
        {
            if (Svc.Condition[ConditionFlag.InFlight])
            {
                return true;
            }
            else
            {
                if (Svc.Condition[ConditionFlag.Jumping]) EzThrottler.Throttle("Jump", 500, true);
                if (EzThrottler.Throttle("Jump"))
                {
                    Chat.Instance.ExecuteCommand($"/generalaction \"{Utils.GetGeneralActionName(2)}\"");
                }
            }
        }
        else
        {
            return true;
        }
        return false;
    }

    public bool? MountIfCan()
    {
        if (Svc.Condition[ConditionFlag.Mounted])
        {
            return true;
        }
        if (C.Mount == -1) return true;
        if (Svc.Condition[ConditionFlag.Unknown57] || Svc.Condition[ConditionFlag.Casting])
        {
            EzThrottler.Throttle("CheckMount", 2000, true);
        }
        if (!EzThrottler.Check("CheckMount")) return false;
        if (ActionManager.Instance()->GetActionStatus(ActionType.GeneralAction, 9) == 0)
        {
            if (EzThrottler.Throttle("SummonMount"))
            {
                if (C.Mount == 0)
                {
                    Chat.Instance.ExecuteCommand($"/generalaction \"{Utils.GetGeneralActionName(9)}\"");
                }
                else
                {
                    Chat.Instance.ExecuteCommand($"/mount \"{Utils.GetMountName(C.Mount)}\"");
                }
            }
        }
        else
        {
            return true;
        }
        return false;
    }

    public void MoveToPosition(MoveData data, float distance)
    {
        var pos = data.Position;
        if (Vector3.Distance(Player.Object.Position, pos) > distance)
        {
            P.NavmeshManager.PathfindAndMoveTo(pos, Svc.Condition[ConditionFlag.InFlight]);
        }
    }

    public bool? WaitUntilArrival(MoveData data, float distance)
    {
        if(data.DataID == 0)
        {
            var obj = GetNearestMTQObject();
            if(obj != null)
            {
                data.Position = obj.Position;
                data.DataID = obj.DataId;
                EzThrottler.Reset("RequeueMoveTo");
                Log($"Correction to MTQ object: {obj.Name}/{obj.DataId:X8}");
            }
            else
            {
                if(Vector3.Distance(data.Position, Player.Object.Position) < 30f)
                {
                    foreach(var x in Svc.Objects.OrderBy(z => Vector3.Distance(data.Position, z.Position)))
                    {
                        if(Vector3.Distance(data.Position, x.Position) < 50f && x.ObjectKind.EqualsAny(ObjectKind.EventNpc | ObjectKind.EventObj) && x.IsTargetable)
                        {
                            data.Position = x.Position;
                            data.DataID = x.DataId;
                            EzThrottler.Reset("RequeueMoveTo");
                            Log($"Correction to non-MTQ object: {x.Name}/{x.DataId:X8}");
                        }
                    }
                }
            }
        }
        var pos = data.Position;
        if (EzThrottler.Throttle("RequeueMoveTo", 5000))
        {
            MoveToPosition(data, distance);
        }
        if (Vector3.Distance(Player.Object.Position, pos) > 10f && !Svc.Condition[ConditionFlag.Mounted] && !Svc.Condition[ConditionFlag.InCombat] && ActionManager.Instance()->GetActionStatus(ActionType.Action, 7557) == 0 && !Player.Object.StatusList.Any(z => z.StatusId == 1199) && EzThrottler.Throttle("CastPeloton"))
        {
            Chat.Instance.ExecuteCommand($"/action \"{Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(7557).Name.ExtractText()}\"");
        }
        return Vector3.Distance(Player.Object.Position, pos) < distance;
    }

    public bool? InteractWithDataID(uint dataID)
    {
        if (!Player.Interactable) return false;
        if (Svc.Targets.Target != null)
        {
            var t = Svc.Targets.Target;
            if (t.IsTargetable && t.DataId == dataID && Vector3.Distance(Player.Object.Position, t.Position) < 10f && !IsOccupied() && Utils.ThrottleAutoInteract())
            {
                TargetSystem.Instance()->InteractWithObject(Svc.Targets.Target.Struct(), false);
                return true;
            }
        }
        else
        {
            foreach (var t in Svc.Objects)
            {
                if (t.IsTargetable && t.DataId == dataID && Vector3.Distance(Player.Object.Position, t.Position) < 10f && !IsOccupied() && EzThrottler.Throttle("SetTarget"))
                {
                    Svc.Targets.Target = t;
                    return false;
                }
            }
        }
        return false;
    }
}
