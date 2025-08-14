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
        Activated += MainMenuPage_Activated;
        Arguments.Add(new PageArgument("Open Task Manager", () => PageManager.Instance.SetActivePage(TaskMenuPage.Instance.Name)));
        Arguments.Add(new PageArgument("Open Ping Pong", () => PageManager.Instance.SetActivePage(PingPongPage.Instance.Name)));
        Arguments.Add(new PageArgument("Open Tetris (W.I.P)", () => { }));
        Arguments.Add(new PageArgument("Open Snake (W.I.P)", () => { })); ;
        Arguments.Add(new PageArgument("Open Battery Tester (W.I.P)", () => { }));
        Arguments.Add(new PageArgument("Open Settings (W.I.P)", () => { }));
        Arguments.Add(new PageArgument("Exit", PageManager.Instance.GoBackPage));
    }
    #endregion

    #region [Event] - Activated
    private void MainMenuPage_Activated()
    {
        ArtFile = AsciiManager.GetRandom();
    }
    #endregion
}
