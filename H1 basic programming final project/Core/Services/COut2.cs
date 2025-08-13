using H1_basic_programming_final_project.Core.Types;
using System.Runtime.InteropServices;
using static H1_basic_programming_final_project.Core.Helper.ConsoleHelper;

namespace H1_basic_programming_final_project.Core.Services;

public static class COut2
{
    #region Properties
    public static int Width => Console.BufferWidth;
    public static int Height => Console.BufferHeight;
    #endregion

    #region Header
    public static void Header(string header, int marginHorizontal = -1, int marginVertical = 1, char fill = '#')
    {
        if (header is null) header = string.Empty;
        int headerLen = header.Length;

        // normalize inputs
        marginVertical = Math.Max(1, marginVertical);
        marginHorizontal = marginHorizontal < 0 ? headerLen : marginHorizontal;

        // target block size
        int width = marginHorizontal + headerLen + marginHorizontal;
        int height = marginVertical + 1 + marginVertical;

        // clamp to window
        int winW = Console.WindowWidth;
        int winH = Console.WindowHeight;
        width = Math.Min(width, Math.Max(1, winW));
        height = Math.Min(height, Math.Max(1, winH));

        // compute top-left of block
        int left = Math.Max(0, (winW - width) / 2);
        int top = Console.CursorTop;
        if (top + height >= winH) top = Math.Max(0, winH - height - 1);

        // cache line once
        string line = new string(fill, width);

        // draw filled block
        for (int y = 0; y < height; y++)
        {
            Console.SetCursorPosition(left, top + y);
            Console.Write(line);
        }

        // write header text centered within block row = top + marginVertical
        int textLeft = left + Math.Max(0, (width - headerLen) / 2);
        int textTop = top + Math.Min(marginVertical, height - 1);

        // optional clip if header longer than width
        int visible = Math.Min(headerLen, Math.Max(0, width));
        Console.SetCursorPosition(textLeft, textTop);
        Console.Write(visible == headerLen ? header : header.AsSpan(0, visible).ToString());
    }

    #endregion

    #region Set Border
    public static void SetBorder(char borderChar = '#', ConsoleColor borderColor = ConsoleColor.Cyan)
    {
        SetColor(borderColor);

        int consoleWidth = Console.WindowWidth;
        int consoleHeight = Console.WindowHeight;

        for (int y = 0; y < consoleHeight; y++)
        {
            for (int x = 0; x < consoleWidth; x++)
            {
                if (x == 0 || x == consoleWidth - 1 || y == 0 || y == consoleHeight - 1)
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(borderChar);
                }
            }
        }

