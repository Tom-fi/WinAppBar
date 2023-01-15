using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using static AppBar.Native.DisplayHelper;

namespace AppBar.Native
{
    public static class DisplayHelper
    {

        // Delegate for the EnumDisplayMonitors function
        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        public static List<Display> GetDisplays()
        {
            List<Display> displays = new List<Display>();

            MonitorEnumProc _callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                MonitorInfo monInfo = new MonitorInfo();
                monInfo.cbSize = Marshal.SizeOf(monInfo);

                GetMonitorInfo(hMonitor, ref monInfo);
                GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
                var display = new Display(monInfo);
                display.Dpi = dpiX; //dpiX and dpiY are same
                displays.Add(display);
                return true;
            };

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, _callback, IntPtr.Zero);

            return displays;
        }

        private const string User32 = "user32.dll";
        private const string ShCore = "shcore.dll";
        [DllImport(User32)]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetMonitorInfo(IntPtr hmon, ref MonitorInfo monInfo);

        [DllImport(ShCore)]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);
    }
}
