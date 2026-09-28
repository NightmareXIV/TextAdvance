using ECommons.EzHookManager;
using FFXIVClientStructs.FFXIV.Client.Sound;
using InteropGenerator.Runtime;

namespace TextAdvance.Services;

public unsafe class VoiceTracker : IDisposable
{
    private EzHook<SoundManager.Delegates.PlayCutsceneVoSound>? playCutsceneVoSoundHook;
    public long LastVoicePlayTime { get; private set; } = 0;
    public string LastVoicePath { get; private set; } = string.Empty;

    public VoiceTracker()
    {
        try
        {
            var addr = SoundManager.Addresses.PlayCutsceneVoSound.Value;
            if (addr != nint.Zero)
            {
                this.playCutsceneVoSoundHook = new EzHook<SoundManager.Delegates.PlayCutsceneVoSound>(addr, this.PlayCutsceneVoSoundDetour);
                PluginLog.Debug($"PlayCutsceneVoSound hooked at 0x{addr:X}");
            }
            else
            {
                PluginLog.Warning("PlayCutsceneVoSound address was zero, voice hook not installed");
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Failed to hook PlayCutsceneVoSound: {ex}");
        }
    }

    private SoundData* PlayCutsceneVoSoundDetour(SoundManager* thisPtr, CStringPointer path)
    {
        try
        {
            this.LastVoicePlayTime = Environment.TickCount64;
            this.LastVoicePath = path.HasValue ? path.ToString() : string.Empty;
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Error in voice detour: {ex}");
        }

        return this.playCutsceneVoSoundHook!.Original(thisPtr, path);
    }

    public void Reset()
    {
        this.LastVoicePlayTime = 0;
        this.LastVoicePath = string.Empty;
    }

    public void Dispose()
    {
        this.playCutsceneVoSoundHook?.Disable();
        this.playCutsceneVoSoundHook = null;
    }
}
