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
        UpdateExists = true;
    }
    #endregion

    #region Render
    public override void Render(Rendere rendere)
    {
        AddText(rendere, "Please Resize your window!", -4);
        AddText(rendere, $"Current Buffer Size: [{PageManager.Instance.BufferWidth} - {PageManager.Instance.BufferHeight}].", -2);
        AddText(rendere, $"Required Buffer Width: [{PageManager.Instance.RequiredMinBufferWidth} - {PageManager.Instance.RequiredMaxBufferWidth}].", 0);
        AddText(rendere, $"Required Buffer Height: [{PageManager.Instance.RequiredMinBufferHeight} - {PageManager.Instance.RequiredMaxBufferHeight}].", 2);
        AddText(rendere, "Please make Console fullscreen!", 4);
    }
    #endregion

    #region Add Text
    public void AddText(Rendere rendere, string text, int yOffset)
    {
        rendere.DrawText(Width / 2, (Height / 2) + yOffset, text, BackgroundText, ForegroundText, Types.HorizontalAlignment.Center);
    }
    #endregion
}
