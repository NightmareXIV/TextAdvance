using ECommons.EzIpcManager;
using ECommons.GameHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAdvance.Navmesh;

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
            Requester = null;
            return true;
        }
        return false;
    }

    [EzIPC]
    public bool IsInExternalControl()
    {
        return Requester != null && ExternalConfig != null;
    }

    [EzIPC] public bool IsEnabled() => P.IsEnabled(true);
    [EzIPC] public bool GetEnableQuestAccept() => P.config.GetEnableQuestAccept();
    [EzIPC] public bool GetEnableQuestComplete() => P.config.GetEnableQuestComplete();
    [EzIPC] public bool GetEnableRewardPick() => P.config.GetEnableRewardPick();
    [EzIPC] public bool GetEnableCutsceneEsc() => P.config.GetEnableCutsceneEsc();
    [EzIPC] public bool GetEnableCutsceneSkipConfirm() => P.config.GetEnableCutsceneSkipConfirm();
    [EzIPC] public bool GetEnableRequestHandin() => P.config.GetEnableRequestHandin();
    [EzIPC] public bool GetEnableRequestFill() => P.config.GetEnableRequestFill();
    [EzIPC] public bool GetEnableTalkSkip() => P.config.GetEnableTalkSkip();
    [EzIPC] public bool GetEnableAutoInteract() => P.config.GetEnableAutoInteract();
    [EzIPC] public bool GetEnableAutoSnipe() => P.config.GetEnableAutoSnipe();
    [EzIPC] public bool IsPaused() => P.BlockList.Count != 0;

    [EzIPC]
    public void EnqueueMoveAndInteract(MoveData data)
    {
        S.MoveManager.EnqueueMoveAndInteract(data, 3f);
    }

    [EzIPC]
    public void EnqueueMoveTo2DPoint(MoveData data, float distance)
    {
        S.MoveManager.MoveTo2DPoint(data, distance);
    }

    [EzIPC]
    public void EnqueueMoveTo3DPoint(MoveData data, float distance)
    {
        S.MoveManager.MoveTo3DPoint(data, distance);
    }

    [EzIPC] public void Stop()
    {
        P.EntityOverlay.TaskManager.Abort();
        if (C.Navmesh) P.NavmeshManager.Stop();
    }
    [EzIPC] public bool IsBusy()
    {
        return P.EntityOverlay.TaskManager.IsBusy;
    }
}
