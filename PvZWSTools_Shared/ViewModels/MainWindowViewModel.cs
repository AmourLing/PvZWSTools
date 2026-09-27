using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using PvZWSTools_Shared;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

public class MainWindowViewModel:ViewModelBase
{
    private readonly List<string> _addressList = new List<string>
    {
        "ws://localhost:8080/Py",
        "ws://localhost:8081/Py",
        "ws://localhost:8082/Py",
        "ws://127.0.0.1:8080/Py",
        "ws://127.0.0.1:8081/Py"
    };

    private readonly IDispatcherTimer _autoConnectTimer;
    private readonly IButtonStateService? _buttonStateService;
    private readonly IConnectionService _connection;
    private readonly IConnectionDiagnostics? _diagnostics;
    private readonly string _defaultPath;
    private readonly IMessageProcessor _messageProcessor;
    private readonly IUserNotifier? _notifier;
    private readonly IScriptExecutionService _scriptExec;
    private readonly ISettingsService _settingsService;
    private readonly IUiThreadInvoker _uiThread;
    private readonly IUpdateService? _updateService;
    private bool _autoConnectEnabled;
    private string _connectionButtonText = Loc.T("连接");
    private int _currentAddressIndex = 0;
    private double _currentWidth = 960;
    private int _failCount = 0;
    private bool _isRetrying = false;
    private int _selectedTabIndex;
    private string _sizeText = "100%";
    private string _probePortInput = "";
    private readonly FailureAnnouncer _announcer = new();
    private bool _stopAutoConnect;
    private bool _suppressConnectionMessage;
    private bool _suppressExitPrompt;

    /// <summary>这一趟断线里已经试过、有人应门但不是游戏的端口。自动指址绕开它们，
    /// 成功连上后清空；手填的静态候选表不受它影响，照样轮得到。</summary>
    private readonly HashSet<int> _burnedPorts = new();
    private bool _userPinnedAddress;
    private bool _writingAddressAutomatically;

    // ---------- 自动更新进度 UI 状态 ----------
    private bool _isUpdating;

    private double _updateProgress;       // 0-100
    private string _updateStatusText = "";
    private string _updateDownloadedMB = "";
    private string _updateTotalMB = "";
    private string _updateSpeed = "";

    private string _wsAddress = "ws://localhost:8080/Py";

    /// <summary>
    /// 最近一次应用（启动恢复或加载预设）的完整状态。
    /// 连接游戏成功后，据此把"开启"的开关脚本同步发送到游戏。
    /// </summary>
    private Dictionary<string, Dictionary<string, string>>? _lastAppliedStates;

    /// <summary>
    /// 默认状态基线（各子 ViewModel 构造后、恢复前的状态）。
    /// 用于 StateWindow 详细信息面板对比，只显示与默认不同的项。
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, string>> _defaultStates;

