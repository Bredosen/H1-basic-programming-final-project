using H1_basic_programming_final_project.Core.Services;
using H1_basic_programming_final_project.Core.Types;

namespace H1_basic_programming_final_project.Core.DataModels;

public abstract class MenuPage : Page
{
    #region Properties
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SelectedArgument = 0;
    public readonly List<PageArgument> Arguments = [];
    #endregion

    #region Constructor
    public MenuPage(string name) : base(name)
    {
        OnKeyPressed += MainMenuPage_OnKeyPressed;
    }
    #endregion

    #region [Event] - On Key Pressed.
    private void MainMenuPage_OnKeyPressed(ushort key)
    {
        switch (key)
        {
            case VirtuelKeys.UP:
                SelectedArgument = (SelectedArgument - 1 + Arguments.Count) % Arguments.Count;
                UpdateExists = true;
                break;

            case VirtuelKeys.DOWN:
                SelectedArgument = (SelectedArgument + 1) % Arguments.Count;
                UpdateExists = true;
                break;

            case VirtuelKeys.RETURN:
                Arguments[SelectedArgument].Action?.Invoke();
                break;
            default:
                break;
        }
    }
    #endregion

    #region Render
    public override void Render()
    {
        RenderLeftMenu();
        RenderRightMenu();
    }
    #endregion

    #region Render Right Menu
    public void RenderRightMenu()
    {
        int panelWidth = (int)(Width / 2.75D);
        int rightPanelX = panelWidth; // starting X of right panel
        int rightPanelWidth = Width - rightPanelX;

        string[] asciiArt =
        {
        "                  ooo OOOAOOO ooo",
        "              oOO       / \\       OOo",
        "          oOO          /   \\          OOo",
        "       oOO            /     \\            OOo",
        "     oOO             /       \\             OOo",
        "   oOO -_-----------/---------\\-----------_- OOo",
        "  oOO     -_       /           \\       _-     OOo",
        " oOO         -_   /             \\   _-         OOo",
        "oOO             -/_             _\\-             OOo",
        "oOO             /  -_         _-  \\             OOo",
        "oOO            /      -_   _-      \\            OOo",
        "oOO           /         _-_         \\           OOo",
        "oOO          /       _-     -_       \\          OOo",
        " oOO        /     _-           -_     \\        OOo",
        "  oOO      /   _-                 -_   \\      OOo",
        "   oOO    / _-                       -_ \\    OOo",
        "     oOO _-                             -_ OOo",
        "      oOO                                OOo",
        "          oOO                         OOo",
        "             oOO                 OOo",
        "                  ooo OOO OOO ooo"
    };

        int artWidth = asciiArt.Max(line => line.Length);
        int artHeight = asciiArt.Length;

        // Center horizontally in right panel
        int startX = rightPanelX + (rightPanelWidth - artWidth) / 2;
        // Center vertically in console
        int startY = (Height - artHeight) / 2;

        for (int index = 0; index < asciiArt.Length; index++)
        {
            COut.DrawText(startX, startY + index, asciiArt[index], ConsoleColor.Black, ConsoleColor.White);
        }
    }
    #endregion


    #region Render Left Menu
    public void RenderLeftMenu()
    {
        int panelX = 2;
        int panelY = 1;
        int panelWidth = (int)(Width / 2.75D);
        int panelHeight = Height - 1;

        // Panel
        COut.FillRect(panelX, panelY, panelWidth, panelHeight, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        COut.DrawRect(panelX, panelY, panelWidth, panelHeight, '=', ConsoleColor.DarkGray, ConsoleColor.Black);

        // header
        COut.FillRect(panelX + (panelWidth / 2) - 15, 3, 30, 5, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        COut.DrawRect(panelX + (panelWidth / 2) - 15, 3, 30, 5, '#', ConsoleColor.DarkGray, ConsoleColor.Black);
        COut.DrawText(panelX + (panelWidth / 2), 5, Title, ConsoleColor.DarkGray, ConsoleColor.Gray, Types.HorizontalAlignment.Center);

        // Decription
        COut.DrawText(panelX + (panelWidth / 2), 11, Description, ConsoleColor.DarkGray, ConsoleColor.Green, Types.HorizontalAlignment.Center);

        // Arguments
        RenderArguments();

        // Help Bottom
        COut.DrawText(panelX + (panelWidth / 2), panelY + (panelHeight - 3), "Use 'Arrow Keys' to navigate and Enter to select.", ConsoleColor.DarkGray, ConsoleColor.Green, Types.HorizontalAlignment.Center);
    }
    #endregion

    #region Render Arguments
    public void RenderArguments()
    {
        int startX = 8;
        int startY = 18;
        int spacing = 3;
        int totalItems = Arguments.Count;

        // Header
        COut.DrawText(startX, startY, "Select an option:", ConsoleColor.DarkGray, ConsoleColor.Green, Types.HorizontalAlignment.Left);
        COut.DrawText(startX, startY + 1, new string('=', 35), ConsoleColor.DarkGray, ConsoleColor.Green, Types.HorizontalAlignment.Left);

        // Draw full vertical wall
        int wallHeight = (totalItems * spacing);
        for (int y = 0; y < wallHeight; y++)
            COut.DrawText(startX, startY + 2 + y, "|", ConsoleColor.DarkGray, ConsoleColor.Green, Types.HorizontalAlignment.Left);

        // Menu entries
        for (int index = 0; index < Arguments.Count; index++)
        {
            RenderLeftMenuArgument(index, Arguments[index].Name, startX, startY + 3 + (index * spacing));
        }
    }
    #endregion

    #region Render Left Menu Argument
    public void RenderLeftMenuArgument(int index, string text, int wallX, int y)
    {
        int indexSpace = 2;
        string indexStr = $"[{index}]";

        COut.DrawText(wallX + 2, y, indexStr, ConsoleColor.DarkGray, ConsoleColor.Green, Types.HorizontalAlignment.Left);
        COut.DrawText(wallX + 2 + indexStr.Length + indexSpace, y, text, ConsoleColor.DarkGray, ConsoleColor.White, Types.HorizontalAlignment.Left);

        if (SelectedArgument == index)
        {
            COut.DrawText(wallX + 2 + indexStr.Length + indexSpace, y + 1,
                new string('═', text.Length), ConsoleColor.DarkGray, ConsoleColor.Green, Types.HorizontalAlignment.Left);
        }
    }
    #endregion
}
