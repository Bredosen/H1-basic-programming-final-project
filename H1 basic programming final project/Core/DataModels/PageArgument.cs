using System.Text;

namespace H1_basic_programming_final_project.Core.DataModelsM;

public sealed class PageArgument
{
    #region Properties
    public readonly List<string> Arguments = [];
    public readonly string Description;
    public readonly Action Action;
    #endregion

    #region Constructor
    public PageArgument(Action action, string description, params string[] arguments)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action), "Action cannot be null.");
        Description = description ?? throw new ArgumentNullException(nameof(description), "Description cannot be null.");
        Arguments.AddRange(arguments);
    }
    #endregion

    #region To String
    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append('[');
        stringBuilder.Append(string.Join(", ", Arguments));
        stringBuilder.Append("] - '");
        stringBuilder.Append(Description);
        stringBuilder.Append("'.");
        return stringBuilder.ToString();
    }
    #endregion
}
