using H1_basic_programming_final_project.Core.DataModels;
using H1_basic_programming_final_project.Core.Manager;
using H1_basic_programming_final_project.Core.Services;
using H1_basic_programming_final_project.Core.Types;
using System.Runtime.InteropServices;
using System.Text;

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
    private string _input = string.Empty;
    private int _caret = 0; // index in _input
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
    }
    #endregion

    #region Base - Render Right Menu
    public override void RenderRightMenu(Rendere rendere)
    {
        switch (CurrentTaskPage)
        {
            case CurrentTaskPage.Selector:
                base.RenderRightMenu(rendere);
                break;
            case CurrentTaskPage.AddTask:
                RenderAddTaskMenu(rendere);
                break;
            case CurrentTaskPage.RemoveTask:
                break;
            case CurrentTaskPage.FinishTask:
                break;
            case CurrentTaskPage.ViewAllTasks:
                break;
        }
    }
    #endregion

    public void RenderAddTaskMenu(Rendere r)
    {
        int headerW = 38, headerH = 5, headerX = (r.Width - headerW) / 2, headerY = 10;
        r.FillRect(headerX, headerY, headerW, headerH, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawRect(headerX, headerY, headerW, headerH, '#', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawText(r.Width / 2, headerY + 2, "Add a task", ConsoleColor.DarkGray, ConsoleColor.Gray, HorizontalAlignment.Center);

        int inputW = 65, inputH = 5, inputX = (r.Width - inputW) / 2, inputY = 25;
        r.FillRect(inputX, inputY, inputW, inputH, ' ', ConsoleColor.DarkGray, ConsoleColor.Black);
        r.DrawRect(inputX, inputY, inputW, inputH, '#', ConsoleColor.DarkGray, ConsoleColor.Black);

        // Text area inside the box
        int pad = 2;
        int maxVisible = inputW - (pad * 2);
        if (maxVisible < 1) maxVisible = 1;

        // Horizontal scroll if text exceeds box
        int start = 0;
        if (_caret > maxVisible) start = _caret - maxVisible;
        if (_input.Length - start > maxVisible) { /* keep windowed */ }
        else if (_input.Length > maxVisible) start = _input.Length - maxVisible;

        string visible = _input.Length == 0 ? "____________" :
                         _input.Substring(start, Math.Min(_input.Length - start, maxVisible));

        r.DrawText(inputX + pad, inputY + 2, visible, ConsoleColor.DarkGray, ConsoleColor.Gray, HorizontalAlignment.Left);

        // Caret position (use console cursor for visual caret; not for input)
        int caretColumn = inputX + pad + Math.Min(_caret - start, maxVisible);
        caretColumn = Math.Clamp(caretColumn, inputX + pad, inputX + pad + maxVisible);
        Console.SetCursorPosition(caretColumn, inputY + 2);
    }
    private void SaveTask(string task)
    {
        if (!string.IsNullOrWhiteSpace(task))
        {
            // TODO: integrate with your task manager
            // TaskManager.Instance.Add(new TaskModel { Title = task.Trim() });
        }
    }

    private void TaskMenuPage_OnKeyPressed(ushort obj)
    {
        if (CurrentTaskPage != CurrentTaskPage.AddTask)
        {
            if (obj == VirtuelKeys.ESCAPE) GoToSelector();
            return;
        }

        switch (obj)
        {
            case VirtuelKeys.ESCAPE:
                _input = string.Empty; _caret = 0; GoToSelector(); return;

            case VirtuelKeys.RETURN:
                if (string.IsNullOrEmpty(_input)) break;
                SaveTask(_input); _input = string.Empty; _caret = 0; GoToSelector(); return;

            case VirtuelKeys.BACK:
                if (_caret > 0)
                {
                    _input = _input.Remove(_caret - 1, 1);
                    _caret--;
                    UpdateExists = true;
                }
                return;

            case VirtuelKeys.DELETE:
                if (_caret < _input.Length)
                {
                    _input = _input.Remove(_caret, 1);
                    UpdateExists = true;
                }
                return;

            case VirtuelKeys.LEFT:
                if (_caret > 0) _caret--;
                return;

            case VirtuelKeys.RIGHT:
                if (_caret < _input.Length) _caret++;
                return;

            case VirtuelKeys.HOME:
                _caret = 0; return;

            case VirtuelKeys.END:
                _caret = _input.Length; return;

            default:
                // Convert VK to text considering Shift/Caps/AltGr and layout
                var s = VkToText(obj);
                if (!string.IsNullOrEmpty(s))
                {
                    _input = _input.Insert(_caret, s);
                    _caret += s.Length;
                    UpdateExists = true;
                }
                return;
        }
    }


    // ---- Win32 for VK->Unicode (handles Shift, Caps, AltGr, layout) ----
    [DllImport("user32.dll")] static extern bool GetKeyboardState(byte[] lpKeyState);
    [DllImport("user32.dll")] static extern IntPtr GetKeyboardLayout(uint idThread);
    [DllImport("user32.dll")] static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
        StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

    const uint MAPVK_VK_TO_VSC = 0;
    // add near your P/Invokes
    [DllImport("user32.dll")] static extern short GetKeyState(int nVirtKey);

    const int VK_SHIFT = 0x10, VK_LSHIFT = 0xA0, VK_RSHIFT = 0xA1;
    const int VK_CONTROL = 0x11;
    const int VK_MENU = 0x12, VK_LMENU = 0xA4, VK_RMENU = 0xA5; // Alt/AltGr
    const int VK_CAPITAL = 0x14;

    private static bool IsDown(int vk) => (GetKeyState(vk) & 0x8000) != 0;
    private static bool IsToggled(int vk) => (GetKeyState(vk) & 0x0001) != 0;

    private static string VkToText(ushort vk)
    {
        var ks = new byte[256];
        GetKeyboardState(ks); // snapshot; we'll force modifiers below

        // force real-time modifier state
        if (IsDown(VK_SHIFT)) { ks[VK_SHIFT] |= 0x80; ks[VK_LSHIFT] |= 0x80; ks[VK_RSHIFT] |= 0x80; }
        if (IsDown(VK_CONTROL)) { ks[VK_CONTROL] |= 0x80; }
        // AltGr is Right-Alt on many layouts; also treat generic Alt
        if (IsDown(VK_MENU) || IsDown(VK_RMENU)) { ks[VK_MENU] |= 0x80; ks[VK_RMENU] |= 0x80; }
        if (IsToggled(VK_CAPITAL)) ks[VK_CAPITAL] = 0x01;

        var kl = GetKeyboardLayout(0);
        uint sc = MapVirtualKeyEx(vk, MAPVK_VK_TO_VSC, kl);
        var sb = new StringBuilder(8);

        int rc = ToUnicodeEx(vk, sc, ks, sb, sb.Capacity, 0, kl);

        if (rc < 0)
        {
            // dead key pressed; clear state so it doesn't stick
            ToUnicodeEx(0x20 /*VK_SPACE*/, MapVirtualKeyEx(0x20, MAPVK_VK_TO_VSC, kl), ks, sb, sb.Capacity, 0, kl);
            return string.Empty;
        }

        return rc > 0 ? sb.ToString() : string.Empty;
    }


    #region Go to selector
    public void GoToSelector()
    {
        CurrentTaskPage = CurrentTaskPage.Selector;
        ListenForArrows = true;
        ListenForEnter = true;
        ListenForNumbers = true;
    }
    #endregion

    #region Add a task
    public void AddATask()
    {
        CurrentTaskPage = CurrentTaskPage.AddTask;
        ListenForArrows = false;
        ListenForEnter = false;
        ListenForNumbers = false;
        _input = string.Empty;
        if (_caret > _input.Length) _caret = _input.Length;
    }
    #endregion

    #region Remove a task
    public void RemoveATask()
    {
        CurrentTaskPage = CurrentTaskPage.RemoveTask;
        ListenForArrows = false;
        ListenForEnter = false;
        ListenForNumbers = false;
    }
    #endregion

    #region Finish a task
    public void FinishATask()
    {
        CurrentTaskPage = CurrentTaskPage.FinishTask;
        ListenForArrows = false;
        ListenForEnter = false;
        ListenForNumbers = false;
    }
    #endregion

    #region View all tasks
    public void ViewAllTasks()
    {
        CurrentTaskPage = CurrentTaskPage.ViewAllTasks;
        ListenForArrows = false;
        ListenForEnter = false;
        ListenForNumbers = false;
    }
    #endregion



}
