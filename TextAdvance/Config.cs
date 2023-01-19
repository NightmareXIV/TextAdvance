using Dalamud.Configuration;

namespace TextAdvance;

[Serializable]
public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 2;
    public HashSet<string> AutoEnableNames = new();
    public Button TempEnableButton = Button.NONE;
    public Button TempDisableButton = Button.NONE;
    public TerritoryConfig MainConfig = new();
    public Dictionary<uint, TerritoryConfig> TerritoryConditions = new();
    public bool GlobalOverridesLocal = false;
    public bool EnableOverlay = true;
    public Vector2 OverlayOffset = new(0, -10);
    public bool NotifyDisableManualState = false;
    public bool NotifyDisableOnLogin = false;

    public bool GetEnableQuestAccept()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableQuestAccept;
        }
        return MainConfig.EnableQuestAccept;
    }
    public bool GetEnableQuestComplete()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableQuestComplete;
        }
        return MainConfig.EnableQuestComplete;
    }
    public bool GetEnableRequestHandin()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableRequestHandin;
        }
        return MainConfig.EnableRequestHandin;
    }
    public bool GetEnableCutsceneEsc()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableCutsceneEsc;
        }
        return MainConfig.EnableCutsceneEsc;
    }
    public bool GetEnableCutsceneSkipConfirm()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableCutsceneSkipConfirm;
        }
        return MainConfig.EnableCutsceneSkipConfirm;
    }
    public bool GetEnableTalkSkip()
    {
        if (!(GlobalOverridesLocal && P.Enabled) && TerritoryConditions.TryGetValue(Svc.ClientState.TerritoryType, out var val))
        {
            return val.EnableTalkSkip;
        }
        return MainConfig.EnableTalkSkip;
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
    public bool EnableRequestHandin = true;
    public bool EnableCutsceneEsc = true;
    public bool EnableCutsceneSkipConfirm = true;
    public bool EnableTalkSkip = true;

    public bool IsEnabled()
    {
        return EnableQuestAccept || EnableQuestComplete || EnableRequestHandin || EnableCutsceneEsc || EnableCutsceneSkipConfirm || EnableTalkSkip;
    }
}
