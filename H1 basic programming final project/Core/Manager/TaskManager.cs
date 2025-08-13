using H1_basic_programming_final_project.Core.Types;

namespace H1_basic_programming_final_project.Core.Manager;

public sealed class TaskManager
{
    #region Members

    #region Singleton
    private static readonly Lazy<TaskManager> _instance = new(() => new(), true);
    public static readonly TaskManager Instance = _instance.Value;
    #endregion

    #region Constants
    public int MaxTasks { get; } = 5;
    public string FilePath = @"C:\Src\School\H1 basic programming final project\H1 basic programming final project\DataBase/Tasks.json";
    #endregion

    #region Properties
    private readonly List<DataModels.Task> Tasks = [];
    #endregion

    #endregion

    #region Constructor
    private TaskManager()
    {

    }
    #endregion

    #region Add Task
    public ErrorCode AddTask(DataModels.Task task)
    {
        if (task is null)
        {
            return ErrorCode.NoSuccess;
        }

        if (Tasks.Count >= MaxTasks)
        {
            return ErrorCode.ToManyElements;
        }

        Tasks.Add(task);
        return ErrorCode.Success;
    }
    #endregion

    #region Remove Task by instance.
    public ErrorCode RemoveTask(DataModels.Task task)
    {
        if (task is null)
        {
            return ErrorCode.NoSuccess;
        }

        if (Tasks.Contains(task) != true)
        {
            return ErrorCode.CouldNotFindElement;
        }

        _ = Tasks.Remove(task);
        return ErrorCode.Success;
    }
    #endregion

    #region Remove Task by name
    public ErrorCode RemoveTask(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return ErrorCode.NoSuccess;
        }

        DataModels.Task? task = RetrieveTask(name);
        if (task is null)
        {
            return ErrorCode.CouldNotFindElement;
        }

        return RemoveTask(task);
    }
    #endregion

    #region Retrieve Task by index
    public DataModels.Task? RetrieveTask(int index)
    {
        if (index < 0 || index >= Tasks.Count)
        {
            return null;
        }
        return Tasks[index];
    }
    #endregion

    #region Retrieve Task by name   
    public DataModels.Task? RetrieveTask(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return Tasks.FirstOrDefault(task => string.Equals(task.Name, name, StringComparison.CurrentCultureIgnoreCase));
    }
    #endregion

    #region Exists by name
    public bool Exists(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        return Tasks.Any(task => string.Equals(task.Name, name, StringComparison.CurrentCultureIgnoreCase));
    }
    #endregion

    #region Get Enumrator
    public IEnumerator<DataModels.Task> GetEnumerator()
    {
        return Tasks.GetEnumerator();
    }
    #endregion

    #region Get List
    public List<DataModels.Task> GetList()
    {
        return [.. Tasks];
    }
    #endregion

    #region Save Tasks
    public void SaveTasks()
    {
        if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(FilePath) ?? string.Empty);
        }
        try
        {
            string json = System.Text.Json.JsonSerializer.Serialize(Tasks);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception exception)
        {
            //  COut.WriteLine($"Error saving tasks: '{exception}'");
        }
    }
    #endregion

    #region Load Tasks
    public void LoadTasks()
    {
        if (!File.Exists(FilePath))
        {
            return;
        }
        try
        {
            string json = File.ReadAllText(FilePath);
            Tasks.Clear();
            Tasks.AddRange(System.Text.Json.JsonSerializer.Deserialize<List<DataModels.Task>>(json) ?? []);
        }
        catch (Exception exception)
        {
            //  COut.WriteLine($"Error saving tasks: '{exception}'");
        }
    }
    #endregion
}
