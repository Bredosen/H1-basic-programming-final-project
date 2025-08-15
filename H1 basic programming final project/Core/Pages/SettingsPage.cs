using H1_basic_programming_final_project.Core.DataModels;
using H1_basic_programming_final_project.Core.Manager;
using H1_basic_programming_final_project.Core.Services;
using H1_basic_programming_final_project.Core.Types;

namespace H1_basic_programming_final_project.Core.Pages;

public sealed class SettingsPage : Page
{
    #region Members

    #region Singleton
    private static readonly Lazy<SettingsPage> _instance = new(() => new SettingsPage(), true);
    public static readonly SettingsPage Instance = _instance.Value;
    #endregion

    #endregion

    #region Constructor
    private SettingsPage() : base(nameof(SettingsPage))
    {
        Activated += SettingsPage_Activated;
        DeActivated += SettingsPage_DeActivated;
    }
    #endregion

    #region Events

    #region [Event] - Page Activated.
    private void SettingsPage_Activated()
    {
        OnKeyPressed += SettingsPage_OnKeyPressed;
    }
    #endregion

    #region [Event] - Page DeActivated.
    private void SettingsPage_DeActivated()
    {
        OnKeyPressed -= SettingsPage_OnKeyPressed;
    }
    #endregion

    #region [Event] - Key Pressed.
    private void SettingsPage_OnKeyPressed(ushort keyCode)
    {
        if (CheckKeyPropagation() != true) return;

        switch (keyCode)
        {
            case VirtuelKeys.ESCAPE:
                PageManager.Instance.GoBackPage();
                goto default;
            default:
                break;
        }
    }
    #endregion

    #endregion

    #region Rendere

    #region [Entry-Point] - Render.
    public override void Render(Rendere rendere)
    {
        // Rendere Title
        rendere.DrawText(rendere.Width / 2, 4, "Settings", ConsoleColor.Black, ConsoleColor.Green, Types.HorizontalAlignment.Center);

        // Render Menu.
        int menuWidth = (int)(rendere.Width / 2.4D);
        int menuHeight = (int)(rendere.Height / 1.4D);
        int menuX = rendere.Width / 2 - (menuWidth / 2);
        int menuY = rendere.Height / 2 - (menuHeight / 2);
        RenderMenu(rendere.CreateSubRenderer(menuX, menuY, menuWidth, menuHeight));

        // Render Bottom Panel.
        int bottomMenuWidth = (int)(menuWidth / 2.75D);
        int bottomMenuHeight = 5;
        int bottomMenuX = rendere.Width / 2 - (bottomMenuWidth / 2);
        int bottomMenuY = menuY + menuHeight - 1;
        RenderBottomPanel(rendere.CreateSubRenderer(bottomMenuX, bottomMenuY, bottomMenuWidth, bottomMenuHeight));
    }
    #endregion

    #region Render Menu
    public void RenderMenu(Rendere rendere)
    {
        rendere.DrawMenu(0, 0, rendere.Width, rendere.Height);
    }
    #endregion

    #region Render Bottom Panel
    public void RenderBottomPanel(Rendere rendere)
    {
        rendere.DrawMenu(0, 0, rendere.Width, rendere.Height);
        rendere.DrawText(rendere.Width / 2, 2, "Press 'Escape' to go back", ConsoleColor.DarkGray, ConsoleColor.Green, Types.HorizontalAlignment.Center);
    }
    #endregion

    #endregion
}
