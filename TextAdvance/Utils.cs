using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameFunctions;
using ECommons.Reflection;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static TextAdvance.SplatoonHandler;

namespace TextAdvance;
public unsafe static class Utils
{
    public static List<Vector3> GetEligibleMapMarkerLocations()
    {
        var ret = new List<Vector3>();
        var markers = AgentHUD.Instance()->MapMarkers.Span;
        for (int i = 0; i < markers.Length; i++)
        {
            var marker = markers[i];
            if (marker.TerritoryTypeId != Svc.ClientState.TerritoryType) continue;
            var id = marker.IconId;
            if(SplatoonHandler.Markers.Map.MSQ.Contains(id) || SplatoonHandler.Markers.Map.ImportantSideProgress.Contains(id))
            {
                ret.Add(new(marker.X, marker.Y, marker.Z));
            }
        }
        return ret;
    }

    public static bool IsMTQ(this GameObject x)
    {
        var id = x.Struct()->NamePlateIconId;
        if (Markers.MSQ.Contains(id) || Markers.ImportantSideProgress.Contains(id) || Markers.SideProgress.Contains(id))
        {
            return true;
        }
        else if (x.ObjectKind == ObjectKind.EventObj && x.IsTargetable && (Markers.EventObjWhitelist.Contains(x.DataId) || Markers.EventObjNameWhitelist.ContainsIgnoreCase(x.Name.ToString())))
        {
            return true;
        }
        return false;
    }

    public static string GetGeneralActionName(int id)
    {
        return Svc.Data.GetExcelSheet<GeneralAction>().GetRow((uint)id).Name;
    }

    public static string GetMountName(int id)
    {
        return Svc.Data.GetExcelSheet<Mount>().GetRow((uint)id).Singular.ExtractText();
    }

    public static bool CanFly() => C.EnableFlight && P.Memory.IsFlightProhibited(P.Memory.FlightAddr) == 0;

    public static bool ThrottleAutoInteract() => EzThrottler.Throttle("AutoInteract");
    public static bool ReThrottleAutoInteract() => EzThrottler.Throttle("AutoInteract", rethrottle:true);
    public static bool CheckThrottleAutoInteract() => EzThrottler.Check("AutoInteract");

    public static List<(int, int)> GetQuestArray()
    {   
        var ret = new List<(int, int)>();
        var manager = QuestManager.Instance();
        for (int i = 0; i < manager->NormalQuestsSpan.Length; i++)
        {
            var q = manager->NormalQuestsSpan[i];
            ret.Add((q.QuestId, q.Sequence));
        }
        return ret;
    }
}
