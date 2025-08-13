namespace H1_basic_programming_final_project.Core.DataModels;

public sealed class PageArgument(string name, Action action)
{
    #region Properties
    public readonly string Name = name;
    public readonly Action Action = action ?? throw new ArgumentNullException(nameof(action), "Action cannot be null.");
    #endregion
}
