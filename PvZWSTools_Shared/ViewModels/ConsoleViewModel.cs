using System.Collections.Concurrent;
using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

/// <summary>
/// 界面内控制台：日志、WebSocket 收发，以及手动发一条脚本给游戏。
/// 取数只接两个口 —— <see cref="Log"/> 的每一行（游戏回传的 print 本来就是经
/// MessageProcessor 落成日志的）和 <see cref="IConnectionService.MessageSent"/>。
/// 再去接 MessageReceived 会把同一条回传显示两遍。
/// </summary>
public sealed class ConsoleViewModel:ViewModelBase
{
    /// <summary>只留最近这些行：状态同步是批量发送，一次关卡回报也能刷出几十行。</summary>
    private const int MaxLines = 600;

    private const int SentPreviewChars = 120;

    /// <summary>攒这几毫秒再重填一次文本：一条一行地刷会让两端列表不停地重排。</summary>
    private const int FlushIntervalMs = 200;

    /// <summary>MessageProcessor 把回传落成日志时带的标记，用来把这些行归到"收发"而不是普通日志。</summary>
    private static readonly string[] ReceivedMarks =
    {
        "[输出]", "[执行结果]", "[执行错误]", "[未知事件]", "[消息解析失败]",
    };

    private readonly List<LogLine> _lines = new();
    private readonly IConnectionService _connection;
    private readonly IUserNotifier? _notifier;
    private readonly IDispatcherTimer _flushTimer;
    private readonly IUiThreadInvoker _uiThread;
    private readonly ConcurrentQueue<LogLine> _pending = new();
    private bool _autoScroll = true;
    private bool _flushScheduled;
    private bool _frozen;
    private int _filterIndex;
    private string _input = string.Empty;
    private IReadOnlyList<LogLine> _shown = Array.Empty<LogLine>();

    public ConsoleViewModel(IConnectionService connection, IUiThreadInvoker uiThread, IUserNotifier? notifier)
    {
        _connection = connection;
        _uiThread = uiThread;
        _notifier = notifier;

        // 建这个 VM 之前的启动日志（配置加载、UI 模式）也补进来，否则界面里看到的是半截过程。
        _lines.AddRange(Log.RecentLines);

        _flushTimer = uiThread.CreateTimer();
        _flushTimer.Interval = TimeSpan.FromMilliseconds(FlushIntervalMs);
        _flushTimer.Tick += (_, _) =>
        {
            _flushTimer.Stop();
            _flushScheduled = false;
            Flush();
        };

        SendCommand = new RelayCommand(_ => Send());
        ClearCommand = new RelayCommand(_ => Clear());
        CycleFilterCommand = new RelayCommand(_ => FilterIndex = (_filterIndex + 1) % Filters.Count);

        Log.LineWritten += Append;
        // 发出去的东西真控制台里看不到（它只往 socket 写，不经过 stdout），所以这一行是这里补的，
        // 用青色和日志区分开。连接状态就不补了：日志/控制台本来就有"自动连接中..."那类行。
        _connection.MessageSent += (_, text) => Append(new LogLine("→ " + Summarize(text), ConsoleColor.Cyan));

        Rebuild();
    }

    /// <summary>三个过滤档：收发指 WebSocket 收到的回传与发出去的脚本。</summary>
    /// <summary>筛选项按编号轮转，文字只是显示层，所以这里过文案表。</summary>
    public static IReadOnlyList<string> Filters { get; } = new[] { Loc.T("全部"), Loc.T("只看日志"), Loc.T("只看收发") };

    public string FilterText => Filters[_filterIndex];

    public int FilterIndex
    {
        get => _filterIndex;
        set
        {
            int next = Math.Clamp(value, 0, Filters.Count - 1);
            if(SetProperty(ref _filterIndex, next))
            {
                OnPropertyChanged(nameof(FilterText));
                Frozen = false;
                Rebuild();
            }
        }
    }

    /// <summary>呈现给两端的行。行数封到 <see cref="MaxLines"/>，所以整段重建不贵。</summary>
    public IReadOnlyList<LogLine> Shown
    {
        get => _shown;
        private set => SetProperty(ref _shown, value);
    }