    public MainWindowViewModel(
        IConnectionService connection,
        ISettingsService settingsService,
        string defaultPath,
        IDialogService dialogService,
        IMessageProcessor messageProcessor,
        IUiThreadInvoker uiThread,
        IUserNotifier? notifier = null,
        IUpdateService? updateService = null,
        IButtonStateService? buttonStateService = null,
        IConnectionDiagnostics? diagnostics = null)
    {
        _connection = connection;
        _defaultPath = defaultPath;
        _settingsService = settingsService;
        _notifier = notifier;
        _uiThread = uiThread;
        _updateService = updateService;
        _buttonStateService = buttonStateService;
        _diagnostics = diagnostics;
        _scriptExec = new ScriptExecutionService(connection, defaultPath, notifier);
        _messageProcessor = messageProcessor;

        Others = new OthersViewModel(_scriptExec, _messageProcessor);
        Level = new LevelViewModel(_scriptExec, _messageProcessor);
        Resources = new ResourcesViewModel(_scriptExec, _messageProcessor);
        Plants = new PlantsViewModel(_scriptExec, _messageProcessor);
        Zombies = new ZombiesViewModel(_scriptExec, _messageProcessor);
        Spawn = new SpawnViewModel(_scriptExec, defaultPath, _messageProcessor, uiThread, dialogService);
        Board = new BoardViewModel(_scriptExec, _messageProcessor);
        Challenge = new ChallengeViewModel(_scriptExec, _messageProcessor);
        Formation = new FormationViewModel(_scriptExec, defaultPath, _messageProcessor, notifier);
        Fun = new FunViewModel(_scriptExec, _messageProcessor);
        QMod = new QModViewModel(_scriptExec, defaultPath);
        Console = new ConsoleViewModel(_connection, uiThread, notifier);

        Garden = new GardenViewModel(_scriptExec, _connection, dialogService, _messageProcessor);

        LoadSettings();
        // 在恢复前导出默认状态作为对比基线
        _defaultStates = GetCurrentButtonStates();
        LoadButtonStates();

        _connection.ConnectionStateChanged += (s, connected) =>
        {
            ConnectionButtonText = connected ? Loc.T("断开连接") : Loc.T("连接");
            if(connected)
            {
                _failCount = 0;
                _burnedPorts.Clear();
                // 下一次断线要重新报一次，哪怕原因和这次一样：中间已经隔了一趟正常连接。
                _announcer.Reset();
                _ = _connection.SendAsync(Sharedstring.GetLogoDisplayString(!SuppressConnectionMessage));
                Log.Info($"已成功连接到{WsAddress}");
                // 连接成功后，把已恢复状态中"开启"的开关同步发送到游戏
                SyncLastStatesToGame();
            }
        };

        _connection.ConnectionError += async (s, error) =>
        {
            await _uiThread.InvokeAsync(() => NotifyConnectionFailure(error));
            Log.Error(error);
            await HandleConnectionErrorAsync();
        };
        _connection.MessageReceived += (s, msg) => _messageProcessor.ProcessMessage(msg);

        _autoConnectTimer = _uiThread.CreateTimer();
        _autoConnectTimer.Interval = TimeSpan.FromSeconds(1);
        _autoConnectTimer.Tick += AutoConnectTimer_Tick;
        _autoConnectTimer.Start();

        ConnectCommand = new RelayCommand(_ => ToggleConnection());
        OpenPathCommand = new RelayCommand(_ => OpenPath());
        UpdateVersionCommand = new RelayCommand(_ => UpdateVersion());
        SettingCommand = new RelayCommand(_ => OpenSettings());
        StateManagerCommand = new RelayCommand(_ => OpenStateManager());
        SizeUpCommand = new RelayCommand(_ => ChangeSize(true));
        SizeDownCommand = new RelayCommand(_ => ChangeSize(false));
        ConnectionDiagnosticsCommand = new RelayCommand(_ => RunConnectionDiagnostics());
        ProbePortCommand = new RelayCommand(_ => RunPortProbe());
    }

    public event EventHandler ShowSettingsDialog;

    /// <summary>
    /// 用户点击"状态管理"按钮时触发，让 View 层打开 StateWindow。
    /// </summary>
    public event EventHandler? ShowStateManagerRequested;

    /// <summary>
    /// 发现新版本时触发（包括启动自动检查和手动点击），让 View 层打开 UpdateWindow。
    /// 参数为 UpdateInfo；启动自动检查时设 isAuto=true，View 层可用 Show() 非阻塞弹出。
    /// </summary>
    public event EventHandler<UpdateInfoEventArgs>? ShowUpdateWindowRequested;

    /// <summary>
    /// 用户点击"获取更新"时触发：直接打开 UpdateWindow，检查在窗口内进行——
    /// 查到新版本展示渠道，失败时窗口内也有常驻网盘入口可手动下载。
    /// </summary>
    public event EventHandler? OpenUpdateWindowRequested;

    public class UpdateInfoEventArgs(UpdateInfo info, bool isAuto):EventArgs
    {
        public UpdateInfo Info { get; } = info;
        public bool IsAuto { get; } = isAuto;
    }

    public bool AllowAutoUpdateButtonStatus { get; private set; }

    // ---------- 自动更新进度 UI 可绑定属性 ----------

    /// <summary>更新流程是否正在进行（决定底部进度条面板 Visibility）。</summary>
    public bool IsUpdating
    {
        get => _isUpdating;
        private set => SetProperty(ref _isUpdating, value);
    }

    /// <summary>进度百分比 0-100；服务器未提供 Content-Length 时为 0（进度条 indeterminate）。</summary>
    public double UpdateProgress
    {
        get => _updateProgress;
        private set => SetProperty(ref _updateProgress, value);
    }

