﻿using Dalamud.Configuration;
using Dalamud.Plugin.Ipc.Exceptions;
using ECommons.Configuration;
using ECommons.Interop;

namespace TextAdvance;

[Serializable]
public class Config : IEzConfig
{
    public int Version { get; set; } = 2;
    public HashSet<string> AutoEnableNames = [];
    public Button TempEnableButton = Button.NONE;
    public Button TempDisableButton = Button.NONE;
    public TerritoryConfig MainConfig = new();
    public Dictionary<uint, TerritoryConfig> TerritoryConditions = [];
    public bool GlobalOverridesLocal = false;
    public bool EnableOverlay = true;
    public Vector2 OverlayOffset = new(0, -10);
    public bool NotifyDisableManualState = false;
    public bool NotifyDisableOnLogin = false;
    public bool QTIEnabled = false;
    public List<PickRewardMethod> PickRewardOrder = [PickRewardMethod.Gear_coffer, PickRewardMethod.High_quality_gear, PickRewardMethod.Highest_vendor_value, PickRewardMethod.Gil_sacks, PickRewardMethod.Equipable_item_for_current_job];
    public bool PickRewardSilent = false;
    public bool ENpcFinder = false;
    public bool EObjFinder = true;
    public LimitedKeys FinderKey = LimitedKeys.LeftControlKey;
    public bool DontAutoDisable = false;
    public bool Navmesh = false;
    public bool NavmeshAutoInteract = false;
    public int Mount = 0;
    public bool NavStatusChat = false;
    public bool DisplayFunding = true;
    public bool EnableFlight = false;

    public bool GetEnableQuestAccept()
    {
        if (S.IPCProvider.IsInExternalControl()) return S.IPCProvider.ExternalConfig.Merge(MainConfig).EnableQuestAccept;
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableQuestAccept;
        }
        return MainConfig.EnableQuestAccept;
    }
    public bool GetEnableQuestComplete()
    {
        if (S.IPCProvider.IsInExternalControl()) return S.IPCProvider.ExternalConfig.Merge(MainConfig).EnableQuestComplete;
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableQuestComplete;
        }
        return MainConfig.EnableQuestComplete;
    }
    public bool GetEnableRequestHandin()
    {
        if (S.IPCProvider.IsInExternalControl()) return S.IPCProvider.ExternalConfig.Merge(MainConfig).EnableRequestHandin;
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableRequestHandin;
        }
        return MainConfig.EnableRequestHandin;
    }
    public bool GetEnableCutsceneEsc()
    {
        if (S.IPCProvider.IsInExternalControl()) return S.IPCProvider.ExternalConfig.Merge(MainConfig).EnableCutsceneEsc;
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableCutsceneEsc;
        }
        return MainConfig.EnableCutsceneEsc;
    }
    public bool GetEnableCutsceneSkipConfirm()
    {
        if (S.IPCProvider.IsInExternalControl()) return S.IPCProvider.ExternalConfig.Merge(MainConfig).EnableCutsceneSkipConfirm;
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableCutsceneSkipConfirm;
        }
        return MainConfig.EnableCutsceneSkipConfirm;
    }
    public bool GetEnableTalkSkip()
    {
        if (S.IPCProvider.IsInExternalControl()) return S.IPCProvider.ExternalConfig.Merge(MainConfig).EnableTalkSkip;
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableTalkSkip;
        }
        return MainConfig.EnableTalkSkip;
    }
    public bool GetEnableRequestFill()
    {
        if (S.IPCProvider.IsInExternalControl()) return S.IPCProvider.ExternalConfig.Merge(MainConfig).EnableRequestFill;
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableRequestFill;
        }
        return MainConfig.EnableRequestFill;
    }


    public Vector4 GetQTAQuestColor()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.QTIQuestColor;
        }
        return MainConfig.QTIQuestColor;
    }
    public bool GetQTAQuestTether()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.QTIQuestTether;
        }
        return MainConfig.QTIQuestTether;
    }
    public float GetQTAQuestThickness()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.QTIQuestThickness;
        }
        return MainConfig.QTIQuestThickness;
    }
    public bool GetQTAQuestHideWhenTargeted()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.QTIQuestHideWhenTargeted;
        }
        return MainConfig.QTIQuestHideWhenTargeted;
    }
    public bool GetQTAQuestEnabled()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.QTIQuestEnabled;
        }
        return MainConfig.QTIQuestEnabled;
    }
    public bool GetQTAlwaysEnabled()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.QTIAlwaysEnabled;
        }
        return MainConfig.QTIAlwaysEnabled;
    }
    public bool GetEnableRewardPick()
    {
        if (S.IPCProvider.IsInExternalControl()) return S.IPCProvider.ExternalConfig.Merge(MainConfig).EnableRewardPick;
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableRewardPick;
        }
        return MainConfig.EnableRewardPick;
    }
    public bool GetEnableAutoInteract()
    {
        if (S.IPCProvider.IsInExternalControl()) return S.IPCProvider.ExternalConfig.Merge(MainConfig).EnableAutoInteract;
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableAutoInteract;
        }
        return MainConfig.EnableAutoInteract;
    }
    public bool GetEnableAutoSnipe()
    {
        if (S.IPCProvider.IsInExternalControl()) return S.IPCProvider.ExternalConfig.Merge(MainConfig).EnableAutoSnipe;
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableAutoSnipe;
        }
        return MainConfig.EnableAutoSnipe;
    }
}

