using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Navmesh;
public class NavmeshManager
{
    [EzIPC("Nav.%m")] public readonly Func<bool> IsReady;
    [EzIPC("Nav.%m")] public readonly Func<float> BuildProgress;
    [EzIPC("Nav.%m")] public readonly Func<bool> Reload;
    [EzIPC("Nav.%m")] public readonly Func<bool> Rebuild;

    [EzIPC("SimpleMove.%m")] public readonly Func<Vector3, bool, bool> PathfindAndMoveTo;
    [EzIPC("SimpleMove.%m")] public readonly Func<bool> PathfindInProgress;

    [EzIPC("Path.%m")] public readonly Action Stop;
    [EzIPC("Path.%m")] public readonly Func<bool> IsRunning;

    public NavmeshManager()
    {
        EzIPC.Init(this, "vnavmesh", SafeWrapper.AnyException);
    }
}