using H1_basic_programming_final_project.Core.Manager;
using H1_basic_programming_final_project.Core.Types;
using static H1_basic_programming_final_project.Core.Helper.ConsoleHelper;

namespace H1_basic_programming_final_project.Core.Services;

public sealed class Rendere
{

    #region Properties
    public static IntPtr ConsoleHandler;
    private static CHAR_INFO[] _consoleBuffer;
    private static int _bufferWidth;
    private static int _bufferHeight;
    #endregion

    #region Initialize
    public static void Initialize()
    {
        ConsoleHandler = GetStdHandle(STD_OUTPUT_HANDLE);
        PageManager.Instance.ConsoleResized += UpdateBuffer;
        UpdateBuffer();
        for (int i = 0; i < _consoleBuffer.Length; i++)
        {
            _consoleBuffer[i].UnicodeChar = ' ';
            _consoleBuffer[i].Attributes = (short)((int)ConsoleColor.Black << 4 | (int)ConsoleColor.Gray);
        }
    }
    #endregion

    #region Update Buffer
    public static void UpdateBuffer()
    {
        _bufferWidth = Console.BufferWidth;
        _bufferHeight = Console.BufferHeight;
        _consoleBuffer = new CHAR_INFO[_bufferWidth * _bufferHeight];
    }
    #endregion

    #region Clear
    public static void Clear(ConsoleColor background = ConsoleColor.Black, ConsoleColor foreground = ConsoleColor.Gray)
    {
        for (int i = 0; i < _consoleBuffer.Length; i++)
        {
            _consoleBuffer[i].UnicodeChar = ' ';
            _consoleBuffer[i].Attributes = (short)(((int)background << 4) | (int)foreground);
        }
    }
    #endregion

    #region Draw Helpers
    private static void SetPixel(int x, int y, char character, ConsoleColor background, ConsoleColor foreground)
    {
        if (x < 0 || x >= _bufferWidth || y < 0 || y >= _bufferHeight)
            return;

        int index = x + y * _bufferWidth;
        _consoleBuffer[index].UnicodeChar = character;
        _consoleBuffer[index].Attributes = (short)(((int)background << 4) | (int)foreground);
    }
    #endregion

    #region DrawRect
    public static void DrawRect(int x, int y, int width, int height, char character, ConsoleColor background, ConsoleColor foreground)
    {
        for (int Y = 0; Y < height; Y++)
        {
            for (int X = 0; X < width; X++)
            {
                if (Y == 0 || Y == height - 1 || X == 0 || X == width - 1)
                {
                    SetPixel(x + X, y + Y, character, background, foreground);
                }
            }
        }
    }
    #endregion

    #region FillRect
    public static void FillRect(int x, int y, int width, int height, char character, ConsoleColor background, ConsoleColor foreground)
    {
        for (int Y = 0; Y < height; Y++)
        {
            for (int X = 0; X < width; X++)
            {
                SetPixel(x + X, y + Y, character, background, foreground);
            }
        }
    }
    #endregion

    #region FillOval
    public static void FillOval(int x, int y, int width, int height, char character, ConsoleColor background, ConsoleColor foreground)
    {
        double rx = width / 2.0;
        double ry = height / 2.0;
        double cx = rx;
        double cy = ry;

        for (int Y = 0; Y < height; Y++)
        {
            for (int X = 0; X < width; X++)
            {
                double dx = (X - cx) / rx;
                double dy = (Y - cy) / ry;
                if ((dx * dx) + (dy * dy) <= 1.0)
                {
                    SetPixel(x + X, y + Y, character, background, foreground);
                }
            }
        }
    }
    #endregion

    #region DrawOval
    public static void DrawOval(int x, int y, int width, int height, char character, ConsoleColor background, ConsoleColor foreground)
    {
        double rx = width / 2.0;
        double ry = height / 2.0;
        double cx = rx;
        double cy = ry;
        double threshold = 0.02;

        for (int Y = 0; Y < height; Y++)
        {
            for (int X = 0; X < width; X++)
            {
                double dx = (X - cx) / rx;
                double dy = (Y - cy) / ry;
                double dist = (dx * dx) + (dy * dy);
                if (Math.Abs(dist - 1.0) <= threshold)
                {
                    SetPixel(x + X, y + Y, character, background, foreground);
                }
            }
        }
    }
    #endregion

    #region DrawText
    public static void DrawText(int x, int y, string text, ConsoleColor background, ConsoleColor foreground, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        if (string.IsNullOrEmpty(text)) return;

        int startX = alignment switch
        {
            HorizontalAlignment.Right => x - text.Length + 1,
            HorizontalAlignment.Center => x - (text.Length / 2),
            _ => x
        };

        for (int i = 0; i < text.Length; i++)
        {
            SetPixel(startX + i, y, text[i], background, foreground);
        }
    }
    #endregion

    #region Render
    public static void Render()
    {
        COORD bufferSize = new((short)_bufferWidth, (short)_bufferHeight);
        COORD bufferCoord = new(0, 0);
        SMALL_RECT writeRegion = new()
        {
            Left = 0,
            Top = 0,
            Right = (short)(_bufferWidth - 1),
            Bottom = (short)(_bufferHeight - 1)
        };

        if (!WriteConsoleOutput(ConsoleHandler, _consoleBuffer, bufferSize, bufferCoord, ref writeRegion))
        {
            Console.WriteLine("Error writing to console buffer: " + Marshal.GetLastWin32Error());
        }
    }
    #endregion
}
