using H1_basic_programming_final_project.Core.Manager;
using H1_basic_programming_final_project.Core.Types;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static H1_basic_programming_final_project.Core.Helper.ConsoleHelper;

namespace H1_basic_programming_final_project.Core.Services;

public sealed class Rendere
{
    #region Static Buffer Members
    public static IntPtr ConsoleHandler;
    private static CHAR_INFO[] _consoleBuffer = Array.Empty<CHAR_INFO>();
    private static int _bufferWidth;
    private static int _bufferHeight;

    public static Rendere Root { get; private set; } = new Rendere(0, 0, 0, 0);
    #endregion

    #region Instance Viewport
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    #endregion

    #region Constructor
    private Rendere(int x, int y, int width, int height)
    {
        X = Math.Max(0, x);
        Y = Math.Max(0, y);
        Width = Math.Max(0, Math.Min(width, Math.Max(0, _bufferWidth - X)));
        Height = Math.Max(0, Math.Min(height, Math.Max(0, _bufferHeight - Y)));
    }
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
            _consoleBuffer[i].Attributes = ((int)ConsoleColor.Black << 4) | (int)ConsoleColor.Gray;
        }

        Root = new Rendere(0, 0, _bufferWidth, _bufferHeight);
    }
    #endregion

    #region Update Buffer
    public static void UpdateBuffer()
    {
        _bufferWidth = Console.BufferWidth;
        _bufferHeight = Console.BufferHeight;
        _consoleBuffer = new CHAR_INFO[_bufferWidth * _bufferHeight];
        // Keep Root spanning the full buffer after resize
        Root = new Rendere(0, 0, _bufferWidth, _bufferHeight);
    }
    #endregion

    #region CreateSubRenderer
    public Rendere CreateSubRenderer(int x, int y, int width, int height)
    {
        // Requested rect is local to this renderer
        int reqX = X + x;
        int reqY = Y + y;
        int reqW = width;
        int reqH = height;

        // Clamp to parent viewport
        int sx = Math.Max(X, reqX);
        int sy = Math.Max(Y, reqY);
        int ex = Math.Min(X + Width, reqX + reqW);
        int ey = Math.Min(Y + Height, reqY + reqH);

        int w = Math.Max(0, ex - sx);
        int h = Math.Max(0, ey - sy);

        return new Rendere(sx, sy, w, h);
    }
    #endregion

    #region Static Clear (entire buffer)
    public static void Clear(ConsoleColor background = ConsoleColor.Black, ConsoleColor foreground = ConsoleColor.Gray)
    {
        for (int i = 0; i < _consoleBuffer.Length; i++)
        {
            _consoleBuffer[i].UnicodeChar = ' ';
            _consoleBuffer[i].Attributes = (short)(((int)background << 4) | (int)foreground);
        }
    }
    #endregion

    #region Instance Clear (viewport only)
    public void ClearLocal(ConsoleColor background = ConsoleColor.Black, ConsoleColor foreground = ConsoleColor.Gray)
    {
        for (int y = 0; y < Height; y++)
        {
            int gy = Y + y;
            if (gy < 0 || gy >= _bufferHeight)
            {
                continue;
            }

            int rowStart = gy * _bufferWidth;
            int gx0 = Math.Max(0, X);
            int gx1 = Math.Min(_bufferWidth, X + Width);

            for (int gx = gx0; gx < gx1; gx++)
            {
                int idx = rowStart + gx;
                _consoleBuffer[idx].UnicodeChar = ' ';
                _consoleBuffer[idx].Attributes = (short)(((int)background << 4) | (int)foreground);
            }
        }
    }
    #endregion

    #region Set Pixel
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetPixel(int x, int y, char character, ConsoleColor background, ConsoleColor foreground)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
        {
            return;
        }

        int gx = X + x, gy = Y + y;
        int idx = (gy * _bufferWidth) + gx;
        _consoleBuffer[idx].UnicodeChar = character;
        _consoleBuffer[idx].Attributes = (short)(((int)background << 4) | (int)foreground);
    }
    #endregion

    #region Instance DrawRect (clipped)
    public void DrawRect(int x, int y, int width, int height, char character, ConsoleColor background, ConsoleColor foreground)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        if (!ClipRectLocal(x, y, width, height, out int x0, out int y0, out int x1, out int y1))
        {
            return;
        }

        int xL = x;
        int xR = x + width - 1;
        int yT = y;
        int yB = y + height - 1;

        // Top
        if ((uint)yT < (uint)Height)
        {
            for (int vx = Math.Max(x0, 0); vx < x1; vx++)
            {
                SetPixel(vx, yT, character, background, foreground);
            }
        }

        // Bottom (avoid double-drawing if height==1)
        if (yB != yT && (uint)yB < (uint)Height)
        {
            for (int vx = Math.Max(x0, 0); vx < x1; vx++)
            {
                SetPixel(vx, yB, character, background, foreground);
            }
        }

        // Left
        if ((uint)xL < (uint)Width)
        {
            for (int vy = Math.Max(y0, 0); vy < y1; vy++)
            {
                SetPixel(xL, vy, character, background, foreground);
            }
        }

        // Right (avoid double-drawing if width==1)
        if (xR != xL && (uint)xR < (uint)Width)
        {
            for (int vy = Math.Max(y0, 0); vy < y1; vy++)
            {
                SetPixel(xR, vy, character, background, foreground);
            }
        }
    }

    #endregion

    #region Instance FillRect (clipped)
    public void FillRect(int x, int y, int width, int height, char character, ConsoleColor background, ConsoleColor foreground)
    {
        if (!ClipRectLocal(x, y, width, height, out int x0, out int y0, out int x1, out int y1))
        {
            return;
        }

        short attr = (short)(((int)background << 4) | (int)foreground);
        for (int vy = y0; vy < y1; vy++)
        {
            int gy = Y + vy;
            int rowStart = (gy * _bufferWidth) + X + x0;
            for (int vx = x0; vx < x1; vx++)
            {
                int idx = rowStart + (vx - x0);
                _consoleBuffer[idx].UnicodeChar = character;
                _consoleBuffer[idx].Attributes = attr;
            }
        }
    }
    #endregion

    #region Instance FillOval (clipped)
    public void FillOval(int x, int y, int width, int height, char character, ConsoleColor background, ConsoleColor foreground)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        if (!ClipRectLocal(x, y, width, height, out int x0, out int y0, out int x1, out int y1))
        {
            return;
        }

        double rx = width / 2.0, ry = height / 2.0;
        double cx = x + rx, cy = y + ry; // center in local coords

        for (int vy = y0; vy < y1; vy++)
        {
            for (int vx = x0; vx < x1; vx++)
            {
                double dx = (vx - cx) / rx;
                double dy = (vy - cy) / ry;
                if ((dx * dx) + (dy * dy) <= 1.0)
                {
                    SetPixel(vx, vy, character, background, foreground);
                }
            }
        }
    }
    #endregion

    #region Instance DrawOval (clipped)
    public void DrawOval(int x, int y, int width, int height, char character, ConsoleColor background, ConsoleColor foreground, double threshold = 0.02)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        if (!ClipRectLocal(x, y, width, height, out int x0, out int y0, out int x1, out int y1))
        {
            return;
        }

        double rx = width / 2.0, ry = height / 2.0;
        double cx = x + rx, cy = y + ry;

        for (int vy = y0; vy < y1; vy++)
        {
            for (int vx = x0; vx < x1; vx++)
            {
                double dx = (vx - cx) / rx;
                double dy = (vy - cy) / ry;
                double dist = (dx * dx) + (dy * dy);
                if (Math.Abs(dist - 1.0) <= threshold)
                {
                    SetPixel(vx, vy, character, background, foreground);
                }
            }
        }
    }
    #endregion

    #region Instance DrawText (clipped)
    public void DrawText(int x, int y, string text, ConsoleColor background, ConsoleColor foreground, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        int startX = alignment switch
        {
            HorizontalAlignment.Right => x - text.Length + 1,
            HorizontalAlignment.Center => x - (text.Length / 2),
            _ => x
        };

        // Visible range within viewport
        int visL = Math.Max(0, startX);
        int visR = Math.Min(Width, startX + text.Length);
        if (visR <= visL || y < 0 || y >= Height)
        {
            return;
        }

        int srcOffset = visL - startX;
        for (int vx = visL; vx < visR; vx++)
        {
            SetPixel(vx, y, text[srcOffset + (vx - visL)], background, foreground);
        }
    }
    #endregion

    #region Static Render (flush buffer)
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

    #region Clip helper
    private bool ClipRectLocal(int x, int y, int width, int height,
        out int x0, out int y0, out int x1, out int y1)
    {
        x0 = Math.Max(0, x);
        y0 = Math.Max(0, y);
        x1 = Math.Min(Width, x + width);
        y1 = Math.Min(Height, y + height);
        return x1 > x0 && y1 > y0;
    }
    #endregion

}
