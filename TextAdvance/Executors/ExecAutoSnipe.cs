namespace TextAdvance.Executors;
public static class ExecAutoSnipe
{
    internal static bool IsEnabled = false;
    public static void Init() => P.Memory.SnipeHook.Enable();
    public static void Shutdown() => P.Memory.SnipeHook.Disable();
}
