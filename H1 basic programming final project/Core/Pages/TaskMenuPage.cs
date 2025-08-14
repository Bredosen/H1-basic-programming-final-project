using H1_basic_programming_final_project.Core.DataModels;
using H1_basic_programming_final_project.Core.Manager;
using H1_basic_programming_final_project.Core.Services;
using H1_basic_programming_final_project.Core.Types;
using H1_basic_programming_final_project.Core.Utils;

namespace H1_basic_programming_final_project.Core.Pages;

#region Current Task Page Enum
public enum CurrentTaskPage
{
    Selector,
    AddTask,
    RemoveTask,
    FinishTask,
    ViewAllTasks
}
#endregion

public sealed class TaskMenuPage : LeftRightMenuPage
{
    #region Members

    #region Singleton
    private static readonly Lazy<TaskMenuPage> _instance = new(() => new TaskMenuPage(), true);
    public static readonly TaskMenuPage Instance = _instance.Value;
    #endregion

    #region Properties
    public CurrentTaskPage CurrentTaskPage { get; set; } = CurrentTaskPage.Selector;
    private readonly TextInputBuffer _taskInput = new();

    // List navigation state for Remove/Finish/View
    private int _listIndex = 0;
    private int _listScroll = 0;
    private const int ListW = 65;
    private const int ListH = 14; // total box height
    #endregion

    #endregion

    #region Constructor
    private TaskMenuPage() : base("TaskMenuPage")
    {
        Title = "Task Manager";
        Description = "Welcome to the task menu!";
        ArtFile = "Yoda";
        Arguments.Add(new PageArgument("Add a task", AddATask));
        Arguments.Add(new PageArgument("Remove a task", RemoveATask));
        Arguments.Add(new PageArgument("Finish a task", FinishATask));
        Arguments.Add(new PageArgument("View all tasks", ViewAllTasks));
        Arguments.Add(new PageArgument("Exit", PageManager.Instance.GoBackPage));
        OnKeyPressed += TaskMenuPage_OnKeyPressed;
        DeActivated += TaskMenuPage_DeActivated;
    }
    #endregion

    #region [Event] - De Activated.
    private void TaskMenuPage_DeActivated()
    {
        TaskManager.Instance.SaveTasks();
    }
    #endregion

    #region Base - Render Right Menu
    public override void RenderRightMenu(Rendere r)
    {
        switch (CurrentTaskPage)
        {
            case CurrentTaskPage.Selector:
                base.RenderRightMenu(r);
                break;
            case CurrentTaskPage.AddTask:
                RenderAddTaskMenu(r);
                break;
            case CurrentTaskPage.RemoveTask:
                RenderRemoveTaskMenu(r);
                break;
            case CurrentTaskPage.FinishTask:
                RenderFinishTaskMenu(r);
                break;
            case CurrentTaskPage.ViewAllTasks:
                RenderViewAllTasksMenu(r);
                break;
        }
    }
    #endregion

