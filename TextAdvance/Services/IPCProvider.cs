using ECommons.EzIpcManager;
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
        if (!this.IsInExternalControl() || this.Requester == requester)
        {
            this.ExternalConfig = config;
            this.Requester = requester;
            return true;
        }
        return false;
    }

    [EzIPC]
    public bool DisableExternalControl(string requester)
    {
        if (!this.IsInExternalControl() || this.Requester == requester)
        {
            this.ExternalConfig = null;
            this.Requester = null;
            return true;
        }
        return false;
    }

    [EzIPC]
    public bool IsInExternalControl()
    {
        return this.Requester != null && this.ExternalConfig != null;
    }

    [EzIPC] public bool IsEnabled() => P.IsEnabled(true);
    [EzIPC] public bool GetEnableQuestAccept() => C.GetEnableQuestAccept();
    [EzIPC] public bool GetEnableQuestComplete() => C.GetEnableQuestComplete();
    [EzIPC] public bool GetEnableRewardPick() => C.GetEnableRewardPick();
    [EzIPC] public bool GetEnableCutsceneEsc() => C.GetEnableCutsceneEsc();
    [EzIPC] public bool GetEnableCutsceneSkipConfirm() => C.GetEnableCutsceneSkipConfirm();
    [EzIPC] public bool GetEnableRequestHandin() => C.GetEnableRequestHandin();
    [EzIPC] public bool GetEnableRequestFill() => C.GetEnableRequestFill();
    [EzIPC] public bool GetEnableTalkSkip() => C.GetEnableTalkSkip();
    [EzIPC] public bool GetEnableAutoInteract() => C.GetEnableAutoInteract();
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

    [EzIPC]
    public void Stop()
    {
        S.EntityOverlay.TaskManager.Abort();
        if (C.Navmesh) P.NavmeshManager.Stop();
    }
    [EzIPC]
    public bool IsBusy()
    {
        return S.EntityOverlay.TaskManager.IsBusy;
    }
}
