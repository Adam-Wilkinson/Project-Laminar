using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPluginBase.Window
{
    class WindowHooks
    {
        public static Action WindowArrangementManuallyChanged { get; set; }

        public delegate void WinEventDelegate(
            IntPtr hWinEventHook,
            NativeMethods.SWEH_Events eventType,
            IntPtr hwnd,
            NativeMethods.SWEH_ObjectId idObject,
            long idChild,
            uint dwEventThread,
            uint dwmsEventTime
        );

        public static IntPtr WinEventHookRange(
            NativeMethods.SWEH_Events eventFrom, NativeMethods.SWEH_Events eventTo,
            WinEventDelegate eventDelegate,
            uint idProcess, uint idThread)
        {
            // new UIPermission(UIPermissionWindow.AllWindows).Demand();
            return NativeMethods.SetWinEventHook(
                eventFrom, eventTo,
                IntPtr.Zero, eventDelegate,
                idProcess, idThread,
                NativeMethods.WinEventHookInternalFlags);
        }

        public static IntPtr WinEventHookOne(
            NativeMethods.SWEH_Events eventId,
            WinEventDelegate eventDelegate,
            uint idProcess,
            uint idThread)
        {
            // new UIPermission(UIPermissionWindow.AllWindows).Demand();
            return NativeMethods.SetWinEventHook(
                eventId, eventId,
                IntPtr.Zero, eventDelegate,
                idProcess, idThread,
                NativeMethods.WinEventHookInternalFlags);
        }

        public static bool WinEventUnhook(IntPtr hWinEventHook) =>
            NativeMethods.UnhookWinEvent(hWinEventHook);

        public static uint GetWindowThread(IntPtr hWnd)
        {
            // new UIPermission(UIPermissionWindow.AllWindows).Demand();
            return NativeMethods.GetWindowThreadProcessId(hWnd, IntPtr.Zero);
        }

        public static RECT GetWindowRect(IntPtr hWnd)
        {
            NativeMethods.GetWindowRect(hWnd, out RECT rect);
            // NativeMethods.DwmGetWindowAttribute(hWnd,
            //    NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
            //    out RECT rect, Marshal.SizeOf<RECT>());
            return rect;
        }

        public static IntPtr GetNextWindow(IntPtr hOriginalWindow)
        {
            return NativeMethods.GetWindow(hOriginalWindow, NativeMethods.GW_Command.GW_HWNDNEXT);
        }

        public static bool SetWindowLayout(IntPtr hWnd, RECT rect, IntPtr priorWindow)
        {
            return NativeMethods.SetWindowPos(hWnd, priorWindow, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, NativeMethods.SWP_Flags.SWP_ASYNCWINDOWPOS | NativeMethods.SWP_Flags.SWP_NOACTIVE);
        }
        public static bool SetWindowRect(IntPtr hWnd, RECT rect)
        {
            return NativeMethods.SetWindowPos(hWnd, IntPtr.Zero, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, NativeMethods.SWP_Flags.SWP_ASYNCWINDOWPOS | NativeMethods.SWP_Flags.SWP_NOACTIVE | NativeMethods.SWP_Flags.SWP_NOOWNERZORDER | NativeMethods.SWP_Flags.SWP_NOZORDER);
        }

        public static string GetWindowTitle(IntPtr hWnd)
        {
            int titleLength = NativeMethods.GetWindowTextLength(hWnd);
            StringBuilder builder = new(titleLength);
            NativeMethods.GetWindowText(hWnd, builder, titleLength + 1);
            return builder.ToString();
        }

        public static Point CurrentCursorPosition()
        {
            NativeMethods.GetCursorPos(out POINT lpPoint);
            return new Point(lpPoint);
        }

        public static Rectangle CurrentMonitorSize()
        {
            NativeMethods.GetCursorPos(out POINT lpPoint);
            IntPtr lPrimaryScreen = NativeMethods.MonitorFromPoint(lpPoint, NativeMethods.MonitorOptions.MONITOR_DEFAULTTOPRIMARY);
            MONITORINFO lPrimaryScreenInfo = new MONITORINFO();
            lPrimaryScreenInfo.cbSize = (uint)Marshal.SizeOf(lPrimaryScreenInfo);
            if (NativeMethods.GetMonitorInfo(lPrimaryScreen, lPrimaryScreenInfo) == false)
            {
                Debug.WriteLine(Marshal.GetLastWin32Error());
                return new Rectangle();
            }
            return new Rectangle { Rect = lPrimaryScreenInfo.rcWork };
            Debug.WriteLine(lPrimaryScreenInfo.rcWork.Top);
            Debug.WriteLine(lPrimaryScreenInfo.rcWork.Left);
            Debug.WriteLine(lPrimaryScreenInfo.rcWork.Bottom);
            Debug.WriteLine(lPrimaryScreenInfo.rcWork.Right);
            // IntPtr monitor = NativeMethods.MonitorFromPoint(lpPoint, NativeMethods.MonitorOptions.MONITOR_DEFAULTTONEAREST);

            // MONITORINFO info = new();
            // bool success = NativeMethods.GetMonitorInfo(monitor, ref info);
            // if (!success)
            // {
            // // Debug.WriteLine(Marshal.GetLastWin32Error());
            // }
            return new Rectangle { Rect = MonitorEnumerator.AllMonitorInfo[0].rcWork };
        }


        private class MonitorEnumerator
        {
            public static readonly List<MONITORINFO> AllMonitorInfo = new();

            static MonitorEnumerator()
            {
                EnumMonitors();
            }

            private static void EnumMonitors()
            {
                NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumCallBack, IntPtr.Zero);
            }

            private static bool MonitorEnumCallBack(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
            {
                Debug.WriteLine(hMonitor);
                MONITORINFO mon_info = new MONITORINFO();
                mon_info.cbSize = (uint)Marshal.SizeOf(mon_info);
                NativeMethods.GetMonitorInfo(hMonitor, mon_info);
                Debug.WriteLine(Marshal.GetLastWin32Error());
                AllMonitorInfo.Add(mon_info);
                return true;
            }
        }

    }

    public static class NativeMethods
    {
        public static long SWEH_CHILDID_SELF = 0;

        public enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }


        //SetWinEventHook() flags
        public enum SWEH_dwFlags : uint
        {
            WINEVENT_OUTOFCONTEXT = 0x0000,     // Events are ASYNC
            WINEVENT_SKIPOWNTHREAD = 0x0001,    // Don't call back for events on installer's thread
            WINEVENT_SKIPOWNPROCESS = 0x0002,   // Don't call back for events on installer's process
            WINEVENT_INCONTEXT = 0x0004         // Events are SYNC, this causes your dll to be injected into every process
        }

        //SetWinEventHook() events
        public enum SWEH_Events : uint
        {
            EVENT_MIN = 0x00000001,
            EVENT_MAX = 0x7FFFFFFF,
            EVENT_SYSTEM_SOUND = 0x0001,
            EVENT_SYSTEM_ALERT = 0x0002,
            EVENT_SYSTEM_FOREGROUND = 0x0003,
            EVENT_SYSTEM_MENUSTART = 0x0004,
            EVENT_SYSTEM_MENUEND = 0x0005,
            EVENT_SYSTEM_MENUPOPUPSTART = 0x0006,
            EVENT_SYSTEM_MENUPOPUPEND = 0x0007,
            EVENT_SYSTEM_CAPTURESTART = 0x0008,
            EVENT_SYSTEM_CAPTUREEND = 0x0009,
            EVENT_SYSTEM_MOVESIZESTART = 0x000A,
            EVENT_SYSTEM_MOVESIZEEND = 0x000B,
            EVENT_SYSTEM_CONTEXTHELPSTART = 0x000C,
            EVENT_SYSTEM_CONTEXTHELPEND = 0x000D,
            EVENT_SYSTEM_DRAGDROPSTART = 0x000E,
            EVENT_SYSTEM_DRAGDROPEND = 0x000F,
            EVENT_SYSTEM_DIALOGSTART = 0x0010,
            EVENT_SYSTEM_DIALOGEND = 0x0011,
            EVENT_SYSTEM_SCROLLINGSTART = 0x0012,
            EVENT_SYSTEM_SCROLLINGEND = 0x0013,
            EVENT_SYSTEM_SWITCHSTART = 0x0014,
            EVENT_SYSTEM_SWITCHEND = 0x0015,
            EVENT_SYSTEM_MINIMIZESTART = 0x0016,
            EVENT_SYSTEM_MINIMIZEEND = 0x0017,
            EVENT_SYSTEM_DESKTOPSWITCH = 0x0020,
            EVENT_SYSTEM_END = 0x00FF,
            EVENT_OEM_DEFINED_START = 0x0101,
            EVENT_OEM_DEFINED_END = 0x01FF,
            EVENT_UIA_EVENTID_START = 0x4E00,
            EVENT_UIA_EVENTID_END = 0x4EFF,
            EVENT_UIA_PROPID_START = 0x7500,
            EVENT_UIA_PROPID_END = 0x75FF,
            EVENT_CONSOLE_CARET = 0x4001,
            EVENT_CONSOLE_UPDATE_REGION = 0x4002,
            EVENT_CONSOLE_UPDATE_SIMPLE = 0x4003,
            EVENT_CONSOLE_UPDATE_SCROLL = 0x4004,
            EVENT_CONSOLE_LAYOUT = 0x4005,
            EVENT_CONSOLE_START_APPLICATION = 0x4006,
            EVENT_CONSOLE_END_APPLICATION = 0x4007,
            EVENT_CONSOLE_END = 0x40FF,
            EVENT_OBJECT_CREATE = 0x8000,               // hwnd ID idChild is created item
            EVENT_OBJECT_DESTROY = 0x8001,              // hwnd ID idChild is destroyed item
            EVENT_OBJECT_SHOW = 0x8002,                 // hwnd ID idChild is shown item
            EVENT_OBJECT_HIDE = 0x8003,                 // hwnd ID idChild is hidden item
            EVENT_OBJECT_REORDER = 0x8004,              // hwnd ID idChild is parent of zordering children
            EVENT_OBJECT_FOCUS = 0x8005,                // hwnd ID idChild is focused item
            EVENT_OBJECT_SELECTION = 0x8006,            // hwnd ID idChild is selected item (if only one), or idChild is OBJID_WINDOW if complex
            EVENT_OBJECT_SELECTIONADD = 0x8007,         // hwnd ID idChild is item added
            EVENT_OBJECT_SELECTIONREMOVE = 0x8008,      // hwnd ID idChild is item removed
            EVENT_OBJECT_SELECTIONWITHIN = 0x8009,      // hwnd ID idChild is parent of changed selected items
            EVENT_OBJECT_STATECHANGE = 0x800A,          // hwnd ID idChild is item w/ state change
            EVENT_OBJECT_LOCATIONCHANGE = 0x800B,       // hwnd ID idChild is moved/sized item
            EVENT_OBJECT_NAMECHANGE = 0x800C,           // hwnd ID idChild is item w/ name change
            EVENT_OBJECT_DESCRIPTIONCHANGE = 0x800D,    // hwnd ID idChild is item w/ desc change
            EVENT_OBJECT_VALUECHANGE = 0x800E,          // hwnd ID idChild is item w/ value change
            EVENT_OBJECT_PARENTCHANGE = 0x800F,         // hwnd ID idChild is item w/ new parent
            EVENT_OBJECT_HELPCHANGE = 0x8010,           // hwnd ID idChild is item w/ help change
            EVENT_OBJECT_DEFACTIONCHANGE = 0x8011,      // hwnd ID idChild is item w/ def action change
            EVENT_OBJECT_ACCELERATORCHANGE = 0x8012,    // hwnd ID idChild is item w/ keybd accel change
            EVENT_OBJECT_INVOKED = 0x8013,              // hwnd ID idChild is item invoked
            EVENT_OBJECT_TEXTSELECTIONCHANGED = 0x8014, // hwnd ID idChild is item w? test selection change
            EVENT_OBJECT_CONTENTSCROLLED = 0x8015,
            EVENT_SYSTEM_ARRANGMENTPREVIEW = 0x8016,
            EVENT_OBJECT_END = 0x80FF,
            EVENT_AIA_START = 0xA000,
            EVENT_AIA_END = 0xAFFF
        }

        //SetWinEventHook() Object Ids
        public enum SWEH_ObjectId : long
        {
            OBJID_WINDOW = 0x00000000,
            OBJID_SYSMENU = 0xFFFFFFFF,
            OBJID_TITLEBAR = 0xFFFFFFFE,
            OBJID_MENU = 0xFFFFFFFD,
            OBJID_CLIENT = 0xFFFFFFFC,
            OBJID_VSCROLL = 0xFFFFFFFB,
            OBJID_HSCROLL = 0xFFFFFFFA,
            OBJID_SIZEGRIP = 0xFFFFFFF9,
            OBJID_CARET = 0xFFFFFFF8,
            OBJID_CURSOR = 0xFFFFFFF7,
            OBJID_ALERT = 0xFFFFFFF6,
            OBJID_SOUND = 0xFFFFFFF5,
            OBJID_QUERYCLASSNAMEIDX = 0xFFFFFFF4,
            OBJID_NATIVEOM = 0xFFFFFFF0
        }

        //SetWindowPos() Flags
        public enum SWP_Flags : uint
        {
            SWP_ASYNCWINDOWPOS = 0x4000,
            SWP_DEFERERASE = 0x2000,
            SWP_DRAWFRAME = 0x0020,
            SWP_FRAMECHANGED = 0x0020,
            SWP_HIDEWINDOW = 0x0080,
            SWP_NOACTIVE = 0x0010,
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOMOVE = 0x0002,
            SWP_NOOWNERZORDER = 0x0200,
            SWP_NOREDRAW = 0x0008,
            SWP_NOREPOSITION = 0x0200,
            SWP_NOSENDCHANGING = 0x0400,
            SWP_NOSIZE = 0x0001,
            SWP_NOZORDER = 0x0004,
            SWP_SHOWWINDOW = 0x0040
        }

        //GetWindow() commands
        public enum GW_Command : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5
        }

        public enum DWMWINDOWATTRIBUTE : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,      // [get] Is non-client rendering enabled/disabled
            DWMWA_NCRENDERING_POLICY,           // [set] DWMNCRENDERINGPOLICY - Non-client rendering policy - Enable or disable non-client rendering
            DWMWA_TRANSITIONS_FORCEDISABLED,    // [set] Potentially enable/forcibly disable transitions
            DWMWA_ALLOW_NCPAINT,                // [set] Allow contents rendered In the non-client area To be visible On the DWM-drawn frame.
            DWMWA_CAPTION_BUTTON_BOUNDS,        // [get] Bounds Of the caption button area In window-relative space.
            DWMWA_NONCLIENT_RTL_LAYOUT,         // [set] Is non-client content RTL mirrored
            DWMWA_FORCE_ICONIC_REPRESENTATION,  // [set] Force this window To display iconic thumbnails.
            DWMWA_FLIP3D_POLICY,                // [set] Designates how Flip3D will treat the window.
            DWMWA_EXTENDED_FRAME_BOUNDS,        // [get] Gets the extended frame bounds rectangle In screen space
            DWMWA_HAS_ICONIC_BITMAP,            // [set] Indicates an available bitmap When there Is no better thumbnail representation.
            DWMWA_DISALLOW_PEEK,                // [set] Don't invoke Peek on the window.
            DWMWA_EXCLUDED_FROM_PEEK,           // [set] LivePreview exclusion information
            DWMWA_CLOAK,                        // [set] Cloak Or uncloak the window
            DWMWA_CLOAKED,                      // [get] Gets the cloaked state Of the window. Returns a DWMCLOACKEDREASON object
            DWMWA_FREEZE_REPRESENTATION,        // [set] BOOL, Force this window To freeze the thumbnail without live update
            PlaceHolder1,
            PlaceHolder2,
            PlaceHolder3,
            DWMWA_ACCENTPOLICY = 19
        }

        public static SWEH_dwFlags WinEventHookInternalFlags =
            SWEH_dwFlags.WINEVENT_OUTOFCONTEXT |
            SWEH_dwFlags.WINEVENT_SKIPOWNPROCESS |
            SWEH_dwFlags.WINEVENT_SKIPOWNTHREAD;

        [DllImport("dwmapi.dll")]
        internal static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr voidProcessId);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetWinEventHook(
            SWEH_Events eventMin,
            SWEH_Events eventMax,
            IntPtr hmodWinEventProc,
            WindowHooks.WinEventDelegate lpfnWinEventProc,
            uint idProcess, uint idThread,
            SWEH_dwFlags dwFlags);

        [DllImport("user32.dll")]
        internal static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int Y, int cx, int cy, SWP_Flags wFlags);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindow(IntPtr hWnd, GW_Command uCmd);

        [DllImport("USER32.DLL")]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        internal static extern int GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("User32.dll", SetLastError = true)]
        internal static extern bool GetMonitorInfo(IntPtr hmonitor, MONITORINFO info);

        [DllImport("User32.dll", ExactSpelling = true)]
        internal static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        internal static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        internal delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);
    }
}
