using ECommons.Reflection;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance;
public static class Utils
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
}
