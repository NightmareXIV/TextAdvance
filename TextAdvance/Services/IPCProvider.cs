using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Services;
public class IPCProvider
{
    public ExternalTerritoryConfig ExternalConfig = null;
    public string Requester = null;

    private IPCProvider()
    {
        EzIPC.Init(this);
    }

    [EzIPC]
    public bool EnableExternalControl(string requester, ExternalTerritoryConfig config)
    {
        if(!IsInExternalControl() || Requester == requester)
        {
            ExternalConfig = config;
            Requester = requester;
            return true;
        }
        return false;
    }

    [EzIPC]
    public bool DisableExternalControl(string requester)
    {
        if(!IsInExternalControl() || Requester == requester)
        {
            ExternalConfig = null;
            Requester = requester;
            return true;
        }
        return false;
    }

    [EzIPC]
    public bool IsInExternalControl()
    {
        return Requester != null && ExternalConfig != null;
    }
}
