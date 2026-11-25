using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace TS_SE_Tool
{
    internal sealed class SingleInstance : IDisposable
    {
        private readonly Mutex mutex;
        private bool disposed;
        internal bool IsPrimary { get; }
        internal static readonly int ActivateMessage = (int)RegisterWindowMessage("TS-ETS2.ActivateMainWindow");
        internal SingleInstance(string name = @"Local\Myzoxxfr.TS-ETS2")
        {
            mutex = new Mutex(true, name, out bool first);
            IsPrimary = first;
        }
        internal static void RequestActivation()
        {
            AllowSetForegroundWindow(uint.MaxValue);
            PostMessage(new IntPtr(0xFFFF), ActivateMessage, IntPtr.Zero, IntPtr.Zero);
        }
        internal static void Activate(IntPtr window)
        {
            if (IsIconic(window)) ShowWindow(window, 9);
            IntPtr popup = GetLastActivePopup(window);
            SetForegroundWindow(popup != IntPtr.Zero && IsWindowVisible(popup) ? popup : window);
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (IsPrimary) mutex.ReleaseMutex();
            mutex.Dispose();
        }
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern uint RegisterWindowMessage(string text);
        [DllImport("user32.dll")] private static extern bool AllowSetForegroundWindow(uint process);
        [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr window);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr window, int command);
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr window);
        [DllImport("user32.dll")] private static extern IntPtr GetLastActivePopup(IntPtr window);
        [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr window);
    }
}
