using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Services;
public class IPCTester
{
    [EzIPC] public Func<string, ExternalTerritoryConfig, bool> EnableExternalControl;
    [EzIPC] public Func<string, bool> DisableExternalControl;
    [EzIPC] public Func<bool> IsInExternalControl;

    private IPCTester()
    {
        EzIPC.Init(this, "TextAdvance", SafeWrapper.AnyException);
    }
}
