using H1_basic_programming_final_project.Core.Manager;
using H1_basic_programming_final_project.Core.Services;
using H1_basic_programming_final_project.Core.Types;

namespace H1_basic_programming_final_project.Core.DataModels;

public abstract class LeftRightMenuPage : Page
{
    #region Properties
    public string Title { get; set; } = string.Empty;
    public bool DisplayArtRight { get; set; } = true;
    public string ArtFile { get; set; } = "HitAnyKeyToContinue";
    public string Description { get; set; } = string.Empty;
    public int SelectedArgument = 0;
    public ConsoleColor SelectedArgumentColor { get; set; } = ConsoleColor.Green;
    public bool ListenForNumbers { get; set; } = true;
    public bool ListenForArrows { get; set; } = true;
    public bool ListenForEnter { get; set; } = true;
    public readonly List<PageArgument> Arguments = [];
    #endregion

    #region Constructor
    public LeftRightMenuPage(string name) : base(name)
    {
        OnKeyPressed += MainMenuPage_OnKeyPressed;
    }
    #endregion

    #region [Event] - On Key Pressed.
    private void MainMenuPage_OnKeyPressed(ushort key)
    {
        // Number keys select by index (0–9) if present
        if (ListenForNumbers && TryGetDigitIndex(key, out int idx))
        {
            if (idx < Arguments.Count)
            {
                SelectedArgument = idx;
                Arguments[SelectedArgument].Action?.Invoke();
            }
            return;
        }

        switch (key)
        {
            case VirtuelKeys.UP:
                if (!ListenForArrows)
                {
                    break;
                }

                SelectedArgument = (SelectedArgument - 1 + Arguments.Count) % Arguments.Count;
                UpdateExists = true;
                StopKeyPropagation = true;
                break;

            case VirtuelKeys.DOWN:
                if (!ListenForArrows)
                {
                    break;
                }

                SelectedArgument = (SelectedArgument + 1) % Arguments.Count;
                UpdateExists = true;
                StopKeyPropagation = true;
                break;

            case VirtuelKeys.RETURN:
                if (!ListenForEnter)
                {
                    break;
                }

                StopKeyPropagation = true;
                Arguments[SelectedArgument].Action?.Invoke();
                break;
        }
    }
    #endregion

    #region Key helpers
    private static bool TryGetDigitIndex(ushort key, out int index)
    {
        // Top row '0'..'9' (0x30..0x39)
        int t = key - 0x30;
        if ((uint)t <= 9) { index = t; return true; }

        // Numpad '0'..'9' (0x60..0x69)
        int n = key - 0x60;
        if ((uint)n <= 9) { index = n; return true; }

        index = -1;
        return false;
    }
    #endregion

    #region Render
    public override void Render(Rendere r)
    {
        Rendere left = r.CreateSubRenderer(2, 1, (int)(r.Width / 2.75), r.Height - 2);
        Rendere right = r.CreateSubRenderer(left.Width + 3, 1, r.Width - (left.Width + 5), r.Height - 2);

        RenderLeftMenu(left);
        RenderRightMenu(right);
    }
    #endregion

    #region Render Left Menu
    public virtual void RenderLeftMenu(Rendere r)
    {
        // Panel
        r.FillRect(0, 0, r.Width, r.Height, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawRect(0, 0, r.Width, r.Height, '=', ConsoleColor.DarkGray, ConsoleColor.Black);

        // Header
        int headerW = 30, headerH = 5, headerX = (r.Width - headerW) / 2, headerY = 3;
        r.FillRect(headerX, headerY, headerW, headerH, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawRect(headerX, headerY, headerW, headerH, '#', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawText(r.Width / 2, headerY + 2, Title, ConsoleColor.DarkGray, ConsoleColor.Gray, HorizontalAlignment.Center);

        // Description
        r.DrawText(r.Width / 2, 11, Description, ConsoleColor.DarkGray, ConsoleColor.Green, HorizontalAlignment.Center);

        // Arguments
        RenderArguments(r);

        // Help
        r.DrawText(r.Width / 2, r.Height - 3, "Use 'Arrow Keys' to navigate and Enter to select.",
            ConsoleColor.DarkGray, ConsoleColor.Green, HorizontalAlignment.Center);
    }
    #endregion

    #region Render Right Menu
    public virtual void RenderRightMenu(Rendere r)
    {
        if (!DisplayArtRight)
        {
            return;
        }

        string[]? art = AsciiManager.Load(ArtFile);
        if (art == null || art.Length == 0)
        {
            return;
        }

        int artW = art.Max(l => l.Length);
        int artH = art.Length;
        int startX = Math.Max(0, (r.Width - artW) / 2);
        int startY = Math.Max(0, (r.Height - artH) / 2);

        for (int i = 0; i < art.Length; i++)
        {
            r.DrawText(startX, startY + i, art[i], ConsoleColor.Black, ConsoleColor.White);
        }
    }
    #endregion

    #region Render Arguments
    public void RenderArguments(Rendere r)
    {
        int startX = 8, startY = 18, spacing = 3;

        r.DrawText(startX, startY, "Select an option:", ConsoleColor.DarkGray, ConsoleColor.Green);
        r.DrawText(startX, startY + 1, new string('=', 35), ConsoleColor.DarkGray, ConsoleColor.Green);

        int wallHeight = Arguments.Count * spacing;
        for (int y = 0; y < wallHeight; y++)
        {
            r.DrawText(startX, startY + 2 + y, "|", ConsoleColor.DarkGray, ConsoleColor.Green);
        }

        for (int i = 0; i < Arguments.Count; i++)
        {
            RenderLeftMenuArgument(r, i, Arguments[i].Name, startX, startY + 3 + (i * spacing));
        }
    }
    #endregion

    #region Render Left Menu Argument
    public void RenderLeftMenuArgument(Rendere r, int index, string text, int wallX, int y)
    {
        string idx = $"[{index}]";
        int idxSpace = 2;
        int textX = wallX + 2 + idx.Length + idxSpace;

        r.DrawText(wallX + 2, y, idx, ConsoleColor.DarkGray, ConsoleColor.Green);
        r.DrawText(textX, y, text, ConsoleColor.DarkGray, ConsoleColor.White);

        if (SelectedArgument == index)
        {
            r.DrawText(textX, y + 1, new string('=', text.Length), ConsoleColor.DarkGray, SelectedArgumentColor);
        }
    }
    #endregion
}
