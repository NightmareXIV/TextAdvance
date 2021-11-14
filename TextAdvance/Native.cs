using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    class Native
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

        public class Keypress
        {
            public const int LControlKey = 162;
            public const int Space = 32;
            public const int Escape = 0x1B;

            const uint WM_KEYUP = 0x101;
            const uint WM_KEYDOWN = 0x100;


            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            public static void SendKeycode(IntPtr hwnd, int keycode)
            {
                SendMessage(hwnd, WM_KEYDOWN, (IntPtr)keycode, (IntPtr)0);
                SendMessage(hwnd, WM_KEYUP, (IntPtr)keycode, (IntPtr)0);
            }
        }
    }
}
