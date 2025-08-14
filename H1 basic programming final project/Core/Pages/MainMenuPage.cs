using H1_basic_programming_final_project.Core.DataModels;
using H1_basic_programming_final_project.Core.Manager;

namespace H1_basic_programming_final_project.Core.Pages;

public sealed class MainMenuPage : LeftRightMenuPage
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
        ArtFile = "HornyOperatingSystem";
        Arguments.Add(new PageArgument("Open Task Manager", () => PageManager.Instance.SetActivePage(TaskMenuPage.Instance.Name)));
        Arguments.Add(new PageArgument("Open Ping Pong", () => PageManager.Instance.SetActivePage(PingPongPage.Instance.Name)));
        Arguments.Add(new PageArgument("Open Tetris", () => { }));
        Arguments.Add(new PageArgument("Open Snake", () => { })); ;
        Arguments.Add(new PageArgument("Open Battery Tester", () => { }));
        Arguments.Add(new PageArgument("Open Settings", () => { }));
        Arguments.Add(new PageArgument("Exit", PageManager.Instance.GoBackPage));
    }
    #endregion
}
