using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBar.Native
{
    enum ABMsg : int
    {
        NEW = 0,
        REMOVE = 1,
        QUERYPOS = 2,
        SETPOS = 3,
        GETSTATE = 4,
        GETTASKBARPOS = 5,
        ACTIVATE = 6,
        GETAUTOHIDEBAR = 7,
        SETAUTOHIDEBAR = 8,
        WINDOWPOSCHANGED = 9,
        SETSTATE = 10
    }

    enum ABNotify : int
    {
        STATECHANGE = 0,
        POSCHANGED,
        FULLSCREENAPP,
        WINDOWARRANGE
    }

    enum ABEdge : int
    {
        LEFT = 0,
        TOP,
        RIGHT,
        BOTTOM,
        NONE
    }

    enum MONITOR_DPI_TYPE : int
    {
        MDT_EFFECTIVE_DPI = 0,
        MDT_ANGULAR_DPI = 1,
        MDT_RAW_DPI = 2,
        MDT_DEFAULT
    }
}
