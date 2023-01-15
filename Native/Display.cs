using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AppBar.Native
{
    public class Display
    {
        public RECT MonitorArea;
        public RECT WorkArea;
        public bool isPrimary;
        public uint Dpi;
        static uint DpiConstant = 96u; // If you set scale to 125% then DpiX/DpiY will be 120

        public Display(MonitorInfo monitorInfo)
        {
            MonitorArea = monitorInfo.rcMonitor;
            WorkArea = monitorInfo.rcWork;
            isPrimary = monitorInfo.isPrimary;
        }
    }
}