        ResetColor();
    }
    #endregion

    #region Set Cursor Horizontal
    public static void SetCursorHorizontal(HorizontalAlignment horizontalAlignment, int horizontalOffset = 0)
    {
        int left = horizontalAlignment switch
        {
            HorizontalAlignment.Left => horizontalOffset,
            HorizontalAlignment.Center => (Console.WindowWidth / 2) - (horizontalOffset / 2),
            HorizontalAlignment.Right => Console.WindowWidth - horizontalOffset,
            _ => Console.CursorLeft
        };
        Console.CursorLeft = left;
    }
    #endregion

    #region Space
    public static void Space(int spaceSize = 1)
    {
        for (int i = 0; i < spaceSize; i++)
        {
            Console.WriteLine();
        }
    }
    #endregion

    #region Write
    public static void Write(object argument, ConsoleColor consoleColor = ConsoleColor.White, HorizontalAlignment verticalAlignment = HorizontalAlignment.Center)
    {
        SetColor(consoleColor);
        Console.CursorLeft = verticalAlignment switch
        {
            HorizontalAlignment.Left => 0,
            HorizontalAlignment.Center => (Console.WindowWidth / 2) - (argument.ToString()?.Length ?? 0) / 2,
            HorizontalAlignment.Right => Console.WindowWidth - (argument.ToString()?.Length ?? 0),
            _ => Console.CursorLeft
        };
        Console.Write(argument);
        ResetColor();
    }
    #endregion

    #region Set Color
    public static void SetColor(ConsoleColor color)
    {
        Console.ForegroundColor = color;
    }
    #endregion

    #region Reset Color
    public static void ResetColor()
    {
        Console.ResetColor();
    }
    #endregion

    #region Write Line
    public static void WriteLine(object argument, ConsoleColor consoleColor = ConsoleColor.White, HorizontalAlignment verticalAlignment = HorizontalAlignment.Center)
    {
        SetColor(consoleColor);
        Console.CursorLeft = verticalAlignment switch
        {
            HorizontalAlignment.Left => 0,
            HorizontalAlignment.Center => (Console.WindowWidth / 2) - (argument.ToString()?.Length ?? 0) / 2,
            HorizontalAlignment.Right => Console.WindowWidth - (argument.ToString()?.Length ?? 0),
            _ => Console.CursorLeft
        };
        Console.WriteLine(argument);
        ResetColor();
    }
    #endregion

    #region Write List
    public static void WriteList<T>(List<T> list, bool printIndex = true, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left, int horizontalOffset = 20)
    {
        foreach (T? item in list)
        {
            if (printIndex)
            {
                string textWithIndex = $"({list.IndexOf(item)}) > {item}";
                SetCursorHorizontal(horizontalAlignment, horizontalOffset);
                Console.Write(new string(' ', horizontalOffset));
                SetColor(ConsoleColor.Magenta);
                Console.Write($"({list.IndexOf(item)})");
                ResetColor();
                SetColor(ConsoleColor.Green);
                Console.WriteLine($" > {item}");
                ResetColor();
                continue;
            }

            string text = $"{item}";
            Console.CursorLeft = horizontalAlignment switch
            {
                HorizontalAlignment.Left => 0,
                HorizontalAlignment.Center => (Console.WindowWidth / 2) - (text.Length / 2),
                HorizontalAlignment.Right => Console.WindowWidth - text.Length,
                _ => Console.CursorLeft
            };
            SetColor(ConsoleColor.Green);
            Console.WriteLine($"{item}");
            ResetColor();
        }
    }
    #endregion

    #region Clear
    public static void Clear()
    {
        Console.Clear();
        Console.WriteLine("\x1b[3J");
    }
    #endregion

    #region Wait For Continue
    public static void WaitForContinue()
    {
        SetColor(ConsoleColor.Yellow);
        WriteLine("Press any key to continue...");
        ResetColor();
        _ = Console.ReadKey();
    }
    #endregion


    #region Fill
    public static void Fill(int x, int y, int width, int height, char character, ConsoleColor background, ConsoleColor foreground)
    {
        COORD bufferSize = new((short)width, (short)height);
        CHAR_INFO[] ConsoleBuffer = new CHAR_INFO[width * height];
        COORD bufferCoord = new((short)x, (short)y);
        SMALL_RECT writeRegion = new()
        {
            Left = (short)x,
            Top = (short)y,
            Right = (short)(x + width),
            Bottom = (short)(y + height)
        };

        for (int Y = 0; Y < height; Y++)
        {
            for (int X = 0; X < width; X++)
            {
                int index = X + Y * width;
                if (index < 0 || index >= ConsoleBuffer.Length) continue;
                ConsoleBuffer[index].UnicodeChar = character;
                int bg = (int)background;
                int fg = (int)foreground;
                ConsoleBuffer[index].Attributes = (short)((bg << 4) | fg);
            }
        }
        if (!WriteConsoleOutput(ConsoleHandler, ConsoleBuffer, bufferSize, bufferCoord, ref writeRegion))
        {
            Console.WriteLine("Error writing to console buffer: " + Marshal.GetLastWin32Error());
        }

    }
    #endregion



    #region Initialize

    public static IntPtr ConsoleHandler;
    public static void Initialize()
    {
        ConsoleHandler = GetStdHandle(STD_OUTPUT_HANDLE);
    }
    #endregion

    #region Get User Input
    public static string GetUserInput(string prompt = " >> ")
    {
        SetColor(ConsoleColor.Green);
        Write(prompt);
        ResetColor();
        if ((Console.ReadLine() ?? string.Empty) is not string input)
        {
            return string.Empty;
        }

        return input;
    }
    #endregion
}
