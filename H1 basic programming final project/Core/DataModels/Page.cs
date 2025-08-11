using H1_basic_programming_final_project.Core.Services;

namespace H1_basic_programming_final_project.Core.DataModels;

public abstract class Page
{
    #region Properties
    public bool PrintTitle { get; private set; } = true;
    public string Title { get; private set; } = string.Empty;

    public bool RepeatPageCycle { get; private set; } = true;

    public bool PrintDescription { get; private set; } = true;
    public string Description { get; private set; } = string.Empty;

    public bool PrintArguments { get; private set; } = true;
    public List<PageArgument> PageArguments { get; private set; } = [];

    public bool ClearPageAtShow { get; private set; } = true;
    public bool AllowIndexAsPageArgument { get; private set; } = true;

    public bool PrintLastArgumentFailedMessage { get; private set; } = true;

    public bool Running { get; private set; } = true;
    #endregion

    #region Show
    public void Show()
    {
        PageBuilder pageBuilder = new(this);
        Build(pageBuilder);
        ParseBuilder(pageBuilder);
        ActivatePage();
    }
    #endregion

    #region Activate Page
    private void ActivatePage()
    {
        Running = true;
        bool lastArgumentFailed = false;
        do
        {
            if (ClearPageAtShow)
            {
                COut.Clear();
            }

            if (PrintTitle)
            {
                COut.Header(Title.Length, Title);
                COut.Space();
            }

            if (PrintDescription)
            {
                COut.SetColor(ConsoleColor.Gray);
                COut.WriteLine(Description);
                COut.ResetColor();
                COut.Space();
            }

            if (PrintArguments)
            {
                COut.WriteList(PageArguments);
                COut.Space();
            }

            if (PrintLastArgumentFailedMessage && lastArgumentFailed)
            {
                lastArgumentFailed = false;
                COut.SetColor(ConsoleColor.Red);
                COut.WriteLine("Invalid Input. Please try again.");
                COut.ResetColor();
                COut.Space();
            }

            string inputArgument = COut.GetUserInput(" >> ");
            if (RetrievePageArgument(inputArgument) is not PageArgument pageArgument)
            {
                lastArgumentFailed = true;
                continue;
            }

            pageArgument.Action?.Invoke();
        } while (RepeatPageCycle && Running);
    }
    #endregion

    #region Shutdown
    public void Shutdown()
    {
        Running = false;
    }
    #endregion

    #region Retrieve Page Argument
    /// <summary>
    /// This method retrieves a PageArgument based on the provided input arguments.
    /// </summary>
    /// <param name="inputArguments"></param>
    /// <returns>PageArgument</returns>
    public PageArgument? RetrievePageArgument(string inputArguments)
    {
        // If the inputArguments is null or empty, we return null.
        if (string.IsNullOrEmpty(inputArguments))
        {
            return null;
        }

        // Check if Index as page argument is allowed.
        // After this we try to parse the input as an index.
        // Lastly, we check if the index is within the bounds of the PageArguments list.
        if (AllowIndexAsPageArgument && int.TryParse(inputArguments, out int indexValue) && indexValue >= 0 && indexValue < PageArguments.Count)
        {
            // Return the PageArgument at the specified index.
            return PageArguments[indexValue];
        }

        // Here we use LinQ to first the first PageArgument that contains the inputArguments in its Arguments list.
        // When we check for the input arguments, we use StringComparison.CurrentCultureIgnoreCase to ensure case-insensitive comparison.
        if (PageArguments?.FirstOrDefault(argument => argument?.Arguments?.Any(arg => string.Equals(arg, inputArguments, StringComparison.CurrentCultureIgnoreCase)) == true) is not PageArgument pageArgument)
        {
            return null;
        }

        // If we found a matching PageArgument, we return it.
        return pageArgument;
    }
    #endregion

    #region Parase Builder
    private void ParseBuilder(PageBuilder pageBuilder)
    {
        PrintTitle = pageBuilder?.PrintTitle ?? true;
        Title = pageBuilder?.Title ?? string.Empty;

        PrintDescription = pageBuilder?.PrintDescription ?? true;
        Description = pageBuilder?.Description ?? string.Empty;

        PrintArguments = pageBuilder?.PrintArguments ?? true;
        PageArguments = pageBuilder?.PageArguments ?? [];

        RepeatPageCycle = pageBuilder?.RepeatPageCycle ?? true;

        ClearPageAtShow = pageBuilder?.ClearPageAtShow ?? true;
        AllowIndexAsPageArgument = pageBuilder?.AllowIndexAsPageArgument ?? true;

        PrintLastArgumentFailedMessage = pageBuilder?.PrintLastArgumentFailedMessage ?? true;
    }
    #endregion

    #region Abstract Builder Method
    public abstract void Build(PageBuilder pageBuilder);
    #endregion
}
