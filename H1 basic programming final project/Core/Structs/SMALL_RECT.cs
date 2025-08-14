using System.Runtime.InteropServices;

namespace H1_basic_programming_final_project.Core.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct SMALL_RECT
{
    public short Left;
    public short Top;
    public short Right;
    public short Bottom;
}
