using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Services;
public unsafe class MoveManager
{
    private MoveManager() { }

    public void EnqueueMoveAndInteract(GameObject obj, float distance)
    {
        if (Svc.Condition[ConditionFlag.InFlight])
        {
            Svc.Toasts.ShowError("[TextAdvance] Flying pathfinding is not supported");
            return;
        }
        if (Vector3.Distance(obj.Position, Player.Object.Position) > 20f)
        {
            P.EntityOverlay.TaskManager.Enqueue(MountIfCan);
        }
        P.EntityOverlay.TaskManager.Enqueue(FlyIfCan);
        P.EntityOverlay.TaskManager.Enqueue(() => MoveToPosition(obj.Position, distance));
        P.EntityOverlay.TaskManager.Enqueue(() => WaitUntilArrival(obj.Position, distance), 10 * 60 * 1000);
        P.EntityOverlay.TaskManager.Enqueue(P.NavmeshManager.Stop);
        if (C.NavmeshAutoInteract && obj.ObjectKind.EqualsAny(ObjectKind.EventNpc, ObjectKind.EventObj))
        {
            P.EntityOverlay.TaskManager.Enqueue(() => InteractWithDataID(obj.DataId));
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

    public void MoveToPosition(Vector3 pos, float distance)
    {
        if (Vector3.Distance(Player.Object.Position, pos) > distance)
        {
            P.NavmeshManager.PathfindAndMoveTo(pos, Svc.Condition[ConditionFlag.InFlight]);
        }
    }

    public bool? WaitUntilArrival(Vector3 pos, float distance)
    {
        if (EzThrottler.Throttle("RequeueMoveTo", 5000))
        {
            MoveToPosition(pos, distance);
        }
        if (Vector3.Distance(Player.Object.Position, pos) > 10f && !Svc.Condition[ConditionFlag.Mounted] && ActionManager.Instance()->GetActionStatus(ActionType.Action, 7557) == 0 && !Player.Object.StatusList.Any(z => z.StatusId == 1199) && EzThrottler.Throttle("CastPeloton"))
        {
            Chat.Instance.ExecuteCommand($"/action \"{Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(7557).Name.ExtractText()}\"");
        }
        return Vector3.Distance(Player.Object.Position, pos) < distance;
    }

    public bool? InteractWithDataID(uint dataID)
    {
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
