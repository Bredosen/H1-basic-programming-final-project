using EnumWnd;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace H1_basic_programming_final_project.Core.Helper
{
    public class ConsoleHelper
    {
        #region Win32 imports

        // Win32 constants
        public const int STD_OUTPUT_HANDLE = -11;

        // Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;
            public COORD(short x, short y) { X = x; Y = y; }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CHAR_INFO
        {
            public char UnicodeChar;
            public short Attributes;
        }

        // P/Invoke
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsoleOutput(
            IntPtr hConsoleOutput,
            CHAR_INFO[] lpBuffer,
            COORD dwBufferSize,
            COORD dwBufferCoord,
            ref SMALL_RECT lpWriteRegion);


        public delegate bool EnumedWindow(IntPtr handleWindow, ArrayList handles);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumedWindow lpEnumFunc, ArrayList lParam);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, [In, Out] ref Rect rect);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("User32")]
        private static extern IntPtr MonitorFromWindow(IntPtr hWnd, int dwFlags);

        [DllImport("user32", EntryPoint = "GetMonitorInfo", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetMonitorInfoEx(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        protected delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int MONITOR_DEFAULTTOPRIMARY = 1;

        #endregion

        #region Public API

        public static bool IsWindowFullscreen(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return false;
            }

            bool foundFullscreen = false;
            _ = EnumWindows((hWnd, _) =>
            {
                if (!IsWindowVisible(hWnd))
                {
                    return true;
                }

                string windowTitle = GetTitle(hWnd);
                if (windowTitle.Length == 0)
                {
                    return true;
                }

                if (windowTitle.IndexOf(title, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return true;
                }

                if (TryGetWindowAndMonitorRect(hWnd, out Rect wnd, out Rect mon))
                {
                    int wndWidth = wnd.Right - wnd.Left;
                    int wndHeight = wnd.Bottom - wnd.Top;

                    int monWidth = mon.Right - mon.Left;
                    int monHeight = mon.Bottom - mon.Top;

                    // Fullscreen if window exactly covers its monitor
                    if (wndWidth == monWidth && wndHeight == monHeight)
                    {
                        foundFullscreen = true;
                    }
                }

                // keep enumerating to find any match
                return !foundFullscreen;
            }, IntPtr.Zero);

            return foundFullscreen;
        }

        public static ArrayList GetWindows()
        {
            ArrayList windowHandles = [];
            EnumedWindow callBackPtr = GetWindowHandle;
            _ = EnumWindows(callBackPtr, windowHandles);
            return windowHandles;
        }

        public static void PrintWindows()
        {
            _ = EnumWindows(EnumTheWindows, IntPtr.Zero);
        }

        #endregion

        #region Internals

        private static bool GetWindowHandle(IntPtr windowHandle, ArrayList windowHandles)
        {
            _ = windowHandles.Add(windowHandle);
            return true;
        }

        private static string GetTitle(IntPtr hWnd)
        {
            int len = GetWindowTextLength(hWnd);
            if (len <= 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new(len + 1);
            _ = GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        private static bool TryGetWindowAndMonitorRect(IntPtr hWnd, out Rect wnd, out Rect mon)
        {
            wnd = default;
            mon = default;

            if (!GetWindowRect(hWnd, ref wnd))
            {
                return false;
            }

            MonitorInfoEx mi = new() { cbSize = Marshal.SizeOf<MonitorInfoEx>() };
            IntPtr hMon = MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY);
            if (hMon == IntPtr.Zero)
            {
                return false;
            }

            if (!GetMonitorInfoEx(hMon, ref mi))
            {
                return false;
            }

            mon = mi.rcMonitor;
            return true;
        }

        private static bool EnumTheWindows(IntPtr hWnd, IntPtr lParam)
        {
            if (!IsWindowVisible(hWnd))
            {
                return true;
            }

            if (!TryGetWindowAndMonitorRect(hWnd, out Rect appBounds, out Rect monBounds))
            {
                return true;
            }

            int size = GetWindowTextLength(hWnd);
            if (size <= 0)
            {
                return true;
            }

            StringBuilder sb = new(size + 1);
            _ = GetWindowText(hWnd, sb, sb.Capacity);

            string title = sb.ToString();
            if (title.Length > 20)
            {
                title = title[..20];
            }

            int windowWidth = appBounds.Right - appBounds.Left;
            int windowHeight = appBounds.Bottom - appBounds.Top;

            int monitorWidth = monBounds.Right - monBounds.Left;
            int monitorHeight = monBounds.Bottom - monBounds.Top;

            bool fullScreen = (windowWidth == monitorWidth) && (windowHeight == monitorHeight);

            Console.WriteLine($"{title} Wnd:({windowWidth} | {windowHeight}) Mtr:({monitorWidth} | {monitorHeight} | Name: {GetMonitorName(hWnd)}) - {fullScreen}");
            return true;
        }

        private static string GetMonitorName(IntPtr hWnd)
        {
            MonitorInfoEx mi = new() { cbSize = Marshal.SizeOf<MonitorInfoEx>() };
            IntPtr hMon = MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY);

            if (hMon == IntPtr.Zero)
            {
                return string.Empty;
            }

            return GetMonitorInfoEx(hMon, ref mi) ? mi.szDeviceName : string.Empty;
        }

        #endregion
    }
}

namespace EnumWnd
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct MonitorInfoEx
    {
        public int cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDeviceName;
    }
}
