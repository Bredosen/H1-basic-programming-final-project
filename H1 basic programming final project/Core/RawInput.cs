namespace H1_basic_programming_final_project.Core;

using System;
using System.Runtime.InteropServices;

public static class RawInput
{
    const uint ENABLE_EXTENDED_FLAGS = 0x0080;
    const uint ENABLE_WINDOW_INPUT = 0x0008;

    [StructLayout(LayoutKind.Sequential)]
    struct INPUT_RECORD
    {
        public ushort EventType;
        public KEY_EVENT_RECORD KeyEvent;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KEY_EVENT_RECORD
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool bKeyDown;
        public ushort wRepeatCount;
        public ushort wVirtualKeyCode;
        public ushort wVirtualScanCode;
        public char UnicodeChar;
        public uint dwControlKeyState;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetConsoleMode(IntPtr hConsoleInput, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetConsoleMode(IntPtr hConsoleInput, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool ReadConsoleInput(IntPtr hConsoleInput,
        [Out] INPUT_RECORD[] lpBuffer,
        uint nLength,
        out uint lpNumberOfEventsRead);

    const int STD_INPUT_HANDLE = -10;
    const ushort KEY_EVENT = 0x0001;

    public static bool WDown, SDown, UpDown, DownDown;

    public static void Poll()
    {
        IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);

        var buffer = new INPUT_RECORD[1];
        uint read;

        while (true)
        {
            if (!ReadConsoleInput(handle, buffer, 1, out read) || read == 0)
                break;

            if (buffer[0].EventType != KEY_EVENT)
                continue;

            var key = buffer[0].KeyEvent;
            bool down = key.bKeyDown;
            switch (key.wVirtualKeyCode)
            {
                case 0x57: WDown = down; break; // W
                case 0x53: SDown = down; break; // S
                case 0x26: UpDown = down; break; // Up arrow
                case 0x28: DownDown = down; break; // Down arrow
            }
        }
    }

}
