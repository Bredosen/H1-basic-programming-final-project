using System.Runtime.InteropServices;

namespace H1_basic_programming_final_project.Core.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct COORD(short x, short y)
{
    #region Properties
    public short X = x;
    public short Y = y;

    #endregion
}
