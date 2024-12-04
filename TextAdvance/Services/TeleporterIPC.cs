using ECommons.EzIpcManager;

namespace TextAdvance.Services;
public class TeleporterIPC
{
    [EzIPC(applyPrefix: false)] public Func<uint, byte, bool> Teleport;

    private TeleporterIPC()
    {
        EzIPC.Init(this, "Teleport", SafeWrapper.AnyException);
    }
}
