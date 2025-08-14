using H1_basic_programming_final_project.Core.Types;

namespace H1_basic_programming_final_project.Core.Structs;
public readonly record struct KEY_EVENT
{
    #region Properties
    public readonly KeyEventType Type;
    public readonly ushort VirtualKey;
    public readonly char Char;
    public readonly ushort ScanCode;
    public readonly uint ControlKeyStat;
    #endregion

    #region Constructor
    public KEY_EVENT(KeyEventType type, ushort vk, char _char, ushort scanCode, uint controlKeyStat)
    {
        Type = type;
        VirtualKey = vk;
        Char = _char;
        ScanCode = scanCode;
        ControlKeyStat = controlKeyStat;
    }
    #endregion
}