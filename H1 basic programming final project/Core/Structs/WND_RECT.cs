using System.Runtime.InteropServices;

namespace H1_basic_programming_final_project.Core.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct WND_RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}
