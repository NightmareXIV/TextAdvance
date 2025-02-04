using ECommons.EzIpcManager;

namespace TextAdvance.Services;
/// <summary>
/// If you are using ECommons, you can copy this class and you will have ready to use IPC. Otherwise, IPC method names are generated as following: "TextAdvance.MethodName"
/// </summary>
public class IPCTester
{
    /// <summary>
    /// Enables external control of TextAdvance. 
    /// First argument = your plugin's name. 
    /// Second argument is options. Copy ExternalTerritoryConfig to your plugin. Configure it as you wish: set "null" values to features that you want to keep as configured by user. Set "true" or "false" to forcefully enable or disable feature. 
    /// Returns whether external control successfully enabled or not. When already in external control, it will succeed if called again if plugin name matches with one that already has control and new settings will take effect, otherwise it will fail.
    /// External control completely disables territory-specific settings.
    /// </summary>
    [EzIPC] public Func<string, ExternalTerritoryConfig, bool> EnableExternalControl;
    /// <summary>
    /// Disables external control. Will fail if external control is obtained from other plugin.
    /// </summary>
    [EzIPC] public Func<string, bool> DisableExternalControl;
    /// <summary>
    /// Indicates whether external control is enabled.
    /// </summary>
    [EzIPC] public Func<bool> IsInExternalControl;

    /// <summary>
    /// Indicates whether user has plugin enabled. Respects territory configuration. If in external control, will return true.
    /// </summary>
    [EzIPC] public Func<bool> IsEnabled;
    /// <summary>
    /// Indicates whether plugin is paused by other plugin.
    /// </summary>
    [EzIPC] public Func<bool> IsPaused;

    /// <summary>
    /// All the functions below return currently configured plugin state with respect for territory config and external control. 
    /// However, it does not includes IsEnabled or IsPaused check. A complete check whether TextAdvance is currently ready to process appropriate event will look like: <br></br>
    /// IsEnabled() &amp;&amp; !IsPaused() &amp;&amp; GetEnableQuestAccept()
    /// </summary>
    [EzIPC] public Func<bool> GetEnableQuestAccept;
    [EzIPC] public Func<bool> GetEnableQuestComplete;
    [EzIPC] public Func<bool> GetEnableRewardPick;
    [EzIPC] public Func<bool> GetEnableCutsceneEsc;
    [EzIPC] public Func<bool> GetEnableCutsceneSkipConfirm;
    [EzIPC] public Func<bool> GetEnableRequestHandin;
    [EzIPC] public Func<bool> GetEnableRequestFill;
    [EzIPC] public Func<bool> GetEnableTalkSkip;
    [EzIPC] public Func<bool> GetEnableAutoInteract;

    private IPCTester()
    {
        EzIPC.Init(this, "TextAdvance", SafeWrapper.AnyException);
    }
}
