using H1_basic_programming_final_project.Core.DataModels;
using H1_basic_programming_final_project.Core.Manager;

namespace H1_basic_programming_final_project.Core.Pages;

public sealed class MainMenuPage : MenuPage
{
    #region Members

    #region Singleton
    private static readonly Lazy<MainMenuPage> _instance = new(() => new MainMenuPage(), true);
    public static readonly MainMenuPage Instance = _instance.Value;
    #endregion

    #endregion

    #region Constructor
    private MainMenuPage() : base("MainMenuPage")
    {
        Title = "Main Menu";
        Description = "Welcome to the main menu!";
        Arguments.Add(new PageArgument("Open Task Manager", OpenPingPong));
        Arguments.Add(new PageArgument("Open Ping Pong", OpenPingPong));
        Arguments.Add(new PageArgument("Open Tetris", OpenPingPong));
        Arguments.Add(new PageArgument("Open Snake", OpenPingPong)); ;
        Arguments.Add(new PageArgument("Open Battery Tester", OpenPingPong));
        Arguments.Add(new PageArgument("Open Settings", OpenPingPong));
        Arguments.Add(new PageArgument("Exit", OpenPingPong));
    }
    #endregion

    #region Open Ping Pong
    public void OpenPingPong()
    {
        PageManager.Instance.SetActivePage(PingPongPage.Instance.Name);

    }
    #endregion
}
