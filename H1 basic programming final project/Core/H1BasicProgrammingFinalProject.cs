using H1_basic_programming_final_project.Core.Manager;
using H1_basic_programming_final_project.Core.Pages;

namespace H1_basic_programming_final_project.Core;

public sealed class H1BasicProgrammingFinalProject
{
    #region Singleton
    private static readonly Lazy<H1BasicProgrammingFinalProject> _instance = new Lazy<H1BasicProgrammingFinalProject>(() => new H1BasicProgrammingFinalProject(), true);
    public static readonly H1BasicProgrammingFinalProject Instance = _instance.Value;
    #endregion

    #region Constructor
    public H1BasicProgrammingFinalProject()
    {

    }
    #endregion

    #region [Entry Point] - Main Start
    public static void Main(string[] args)
    {
        Instance.Initialize();
        Instance.Terminate();
    }
    #endregion

    #region Initialize
    public void Initialize()
    {
        TaskManager.Instance.LoadTasks();
        MainMenuPage.Instance.Show();
    }
    #endregion

    #region Terminate
    public void Terminate()
    {
        TaskManager.Instance.SaveTasks();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
    #endregion
}
