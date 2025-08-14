using H1_basic_programming_final_project.Core.Handler;
using H1_basic_programming_final_project.Core.Services;
using System.Diagnostics;

namespace H1_basic_programming_final_project.Core.DataModels;

public abstract class Page
{
    #region Properties
    public string Name { get; private set; }
    public int Width => Console.WindowWidth;
    public int Height => Console.WindowHeight;
    public Stopwatch LifeCycleWatch;

    public bool ClearAtRender { get; set; } = true;
    public bool AutoRender { get; set; } = true;
    public bool AutoHandleInput { get; set; } = true;

    public event Action? Activated;
    public event Action? DeActivated;

    public event Func<bool>? CheckForUpdate;

    public bool StopKeyPropagation { get; set; } = false;

    public bool UpdateExists = false;
    private long LastUpdate = long.MinValue;
    private readonly long AutoUpdateRate = 300;
    private long LastInput = long.MinValue;

    public event Action<ushort>? OnKeyPressed;
    #endregion

    #region Constructor
    public Page(string name)
    {
        Name = name;
        LifeCycleWatch = Stopwatch.StartNew();
        LastInput = LifeCycleWatch.ElapsedMilliseconds;
    }
    #endregion

    #region Activate
    public void Activate()
    {
        Activated?.Invoke();
        LastUpdate = LifeCycleWatch.ElapsedMilliseconds;
        UpdateExists = true;
        RenderPage();
    }
    #endregion

    #region DeActivate
    public void DeActivate()
    {
        DeActivated?.Invoke();
    }
    #endregion

    #region Render Page
    public void RenderPage()
    {
        if (AutoHandleInput)
        {
            if (!HasUpdate())
            {
                return;
            }
        }

        if (ClearAtRender)
        {
            Rendere.Clear();
        }

        Render(Rendere.Root);
        if (AutoRender)
        {
            Rendere.Render();
        }
    }
    #endregion

    #region [Abstract] - Render
    public abstract void Render(Rendere rendere);
    #endregion

    #region Handle Input Tick (event-driven via RawInput)
    public void HandleInputTick(double inputIntervalMs, int optionCount)
    {
        long now = LifeCycleWatch.ElapsedMilliseconds;
        if (now - LastInput < inputIntervalMs)
        {
            return;
        }

        bool consumed = false;

        while (RawInput.TryDequeue(out RawInput.KeyEvent e))
        {
            if (e.Type != RawInput.KeyEventType.Click)
            {
                continue; // only first-down
            }

            OnKeyPressed?.Invoke(e.VirtualKey);
            consumed = true;

            if (consumed)
            {
                break; // one action per tick
            }
        }

        if (consumed)
        {
            LastInput = now;
        }
    }
    #endregion

    #region Has Update
    public bool HasUpdate()
    {
        HandleInputTick(10, 5);

        bool hasUpdateFromExternal = CheckForUpdate?.Invoke() ?? false;
        long now = LifeCycleWatch.ElapsedMilliseconds;
        if (now >= LastUpdate + AutoUpdateRate || UpdateExists || hasUpdateFromExternal)
        {
            LastUpdate = now;
            UpdateExists = false;
            return true;
        }
        return false;
    }
    #endregion
}
