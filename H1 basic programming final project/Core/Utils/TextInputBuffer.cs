using H1_basic_programming_final_project.Core.Types;
using System.Runtime.InteropServices;
using System.Text;

namespace H1_basic_programming_final_project.Core.Utils;

public sealed class TextInputBuffer
{
    #region Members

    #region Constants
    private const uint MAPVK_VK_TO_VSC = 0;
    private const int VK_SHIFT = 0x10, VK_CONTROL = 0x11, VK_MENU = 0x12;
    private const int VK_CAPITAL = 0x14, VK_NUMLOCK = 0x90, VK_SCROLL = 0x91;
    private const int VK_LSHIFT = 0xA0, VK_RSHIFT = 0xA1;
    private const int VK_RMENU = 0xA5;
    #endregion

    #region Properties
    public string Text { get; private set; } = string.Empty;
    public int Caret { get; private set; } = 0;
    public int MaxLength { get; set; } = int.MaxValue;
    private static IntPtr _forcedLayout = IntPtr.Zero; // set if you want to lock to Danish
    #endregion

    #endregion

    #region Native
    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleCP(uint id);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleOutputCP(uint id);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    private static extern IntPtr GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);
    #endregion

    #region Constructor
    public TextInputBuffer()
    {
        _ = SetConsoleCP(65001);
        _ = SetConsoleOutputCP(65001);

        UseDanishLayout(true);
    }
    #endregion

    #region Reset
    public void Reset(string? text = null)
    {
        Text = text ?? string.Empty;
        Caret = Text.Length;
    }
    #endregion

    #region Handle Key
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
                string s = VkToText(vk);
                if (string.IsNullOrEmpty(s))
                {
                    return false;
                }

                if (Text.Length >= MaxLength)
                {
                    return false;
                }

                if (Text.Length + s.Length > MaxLength)
                {
                    s = s[..(MaxLength - Text.Length)];
                }

                if (s.Length == 0)
                {
                    return false;
                }

                Text = Text.Insert(Caret, s);
                Caret += s.Length;
                return true;
        }
    }
    #endregion

    #region Get Visible Segment
    public (string visible, int start, int caretOffset) GetVisibleSegment(int maxVisible)
    {
        if (maxVisible < 1)
        {
            maxVisible = 1;
        }

        int start = 0;
        if (Caret > maxVisible)
        {
            start = Caret - maxVisible;
        }

        if (Text.Length - start > maxVisible) { }
        else if (Text.Length > maxVisible)
        {
            start = Text.Length - maxVisible;
        }

        int len = Math.Min(Text.Length - start, maxVisible);
        string vis = len > 0 ? Text.Substring(start, len) : string.Empty;
        int caretOff = Math.Min(Math.Max(Caret - start, 0), maxVisible);
        return (vis, start, caretOff);
    }
    #endregion

    #region Is Down
    private static bool IsDown(int vk)
    {
        return (GetKeyState(vk) & 0x8000) != 0;
    }
    #endregion

    #region Is Toggled
    private static bool IsToggled(int vk)
    {
        return (GetKeyState(vk) & 0x0001) != 0;
    }
    #endregion

    #region VK To Text
    private static string VkToText(ushort vk)
    {
        byte[] ks = new byte[256];
        FillKeyboardState(ks);

        nint kl = GetActiveKeyboardLayout();
        uint sc = MapVirtualKeyEx(vk, MAPVK_VK_TO_VSC, kl);

        // Allow AltGr (RightAlt+Control), block plain Ctrl/Alt combos
        bool ctrl = (ks[VK_CONTROL] & 0x80) != 0;
        bool alt = (ks[VK_MENU] & 0x80) != 0;
        bool altgr = ((ks[VK_RMENU] & 0x80) != 0) && ctrl;
        if ((ctrl || alt) && !altgr)
        {
            return string.Empty;
        }

        StringBuilder sb = new(8);
        int rc = ToUnicodeEx(vk, sc, ks, sb, sb.Capacity, 0, kl);

        if (rc < 0)
        { // clear dead-key state
            _ = ToUnicodeEx(0x20, MapVirtualKeyEx(0x20, MAPVK_VK_TO_VSC, kl), ks, sb, sb.Capacity, 0, kl);
            return string.Empty;
        }
        return rc > 0 ? sb.ToString() : string.Empty;
    }
    #endregion

    #region Use Danish Layout
    public static void UseDanishLayout(bool enable = true)
    {
        // 00000406 = Danish. Call once at startup if you want to hard-lock layout.
        _forcedLayout = enable ? LoadKeyboardLayout("00000406", 0) : IntPtr.Zero;
    }
    #endregion

    #region Fill Keyboard State
    private static void FillKeyboardState(byte[] ks)
    {
        for (int i = 0; i < 256; i++)
        {
            ks[i] = (GetAsyncKeyState(i) & 0x8000) != 0 ? (byte)0x80 : (byte)0x00;
        }

        if ((GetKeyState(VK_CAPITAL) & 0x1) != 0)
        {
            ks[VK_CAPITAL] |= 0x01;
        }

        if ((GetKeyState(VK_NUMLOCK) & 0x1) != 0)
        {
            ks[VK_NUMLOCK] |= 0x01;
        }

        if ((GetKeyState(VK_SCROLL) & 0x1) != 0)
        {
            ks[VK_SCROLL] |= 0x01;
        }
    }
    #endregion

    #region Get Active Keyboard Layout
    private static IntPtr GetActiveKeyboardLayout()
    {
        if (_forcedLayout != IntPtr.Zero)
        {
            return _forcedLayout; // lock to Danish if set
        }

        nint hwnd = GetForegroundWindow();
        uint tid = GetWindowThreadProcessId(hwnd, out _);
        return GetKeyboardLayout(tid);
    }
    #endregion

}
