using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_WPF.Platform;

/// <summary>
/// WPF 实现的 UI 线程调用器（基于 Dispatcher）。
/// </summary>
public class WpfUiThreadInvoker:IUiThreadInvoker
{
    private readonly Dispatcher _dispatcher;

    public WpfUiThreadInvoker(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void Post(Action action) => _dispatcher.BeginInvoke(action);

    public void Invoke(Action action)
    {
        if(_dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            _dispatcher.Invoke(action);
        }
    }

    public async Task InvokeAsync(Action action)
    {
        if(_dispatcher.CheckAccess())
        {
            action();
            return;
        }
        await _dispatcher.InvokeAsync(action);
    }

    public IDispatcherTimer CreateTimer() => new WpfDispatcherTimer(new DispatcherTimer());
}

/// <summary>WPF 实现的计时器。</summary>
public class WpfDispatcherTimer:IDispatcherTimer
{
    private readonly DispatcherTimer _timer;

    public WpfDispatcherTimer(DispatcherTimer timer)
    {
        _timer = timer;
    }

    public TimeSpan Interval
    {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }

    public event EventHandler Tick
    {
        add => _timer.Tick += value;
        remove => _timer.Tick -= value;
    }

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();
}
