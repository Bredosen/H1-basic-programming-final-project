using H1_basic_programming_final_project.Core.DataModels;

namespace H1_basic_programming_final_project.Core.Pages;

public sealed class MainMenuPage : Page
{
    #region Singleton
    private static readonly Lazy<MainMenuPage> _instance = new(() => new MainMenuPage(), true);
    public static readonly MainMenuPage Instance = _instance.Value;
    #endregion

    #region Constructor
    private MainMenuPage()
    {

    }
    #endregion

    #region Build
    public override void Build(PageBuilder pageBuilder)
    {
        pageBuilder.PrintTitle = true;
        pageBuilder.Title = "Main Menu";

        pageBuilder.PrintDescription = true;
        pageBuilder.Description = "Welcome to the Main Menu of the H1 Basic Programming Final Project.";

        pageBuilder.RepeatPageCycle = true;

        pageBuilder.AddPageArgument("Go to Task Manager", TaskManagerPage.Instance.Show, "Task Manager", "TM");
        pageBuilder.AddPageArgument("Go Ping Pong", PingPongPage.Instance.Show, "Ping Pong", "PP");
        pageBuilder.AddExitPageArgument();
    }
    #endregion
}
