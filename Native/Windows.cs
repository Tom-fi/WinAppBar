using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace AppBar.Native
{
    public partial class AppBarWindow : Window
    {
        //A bit of copy paste from here: https://www.codeproject.com/Articles/6741/AppBar-using-C
        //Had this bookmarked for quite a while (since about 2013?)
        
        private int uCallBack;

        private void RegisterBar()
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = this.Handle;
            if (!fBarRegistered)
            {
                uCallBack = RegisterWindowMessage("AppBarMessage");
                abd.uCallbackMessage = uCallBack;

                uint ret = SHAppBarMessage((int)ABMsg.NEW, ref abd);
                fBarRegistered = true;

                ABSetPos();
            }
            else
            {
                SHAppBarMessage((int)ABMsg.REMOVE, ref abd);
                fBarRegistered = false;
            }
        }

        private void ABSetPos()
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = this.Handle;
            abd.uEdge = (int)ABEdge.TOP;

            if (abd.uEdge == (int)ABEdge.LEFT || abd.uEdge == (int)ABEdge.RIGHT)
            {
                abd.rc.top = 0;
                abd.rc.bottom = SystemInformation.PrimaryMonitorSize.Height;
                if (abd.uEdge == (int)ABEdge.LEFT)
                {
                    abd.rc.left = 0;
                    abd.rc.right = Size.Width;
                }
                else
                {
                    abd.rc.right = SystemInformation.PrimaryMonitorSize.Width;
                    abd.rc.left = abd.rc.right - Size.Width;
                }

            }
            else
            {
                abd.rc.left = 0;
                abd.rc.right = SystemInformation.PrimaryMonitorSize.Width;
                if (abd.uEdge == (int)ABEdge.TOP)
                {
                    abd.rc.top = 0;
                    abd.rc.bottom = Size.Height;
                }
                else
                {
                    abd.rc.bottom = SystemInformation.PrimaryMonitorSize.Height;
                    abd.rc.top = abd.rc.bottom - Size.Height;
                }
            }

            // Query the system for an approved size and position. 
            SHAppBarMessage((int)ABMsg.QUERYPOS, ref abd);

            // Adjust the rectangle, depending on the edge to which the 
            // appbar is anchored. 
            switch (abd.uEdge)
            {
                case (int)ABEdge.LEFT:
                    abd.rc.right = abd.rc.left + Size.Width;
                    break;
                case (int)ABEdge.RIGHT:
                    abd.rc.left = abd.rc.right - Size.Width;
                    break;
                case (int)ABEdge.TOP:
                    abd.rc.bottom = abd.rc.top + Size.Height;
                    break;
                case (int)ABEdge.BOTTOM:
                    abd.rc.top = abd.rc.bottom - Size.Height;
                    break;
            }

            // Pass the final bounding rectangle to the system. 
            SHAppBarMessage((int)ABMsg.SETPOS, ref abd);

            // Move and size the appbar so that it conforms to the 
            // bounding rectangle passed to the system. 
            MoveWindow(abd.hWnd, abd.rc.left, abd.rc.top,
                abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, true);
        }

        ///////// Refactor

        private bool isBarRegistered = false;

        public AppBarWindow()
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook(WndProc);

            var abd = InitData();
            SHAppBarMessage(ABMsg.NEW, ref abd);

            isBarRegistered = true;
            InitAppBar();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (e.Cancel)
            {
                return;
            }

            if (isBarRegistered)
            {
                var abd = InitData();
                SHAppBarMessage(ABMsg.REMOVE, ref abd);
                isBarRegistered = false;
            }
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (int)ABNotify.POSCHANGED)
            {
                var abd = InitData();
                SHAppBarMessage(ABMsg.WINDOWPOSCHANGED, ref abd);
                //Old
                //ABSetPos();
            }
            else if(msg==(int)ABNotify.FULLSCREENAPP)
        {
                //::TODO:: Fullscreen app opened/closed
                //Need to adjust topmost otherwise it will be over the fullscreen app
        }

            return IntPtr.Zero;
        }

        private int _WindowMessageId;
        public int WindowMessageId
        {
            get {
                if (_WindowMessageId == 0)
                    _WindowMessageId = RegisterWindowMessage("AppBarMessage"); //::TODO:: This shouldn't be hardcoded
                return _WindowMessageId;
            }
        }

        private APPBARDATA InitData()
        {
            APPBARDATA data = new APPBARDATA();
            data.cbSize = Marshal.SizeOf(data);
            data.hWnd = new WindowInteropHelper(this).Handle;
            data.uCallbackMessage = WindowMessageId;
            return data;
        }
        public void InitAppBar()
        {

            APPBARDATA abd = InitData();
            abd.uEdge = (int)ABEdge.LEFT; //::TODO:: Read from Settings(?)
            if (SHAppBarMessage(ABMsg.NEW, ref abd)==1)
                return;
            int size = 200; //::TODO:: This should be a variable set somewhere else. Possibly let the user resize.
            string selectedDisplay; //::TODO:: read from settings if not set use primary if saved one is not available use primary.
            var screens = DisplayHelper.GetDisplays();
            var primaryScreen = screens.First(s => s.isPrimary);
            abd.rc = new RECT(primaryScreen.MonitorArea.left, primaryScreen.MonitorArea.top, primaryScreen.MonitorArea.right, primaryScreen.MonitorArea.bottom);
            SHAppBarMessage(ABMsg.QUERYPOS, ref abd);
            //::TODO:: next lines should be dependant on selected Edge
            abd.rc = new RECT(abd.rc.left, abd.rc.top, abd.rc.left+size, primaryScreen.WorkArea.bottom);
            SHAppBarMessage(ABMsg.SETPOS, ref abd);
            MoveWindow(abd.hWnd, abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, true);
            RECT wtf;
            GetWindowRect(abd.hWnd, out wtf);
            var ttt = 1;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        #region DllImports
        private const string User32 = "user32.dll";
        private const string Shell32 = "shell32.dll";
        [DllImport(Shell32, CallingConvention = CallingConvention.StdCall)]
        static extern uint SHAppBarMessage(ABMsg dwMessage, ref APPBARDATA pData);
        [DllImport(User32, CharSet = CharSet.Unicode)]
        private static extern int RegisterWindowMessage(string msg);
        [DllImport(User32)]
        private static extern long GetWindowLong(IntPtr hwnd, int index);

        [DllImport(User32)]
        private static extern long GetWindowLongPtr(IntPtr hwnd, int index);

        [DllImport(User32)]
        private static extern long SetWindowLong(IntPtr hwnd, int index, long newStyle);

        [DllImport(User32)]
        private static extern long SetWindowLongPtr(IntPtr hwnd, int index, long newStyle);
        [DllImport(User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);
        #endregion

        private static class Window_Style
        {
            public static int GWL_EXSTYLE = -20;
            public static long WS_EX_TRANSPARENT = 0x00000020;
            public static long WS_EX_TOOLWINDOW = 0x00000080;

        }
        private void SetExtStyle(long? add, long? remove)
        {
            IntPtr _hwnd = new WindowInteropHelper(this).Handle;
            bool _is64bit = IntPtr.Size == 8;
            long _style = GetWindowLongPtr(_hwnd, (int)Window_Style.GWL_EXSTYLE);
            if(add.HasValue)
            {
                _style |= add.Value;
            }
            if(remove.HasValue)
            {
                _style &= ~remove.Value;
            }
            SetWindowLongPtr(_hwnd, (int)Window_Style.GWL_EXSTYLE, _style);
        }

    }
}
