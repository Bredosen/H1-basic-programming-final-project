using H1_basic_programming_final_project.Core.DataModels;

namespace H1_basic_programming_final_project.Core.DataModels;

public sealed class PageBuilder
{
    #region Members

    #region References
    public Page Page { get; init; }
    #endregion

    #region Properties
    public bool PrintTitle { get; set; } = true;
    public string Title { get; set; } = string.Empty;

    public bool RepeatPageCycle { get; set; } = true;

    public bool PrintDescription { get; set; } = true;
    public string Description { get; set; } = string.Empty;

    public bool PrintArguments { get; set; } = true;
    public List<PageArgument> PageArguments { get; set; } = [];

    public bool ClearPageAtShow { get; set; } = true;
    public bool AllowIndexAsPageArgument { get; set; } = true;

    public bool PrintLastArgumentFailedMessage { get; set; } = true;
    #endregion

    #endregion

    #region Constructor
    public PageBuilder(Page page)
    {
        Page = page;
    }
    #endregion

    #region Add Page Arguments
    public void AddPageArgument(string description, Action action, params string[] arguments)
    {
        PageArguments.Add(new PageArgument(action, description, arguments));
    }
    #endregion

    #region Add Exit Page Argument
    public void AddExitPageArgument()
    {
        AddPageArgument("Exit the page.", Page.Shutdown, "Exit", "Shutdown");


    }
    #endregion
}
