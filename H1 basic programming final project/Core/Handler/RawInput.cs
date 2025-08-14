
using H1_basic_programming_final_project.Core.Structs;
using H1_basic_programming_final_project.Core.Types;
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
    private static readonly ConcurrentQueue<KEY_EVENT> _events = new();
    #endregion

    #region Computed Properties
    public static bool HasEvents => !_events.IsEmpty;
    #endregion

    #region Native
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadConsoleInput(nint hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);
    #endregion

    #region Is Held
    public static bool IsHeld(ushort virtualKey)
    {
        return _held.Contains(virtualKey);
    }
    #endregion

    #region Try Dequeue
    public static bool TryDequeue(out KEY_EVENT e)
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
                if (_held.Add(vk))
                {
                    _events.Enqueue(new KEY_EVENT(
                        KeyEventType.Click, vk, ke.UnicodeChar, ke.wVirtualScanCode, ke.dwControlKeyState));
                }
            }
            else
            {
                if (_held.Remove(vk))
                {
                    _events.Enqueue(new KEY_EVENT(
                        KeyEventType.Up, vk, ke.UnicodeChar, ke.wVirtualScanCode, ke.dwControlKeyState));
                }
            }
        }
    }
    #endregion
}
