using H1_basic_programming_final_project.Core.DataModels;
using H1_basic_programming_final_project.Core.Manager;
using H1_basic_programming_final_project.Core.Services;
using H1_basic_programming_final_project.Core.Types;

namespace H1_basic_programming_final_project.Core.Pages;

public sealed class TaskManagerPage : Page
{
    #region Singleton
    private static readonly Lazy<TaskManagerPage> _instance = new(() => new TaskManagerPage(), true);
    public static readonly TaskManagerPage Instance = _instance.Value;
    #endregion

    #region Constructor
    private TaskManagerPage() : base("TaskManagerPage")
    {

    }
    #endregion

    public override void Render()
    {

    }

    /*
    #region Build
    public void Build(PageBuilder pageBuilder)
    {
        pageBuilder.PrintTitle = true;
        pageBuilder.Title = "Task Manager";

        pageBuilder.PrintDescription = true;
        pageBuilder.Description = "Welcome to the Task Manager of the H1 Basic Programming Final Project.";

        pageBuilder.RepeatPageCycle = true;

        pageBuilder.AddPageArgument("Add a task", AddTask, "Add Task", "Add");
        pageBuilder.AddPageArgument("Finish a task", FinishTask, "Finish Task", "Finish");
        pageBuilder.AddPageArgument("Remove a task", RemoveTask, "Remove Task", "Remove");
        pageBuilder.AddPageArgument("See all tasks", ListTasks, "List Tasks", "List");
        pageBuilder.AddPageArgument("Save tasks", SaveTasks, "Save Tasks", "Save");
        pageBuilder.AddExitPageArgument();
    }
    #endregion

    #region Save Tasks
    public void SaveTasks()
    {
        TaskManager.Instance.SaveTasks();
        COut.WriteLine("Tasks have been saved successfully.");
        COut.WaitForContinue();
    }
    #endregion

    #region Add Task
    public void AddTask()
    {
        
        COut.Space();
        COut.WriteLine("Please write the name of the new task");

        string inputArgument = COut.GetUserInput("[Name] >> ");
        if (TaskManager.Instance.Exists(inputArgument))
        {
            COut.WriteLine($"A task with the name '{inputArgument}' already exists.");
            COut.Space();
            COut.WaitForContinue();
            return;
        }

        switch (TaskManager.Instance.AddTask(new DataModels.Task(inputArgument)))
        {
            case ErrorCode.Success:
                COut.WriteLine($"Task '{inputArgument}' has been added successfully.");
                break;
            case ErrorCode.ToManyElements:
                COut.WriteLine("You have reached the maximum number of tasks.");
                break;
            case ErrorCode.NoSuccess:
            default:
                COut.WriteLine("An error occurred while adding the task.");
                break;
        }

        COut.Space();
        COut.WaitForContinue();
    }
    #endregion

    #region Finish Task
    public void FinishTask()
    {
        COut.WriteLine("Please write the name of the task you want to finish");
        string inputArgument = COut.GetUserInput("[Name] >> ");
        if (TaskManager.Instance.Exists(inputArgument) is false)
        {
            COut.WriteLine($"No task with the name '{inputArgument}' exists.");
            COut.WriteLine($"Type \"List Tasks\", to see all available tasks.");
            COut.Space();
            COut.WaitForContinue();
            return;
        }

        if (TaskManager.Instance.RetrieveTask(inputArgument) is DataModels.Task task)
        {
            task.Finish();
            COut.WriteLine($"Task '{inputArgument}' has been finished successfully.");
        }
        else
        {
            COut.WriteLine($"An error occurred while finishing the task '{inputArgument}'.");
        }

        COut.Space();
        COut.WaitForContinue();
    }
    #endregion

    #region Remove Task
    public void RemoveTask()
    {
        COut.WriteLine("Please write the name of the task you want to remove");
        string inputArgument = COut.GetUserInput("[Name] >> ");
        if (TaskManager.Instance.Exists(inputArgument) is false)
        {
            COut.WriteLine($"No task with the name '{inputArgument}' exists.");
            COut.WriteLine($"Type \"List Tasks\", to see all available tasks.");
            COut.Space();
            COut.WaitForContinue();
            return;
        }

        switch (TaskManager.Instance.RemoveTask(inputArgument))
        {
            case ErrorCode.Success:
                COut.WriteLine($"Task '{inputArgument}' has been removed successfully.");
                break;
            case ErrorCode.CouldNotFindElement:
                COut.WriteLine($"Could not find the task '{inputArgument}' to remove.");
                break;
            case ErrorCode.NoSuccess:
            default:
                COut.WriteLine($"An error occurred while removing the task '{inputArgument}'.");
                break;
        }

        COut.Space();
        COut.WaitForContinue();
    }
    #endregion

    #region List Tasks
    public void ListTasks()
    {
        COut.Space();
        COut.Header("Tasks", 10);
        List<DataModels.Task> list = TaskManager.Instance.GetList();
        list.Sort((a, b) => (a.IsFinished ? 1 : 0).CompareTo(b.IsFinished ? 1 : 0));
        COut.WriteList(list);
        COut.Space();
        COut.WaitForContinue();
    }
    #endregion

    */
}
