using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.UIInput;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Reflection;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Component.SteamApi.Callbacks;
using System.Collections.Frozen;
using System.Reflection;
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
        new(1020539, 622, ObjectKind.EventNpc, new(498.3f, 40.8f, -508.2f)), //Cirina at The Azim Steppe
        new(1024066, 737, ObjectKind.EventNpc, new(245.1f, 122.0f, -349.1f)), //Arenvald at Royal Palace
        new(1024822, 613, ObjectKind.EventNpc, new(885.1f, 1.2f, 861.4f)), //Alphinaud at The Ruby Sea
        new(1024986, 614, ObjectKind.EventNpc, new(-348.9f, 1.2f, -344.1f)), //Asahi at Yanxia
        new(2009591, 622, ObjectKind.EventObj, new(-210.3f, 2.6f, 519.3f)), //Destination at The Azim Steppe
        new(1026845, 829, ObjectKind.EventNpc, new(-476.7f, 107.6f, 103.7f)), //Raubahn at Eorzean Alliance Headquarters
        new(1028782, 620, ObjectKind.EventNpc, new(50.5f, 118.3f, -760.2f)), //Ranaa Mihgo at The Peaks
        new(1028955, 813, ObjectKind.EventNpc, new(-755.7f, 200.2f, -293.9f)), //Crystarium Scout at Lakeland
        new(1004884, 212, ObjectKind.EventNpc, new(38.9f, 1.2f, 3.5f)), //Serpent Officer at The Waking Sands
        new(1004883, 212, ObjectKind.EventNpc, new(37.9f, 1.2f, 4.1f)), //Flame Officer at The Waking Sands
        new(1004885, 212, ObjectKind.EventNpc, new(36.5f, 1.2f, 4.7f)), //Storm Officer at The Waking Sands
        new(1004887, 140, ObjectKind.EventNpc, new(160.8f, 54.9f, -45.6f)), //Airship Crewman at Western Thanalan
        new(1000563, 152, ObjectKind.EventNpc, new(-3.8f, -5.9f, 217.3f)), //Ameexia at East Shroud
        new(1000587, 152, ObjectKind.EventNpc, new(-7.2f, -8.4f, 268.2f)), //Dellexia at East Shroud
        new(2001592, 145, ObjectKind.EventObj, new(-356.7f, -10.4f, -242.5f)), //Destination at Eastern Thanalan
        new(1002998, 152, ObjectKind.EventNpc, new(-283.9f, 12.2f, -39.4f)), //Claxio at East Shroud
        new(1006203, 153, ObjectKind.EventNpc, new(32.7f, 10.7f, -13.0f)), //Laurentius at South Shroud
        new(1004886, 152, ObjectKind.EventNpc, new(30.3f, 8.4f, 475.3f)), //Airship Crewman at East Shroud
        new(2000709, 153, ObjectKind.EventObj, new(-90.7f, 0.0f, 61.1f)), //Destination at South Shroud
        new(1000576, 152, ObjectKind.EventNpc, new(21.5f, -4.6f, 221.8f)), //Knolexia at East Shroud
        new(1030382, 817, ObjectKind.EventNpc, new(-113.1f, -18.5f, 324.2f)), //Placid Elder at The Rak'tika Greatwood
        new(1027750, 817, ObjectKind.EventNpc, new(-24.7f, -25.3f, 305.6f)), //Asgeir at The Rak'tika Greatwood
        new(1027463, 817, ObjectKind.EventNpc, new(-130.3f, -18.5f, 246.6f)), //Vondia at The Rak'tika Greatwood
        new(1029243, 817, ObjectKind.EventNpc, new(121.4f, -8.8f, -890.6f)), //Y'shtola at The Rak'tika Greatwood
        new(1006230, 146, ObjectKind.EventNpc, new(34.9f, 3.4f, -343.7f)), //Wilred at Southern Thanalan
        new(2002329, 156, ObjectKind.EventObj, new(-169.8f, 14.1f, -609.5f)), //Destination at Mor Dhona
        new(1006561, 156, ObjectKind.EventNpc, new(-59.3f, 3.3f, -637.8f)), //Magitek Reaper at Mor Dhona
        new(1046681, 1185, ObjectKind.EventNpc, new(-194.8f, 120.8f, -359.2f)), //Sunperch Guard at Tuliyollal
        new(1046678, 1188, ObjectKind.EventNpc, new(605.6f, 119.5f, 185.0f)), //Kind-eyed Xbr'aal at Kozama'uka
        new(1046679, 1188, ObjectKind.EventNpc, new(590.4f, 119.5f, 144.1f)), //Enthusiastic Moblin at Kozama'uka
        new(1046680, 1188, ObjectKind.EventNpc, new(545.4f, 116.7f, 152.9f)), //Cheery Pelu at Kozama'uka
        new(1047570, 1189, ObjectKind.EventNpc, new(-526.9f, 28.8f, -426.0f)), //Wuk Lamat at Yak T'el
        new(1047682, 1189, ObjectKind.EventNpc, new(353.6f, -114.0f, 597.0f)), //Wuk Lamat at Yak T'el
        new(1046974, 1190, ObjectKind.EventNpc, new(-386.8f, 18.2f, -136.7f)), //Wihuwte at Shaaloani




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

    static float GetMinDistance(IGameObject obj)
    {
        var ret = -999f;
        if (obj.ObjectKind == ObjectKind.Aetheryte) ret = 8f;
        else if (obj.ObjectKind == ObjectKind.EventNpc) ret = 6f + obj.HitboxRadius;
        else if (obj.ObjectKind == ObjectKind.EventObj) ret = 3f;
        if(ret > P.config.AutoInteractMaxRadius)
        {
            ret = P.config.AutoInteractMaxRadius;
        }
        return ret;
    }

    static void Interact(IGameObject obj)
    {
        if (WasInteracted(obj)) return;
        if (Svc.Targets.Target.AddressEquals(obj))
        {
            if (obj.IsTargetable && Vector3.Distance(Player.Object.Position, obj.Position) < GetMinDistance(obj) && !Player.IsAnimationLocked && Utils.ThrottleAutoInteract())
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

    static void RecordInteractionWith(IGameObject obj)
    {
        InteractedObjects.RemoveWhere(x => x.DataID == obj.DataId);
        InteractedObjects.Add(new(obj.DataId));
    }

    public static bool WasInteracted(IGameObject obj)
    {
        if(obj == null) return false;
        return InteractedObjects.Contains(new(obj.DataId));
    }
}
