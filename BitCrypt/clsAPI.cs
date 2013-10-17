using System.Runtime.InteropServices;
using System;

namespace BitCrypt
{
    public static class WinAPI
    {
        private const int SW_SHOWDEFAULT = 10;
        private const int SW_SHOW = 5;
        private const int SW_HIDE = 0;

        [DllImport("user32.dll")]
        private static extern void ShowWindow(IntPtr Handle, int command);
        [DllImport("user32.dll")]
        private static extern void SetForegroundWindow(IntPtr Handle);

        /// <summary>
        /// old handle in case the window is minimized
        /// </summary>
        private static IntPtr oldHandle;

        /// <summary>
        /// Current Handle, only works if shown!!
        /// </summary>
        private static IntPtr Handle
        {
            get
            {
                return System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            }
        }

        /// <summary>
        /// Shows and activates the current window
        /// </summary>
        public static void Show()
        {
            ShowWindow(oldHandle, SW_SHOW);
            SetForegroundWindow(oldHandle);
        }

        /// <summary>
        /// Hides the current window
        /// </summary>
        public static void Hide()
        {
            ShowWindow(oldHandle=Handle, SW_HIDE);
        }
    }
}
