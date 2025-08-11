namespace H1_basic_programming_final_project.Core.DataModels;

public sealed class Task(string name)
{
    #region Properties
    public string Name { get; private set; } = name ?? throw new ArgumentNullException(nameof(name), "Task name cannot be null.");
    public bool IsFinished { get; private set; } = false;
    #endregion

    #region Finish
    public void Finish()
    {
        IsFinished = true;
    }
    #endregion

    #region To String
    public override string ToString()
    {
        return $"{Name} - {(IsFinished ? "Finished" : "Not Finished")}";
    }
    #endregion

}
