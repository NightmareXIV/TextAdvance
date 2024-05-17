using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Component.SteamApi.Callbacks;
using System.Collections.Frozen;
using static TextAdvance.SplatoonHandler;

namespace TextAdvance.Executors;
public unsafe static class ExecAutoInteract
{
    public readonly static ObjectDescriptor[] Blacklist = ((ObjectDescriptor[])[
        new(1006568, 335, ObjectKind.EventNpc, new(-559.4f, -1.9f, -318.3f)), //Imperial Courier at Mor Dhona
        new(1006567, 335, ObjectKind.EventNpc, new(-532.2f, -1.9f, -284.5f)), //Imperial Decurion at Mor Dhona
        new(1006569, 335, ObjectKind.EventNpc, new(-491.4f, -3.9f, -300.8f)), //Imperial Guard at Mor Dhona
        new(1007611, 335, ObjectKind.EventNpc, new(-536.4f, -1.9f, -308.0f)), //Imperial Centurion at Mor Dhona
        new(1006649, 147, ObjectKind.EventNpc, new(-34.5f, 47.0f, 32.7f)), //Flame Private First Class at Northern Thanalan
        new(1006648, 147, ObjectKind.EventNpc, new(-76.1f, 50.0f, -59.5f)), //Flame Private Second Class at Northern Thanalan
        new(1006650, 147, ObjectKind.EventNpc, new(-85.2f, 48.0f, -34.4f)), //Flame Private Third Class at Northern Thanalan
        new(1046055, 133, ObjectKind.EventNpc, new(-30.2f, 10.3f, -258.1f)), //Clive at Old Gridania
        new(1006416, 155, ObjectKind.EventNpc, new(445.1f, 304.9f, -258.6f)), //Hourlinet at Coerthas Central Highlands
        new(1006469, 155, ObjectKind.EventNpc, new(-886.3f, 228.3f, 1.6f)), //House Durendaire Knight at Coerthas Central Highlands
        new(2002189, 138, ObjectKind.EventObj, new(-307.8f, -41.7f, 695.4f)), //Campfire at Western La Noscea
        new(1006525, 155, ObjectKind.EventNpc, new(-478.4f, 288.5f, 168.0f)), //Y'shtola at Coerthas Central Highlands
        new(1006560, 156, ObjectKind.EventNpc, new(-60.1f, 3.1f, -641.4f)), //Biggs at Mor Dhona
        new(2002376, 335, ObjectKind.EventObj, new(-427.8f, -0.8f, -273.8f)), //Steel Door at Mor Dhona
        new(2002252, 140, ObjectKind.EventObj, new(-662.3f, 62.3f, -805.5f)), //Destination at Western Thanalan
        new(1039201, 1053, ObjectKind.EventNpc, new(-704.0f, -185.7f, 480.0f)), //Lahabrea at The Porta Decumana
        new(1009282, 156, ObjectKind.EventNpc, new(133.0f, -2.2f, -556.2f)), //Hoary Boulder at Mor Dhona
        new(1010046, 147, ObjectKind.EventNpc, new(-140.1f, 58.3f, -82.1f)), //Ilberd at Northern Thanalan
        new(1006454, 155, ObjectKind.EventNpc, new(-168.8f, 304.2f, -328.7f)), //Marcelain at Coerthas Central Highlands



        ]);

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
            if (!x.ObjectKind.EqualsAny(ObjectKind.EventNpc, ObjectKind.EventObj, ObjectKind.Aetheryte)) continue;
            var id = x.Struct()->NamePlateIconId;
            var descriptor = new ObjectDescriptor(x);
            if (Blacklist.Contains(descriptor)) continue;
            if (Markers.MSQ.Contains(id) || Markers.ImportantSideProgress.Contains(id) || Markers.SideProgress.Contains(id))
            {
                Interact(x);
            }
            else if (x.ObjectKind.EqualsAny(ObjectKind.EventObj, ObjectKind.Aetheryte) && x.IsTargetable && (Markers.EventObjWhitelist.Contains(x.DataId) || Markers.EventObjNameWhitelist.ContainsIgnoreCase(x.Name.ToString())))
            {
                Interact(x);
            }
        }
    }

    static float GetMinDistance(GameObject obj)
    {
        if (obj.ObjectKind == ObjectKind.Aetheryte) return 8f;
        if (obj.ObjectKind == ObjectKind.EventNpc) return 6f + obj.HitboxRadius;
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
                P.EntityOverlay.TaskManager.Abort();
                P.NavmeshManager.Stop();
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
