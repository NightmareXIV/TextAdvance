using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Services;
public class IPCProvider
{
    public TerritoryConfig Config = null;
    public string Requester = null;

    private IPCProvider()
    {
        EzIPC.Init(this);
    }

    [EzIPC]
    public bool EnableExternalControl(string requester, TerritoryConfig config)
    {
        if(!IsInExternalControl() || Requester == requester)
        {
            Config = config;
            return true;
        }
        return false;
    }

    [EzIPC]
    public bool DisableExternalControl(string requester)
    {
        if(!IsInExternalControl() || Requester == requester)
        {
            Config = null;
            return true;
        }
        return false;
    }

    [EzIPC]
    public bool IsInExternalControl()
    {
        return Requester != null && Config != null;
    }
}
