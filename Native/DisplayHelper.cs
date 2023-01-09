using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace AppBar.Native
{
    public static class DisplayHelper
    {

        // Delegate for the EnumDisplayMonitors function
        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MonitorInfoEx
        {
            public int cbSize;
            public Rect rcMonitor;
            public Rect rcWork;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice;
        }

        public static List<MonitorInfoEx> GetDisplays()
        {
            List<MonitorInfoEx> displays = new List<MonitorInfoEx>();

            MonitorEnumProc _callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData) =>
            {
                MonitorInfoEx monInfo = new MonitorInfoEx();
                monInfo.cbSize = Marshal.SizeOf(monInfo);

                GetMonitorInfo(hMonitor, ref monInfo);

                displays.Add(monInfo);
                return true;
            };

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, _callback, IntPtr.Zero);

            return displays;
        }

        private const string User32 = "user32.dll";
        [DllImport(User32)]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);
        [DllImport(User32)]
        private static extern bool GetMonitorInfo(IntPtr hmon, ref MonitorInfoEx monInfo);
    }
}
