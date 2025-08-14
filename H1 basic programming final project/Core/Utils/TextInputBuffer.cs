// File: Core/Utils/TextInputBuffer.cs
using System.Runtime.InteropServices;
using System.Text;
using H1_basic_programming_final_project.Core.Types;

namespace H1_basic_programming_final_project.Core.Utils;

public sealed class TextInputBuffer
{
    #region Members
    public string Text { get; private set; } = string.Empty;
    public int Caret { get; private set; } = 0;
    public int MaxLength { get; set; } = int.MaxValue;
    #endregion

    #region Reset
    public void Reset(string? text = null)
    {
        Text = text ?? string.Empty;
        Caret = Text.Length;
    }
    #endregion

    #region HandleKey
    public bool HandleKey(ushort vk)
    {
        switch (vk)
        {
            case VirtuelKeys.BACK:
                if (Caret > 0) { Text = Text.Remove(Caret - 1, 1); Caret--; return true; }
                return false;
            case VirtuelKeys.DELETE:
                if (Caret < Text.Length) { Text = Text.Remove(Caret, 1); return true; }
                return false;
            case VirtuelKeys.LEFT:
                if (Caret > 0) { Caret--; return true; }
                return false;
            case VirtuelKeys.RIGHT:
                if (Caret < Text.Length) { Caret++; return true; }
                return false;
            case VirtuelKeys.HOME:
                if (Caret != 0) { Caret = 0; return true; }
                return false;
            case VirtuelKeys.END:
                if (Caret != Text.Length) { Caret = Text.Length; return true; }
                return false;
            default:
                var s = VkToText(vk);
                if (string.IsNullOrEmpty(s)) return false;
                if (Text.Length >= MaxLength) return false;
                if (Text.Length + s.Length > MaxLength) s = s[..(MaxLength - Text.Length)];
                if (s.Length == 0) return false;
                Text = Text.Insert(Caret, s);
                Caret += s.Length;
                return true;
        }
    }
    #endregion

    #region GetVisibleSegment
    public (string visible, int start, int caretOffset) GetVisibleSegment(int maxVisible)
    {
        if (maxVisible < 1) maxVisible = 1;

        int start = 0;
        if (Caret > maxVisible) start = Caret - maxVisible;
        if (Text.Length - start > maxVisible) { }
        else if (Text.Length > maxVisible) start = Text.Length - maxVisible;

        int len = Math.Min(Text.Length - start, maxVisible);
        string vis = len > 0 ? Text.Substring(start, len) : string.Empty;
        int caretOff = Math.Min(Math.Max(Caret - start, 0), maxVisible);
        return (vis, start, caretOff);
    }
    #endregion

    #region Native
    [DllImport("user32.dll")] static extern bool GetKeyboardState(byte[] lpKeyState);
    [DllImport("user32.dll")] static extern IntPtr GetKeyboardLayout(uint idThread);
    [DllImport("user32.dll")] static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
        StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);
    [DllImport("user32.dll")] static extern short GetKeyState(int nVirtKey);

    const uint MAPVK_VK_TO_VSC = 0;
    const int VK_SHIFT = 0x10, VK_LSHIFT = 0xA0, VK_RSHIFT = 0xA1;
    const int VK_CONTROL = 0x11;
    const int VK_MENU = 0x12, VK_RMENU = 0xA5;
    const int VK_CAPITAL = 0x14;

    static bool IsDown(int vk) => (GetKeyState(vk) & 0x8000) != 0;
    static bool IsToggled(int vk) => (GetKeyState(vk) & 0x0001) != 0;

    static string VkToText(ushort vk)
    {
        var ks = new byte[256];
        GetKeyboardState(ks);

        if (IsDown(VK_SHIFT)) { ks[VK_SHIFT] |= 0x80; ks[VK_LSHIFT] |= 0x80; ks[VK_RSHIFT] |= 0x80; }
        if (IsDown(VK_CONTROL)) ks[VK_CONTROL] |= 0x80;
        if (IsDown(VK_MENU) || IsDown(VK_RMENU)) { ks[VK_MENU] |= 0x80; ks[VK_RMENU] |= 0x80; }
        if (IsToggled(VK_CAPITAL)) ks[VK_CAPITAL] = 0x01;

        var kl = GetKeyboardLayout(0);
        uint sc = MapVirtualKeyEx(vk, MAPVK_VK_TO_VSC, kl);
        var sb = new StringBuilder(8);
        int rc = ToUnicodeEx(vk, sc, ks, sb, sb.Capacity, 0, kl);

        if (rc < 0)
        {
            ToUnicodeEx(0x20, MapVirtualKeyEx(0x20, MAPVK_VK_TO_VSC, kl), ks, sb, sb.Capacity, 0, kl);
            return string.Empty;
        }
        return rc > 0 ? sb.ToString() : string.Empty;
    }
    #endregion
}
