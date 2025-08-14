using System.Text.Json.Serialization;

namespace H1_basic_programming_final_project.Core.DataModels;

public sealed class Task
{
    public string Name { get; private set; }
    public bool IsFinished { get; private set; }

    [JsonConstructor]
    public Task(string name, bool isFinished)
    {
        Name = name;
        IsFinished = isFinished;
    }

    #region Finish
    public void Finish(bool finished)
    {
        IsFinished = finished;
    }
    #endregion

    #region To String
    public override string ToString()
    {
        return $"{Name} - {(IsFinished ? "Finished" : "Not Finished")}";
    }
    #endregion

}
