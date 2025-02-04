using TextAdvance.Gui;
using TextAdvance.Navmesh;

namespace TextAdvance.Services;
public static class ServiceManager
{
    public static ConfigGui ConfigGui;
    public static IPCProvider IPCProvider;
    public static MoveManager MoveManager;
    public static IPCTester IPCTester;
    public static TeleporterIPC TeleporterIPC;
    public static EntityOverlay EntityOverlay;
    public static Memory Memory;
}
