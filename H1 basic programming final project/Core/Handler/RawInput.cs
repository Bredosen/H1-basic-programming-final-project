namespace H1_basic_programming_final_project.Core.Handler;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class RawInput
{
    // Win32
    const int STD_INPUT_HANDLE = -10;
    const ushort KEY_EVENT = 0x0001;

    [StructLayout(LayoutKind.Sequential)]
    struct INPUT_RECORD { public ushort EventType; public KEY_EVENT_RECORD KeyEvent; }

    [StructLayout(LayoutKind.Sequential)]
    struct KEY_EVENT_RECORD
    {
        [MarshalAs(UnmanagedType.Bool)] public bool bKeyDown;
        public ushort wRepeatCount;
        public ushort wVirtualKeyCode;
        public ushort wVirtualScanCode;
        public char UnicodeChar;              // 0 for non-printables
        public uint dwControlKeyState;
    }

    [DllImport("kernel32.dll", SetLastError = true)] static extern nint GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool ReadConsoleInput(nint hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);

    // API
    public enum KeyEventType { Click, Up }

    public readonly record struct KeyEvent(
        KeyEventType Type,
        ushort VirtualKey,
        char Char,
        ushort ScanCode,
        uint ControlKeyState
    );

    // state
    private static readonly HashSet<ushort> _held = new();
    private static readonly ConcurrentQueue<KeyEvent> _events = new();

    /// <summary>Returns true if a key is currently held.</summary>
    public static bool IsHeld(ushort virtualKey) => _held.Contains(virtualKey);

    /// <summary>True if any pending key event exists.</summary>
    public static bool HasEvents => !_events.IsEmpty;

    /// <summary>Try to pop the next key event.</summary>
    public static bool TryDequeue(out KeyEvent e) => _events.TryDequeue(out e);

    /// <summary>Poll and generate events for all keys.</summary>
    public static void Poll()
    {
        nint handle = GetStdHandle(STD_INPUT_HANDLE);
        var buffer = new INPUT_RECORD[1];

        while (true)
        {
            if (!ReadConsoleInput(handle, buffer, 1, out uint read) || read == 0) break;
            if (buffer[0].EventType != KEY_EVENT) continue;

            var ke = buffer[0].KeyEvent;
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
}
