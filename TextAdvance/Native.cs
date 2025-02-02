using System.Diagnostics;

namespace TextAdvance;

internal class Native
{
    /// <summary>Returns true if the current application has focus, false otherwise</summary>
    public static bool ApplicationIsActivated()
    {
        var activatedHandle = GetForegroundWindow();
        if (activatedHandle == IntPtr.Zero)
        {
            return false;       // No window is currently activated
        }

        var procId = Process.GetCurrentProcess().Id;
        int activeProcId;
        GetWindowThreadProcessId(activatedHandle, out activeProcId);

        return activeProcId == procId;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

    public class Keypress
    {
        public const int LControlKey = 162;
        public const int Space = 32;
        public const int Escape = 0x1B;
        private const uint WM_KEYUP = 0x101;
        private const uint WM_KEYDOWN = 0x100;


        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public static void SendKeycode(IntPtr hwnd, int keycode)
        {
            SendMessage(hwnd, WM_KEYDOWN, (IntPtr)keycode, (IntPtr)0);
            SendMessage(hwnd, WM_KEYUP, (IntPtr)keycode, (IntPtr)0);
        }
    }

    public static bool TryFindGameWindow(out IntPtr hwnd)
    {
        hwnd = IntPtr.Zero;
        while (true)
        {
            hwnd = FindWindowEx(IntPtr.Zero, hwnd, "FFXIVGAME", null);
            if (hwnd == IntPtr.Zero) break;
            GetWindowThreadProcessId(hwnd, out var pid);
            if (pid == Process.GetCurrentProcess().Id) break;
        }
        return hwnd != IntPtr.Zero;
    }
}
