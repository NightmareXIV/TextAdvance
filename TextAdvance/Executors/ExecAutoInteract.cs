using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Component.SteamApi.Callbacks;
using static TextAdvance.SplatoonHandler;

namespace TextAdvance.Executors;
public unsafe static class ExecAutoInteract
{
    public readonly static HashSet<ObjectQuestID> InteractedObjects = [];
    public static void Tick()
    {
        if (!Player.Interactable) return;
        if (!IsScreenReady()) return;
        if (Svc.Condition[ConditionFlag.InFlight] || Svc.Condition[ConditionFlag.Jumping]) return;
        if (IsOccupied())
        {
            EzThrottler.Throttle("Occupied", 500, true);
            return;
        }
        if (!EzThrottler.Check("Occupied")) return;
        foreach (var x in Svc.Objects)
        {
            if (!x.ObjectKind.EqualsAny(ObjectKind.EventNpc, ObjectKind.EventObj)) continue;
            var id = x.Struct()->NamePlateIconId;
            if (Markers.MSQ.Contains(id) || Markers.ImportantSideProgress.Contains(id) || Markers.SideProgress.Contains(id))
            {
                Interact(x);
            }
            else if (x.ObjectKind == ObjectKind.EventObj && x.IsTargetable && (Markers.EventObjWhitelist.Contains(x.DataId) || Markers.EventObjNameWhitelist.ContainsIgnoreCase(x.Name.ToString())))
            {
                Interact(x);
            }
        }
    }

    static float GetMinDistance(GameObject obj)
    {
        if (obj.ObjectKind == ObjectKind.EventNpc) return 7f;
        if (obj.ObjectKind == ObjectKind.EventObj) return 3f;
        return -999f;
    }

    static void Interact(GameObject obj)
    {
        if (WasInteracted(obj)) return;
        if (Svc.Targets.Target.AddressEquals(obj))
        {
            if (obj.IsTargetable && Vector3.Distance(Player.Object.Position, obj.Position) < GetMinDistance(obj) && Utils.ThrottleAutoInteract())
            {
                P.EntityOverlay.TaskManager.Abort();
                P.NavmeshManager.Stop();
                RecordInteractionWith(obj);
                TargetSystem.Instance()->InteractWithObject(Svc.Targets.Target.Struct(), false);
            }
        }
        else
        {
            if (obj.IsTargetable && Vector3.Distance(Player.Object.Position, obj.Position) < GetMinDistance(obj) && !IsOccupied() && EzThrottler.Throttle("SetTargetIN"))
            {
                Svc.Targets.Target = obj;
            }
        }
    }

    static void RecordInteractionWith(GameObject obj)
    {
        InteractedObjects.RemoveWhere(x => x.DataID == obj.DataId);
        InteractedObjects.Add(new(obj.DataId));
    }

    public static bool WasInteracted(GameObject obj)
    {
        if(obj == null) return false;
        return InteractedObjects.Contains(new(obj.DataId));
    }
}
