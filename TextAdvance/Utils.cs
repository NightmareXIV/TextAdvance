using ECommons.Reflection;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TextAdvance;
public unsafe static class Utils
{
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
