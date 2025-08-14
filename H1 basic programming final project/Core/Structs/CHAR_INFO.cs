using System.Runtime.InteropServices;

namespace H1_basic_programming_final_project.Core.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct CHAR_INFO
{
    public char UnicodeChar;
    public short Attributes;
}