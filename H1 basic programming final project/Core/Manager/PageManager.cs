using H1_basic_programming_final_project.Core.DataModels;
using H1_basic_programming_final_project.Core.Handler;
using H1_basic_programming_final_project.Core.Helper;
using H1_basic_programming_final_project.Core.Pages;
using H1_basic_programming_final_project.Core.Services;

namespace H1_basic_programming_final_project.Core.Manager;

public sealed class PageManager
{
    #region Members

    #region Singleton
    private static readonly Lazy<PageManager> _instance = new(() => new PageManager(), true);
    public static readonly PageManager Instance = _instance.Value;
    #endregion

    #region Properties
    public readonly Dictionary<string, Page> Pages = [];
    public List<Page> PageHistory = [];
    public Page? ActivePage { get; private set; }
    public bool DisplayConsoleSizeWarning { get; set; } = false;
    public int BufferWidth { get; private set; } = Console.BufferWidth;
    public int BufferHeight { get; private set; } = Console.BufferHeight;
    public bool IsFullscreen { get; private set; } = false;

    public bool RequireFullscreen { get; set; } = true;
    public int RequiredMinBufferWidth { get; set; } = 200;
    public int RequiredMaxBufferWidth { get; set; } = 210;
    public int RequiredMinBufferHeight { get; set; } = 60;
    public int RequiredMaxBufferHeight { get; set; } = 70;
    #endregion

    #region Events
    public event Action? ConsoleResized;
    #endregion

    #endregion

    #region Constructor
    public PageManager() { }
    #endregion

    #region Initialize
    public void Initialize()
    {
        Pages.Add(ConsoleSizeWarningPage.Instance.Name, ConsoleSizeWarningPage.Instance);
        Pages.Add(MainMenuPage.Instance.Name, MainMenuPage.Instance);
        Pages.Add(PingPongPage.Instance.Name, PingPongPage.Instance);
        Pages.Add(TaskMenuPage.Instance.Name, TaskMenuPage.Instance);

        Rendere.Initialize();
        Console.CursorVisible = false;
        _ = System.Threading.Tasks.Task.Run(StartResizeListener);
        _ = System.Threading.Tasks.Task.Run(RawInput.Poll);
        ConsoleResized += ConsoleResizedHandler;
    }
    #endregion

    #region Console Resized
    public void ConsoleResizedHandler()
    {
        if (RequireFullscreen != IsFullscreen || BufferWidth < RequiredMinBufferWidth || BufferWidth > RequiredMaxBufferWidth || BufferHeight < RequiredMinBufferHeight || BufferHeight > RequiredMaxBufferHeight)
        {
            DisplayConsoleSizeWarning = true;
            return;
        }

        DisplayConsoleSizeWarning = false;
    }
    #endregion

    #region Set Active Page
    public void SetActivePage(string pageName)
    {
        if (Pages.TryGetValue(pageName, out Page? page))
        {
            PageHistory.Add(page);
            ActivePage = page;
            ActivePage.Activate();
            ConsoleResizedHandler();
        }
    }
    #endregion

    #region Go Back a page
    public void GoBackPage()
    {
        if (PageHistory.Count > 0)
        {
            PageHistory.LastOrDefault()?.DeActivate();
            PageHistory.RemoveAt(PageHistory.Count - 1);
            if (PageHistory.Count > 0)
            {
                ActivePage = PageHistory[^1];
                ActivePage.Activate();
                ConsoleResizedHandler();
            }
            else
            {
                ActivePage = null;
                Environment.Exit(0);
            }
        }
    }
    #endregion

    #region Start Resize Listener
    public void StartResizeListener()
    {
        bool lastIsFullscreen = false;
        int lastBufferWidth = 0;
        int lastBufferHeight = 0;
        while (true)
        {
            IsFullscreen = ConsoleHelper.IsWindowFullscreen(title: Console.Title);
            if (lastIsFullscreen != IsFullscreen)
            {
                lastIsFullscreen = IsFullscreen;
                ConsoleResized?.Invoke();
            }

            BufferWidth = Console.BufferWidth;
            BufferHeight = Console.BufferHeight;
            if (lastBufferWidth != BufferWidth || lastBufferHeight != BufferHeight)
            {
                lastBufferWidth = BufferWidth;
                lastBufferHeight = BufferHeight;
                ConsoleResized?.Invoke();
            }
            _ = System.Threading.Tasks.Task.Delay(1000);
        }
    }
    #endregion

    #region Terminate
    public void Terminate()
    {
        Pages.Clear();
    }
    #endregion

    #region Run
    public void Run()
    {
        ConsoleResizedHandler();
        while (true)
        {
            Page? page = DisplayConsoleSizeWarning ? ConsoleSizeWarningPage.Instance : ActivePage;
            if (page == null)
            {
                //    COut.WriteLine("No active page to run.");
                return;
            }

            page.RenderPage();
        }
    }
    #endregion
}
