using System.Runtime.InteropServices;

namespace H1_basic_programming_final_project.Core.Structs;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct MONITOR_INFO_EX
{
    public int cbSize;
    public WND_RECT rcMonitor;
    public WND_RECT rcWork;
    public uint dwFlags;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string szDeviceName;
}
