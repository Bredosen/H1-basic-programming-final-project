using System.Runtime.InteropServices;

namespace H1_basic_programming_final_project.Core.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct INPUT_RECORD
{
    public ushort EventType;
    public KEY_EVENT_RECORD KeyEvent;
}
