using H1_basic_programming_final_project.Core.Structs;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace H1_basic_programming_final_project.Core.Helper;

public class ConsoleHelper
{
    #region Constants
    public const int STD_OUTPUT_HANDLE = -11;
    private const int MONITOR_DEFAULTTOPRIMARY = 1;
    #endregion

    #region Win32 Natives
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteConsoleOutput(IntPtr hConsoleOutput, CHAR_INFO[] lpBuffer, COORD dwBufferSize, COORD dwBufferCoord, ref SMALL_RECT lpWriteRegion);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumWindows(EnumedWindow lpEnumFunc, ArrayList lParam);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, [In, Out] ref WND_RECT rect);

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
    private static extern bool GetMonitorInfoEx(IntPtr hMonitor, ref MONITOR_INFO_EX lpmi);
    #endregion

    #region Delegates
    public delegate bool EnumedWindow(IntPtr handleWindow, ArrayList handles);

    protected delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    #endregion

    #region Is Window Fullscreen
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

            if (TryGetWindowAndMonitorRect(hWnd, out WND_RECT wnd, out WND_RECT mon))
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
    #endregion

    #region Get Windows
    public static ArrayList GetWindows()
    {
        ArrayList windowHandles = [];
        EnumedWindow callBackPtr = GetWindowHandle;
        _ = EnumWindows(callBackPtr, windowHandles);
        return windowHandles;
    }
    #endregion

    #region Get Window Handle
    private static bool GetWindowHandle(IntPtr windowHandle, ArrayList windowHandles)
    {
        _ = windowHandles.Add(windowHandle);
        return true;
    }
    #endregion

    #region Get Title
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
    #endregion

    #region Try Get Window And Monitor Rect
    private static bool TryGetWindowAndMonitorRect(IntPtr hWnd, out WND_RECT wnd, out WND_RECT mon)
    {
        wnd = default;
        mon = default;

        if (!GetWindowRect(hWnd, ref wnd))
        {
            return false;
        }

        MONITOR_INFO_EX mi = new() { cbSize = Marshal.SizeOf<MONITOR_INFO_EX>() };
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
    #endregion

    #region Enum The Windows
    private static bool EnumTheWindows(IntPtr hWnd, IntPtr lParam)
    {
        if (!IsWindowVisible(hWnd))
        {
            return true;
        }

        if (!TryGetWindowAndMonitorRect(hWnd, out WND_RECT appBounds, out WND_RECT monBounds))
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
    #endregion

    #region Get Monitor Name
    private static string GetMonitorName(IntPtr hWnd)
    {
        MONITOR_INFO_EX mi = new() { cbSize = Marshal.SizeOf<MONITOR_INFO_EX>() };
        IntPtr hMon = MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY);

        if (hMon == IntPtr.Zero)
        {
            return string.Empty;
        }

        return GetMonitorInfoEx(hMon, ref mi) ? mi.szDeviceName : string.Empty;
    }
    #endregion
}