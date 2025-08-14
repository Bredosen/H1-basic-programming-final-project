
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace H1_basic_programming_final_project.Core.Handler;
public static class RawInput
{
    #region Constants
    private const int STD_INPUT_HANDLE = -10;
    private const ushort KEY_EVENT = 0x0001;
    #endregion

    #region Properties
    private static readonly HashSet<ushort> _held = [];
    private static readonly ConcurrentQueue<KeyEvent> _events = new();
    #endregion

    #region Computed Properties
    public static bool HasEvents => !_events.IsEmpty;
    #endregion

    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT_RECORD { public ushort EventType; public KEY_EVENT_RECORD KeyEvent; }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEY_EVENT_RECORD
    {
        [MarshalAs(UnmanagedType.Bool)] public bool bKeyDown;
        public ushort wRepeatCount;
        public ushort wVirtualKeyCode;
        public ushort wVirtualScanCode;
        public char UnicodeChar;
        public uint dwControlKeyState;
    }
    public readonly record struct KeyEvent(
        KeyEventType Type,
        ushort VirtualKey,
        char Char,
        ushort ScanCode,
        uint ControlKeyState
    );
    #endregion

    #region Native
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadConsoleInput(nint hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);
    #endregion

    #region Enums
    public enum KeyEventType { Click, Up }
    #endregion

    #region Is Held
    public static bool IsHeld(ushort virtualKey)
    {
        return _held.Contains(virtualKey);
    }
    #endregion

    #region Try Dequeue
    public static bool TryDequeue(out KeyEvent e)
    {
        return _events.TryDequeue(out e);
    }
    #endregion

    #region Poll
    public static void Poll()
    {
        nint handle = GetStdHandle(STD_INPUT_HANDLE);
        INPUT_RECORD[] buffer = new INPUT_RECORD[1];

        while (true)
        {
            if (!ReadConsoleInput(handle, buffer, 1, out uint read) || read == 0)
            {
                break;
            }

            if (buffer[0].EventType != KEY_EVENT)
            {
                continue;
            }

            KEY_EVENT_RECORD ke = buffer[0].KeyEvent;
            ushort vk = ke.wVirtualKeyCode;

            if (ke.bKeyDown)
            {
                // first down only -> Click
                if (_held.Add(vk))
                {
                    _events.Enqueue(new KeyEvent(
                        KeyEventType.Click, vk, ke.UnicodeChar, ke.wVirtualScanCode, ke.dwControlKeyState));
                }
                // else: auto-repeat; ignore
            }
            else
            {
                if (_held.Remove(vk))
                {
                    _events.Enqueue(new KeyEvent(
                        KeyEventType.Up, vk, ke.UnicodeChar, ke.wVirtualScanCode, ke.dwControlKeyState));
                }
            }
        }
    }
    #endregion
}
