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
        new(1013498, 401, ObjectKind.EventNpc, new(-293.0f, -183.7f, 812.7f)), //Emmanellain at The Sea of Clouds
        new(1012359, 401, ObjectKind.EventNpc, new(-260.9f, -184.9f, 705.4f)), //House Haillenarte Knight at The Sea of Clouds
        new(1012360, 401, ObjectKind.EventNpc, new(-337.8f, -186.1f, 618.6f)), //House Haillenarte Knight at The Sea of Clouds
        new(1012381, 419, ObjectKind.EventNpc, new(120.7f, 15.0f, -156.6f)), //Haurchefant at The Pillars
        new(1013028, 145, ObjectKind.EventNpc, new(-380.9f, -23.1f, 388.6f)), //Hozan at Eastern Thanalan
        new(1011905, 397, ObjectKind.EventNpc, new(617.5f, 78.4f, 217.3f)), //Tristechambel at Coerthas Western Highlands
        new(1014811, 397, ObjectKind.EventNpc, new(319.7f, 161.1f, 147.4f)), //Rostnsthal at Coerthas Western Highlands
        new(1012773, 418, ObjectKind.EventNpc, new(92.5f, 15.0f, 37.2f)), //Hilda at Foundation
        new(1012070, 401, ObjectKind.EventNpc, new(-554.1f, -57.6f, -547.4f)), //Kunu Vali at The Sea of Clouds
        new(1012827, 402, ObjectKind.EventNpc, new(228.2f, 14.9f, 554.0f)), //Guidance Node at Azys Lla
        new(1016034, 419, ObjectKind.EventNpc, new(-182.4f, -0.2f, -70.7f)), //Aymeric at The Pillars
        new(1016587, 155, ObjectKind.EventNpc, new(-133.9f, 304.2f, -300.8f)), //Emmanellain at Coerthas Central Highlands
        new(1017764, 146, ObjectKind.EventNpc, new(4.2f, 0.9f, -9.7f)), //Alphinaud at Southern Thanalan
        new(1018044, 156, ObjectKind.EventNpc, new(127.7f, -15.4f, -421.1f)), //Cid at Mor Dhona
        new(1020503, 612, ObjectKind.EventNpc, new(-494.2f, 82.1f, -266.9f)), //Raubahn at The Fringes
        new(1020399, 612, ObjectKind.EventNpc, new(-115.8f, 40.8f, -37.8f)), //Pipin at The Fringes
        new(1020040, 614, ObjectKind.EventNpc, new(436.1f, 14.6f, 706.2f)), //Imperial Decurion at Yanxia
        new(1020041, 614, ObjectKind.EventNpc, new(446.7f, 14.6f, 705.0f)), //Imperial Soldier at Yanxia
        new(1020056, 614, ObjectKind.EventNpc, new(-247.9f, 16.9f, 577.4f)), //Yugiri at Yanxia
        new(1020559, 612, ObjectKind.EventNpc, new(-188.9f, 43.2f, -149.6f)), //Conrad at The Fringes
        new(1021705, 621, ObjectKind.EventNpc, new(-649.9f, 50.0f, -2.0f)), //Lyse at The Lochs
        new(1021716, 621, ObjectKind.EventNpc, new(676.0f, 60.0f, 460.3f)), //Lyse at The Lochs
        new(1022738, 621, ObjectKind.EventNpc, new(509.6f, 111.2f, -107.1f)), //Alphinaud at The Lochs
        new(1012366, 401, ObjectKind.EventNpc, new(531.2f, -99.8f, 350.0f)), //Honoroit at The Sea of Clouds



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
