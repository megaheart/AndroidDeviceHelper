using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;

namespace AndroidDeviceHelper.Native
{
    class WindowResizer
    {
        private const int WM_SYSCOMMAND = 0x112;
        private WindowInteropHelper hWnd;
        private Window ActiveWindow;
        //bool HasMaxWidth = false;
        //bool HasMaxHeight = false;
        public WindowResizer(Window activeWindow)
        {
            ActiveWindow = activeWindow;
            hWnd = new WindowInteropHelper(activeWindow);
            //activeWindow.ResizeMode = ResizeMode.NoResize; //Very important to have correct parameters
            activeWindow.SourceInitialized += (sender, e) =>
            {
                HwndSource.FromHwnd(hWnd.Handle).AddHook(new HwndSourceHook(WindowProc));
            };
        }
        #region Maximize Window
        private System.IntPtr WindowProc(
              System.IntPtr hwnd,
              int msg,
              System.IntPtr wParam,
              System.IntPtr lParam,
              ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (System.IntPtr)0;
        }

        private void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            System.IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != System.IntPtr.Zero)
            {

                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                double scale = rcMonitorArea.Width / SystemParameters.VirtualScreenWidth;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                mmi.ptMinTrackSize.x = (int)Math.Floor(ActiveWindow.MinWidth * scale);
                mmi.ptMinTrackSize.y = (int)Math.Floor(ActiveWindow.MinHeight * scale);
                //if (ActiveWindow.MaxHeight != Double.PositiveInfinity)
                //{
                //    mmi.ptMaxTrackSize.y = (int)Math.Floor(ActiveWindow.MaxHeight * scale);
                //}
                //if (ActiveWindow.MaxWidth != Double.PositiveInfinity)
                //{
                //    mmi.ptMaxTrackSize.x = (int)Math.Floor(ActiveWindow.MaxWidth * scale);
                //}
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }
        /// <summary>
        /// POINT aka POINTAPI
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width
            {
                get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
            }
            public int Height
            {
                get { return bottom - top; }
            }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }
            //public bool IsEmpty
            //{
            //    get
            //    {
            //        // BUGBUG : On Bidi OS (hebrew arabic) left > right
            //        return left >= right || top >= bottom;
            //    }
            //}
            public override string ToString()
            {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }
        }
        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
        #endregion
        #region Resize Window
        public enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(hWnd.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }
        public void resizeWindow(Thumb resizingThumb)
        {
            switch (resizingThumb.Name)
            {
                case "top":
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "bottom":
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "left":
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "right":
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "topLeft":
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "topRight":
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "bottomLeft":
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "bottomRight":
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