    /// <summary>状态描述：正在下载 / 正在校验 / 正在应用 / 完成 / 失败。</summary>
    public string UpdateStatusText
    {
        get => _updateStatusText;
        private set => SetProperty(ref _updateStatusText, value);
    }

    /// <summary>已下载 MB 文本，如 "32.5"。</summary>
    public string UpdateDownloadedMB
    {
        get => _updateDownloadedMB;
        private set => SetProperty(ref _updateDownloadedMB, value);
    }

    /// <summary>总 MB 文本，如 "60.6"；未知时为空。</summary>
    public string UpdateTotalMB
    {
        get => _updateTotalMB;
        private set => SetProperty(ref _updateTotalMB, value);
    }

    /// <summary>速度文本，如 "2.3 MB/s"；无速度信息时为空。</summary>
    public string UpdateSpeed
    {
        get => _updateSpeed;
        private set => SetProperty(ref _updateSpeed, value);
    }

    public bool AutoConnectEnabled
    {
        get => _autoConnectEnabled;
        set => SetProperty(ref _autoConnectEnabled, value);
    }

    public BoardViewModel Board { get; }

    public ChallengeViewModel Challenge { get; }

    /// <summary>界面内控制台（日志 / WebSocket 收发 / 手动发脚本），两端同一个数据源。</summary>
    public ConsoleViewModel Console { get; }

    public ICommand ConnectCommand { get; }

    public string ConnectionButtonText
    {
        get => _connectionButtonText;
        set => SetProperty(ref _connectionButtonText, value);
    }

    public double CurrentWidth
    {
        get => _currentWidth;
        set => SetProperty(ref _currentWidth, value);
    }

    public FormationViewModel Formation { get; }

    public FunViewModel Fun { get; }

    public GardenViewModel Garden { get; }

    public LevelViewModel Level { get; }

    public ICommand OpenPathCommand { get; }

    public OthersViewModel Others { get; }

    public PlantsViewModel Plants { get; }

    public QModViewModel QMod { get; }