    #region Render Add Task Menu
    public void RenderAddTaskMenu(Rendere r)
    {
        // Header
        int headerW = 38, headerH = 5, headerX = (r.Width - headerW) / 2, headerY = 10;
        r.FillRect(headerX, headerY, headerW, headerH, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawRect(headerX, headerY, headerW, headerH, '#', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawText(r.Width / 2, headerY + 2, "Add a task", ConsoleColor.DarkGray, ConsoleColor.Green, HorizontalAlignment.Center);

        // Input
        int inputW = 65, inputH = 5, inputX = (r.Width - inputW) / 2, inputY = 25;
        r.FillRect(inputX, inputY - 2, inputW, inputH + 2, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawRect(inputX, inputY - 2, inputW, inputH + 2, '#', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawRect(inputX, inputY, inputW, inputH, '#', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawText(r.Width / 2, inputY - 1, "Enter your name for the task", ConsoleColor.DarkGray, ConsoleColor.Green, HorizontalAlignment.Center);

        int pad = 2;
        int maxVisible = inputW - (pad * 2);
        var (visible, _, _) = _taskInput.GetVisibleSegment(maxVisible);
        string toDraw = string.IsNullOrEmpty(_taskInput.Text) ? "|" : visible;
        r.DrawText(r.Width / 2, inputY + 2, toDraw, ConsoleColor.DarkGray, ConsoleColor.Gray, HorizontalAlignment.Center);

        // Press Escape
        DrawHintTopLeft(r, "Press Escape to go back");

        // Press Enter
        DrawHintBottomCentered(r, "Press Enter to continue");
    }
    #endregion

    #region Render Remove Task Menu
    public void RenderRemoveTaskMenu(Rendere r)
    {
        RenderListMenuCore(
            r,
            "Remove a task",
            TaskManager.Instance.GetList(),
            showStatus: true,
            footer: "Enter=Remove  ↑/↓=Select  Esc=Back"
        );
    }
    #endregion

    #region Render Finish Task Menu
    public void RenderFinishTaskMenu(Rendere r)
    {
        RenderListMenuCore(
            r,
            "Finish a task",
            TaskManager.Instance.GetList(),
            showStatus: true,
            footer: "Enter=Mark finished  ↑/↓=Select  Esc=Back"
        );
    }
    #endregion

    #region Render View All Tasks Menu
    public void RenderViewAllTasksMenu(Rendere r)
    {
        RenderListMenuCore(
            r,
            "All tasks",
            TaskManager.Instance.GetList(),
            showStatus: true,
            footer: "Esc=Back",
            readOnly: true
        );
    }
    #endregion

    #region List Menu Core
    private void RenderListMenuCore(
        Rendere r,
        string title,
        IReadOnlyList<DataModels.Task> tasks,
        bool showStatus,
        string footer,
        bool readOnly = false)
    {
        // Header
        int headerW = 38, headerH = 5, headerX = (r.Width - headerW) / 2, headerY = 10;
        r.FillRect(headerX, headerY, headerW, headerH, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawRect(headerX, headerY, headerW, headerH, '#', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawText(r.Width / 2, headerY + 2, title, ConsoleColor.DarkGray, ConsoleColor.Green, HorizontalAlignment.Center);

        // List box
        int listX = (r.Width - ListW) / 2;
        int listY = 21;
        r.FillRect(listX, listY, ListW, ListH, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawRect(listX, listY, ListW, ListH, '#', ConsoleColor.DarkGray, ConsoleColor.Black);

        // Empty state
        if (tasks.Count == 0)
        {
            r.DrawText(r.Width / 2, listY + (ListH / 2), "No tasks", ConsoleColor.DarkGray, ConsoleColor.Gray, HorizontalAlignment.Center);
            DrawHintTopLeft(r, "Press Escape to go back");
            DrawHintBottomCentered(r, footer);
            return;
        }

        // Clamp selection and scroll
        _listIndex = Clamp(_listIndex, 0, tasks.Count - 1);
        int innerRows = ListH - 2;
        EnsureVisible(innerRows, tasks.Count);

        // Render rows
        for (int row = 0; row < innerRows; row++)
        {
            int itemIdx = _listScroll + row;
            int y = listY + 1 + row;

            string line;
            if (itemIdx >= tasks.Count)
            {
                line = string.Empty;
            }
            else
            {
                var t = tasks[itemIdx];
                string status = showStatus ? (t.IsFinished ? "[x]" : "[ ]") : "   ";
                string name = TrimToWidth(t.Name ?? "(unnamed)", ListW - 6 - 4); // borders + status + spacing
                line = $"{status} {name}";
            }

            // Selected row highlight
            bool selected = itemIdx == _listIndex && !readOnly && itemIdx < tasks.Count;
            var fg = selected ? ConsoleColor.Black : ConsoleColor.Gray;
            var bg = selected ? ConsoleColor.Green : ConsoleColor.Black;

            r.DrawText(listX + 2, y, line, ConsoleColor.DarkGray, bg, HorizontalAlignment.Left);
            // Right-align index display
            string idxText = itemIdx < tasks.Count ? $"#{itemIdx + 1}" : "";
            r.DrawText(listX + ListW - 3, y, idxText, ConsoleColor.DarkGray, bg, HorizontalAlignment.Left);
        }

        // Hints
        DrawHintTopLeft(r, "Press Escape to go back");
        DrawHintBottomCentered(r, footer);
    }
    #endregion

    #region Helpers: UI hints and selection math
    private static void DrawHintTopLeft(Rendere r, string text)
    {
        int w = 10 + text.Length;
        r.FillRect(0, 0, w, 5, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawRect(0, 0, w, 5, '#', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawText(w / 2, 2, text, ConsoleColor.DarkGray, ConsoleColor.Green, HorizontalAlignment.Center);
    }

    private static void DrawHintBottomCentered(Rendere r, string text)
    {
        int w = 10 + text.Length;
        int x = r.Width / 2 - (w / 2);
        int y = 35;
        r.FillRect(x, y, w, 3, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawRect(x, y, w, 3, '#', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawText(r.Width / 2, y + 1, text, ConsoleColor.DarkGray, ConsoleColor.Green, HorizontalAlignment.Center);
    }

    private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);

    private void EnsureVisible(int innerRows, int count)
    {
        // keep _listIndex within [ _listScroll, _listScroll + innerRows - 1 ]
        if (_listIndex < _listScroll) _listScroll = _listIndex;
        if (_listIndex >= _listScroll + innerRows) _listScroll = _listIndex - innerRows + 1;

        // keep scroll valid
        int maxScroll = Math.Max(0, count - innerRows);
        _listScroll = Clamp(_listScroll, 0, maxScroll);
    }

    private static string TrimToWidth(string s, int max)
    {
        if (max <= 0) return string.Empty;
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Length <= max ? s : s.Substring(0, Math.Max(0, max - 1)) + "…";
    }
    #endregion

    #region Save Task
    private void SaveTask(string taskName)
    {
        if (string.IsNullOrWhiteSpace(taskName)) return;
        TaskManager.Instance.AddTask(new DataModels.Task(taskName));
    }
    #endregion

    #region [Event] - On Key Pressed.
    private void TaskMenuPage_OnKeyPressed(ushort key)
    {
        if (StopKeyPropagation) { StopKeyPropagation = false; return; }

        switch (CurrentTaskPage)
        {
            case CurrentTaskPage.AddTask:
                HandleAddTaskKeys(key);
                return;

            case CurrentTaskPage.RemoveTask:
                HandleListKeys(
                    key,
                    onEnter: () =>
                    {
                        var list = TaskManager.Instance.GetList();
                        if (list.Count == 0) return;
                        _listIndex = Clamp(_listIndex, 0, list.Count - 1);
                        var t = list[_listIndex];
                        TaskManager.Instance.RemoveTask(t);
                        if (_listIndex >= Math.Max(0, TaskManager.Instance.GetList().Count - 1))
                            _listIndex = Math.Max(0, _listIndex - 1);
                    });
                return;

            case CurrentTaskPage.FinishTask:
                HandleListKeys(
                    key,
                    onEnter: () =>
                    {
                        var list = TaskManager.Instance.GetList();
                        if (list.Count == 0) return;
                        _listIndex = Clamp(_listIndex, 0, list.Count - 1);
                        var task = list[_listIndex];
                        task.Finish(!task.IsFinished);
                    });
                return;

            case CurrentTaskPage.ViewAllTasks:
                if (key == VirtuelKeys.ESCAPE) { GoToSelector(); return; }
                // read-only
                return;

            default:
                // In selector we let base navigation handle arrows/numbers/enter.
                if (key == VirtuelKeys.ESCAPE) { GoToSelector(); }
                return;
        }
    }
    #endregion

    #region Key Handlers
    private void HandleAddTaskKeys(ushort key)
    {
        switch (key)
        {
            case VirtuelKeys.ESCAPE:
                _taskInput.Reset();
                GoToSelector();
                return;

            case VirtuelKeys.RETURN:
                if (string.IsNullOrWhiteSpace(_taskInput.Text)) break;
                SaveTask(_taskInput.Text);
                _taskInput.Reset();
                GoToSelector();
                return;

            case VirtuelKeys.BACK:
            case VirtuelKeys.DELETE:
            case VirtuelKeys.LEFT:
            case VirtuelKeys.RIGHT:
            case VirtuelKeys.HOME:
            case VirtuelKeys.END:
            default:
                if (_taskInput.HandleKey(key)) UpdateExists = true;
                return;
        }
    }

    private void HandleListKeys(ushort key, Action onEnter)
    {
        var list = TaskManager.Instance.GetList();
        int count = list.Count;

        if (key == VirtuelKeys.ESCAPE) { GoToSelector(); return; }
        if (count == 0) { UpdateExists = true; return; }

        int innerRows = ListH - 2;

        switch (key)
        {
            case VirtuelKeys.UP:
                _listIndex = Clamp(_listIndex - 1, 0, count - 1);
                EnsureVisible(innerRows, count);
                UpdateExists = true;
                return;

            case VirtuelKeys.DOWN:
                _listIndex = Clamp(_listIndex + 1, 0, count - 1);
                EnsureVisible(innerRows, count);
                UpdateExists = true;
                return;

            case VirtuelKeys.HOME:
                _listIndex = 0;
                EnsureVisible(innerRows, count);
                UpdateExists = true;
                return;

            case VirtuelKeys.END:
                _listIndex = count - 1;
                EnsureVisible(innerRows, count);
                UpdateExists = true;
                return;

            case VirtuelKeys.RETURN:
            case VirtuelKeys.DELETE:
                onEnter();
                // refresh bounds after mutation
                count = TaskManager.Instance.GetList().Count;
                _listIndex = Clamp(_listIndex, 0, Math.Max(0, count - 1));
                EnsureVisible(innerRows, count);
                UpdateExists = true;
                return;

            default:
                return;
        }
    }
    #endregion

    #region Go to selector
    public void GoToSelector()
    {
        CurrentTaskPage = CurrentTaskPage.Selector;
        ListenForArrows = true;
        ListenForEnter = true;
        ListenForNumbers = true;
        SelectedArgumentColor = ConsoleColor.Green;
        _listIndex = 0;
        _listScroll = 0;
        UpdateExists = true;
    }
    #endregion

    #region Add a task
    public void AddATask()
    {
        CurrentTaskPage = CurrentTaskPage.AddTask;
        ListenForArrows = false;
        ListenForEnter = false;
        ListenForNumbers = false;
        _taskInput.Reset();
        SelectedArgumentColor = ConsoleColor.Gray;
        UpdateExists = true;
    }
    #endregion

    #region Remove a task
    public void RemoveATask()
    {
        CurrentTaskPage = CurrentTaskPage.RemoveTask;
        ListenForArrows = false;
        ListenForEnter = false;
        ListenForNumbers = false;
        SelectedArgumentColor = ConsoleColor.Gray;
        _listIndex = 0;
        _listScroll = 0;
        UpdateExists = true;
    }
    #endregion

    #region Finish a task
    public void FinishATask()
    {
        CurrentTaskPage = CurrentTaskPage.FinishTask;
        ListenForArrows = false;
        ListenForEnter = false;
        ListenForNumbers = false;
        SelectedArgumentColor = ConsoleColor.Gray;
        _listIndex = 0;
        _listScroll = 0;
        UpdateExists = true;
    }
    #endregion

    #region View all tasks
    public void ViewAllTasks()
    {
        CurrentTaskPage = CurrentTaskPage.ViewAllTasks;
        ListenForArrows = false;
        ListenForEnter = false;
        ListenForNumbers = false;
        SelectedArgumentColor = ConsoleColor.Gray;
        _listIndex = 0;
        _listScroll = 0;
        UpdateExists = true;
    }
    #endregion
}
