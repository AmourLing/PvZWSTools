using Android.OS;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_Avalonia.Platform;

/// <summary>
/// Android 实现的 UI 线程调用器。刻意不依赖 Activity：ViewModel 图要在 MainActivity
/// 之前就能建起来，而且 Activity 重建（旋转/回收）后不该把已建好的图换掉。
/// </summary>
public class AndroidUiThreadInvoker:IUiThreadInvoker
{
    private readonly Handler _main = new(Looper.MainLooper!);

    public void Post(Action action) => _main.Post(action);

    public void Invoke(Action action)
    {
        if(IsOnMainThread())
        {
            action();
            return;
        }

        // 已确定不在主线程，等它不会自锁；超时只是兜底，避免后台线程永久挂在卡死 UI 上。
        using var done = new ManualResetEventSlim(false);
        Exception? error = null;
        _main.Post(() =>
        {
            try
            {
                action();
            }
            catch(Exception ex)
            {
                error = ex;
            }
            finally
            {
                done.Set();
            }
        });

        if(!done.Wait(TimeSpan.FromSeconds(5)))
            throw new TimeoutException("主线程 5 秒内未响应 UI 调用");
        if(error != null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
    }

    public async Task InvokeAsync(Action action)
    {
        if(IsOnMainThread())
        {
            action();
            return;
        }

        var tcs = new TaskCompletionSource();
        _main.Post(() =>
        {
            try
            {
                action();
                tcs.SetResult();
            }
            catch(Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        await tcs.Task;
    }

    public IDispatcherTimer CreateTimer() => new AndroidDispatcherTimer();

    private static bool IsOnMainThread() => Looper.MyLooper() == Looper.MainLooper;
}

/// <summary>
/// 由主 Looper 驱动的计时器：每次 Tick 跑完再排下一次，所以 UI 卡住时不会像
/// System.Threading.Timer 那样把 Tick 堆成一串。语义对齐 WPF 的 DispatcherTimer。
/// </summary>
public class AndroidDispatcherTimer:IDispatcherTimer
{
    private readonly Handler _handler = new(Looper.MainLooper!);
    private readonly Action _step;
    private bool _running;

    public AndroidDispatcherTimer() => _step = TickOnce;

    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);

    public event EventHandler? Tick;

    public void Start()
    {
        if(_running) return;
        _running = true;
        Schedule();
    }

    public void Stop()
    {
        _running = false;
        _handler.RemoveCallbacksAndMessages(null);
    }

    private void Schedule() => _handler.PostDelayed(_step, (long)Interval.TotalMilliseconds);

    private void TickOnce()
    {
        if(!_running) return;
        Tick?.Invoke(this, EventArgs.Empty);
        if(_running) Schedule();
    }
}