    public string Input
    {
        get => _input;
        set => SetProperty(ref _input, value);
    }

    /// <summary>关掉它才能往上翻着看历史，不然新行一来就跳回底部。</summary>
    public bool AutoScroll
    {
        get => _autoScroll;
        set => SetProperty(ref _autoScroll, value);
    }

    /// <summary>暂停只是不刷新呈现，行照样收着；解开暂停就能看到攒下的那些。
    /// 开着自动重连、游戏又没起的时候，每几秒就来两行，不暂停根本读不完一屏。</summary>
    public bool Frozen
    {
        get => _frozen;
        set
        {
            if(SetProperty(ref _frozen, value) && !value)
                Rebuild();
        }
    }

    public ICommand SendCommand { get; }

    public ICommand ClearCommand { get; }

    public ICommand CycleFilterCommand { get; }

    /// <summary>控制台不参与状态管理：Shown/Input 是这一趟的运行内容，存进 button_states.json 只会污染方案。</summary>
    public override Dictionary<string, string> ExportButtonStates() => new();

    /// <summary>收行的是标准输出，写日志的线程五花八门，所以队列用并发集合、这里不加锁：
    /// <see cref="Helpers.Log"/> 可能正持有它自己的写锁，回调里再抢一把锁没有意义。</summary>
    private void Append(LogLine line)
    {
        _pending.Enqueue(line);
        _uiThread.Post(EnsureFlushScheduled);
    }

    private void EnsureFlushScheduled()
    {
        if(_flushScheduled) return;

        _flushScheduled = true;
        _flushTimer.Start();
    }

    private void Flush()
    {
        if(_pending.IsEmpty) return;

        while(_pending.TryDequeue(out LogLine line))
            _lines.Add(line);

        int overflow = _lines.Count - MaxLines;
        if(overflow > 0)
            _lines.RemoveRange(0, overflow);

        Rebuild();
    }

    private void Clear()
    {
        while(_pending.TryDequeue(out _)) { }
        _lines.Clear();

        // 清空/换过滤是用户主动动作，暂停状态下也得立刻看到结果。
        Frozen = false;
        Rebuild();
    }

    private void Rebuild()
    {
        if(_frozen) return;

        var shown = new List<LogLine>(_lines.Count);
        foreach(var line in _lines)
        {
            if(PassesFilter(line))
                shown.Add(line);
        }
        Shown = shown;
    }

    private bool PassesFilter(LogLine line) => _filterIndex switch
    {
        1 => !IsTraffic(line),
        2 => IsTraffic(line),
        _ => true,
    };

    /// <summary>日志行带着 "[时间 级别] " 头，先剥掉再看它是不是回传。</summary>
    private static bool IsTraffic(LogLine line)
    {
        string text = line.Text;
        if(text.Length == 0) return false;
        if(text[0] == '→') return true;

        int head = text.IndexOf("] ", StringComparison.Ordinal);
        string body = head > 0 ? text[(head + 2)..] : text;
        return ReceivedMarks.Any(m => body.StartsWith(m, StringComparison.Ordinal));
    }

    /// <summary>发出去的多半是整篇脚本，列头一行加行数就够，全文会在日志里以别的形式看到。</summary>
    private static string Summarize(string text)
    {
        if(text.Length <= SentPreviewChars && !text.Contains('\n')) return text;

        int lines = 1;
        foreach(var c in text)
            if(c == '\n') lines++;

        string head = text[..Math.Min(text.Length, SentPreviewChars)]
            .Replace('\r', ' ')
            .Replace('\n', ' ');
        return $"[发送 {lines} 行 / {text.Length} 字] {head}";
    }

    private void Send()
    {
        string text = _input.Trim();
        if(text.Length == 0) return;

        if(!_connection.IsConnected)
        {
            _notifier?.Warn("错误", "WebSocket未连接");
            return;
        }

        Input = string.Empty;
        _ = _connection.SendAsync(text);
    }
}
