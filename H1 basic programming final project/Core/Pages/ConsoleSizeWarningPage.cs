using H1_basic_programming_final_project.Core.DataModels;
using H1_basic_programming_final_project.Core.Manager;
using H1_basic_programming_final_project.Core.Services;

namespace H1_basic_programming_final_project.Core.Pages;

public class ConsoleSizeWarningPage : Page
{
    #region Memebers

    #region Singleton
    private static readonly Lazy<ConsoleSizeWarningPage> _instance = new(() => new ConsoleSizeWarningPage(), true);
    public static readonly ConsoleSizeWarningPage Instance = _instance.Value;
    #endregion

    #region Properties
    public bool UpdateRender = false;
    public ConsoleColor BackgroundText = ConsoleColor.DarkRed;
    public ConsoleColor ForegroundText = ConsoleColor.White;
    #endregion

    #endregion

    #region Constructor
    private ConsoleSizeWarningPage() : base(nameof(ConsoleSizeWarningPage))
    {
        PageManager.Instance.ConsoleResized += Instance_ConsoleResized;
    }
    #endregion

    #region [Event] - Console Resized
    private void Instance_ConsoleResized()
    {
        UpdateRender = true;
    }
    #endregion


    #region Render
    public override void Render()
    {
        COut.Clear();
        AddText("Please Resize your window!", -4);
        AddText($"Current Buffer Size: [{PageManager.Instance.BufferWidth} - {PageManager.Instance.BufferHeight}].", -2);
        AddText($"Required Buffer Width: [{PageManager.Instance.RequiredMinBufferWidth} - {PageManager.Instance.RequiredMaxBufferWidth}].", 0);
        AddText($"Required Buffer Height: [{PageManager.Instance.RequiredMinBufferHeight} - {PageManager.Instance.RequiredMaxBufferHeight}].", 2);
        AddText("Please make Console fullscreen!", 4);
        COut.Render();
        while (UpdateRender != true)
        {
            Thread.Sleep(50);
        }
        UpdateRender = false;
    }
    #endregion


    #region Add Text
    public void AddText(string text, int yOffset)
    {
        COut.DrawText(Width / 2, Height / 2 + yOffset, text, BackgroundText, ForegroundText, Types.HorizontalAlignment.Center);
    }
    #endregion
}
