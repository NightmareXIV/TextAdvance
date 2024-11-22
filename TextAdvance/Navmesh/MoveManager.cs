using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using ECommons;
using ECommons.Automation;
using ECommons.ChatMethods;
using ECommons.CircularBuffers;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Interop;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            ChatPrinter.PrintColored(ECommons.ChatMethods.UIColor.WarmSeaBlue, $"[TextAdvance] {message}");
        }
    }

    public void MoveToFlag()
    {
        if (!Player.Available) return;
        if (AgentMap.Instance()->IsFlagMarkerSet == 0)
        {
            DuoLog.Warning($"Flag is not set");
            return;
        }
        if(!P.config.EnableTeleportToFlag && AgentMap.Instance()->FlagMapMarker.TerritoryId != Svc.ClientState.TerritoryType)
        {
            DuoLog.Warning($"Flag is in different zone than current");
            return;
        }
        var m = AgentMap.Instance()->FlagMapMarker;

        // Don't try to teleport if it's not enabled
        if (P.config.EnableTeleportToFlag)
        {
            var n = GetNearestAetheryteTo(m);
            if (n != null && n.HasValue)
            {
                S.TeleporterIPC.Teleport(n.Value.RowId, 0);
                return;
            }
        }

        var pos = P.NavmeshManager.PointOnFloor(new(m.XFloat, 1024, m.YFloat), false, 5);
        var iterations = 0;
        if (pos == null)
        {
            for (var extent = 0; extent < 100; extent += 5)
            {
                for (var i = 0; i < 1000; i+= 5)
                {
                    iterations++;
                    pos ??= P.NavmeshManager.NearestPoint(new(m.XFloat, Player.Object.Position.Y + i, m.YFloat), extent, 5);
                    pos ??= P.NavmeshManager.NearestPoint(new(m.XFloat, Player.Object.Position.Y - i, m.YFloat), extent, 5);
                    if (pos != null) break;
                }
            }
        }
        if(pos == null)
        {
            DuoLog.Error($"Failed to move to flag");
            return;
        }
        EnqueueMoveAndInteract(new(pos.Value, 0, true), 3f);
        Log($"Nav to flag {pos.Value:F1}, {iterations} corrections");
    }

    private Aetheryte? GetNearestAetheryteTo(FlagMapMarker flag)
    {
        // Get the closest Aetheryte to the FlagMapMarker
        var nA = Svc.Data.GetExcelSheet<Aetheryte>()
                         .Where(a => flag.MapId == a.Map.RowId)
                         .Select(a => new KeyValuePair<Aetheryte, MapMarker?>(
                             a, Svc.Data.GetSubrowExcelSheet<MapMarker>().AllRows().FirstOrNull(m => (m.DataType == 3 && m.DataKey.RowId == a.RowId))))
                         .Where(a => a.Value != null && a.Value.HasValue && a.Key.Map.IsValid)
                         .OrderBy(a => Vector2.Distance(
                             ConvertFlagMapMarkerToMapCoordinate(flag, a.Key.Map.Value.SizeFactor),
                             ConvertMapMarkerToMapCoordinate(a.Value.Value.X, a.Value.Value.Y, a.Key.Map.Value.SizeFactor)))
                         .First();

        if (!nA.Key.Map.IsValid || nA.Value == null || !nA.Value.HasValue)
        {
            return null;
        }

        var localPlayer = Svc.ClientState.LocalPlayer;
        if (!localPlayer.IsValid()) 
        { 
            return null; 
        }

        // Compare the flag's position to the player and the nearest Aetheryte and only teleport if the Aetheryte is closer
        // Add a buffer to the Aetheryte's distance to account for teleport and load time
        var fMC = ConvertFlagMapMarkerToMapCoordinate(flag, nA.Key.Map.Value.SizeFactor);
        var pMC = new Vector2(localPlayer.GetMapCoordinates().X, localPlayer.GetMapCoordinates().Y);
        var nAMC = ConvertMapMarkerToMapCoordinate(nA.Value.Value.X, nA.Value.Value.Y, nA.Key.Map.Value.SizeFactor);
        if (Svc.ClientState.TerritoryType == flag.TerritoryId && Vector2.Distance(fMC, pMC) < Vector2.Distance(fMC, nAMC) + 3)
        {
            return null;
        }

        return nA.Key;
    }

    private Vector2 ConvertFlagMapMarkerToMapCoordinate(FlagMapMarker flag, float scale)
    {
        float num = 100f / scale;
        float convertedX = ((flag.XFloat + (1024f * num)) / (1024f * num * 2)) * (41f * num) + 1f;
        float convertedY = ((flag.YFloat + (1024f * num)) / (1024f * num * 2)) * (41f * num) + 1f;

        return new Vector2(convertedX, convertedY);
    }

    private Vector2 ConvertMapMarkerToMapCoordinate(float x, float y, float scale)
    {
        float num = scale / 100f;
        var rawX = (int)((float)(x - 1024.0) / num * 1000f);
        var rawY = (int)((float)(y - 1024.0) / num * 1000f);

        return ConvertRawPositionToMapCoordinate(rawX, rawY, scale);
    }

    private Vector2 ConvertRawPositionToMapCoordinate(float x, float y, float scale)
    {
        float num = scale / 100f;

        return new Vector2((float)((x / 1000f * num + 1024.0) / 2048.0 * 41.0 / num + 1.0), (float)((y / 1000f * num + 1024.0) / 2048.0 * 41.0 / num + 1.0));
    }

    public void MoveTo2DPoint(MoveData data, float distance)
    {
        var pos = P.NavmeshManager.PointOnFloor(new(data.Position.X, 1024, data.Position.Z), false, 5);
        var iterations = 0;
        if (pos == null)
        {
            for (var extent = 0; extent < 100; extent += 5)
            {
                for (var i = 0; i < 1000; i += 5)
                {
                    iterations++;
                    pos ??= P.NavmeshManager.NearestPoint(new(data.Position.X, Player.Object.Position.Y + i, data.Position.Z), extent, 5);
                    pos ??= P.NavmeshManager.NearestPoint(new(data.Position.X, Player.Object.Position.Y - i, data.Position.Z), extent, 5);
                    if (pos != null) break;
                }
            }
        }
        if (pos == null)
        {
            DuoLog.Error($"Failed to move to 2d point");
            return;
        }
        data.Position = pos.Value;
        EnqueueMoveAndInteract(data, distance);
        Log($"Nav to 2d point {pos.Value:F1}, {iterations} corrections, distance={distance:F1}");
    }
    public void MoveTo3DPoint(MoveData data, float distance)
    {
        EnqueueMoveAndInteract(data, distance);
        Log($"Nav to 3d point {data.Position:F1}, distance={distance:F1}");
    }

    public void MoveToQuest()
    {
        if (!Player.Available) return;
        //P.EntityOverlay.AutoFrame = CSFramework.Instance()->FrameCounter + 1;
        if(EzThrottler.Throttle("WarnMTQ", int.MaxValue))
        {
            //ChatPrinter.Red($"[TextAdvance] MoveToQuest function may not work correctly until complete Dalamud update");
        }
        var obj = GetNearestMTQObject();
        if(obj != null)
        {
            EnqueueMoveAndInteract(new(obj.Position, obj.DataId, false), 3f);
            Log($"Precise nav: {obj.Name}/{obj.DataId:X8}");
        }
        else
        {
            Utils.GetEligibleMapMarkerLocationsAsync(Callback);
            void Callback(List<Vector3> markers)
            {
                if (markers.Count > 0)
                {
                    var marker = markers.OrderBy(x => Vector3.Distance(x, Player.Object.Position)).First();
                    EnqueueMoveAndInteract(new(marker, 0, false), 3f);
                    Log($"Non-precise nav: {marker:F1}");
                }
            }
        }
    }

    private IGameObject GetNearestMTQObject(Vector3? reference = null, float? maxDistance = null)
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

    public void EnqueueMoveAndInteract(MoveData data, float distance)
    {
        SpecialAdjust(data);
        P.NavmeshManager.Stop();
        P.EntityOverlay.TaskManager.Abort();
        /*if (Svc.Condition[ConditionFlag.InFlight])
        {
            Svc.Toasts.ShowError("[TextAdvance] Flying pathfinding is not supported");
            return;
        }*/
        if (data.Mount ?? Vector3.Distance(data.Position, Player.Object.Position) > 20f)
        {
            P.EntityOverlay.TaskManager.Enqueue(MountIfCan);
        }
        if(data.Fly != false) P.EntityOverlay.TaskManager.Enqueue(FlyIfCan);
        P.EntityOverlay.TaskManager.Enqueue(() => MoveToPosition(data, distance));
        P.EntityOverlay.TaskManager.Enqueue(() => WaitUntilArrival(data, distance), 10 * 60 * 1000);
        P.EntityOverlay.TaskManager.Enqueue(P.NavmeshManager.Stop);
        if (C.NavmeshAutoInteract && !data.NoInteract)
        {
            P.EntityOverlay.TaskManager.Enqueue(() =>
            {
                var obj = data.GetIGameObject();
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
            var mount = C.Mount;
            if (!PlayerState.Instance()->IsMountUnlocked((uint)mount))
            {
                DuoLog.Warning($"Mount {Utils.GetMountName(mount)} is not unlocking. Falling back to Mount roulette.");
                mount = 0;
            }
            if (!Player.IsAnimationLocked && EzThrottler.Throttle("SummonMount"))
            {
                if (mount == 0)
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
            LastPositionUpdate = Environment.TickCount64;
            LastPosition = Player.Position;
            P.NavmeshManager.PathfindAndMoveTo(pos, Svc.Condition[ConditionFlag.InFlight]);
        }
    }

    internal Vector3 LastPosition = Vector3.Zero;
    internal long LastPositionUpdate = 0;
    internal CircularBuffer<long> Unstucks = new(5);

    public bool? WaitUntilArrival(MoveData data, float distance)
    {
        if (!Player.Available) return null;
        if(!P.NavmeshManager.IsRunning())
        {
            LastPositionUpdate = Environment.TickCount64;
        }
        else
        {
            if(Vector3.Distance(LastPosition, Player.Position) > 0.5f)
            {
                LastPositionUpdate = Environment.TickCount64;
                LastPosition = Player.Position;
            }
        }
        if (data.Mount != false && P.config.Mount != -1 && Vector3.Distance(data.Position, Player.Object.Position) > 20f && !Svc.Condition[ConditionFlag.Mounted] && ActionManager.Instance()->GetActionStatus(ActionType.GeneralAction, 9) == 0)
        {
            EnqueueMoveAndInteract(data, distance);
            return false;
        }
        if (!data.NoInteract)
        {
            if (data.DataID == 0)
            {
                var obj = GetNearestMTQObject();
                if (obj != null)
                {
                    data.Position = obj.Position;
                    data.DataID = obj.DataId;
                    Log($"Correction to MTQ object: {obj.Name}/{obj.DataId:X8}");
                    MoveToPosition(data, distance);
                }
                else
                {
                    if (Vector3.Distance(data.Position, Player.Object.Position) < 30f)
                    {
                        foreach (var x in Svc.Objects.OrderBy(z => Vector3.Distance(data.Position, z.Position)))
                        {
                            if (Vector3.Distance(data.Position, x.Position) < 100f && x.ObjectKind.EqualsAny(ObjectKind.EventNpc | ObjectKind.EventObj) && x.IsTargetable)
                            {
                                data.Position = x.Position;
                                data.DataID = x.DataId;
                                Log($"Correction to non-MTQ object: {x.Name}/{x.DataId:X8}");
                                MoveToPosition(data, distance);
                                break;
                            }
                        }
                    }
                }
            }
        }
        var pos = data.Position;
        if (Environment.TickCount64 - LastPositionUpdate > 500 && EzThrottler.Throttle("RequeueMoveTo", 1000))
        {
            var cnt = Unstucks.Count(x => Environment.TickCount64 - x < 10000);
            if (cnt < 5)
            {
                Log($"Stuck, rebuilding path ({cnt + 1}/5)");
                MoveToPosition(data, distance);
                Unstucks.PushFront(Environment.TickCount64);
            }
            else
            {
                DuoLog.Error($"Stuck, move manually");
                P.NavmeshManager.Stop();
                return null;
            }
        }
        if (Vector3.Distance(Player.Object.Position, pos) > 12f && !Svc.Condition[ConditionFlag.Mounted] && !Svc.Condition[ConditionFlag.InCombat] && !Player.IsAnimationLocked)
        {
            if(ActionManager.Instance()->GetActionStatus(ActionType.Action, 3) == 0 && !Player.Object.StatusList.Any(z => z.StatusId == 50))
            {
                if (EzThrottler.Throttle("CastSprintPeloton", 2000))
                {
                    Chat.Instance.ExecuteCommand($"/action \"{Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow(3).Name.ExtractText()}\"");
                }
            }
            else if (ActionManager.Instance()->GetActionStatus(ActionType.Action, 7557) == 0 && !Player.Object.StatusList.Any(z => z.StatusId.EqualsAny<uint>(1199, 50)))
            {
                if (EzThrottler.Throttle("CastSprintPeloton", 2000))
                {
                    Chat.Instance.ExecuteCommand($"/action \"{Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow(7557).Name.ExtractText()}\"");
                }
            }
        }
        if(data.NoInteract)
        {
            if (Vector2.Distance(Player.Object.Position.ToVector2(), pos.ToVector2()) < distance)
            {
                Log("Stopped by 2D distance");
                return true;
            }
        }
        return Vector3.Distance(Player.Object.Position, pos) < distance;
    }
    public bool? InteractWithDataID(uint dataID)
    {
        if (!Player.Interactable) return false;
        if (dataID == 0) return true;
        if (Svc.Targets.Target != null)
        {
            var t = Svc.Targets.Target;
            if (t.IsTargetable && t.DataId == dataID && Vector3.Distance(Player.Object.Position, t.Position) < 10f && !IsOccupied() && !Player.IsAnimationLocked && Utils.ThrottleAutoInteract())
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

    public void SpecialAdjust(MoveData data)
    {
        if(Player.Territory == 212) //adjust for walking sands
        {
            if(Player.Position.X < 24.5f && data.Position.X > 24.5f)
            {
                Log("Special adjustment: Entrance to the Solar at The Waking Sands");
                //new(2001715, 212, ObjectKind.EventObj, new(23.2f, 2.1f, -0.0f)), //Entrance to the Solar at The Waking Sands
                data.DataID = 2001715;
                data.Position = new(23.2f, 2.1f, -0.0f);
            }
            else if(Player.Position.X > 24.5f && data.Position.X < 24.5f)
            {
                Log("Special adjustment: Exit to the Waking Sands at The Waking Sands");
                //new(2001717, 212, ObjectKind.EventObj, new(25.5f, 2.1f, -0.0f)), //Exit to the Waking Sands at The Waking Sands
                data.DataID = 2001717;
                data.Position = new(25.5f, 2.1f, -0.0f);
            }
        }
        else if(Player.Territory == 351) //rising stones
        {
            if (Player.Position.Z < -28.0f && data.Position.Z > -28.0f)
            {
                Log("Special adjustment: Exit to the Rising Stones at The Rising Stones");
                //new(2002880, 351, ObjectKind.EventObj, new(-0.0f, -1.0f, -29.3f)), //Exit to the Rising Stones at The Rising Stones
                data.DataID = 2002880;
                data.Position = new(-0.0f, -1.0f, -29.3f);
            }
            else if (Player.Position.Z > -28.0f && data.Position.Z < -28.0f)
            {
                Log("Special adjustment: Entrance to the Solar at The Rising Stones");
                //new(2002878, 351, ObjectKind.EventObj, new(-0.0f, -1.0f, -26.8f)), //Entrance to the Solar at The Rising Stones
                data.DataID = 2002878;
                data.Position = new(-0.0f, -1.0f, -26.8f);
            }
    }
}
}
