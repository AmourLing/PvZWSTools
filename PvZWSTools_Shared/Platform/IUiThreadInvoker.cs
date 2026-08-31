using System;
using System.Threading.Tasks;

namespace PvZWSTools_Shared.Helpers;

/// <summary>
/// 平台无关的 UI 线程调用抽象。
/// WPF 实现基于 <c>System.Windows.Threading.Dispatcher</c>，Avalonia 实现基于 <c>Avalonia.Threading.Dispatcher.UIThread</c>。
/// </summary>
public interface IUiThreadInvoker
{
    /// <summary>异步投递到 UI 线程（不等待执行完成）。</summary>
    void Post(Action action);

    /// <summary>同步在 UI 线程执行（若已在 UI 线程则直接执行）。</summary>
    void Invoke(Action action);

    /// <summary>在 UI 线程异步执行并等待完成。</summary>
    Task InvokeAsync(Action action);

    /// <summary>创建一个与 UI 线程绑定的计时器。</summary>
    IDispatcherTimer CreateTimer();
}

/// <summary>
/// 平台无关的计时器抽象（Tick 事件、Interval、Start/Stop）。
/// </summary>
public interface IDispatcherTimer
{
    TimeSpan Interval { get; set; }

    event EventHandler Tick;

    void Start();

    void Stop();
}