    public ResourcesViewModel Resources { get; }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if(!SetProperty(ref _selectedTabIndex, value)) return;
            if(value >= 0 && value < ClassicTabOrder.Length)
                RefreshButtonStatesForPage(ClassicTabOrder[value]);
        }
    }

    /// <summary>经典窗口的页签顺序，用来把 SelectedIndex 换成页签名。</summary>
    private static readonly string[] ClassicTabOrder =
    {
        "杂项", "关卡", "资源", "植物", "僵尸", "出怪", "战场", "挑战", "阵型", "娱乐", "快捷脚本", "花园"
    };

    /// <summary>切到某一页时向游戏要一次真实开关状态（只有部分页有对应的回报脚本）。
    /// 安卓抽屉没有 SelectedIndex 可绑，所以按页签名进来，两端共用这一份判断。</summary>
    public void RefreshButtonStatesForPage(string pageTitle)
    {
        // 仅在开启"允许自动更新按钮状态"时才自动发送脚本刷新开关状态
        if(!AllowAutoUpdateButtonStatus)
            return;

        switch(pageTitle)
        {
            case "杂项":
                Others?.UpdateButtonStatusCommand?.Execute(null);
                break;

            case "植物":
                Plants?.UpdateButtonStatusCommand?.Execute(null);
                break;

            case "僵尸":
                Zombies?.UpdateButtonStatusCommand?.Execute(null);
                break;

            case "出怪":
                Spawn?.UpdateButtonStatusCommand?.Execute(null);
                break;

            case "战场":
                Board?.UpdateButtonStatusCommand?.Execute(null);
                break;

            case "娱乐":
                Fun?.UpdateButtonStatusCommand?.Execute(null);
                break;

            case "挑战":
                Challenge?.UpdateButtonStatusCommand?.Execute(null);
                break;
        }
    }

    public ICommand SettingCommand { get; }

    /// <summary>"连接诊断"：把本机 TCP 现场和当前地址对一遍，证据行进控制台，结论弹一句。</summary>
    public ICommand ConnectionDiagnosticsCommand { get; }

    /// <summary>"端口占用探测"的输入端口。只有显式点它才会去 bind。</summary>
    public string ProbePortInput
    {
        get => _probePortInput;
        set => SetProperty(ref _probePortInput, value);
    }

    public ICommand ProbePortCommand { get; }

    public ICommand StateManagerCommand { get; }

    public ICommand SizeDownCommand { get; }

    public string SizeText
    {
        get => _sizeText;
        set => SetProperty(ref _sizeText, value);
    }

    public ICommand SizeUpCommand { get; }

    public SpawnViewModel Spawn { get; }

    public bool SuppressConnectionMessage
    {
        get => _suppressConnectionMessage;
        set => SetProperty(ref _suppressConnectionMessage, value);
    }

    /// <summary>退出时不再弹确认框（确认框里勾选"不再提示"或设置里勾选后为 true）。退出仍走安全退出。</summary>
    public bool SuppressExitPrompt
    {
        get => _suppressExitPrompt;
        set => SetProperty(ref _suppressExitPrompt, value);
    }

    public ICommand UpdateVersionCommand { get; }

    public string WsAddress
    {
        get => _wsAddress;
        set
        {
            // 自动改指一律走 SetAddressAutomatically；从这里进来的其它写入就是用户在输入框里改的，
            // 从这一刻起不再自动覆盖他的选择——他要连别的机器时，本机的表说了不算。
            if(!_writingAddressAutomatically) _userPinnedAddress = true;
            SetProperty(ref _wsAddress, value);
        }
    }

    public ZombiesViewModel Zombies { get; }

    public void ReloadSettingsFromService() => LoadSettings();

    public void SaveSettings()
    {
        _settingsService.Save();
    }

    /// <summary>
    /// 退出确认框里勾选"不再提示"：写回设置并持久化，下次退出直接安全退出、不再二次确认。
    /// </summary>
    public void RememberSuppressExitPrompt()
    {
        try
        {
            _settingsService.Settings.SuppressExitPrompt = true;
            _settingsService.Save();
            SuppressExitPrompt = true;
        }
        catch(Exception ex)
        {
            Log.Error($"保存退出提示设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 从持久化存储加载按钮状态并应用到各子 ViewModel。
    /// 仅当设置中勾选了"自动应用上次配置"才在启动时自动恢复。
    /// 仅恢复 UI 显示状态，不主动发送脚本到游戏。
    /// 连接游戏后可通过"允许自动更新按钮状态"从游戏同步实际状态。
    /// </summary>
    private void LoadButtonStates()
    {
        if(_buttonStateService == null) return;
        try
        {
            if(!_settingsService.Settings.AutoApplyLastState)
            {
                Log.Info("未开启自动应用上次配置，跳过状态恢复");
                return;
            }

            var allStates = _buttonStateService.Load();
            if(allStates.Count == 0) return;

            ApplyButtonStates(allStates, persist: false);
            Log.Info($"按钮状态恢复完成：{allStates.Sum(kv => kv.Value.Count)} 项");
        }
        catch(Exception ex)
        {
            Log.Error($"按钮状态恢复失败: {ex}");
        }
    }

    /// <summary>
    /// 收集所有子 ViewModel 的按钮状态并保存到持久化存储。
    /// 由 View 层在窗口关闭时调用。
    /// </summary>
    public void SaveButtonStates()
    {
        if(_buttonStateService == null) return;
        try
        {
            var allStates = new Dictionary<string, Dictionary<string, string>>();
            foreach(var prop in GetType().GetProperties())
            {
                if(typeof(ViewModelBase).IsAssignableFrom(prop.PropertyType))
                {
                    var subVm = prop.GetValue(this) as ViewModelBase;
                    if(subVm != null)
                    {
                        var states = subVm.ExportButtonStates();
                        if(states.Count > 0)
                            allStates[prop.Name] = states;
                    }
                }
            }
            _buttonStateService.Save(allStates);
        }
        catch(Exception ex)
        {
            Log.Error($"按钮状态保存失败: {ex}");
        }
    }

    /// <summary>
    /// 安全退出的共享清理：停自动重连定时器 → 保存按钮状态 → 断开连接。
    /// 与 Android 端 MainActivity.SafeExit 同一份语义，由 View 层在用户确认退出后调用。
    /// </summary>
    public void PrepareExit()
    {
        try
        {
            Log.Info("用户确认退出，执行安全退出");
            _autoConnectTimer?.Stop();
            _stopAutoConnect = true;
            SaveButtonStates();
            if(_connection.IsConnected)
                _connection.Disconnect();
        }
        catch(Exception ex)
        {
            Log.Error($"安全退出时发生异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 按属性名获取子 ViewModel 实例（用于恢复状态）。
    /// </summary>
    private ViewModelBase? GetSubViewModelByName(string name)
    {
        var prop = GetType().GetProperty(name);
        return prop?.GetValue(this) as ViewModelBase;
    }

    /// <summary>
    /// 收集当前所有子 ViewModel 的状态（供 StateWindow 保存预设）。
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> GetCurrentButtonStates()
    {
        var allStates = new Dictionary<string, Dictionary<string, string>>();
        foreach(var prop in GetType().GetProperties())
        {
            if(typeof(ViewModelBase).IsAssignableFrom(prop.PropertyType))
            {
                var subVm = prop.GetValue(this) as ViewModelBase;
                if(subVm != null)
                {
                    var states = subVm.ExportButtonStates();
                    if(states.Count > 0)
                        allStates[prop.Name] = states;
                }
            }
        }
        return allStates;
    }

    /// <summary>
    /// 把指定状态应用到各子 ViewModel（启动恢复或 StateWindow 加载预设）。
    /// 先恢复 UI 显示；若当前已连接游戏，则立即把"开启"的开关脚本同步发送到游戏，
    /// 否则记录状态，待连接成功后由 <see cref="SyncLastStatesToGame"/> 发送。
    /// </summary>
    /// <param name="persist">是否同时写回"上次状态"文件；启动自动恢复时为 false。</param>
    public void ApplyButtonStates(Dictionary<string, Dictionary<string, string>> states, bool persist = true)
    {
        foreach(var kvp in states)
        {
            var subVm = GetSubViewModelByName(kvp.Key);
            if(subVm != null && kvp.Value.Count > 0)
                subVm.ImportButtonStates(kvp.Value);
        }

        _lastAppliedStates = states;

        if(persist)
            _buttonStateService?.Save(states);

        // 已连接则立即同步开关到游戏；未连接则等连接成功事件
        if(_connection.IsConnected)
            SyncLastStatesToGame();
    }

    /// <summary>
    /// 把最近应用状态中所有"开启"的开关脚本发送到游戏（仅 Checkbox）。
    /// 在连接成功后或加载预设时（已连接）调用。
    /// 批量发送期间开启静默模式：脚本缺失/失败只记日志，不弹模态框打断流程。
    /// </summary>
    private void SyncLastStatesToGame()
    {
        if(_lastAppliedStates == null) return;
        bool previousSilent = _scriptExec.SilentMode;
        _scriptExec.SilentMode = true;
        try
        {
            int sent = 0;
            foreach(var kvp in _lastAppliedStates)
            {
                var subVm = GetSubViewModelByName(kvp.Key);
                if(subVm == null) continue;
                sent += subVm.SyncToggleStatesToGame(kvp.Value);
            }
            if(sent > 0)
                Log.Info($"已将 {sent} 个开启状态同步到游戏");
        }
        catch(Exception ex)
        {
            Log.Error($"同步开关状态到游戏失败: {ex}");
        }
        finally
        {
            _scriptExec.SilentMode = previousSilent;
        }
    }

    /// <summary>
    /// 读取上次关闭时自动保存的状态（供 StateWindow 中固定的"上次状态"条目加载）。
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> LoadLastButtonStates()
    {
        return _buttonStateService?.Load() ?? new Dictionary<string, Dictionary<string, string>>();
    }

    /// <summary>
    /// 返回默认状态基线（供 StateWindow 详细信息面板对比差异）。
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> GetDefaultButtonStates()
    {
        return _defaultStates ?? new Dictionary<string, Dictionary<string, string>>();
    }

    private void OpenStateManager()
    {
        Log.Info("打开状态管理窗口");
        ShowStateManagerRequested?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateSize(double width)
    {
        double percentage = (width / 640.0) * 100;
        SizeText = $"{percentage:F0}%";
    }

    private async void AutoConnectTimer_Tick(object sender, EventArgs e)
    {
        if(_stopAutoConnect || !AutoConnectEnabled || _connection.IsConnected || string.IsNullOrWhiteSpace(WsAddress))
            return;

        _autoConnectTimer.Stop();
        Log.Info("自动连接中...");
        // 先问表再连：游戏在 8081 就别先去撞 8080 吃三次拒绝。
        TryApplyBestAddress();
        await _connection.ConnectAsync(WsAddress);
        _autoConnectTimer.Start();
    }

    private void ChangeSize(bool up)
    {
        int level = (int)Math.Round(CurrentWidth / 64.0);
        level = up ? level + 1 : Math.Max(1, level - 1);
        CurrentWidth = level * 64;
    }

    /// <summary>连接失败的提示。有采集能力就把现场证据归成一句结论再报；
    /// 没有这能力的平台只能给通用指引，那句指引必须是我能站得住的——不能拿一份空快照编出"游戏没开"。</summary>
    private void NotifyConnectionFailure(string error)
    {
        if(_diagnostics == null)
        {
            // 安卓端连的是别的机器上的游戏，所以给的是"跨机才成立"的那两条检查，
            // 而不是本机回环那套；错误原文一起附上，出问题时有据可查。
            _notifier?.Warn("连接失败", $"连不上 {WsAddress}：确认游戏已打开；跨机时确认对端放行了入站连接。（{error}）");
            return;
        }

        var report = Diagnose(error, _connection.LastSocketErrorCode);
        // 同一趟断线里同因只报一次（规则在 FailureAnnouncer 里，离线门钉着它）。
        if(!_announcer.ShouldReport(report.Kind)) return;

        foreach (var line in report.Lines) Log.Info(line);
        _notifier?.Warn("连接失败", report.Conclusion);
    }

    /// <summary>手动点的"连接诊断"：此刻没有 socket 错误码，所以按体检的措辞走。</summary>
    private void RunConnectionDiagnostics()
    {
        if(_diagnostics == null)
        {
            Log.Info("这个平台没有连接现场的采集能力，只能给错误原文。");
            return;
        }

        var report = Diagnose(string.Empty, 0);
        foreach (var line in report.Lines) Log.Info(line);
        // 监听正常就不弹模态框：体检的回报进日志，弹框留给真出了问题的场合。
        if(report.Kind != ConnectFailureKind.TargetAvailable)
            _notifier?.Warn("连接诊断", report.Conclusion);
    }

    /// <summary>端口占用探测。这是唯一会去 bind 的动作，所以只能由这一下点击触发。</summary>
    private void RunPortProbe()
    {
        if(_diagnostics == null)
        {
            Log.Info("这个平台没有端口探测能力。");
            return;
        }
        if(!int.TryParse(ProbePortInput, out int port))
        {
            Log.Info($"端口占用探测：先填一个端口号（当前输入“{ProbePortInput}”）");
            return;
        }
        foreach (var line in _diagnostics.ProbeBind(port)) Log.Info(line);
    }

    private DiagnosisReport Diagnose(string rawError, int socketErrorCode)
    {
        try
        {
            return ConnectFailure.Diagnose(socketErrorCode, _diagnostics!.Capture(), WsAddress, rawError);
        }
        catch(Exception ex)
        {
            // 采集失败不能当成"没问题"：明说采不到，并把连接错误原文一起带出来。
            Log.Error("连接现场采集失败", ex);
            return new DiagnosisReport
            {
                Kind = ConnectFailureKind.Other,
                Conclusion = string.IsNullOrEmpty(rawError)
                    ? $"本机连接现场没采到：{ex.Message}"
                    : $"本机连接现场没采到（{ex.Message}），连接错误原文：{rawError}",
                Lines = new[] { $"连接诊断 {WsAddress}", "结论：本机连接现场没采到：" + ex.Message },
            };
        }
    }

    /// <summary>连之前先按现场把地址指到游戏真正连得到的那一个。已经是了对的位置就什么都不做。</summary>
    private bool TryApplyBestAddress()
    {
        if(_diagnostics == null || _userPinnedAddress) return false;
        try
        {
            var best = ConnectFailure.BestAddressFor(_diagnostics.Capture(), WsAddress, _burnedPorts);
            if(best == null) return false;
            SetAddressAutomatically(best);
            Log.Info($"自动指向游戏：{best}");
            return true;
        }
        catch(Exception ex)
        {
            Log.Error("连接现场采集失败", ex);
            return false;
        }
    }

    /// <summary>把"有人应门，但应答的不是我们的协议"的那个端口烧掉，下次自动指址绕开它。
    /// 连接被拒（游戏还没起来）不算——那种端口等一下就是它的，烧了反而绕远。</summary>
    private void BurnPortIfSomebodyElseAnswered()
    {
        if(_diagnostics == null) return;
        if(_connection.LastSocketErrorCode == ConnectFailure.WSAECONNREFUSED) return;
        if(!ConnectFailure.TryParseTarget(WsAddress, out var host, out int port)) return;
        if(!ConnectFailure.IsLocalTarget(host)) return;
        try
        {
            if(_diagnostics.Capture().OnPort(port).Any()) _burnedPorts.Add(port);
        }
        catch(Exception ex)
        {
            Log.Error("连接现场采集失败", ex);
        }
    }

    /// <summary>只有工具自己改地址才走这里：绕开 WsAddress 的"用户改过就顶住"判定。</summary>
    private void SetAddressAutomatically(string address)
    {
        _writingAddressAutomatically = true;
        try { WsAddress = address; }
        finally { _writingAddressAutomatically = false; }
    }

    private async Task HandleConnectionErrorAsync()
    {
        if(_isRetrying || _connection.IsConnected) return;
        _isRetrying = true;
        try
        {
            _failCount++;
            if(_failCount >= 3)
            {
                _failCount = 0;
                BurnPortIfSomebodyElseAnswered();
                if(!TryApplyBestAddress())
                {
                    // 表上没有更好的选择了（比如游戏压根没在听）：退回原来的候选表轮询。
                    _currentAddressIndex = (_currentAddressIndex + 1) % _addressList.Count;
                    SetAddressAutomatically(_addressList[_currentAddressIndex]);
                    Log.Info($"切换地址至: {WsAddress}");
                }
                await _connection.ConnectAsync(WsAddress);
            }
        }
        finally
        {
            _isRetrying = false;
        }
    }

    private void LoadSettings()
    {
        var settings = _settingsService.Settings;
        AutoConnectEnabled = settings.AutoConnectEnabled;
        SuppressConnectionMessage = settings.SuppressConnectionMessage;
        AllowAutoUpdateButtonStatus = settings.AllowAutoUpdateButtonStatus;
        SuppressExitPrompt = settings.SuppressExitPrompt;
    }

    private void OpenPath()
    {
#if ANDROID
        Log.Info("Android 端不支持打开文件目录");
#else
        string path = Path.Combine(_defaultPath, Constants.Folder_Need);
        if(Directory.Exists(path))
            _ = Process.Start("explorer.exe", path);
#endif
    }

    private void OpenSettings()
    {
        ShowSettingsDialog?.Invoke(this, EventArgs.Empty);
    }

    private async void ToggleConnection()
    {
        if(_connection.IsConnected)
        {
            _connection.Disconnect();
            _stopAutoConnect = true;
        }
        else
        {
            _stopAutoConnect = false;
            TryApplyBestAddress();
            await _connection.ConnectAsync(WsAddress);
        }
    }

    private void UpdateVersion()
    {
#if ANDROID
        // Android 端由 MainActivity 的 nav_updateversion 菜单项直接处理，
        // ViewModel 不参与（AndroidUpdateService 在 MainActivity 内部使用）
        try
        {
            var context = global::Android.App.Application.Context;
            var intent = new global::Android.Content.Intent(global::Android.Content.Intent.ActionView,
                global::Android.Net.Uri.Parse(Sharedstring.BaseUpdateUrl));
            intent.AddFlags(global::Android.Content.ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
        catch(Exception ex)
        {
            Log.Error($"打开更新页面失败: {ex.Message}");
        }
#else
        // WPF 端：直接打开更新窗口，检查在窗口内进行（失败时窗口里仍有常驻网盘入口）
        OpenUpdateWindowRequested?.Invoke(this, EventArgs.Empty);
#endif
    }

    /// <summary>
    /// 启动时由 App 层调用的自动检查入口；用户主动点击"获取更新"按钮时
    /// <paramref name="isManual"/>=true，无论是否开启"启动时自动检查更新"都会执行。
    /// </summary>
    public async Task CheckAndApplyUpdateAsync(bool isManual = false)
    {
        if(_updateService == null)
        {
            if(isManual)
                _notifier?.Warn("检查更新", "当前版本未启用自动更新服务，请前往发布页手动下载。");
            return;
        }

        // 非手动模式且未开启"启动时自动检查更新"则跳过
        if(!isManual && !_settingsService.Settings.AutoCheckUpdateEnabled)
            return;

        // 防止重复触发
        if(IsUpdating) return;

        UpdateInfo? info = null;
        try
        {
            info = await _updateService.CheckForUpdatesAsync(Sharedstring.AssetNameWindows);
        }
        catch(Exception ex)
        {
            Log.Error($"检查更新失败: {ex.Message}");
            if(isManual)
                _notifier?.Warn("检查更新", $"检查更新失败：{ex.Message}");
            return;
        }

        if(info == null)
        {
            if(isManual)
                _notifier?.Warn("检查更新", "检查更新失败，请稍后重试或前往发布页手动下载。");
            return;
        }

        if(!info.IsNewerThan(_updateService.CurrentVersion))
        {
            if(isManual)
                _notifier?.Warn("检查更新", $"当前已是最新版本（{info.TagName}）。");
            return;
        }

        // 发现新版本：raise 事件让 View 层打开 UpdateWindow（带渠道选择）
        // 如果 View 层已订阅，把控制权交给它；否则回退到旧流程（MessageBox）
        if(ShowUpdateWindowRequested != null)
        {
            ShowUpdateWindowRequested(this, new UpdateInfoEventArgs(info, isAuto: !isManual));
            return;
        }

        // ---------- 旧流程 fallback（MessageBox + 内联进度条） ----------
        string currentVerStr = _updateService.CurrentVersionDisplay;
        string versionLine = $"发现新版本 {info.TagName}  （当前 v{currentVerStr}）";
        string notes = string.IsNullOrWhiteSpace(info.ReleaseNotes) ? "" : $"\n\n{info.ReleaseNotes}";
        bool accept = _notifier?.Confirm("发现新版本", $"{versionLine}{notes}\n\n是否立即下载并更新？") ?? false;
        if(!accept) return;

        // ---------- 进入更新流程：内联进度条，替换 MessageBox ----------
        IsUpdating = true;
        UpdateProgress = 0;
        UpdateStatusText = "正在下载更新包...";
        UpdateDownloadedMB = "0";
        UpdateTotalMB = info.Size.HasValue ? $"{info.Size.Value / 1048576.0:F1}" : "";
        UpdateSpeed = "";

        // 用 Progress<T> 把后台进度安全 marshal 到 UI 线程
        var progress = new Progress<DownloadProgress>(p =>
        {
            // Progress<T> 的 Report 默认在捕获的 SynchronizationContext 上执行
            UpdateProgress = p.Percentage ?? 0;
            UpdateDownloadedMB = $"{p.BytesDownloaded / 1048576.0:F1}";
            UpdateTotalMB = p.TotalBytes.HasValue ? $"{p.TotalBytes.Value / 1048576.0:F1}" : "";
            UpdateSpeed = p.BytesPerSecond.HasValue ? FormatSpeed(p.BytesPerSecond.Value) : "";
            UpdateStatusText = p.Percentage.HasValue
                ? $"正在下载... {p.Percentage}%"
                : "正在下载...";
        });

        string? downloaded = null;
        try
        {
            downloaded = await _updateService.DownloadUpdateAsync(info, progress);
        }
        catch(Exception ex)
        {
            Log.Error($"下载异常: {ex.Message}");
        }

        if(string.IsNullOrEmpty(downloaded))
        {
            UpdateStatusText = "下载失败";
            _notifier?.Error("更新失败", "下载更新包失败，请稍后重试或前往发布页手动下载。");
            IsUpdating = false;
            return;
        }

        // 下载完成 → 校验阶段
        UpdateProgress = 100;
        UpdateStatusText = "下载完成，正在校验...";
        UpdateSpeed = "";

        // 应用更新（bat 会等主进程退出后替换并重启）
        UpdateStatusText = "正在应用更新，即将重启...";
        bool applied = await _updateService.ApplyUpdateAsync(downloaded);
        if(!applied)
        {
            UpdateStatusText = "应用更新失败";
            _notifier?.Error("更新失败", "应用更新失败，请前往发布页手动下载。");
            IsUpdating = false;
        }
        // 应用成功时主程序已退出，IsUpdating 状态不需要清除
    }

    private static string FormatSpeed(double bytesPerSecond)
    {
        if(bytesPerSecond >= 1048576)
            return $"{bytesPerSecond / 1048576.0:F2} MB/s";
        if(bytesPerSecond >= 1024)
            return $"{bytesPerSecond / 1024.0:F1} KB/s";
        return $"{bytesPerSecond:F0} B/s";
    }
}