public enum Button
{
    NONE, ALT, SHIFT, CTRL
}

[Serializable]
public class TerritoryConfig
{
    public bool EnableQuestAccept = true;
    public bool EnableQuestComplete = true;
    public bool EnableRewardPick = true;
    public bool EnableRequestHandin = true;
    public bool EnableCutsceneEsc = true;
    public bool EnableCutsceneSkipConfirm = true;
    public bool EnableTalkSkip = true;
    public bool EnableRequestFill = true;
    public bool EnableAutoInteract = false;
    public bool EnableAutoSnipe = false;

    public bool QTIQuestEnabled = true;
    public Vector4 QTIQuestColor = EColor.PurpleBright;
    public bool QTIQuestTether = true;
    public float QTIQuestThickness = 2f;
    public bool QTIQuestHideWhenTargeted = false;
    public bool QTIAlwaysEnabled = false;

    public TerritoryConfig Clone()
    {
        var ret = new TerritoryConfig();
        foreach (var x in ret.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            x.SetValue(ret, x.GetValue(this));
        }
        return ret;
    }

    public bool IsEnabled()
    {
        return EnableQuestAccept || EnableQuestComplete || EnableRequestHandin || EnableCutsceneEsc || EnableCutsceneSkipConfirm || EnableTalkSkip || EnableRequestFill || EnableRewardPick || EnableAutoInteract || EnableAutoSnipe;
    }
}

[Serializable]
public class ExternalTerritoryConfig
{
    public bool? EnableQuestAccept = null;
    public bool? EnableQuestComplete = null;
    public bool? EnableRewardPick = null;
    public bool? EnableRequestHandin = null;
    public bool? EnableCutsceneEsc = null;
    public bool? EnableCutsceneSkipConfirm = null;
    public bool? EnableTalkSkip = null;
    public bool? EnableRequestFill = null;
    public bool? EnableAutoInteract = null;
    public bool? EnableAutoSnipe = null;

    public TerritoryConfig Merge(TerritoryConfig other)
    {
        //([a-zA-Z]+) = null;
        //\1 ?? other.\1;
        var ret = other.Clone();
        ret.EnableQuestAccept = this.EnableQuestAccept ?? other.EnableQuestAccept;
        ret.EnableQuestComplete = this.EnableQuestComplete ?? other.EnableQuestComplete;
        ret.EnableRewardPick = this.EnableRewardPick ?? other.EnableRewardPick;
        ret.EnableRequestHandin = this.EnableRequestHandin ?? other.EnableRequestHandin;
        ret.EnableCutsceneEsc = this.EnableCutsceneEsc ?? other.EnableCutsceneEsc;
        ret.EnableCutsceneSkipConfirm = this.EnableCutsceneSkipConfirm ?? other.EnableCutsceneSkipConfirm;
        ret.EnableTalkSkip = this.EnableTalkSkip ?? other.EnableTalkSkip;
        ret.EnableRequestFill = this.EnableRequestFill ?? other.EnableRequestFill;
        ret.EnableAutoInteract = this.EnableAutoInteract ?? other.EnableAutoInteract;
        ret.EnableAutoSnipe = this.EnableAutoSnipe ?? other.EnableAutoSnipe;
        return ret;
    }
}