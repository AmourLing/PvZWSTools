using System.Diagnostics;
using System.IO;
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
    private readonly IConnectionService _connection;
    private readonly string _defaultPath;
    private readonly IMessageProcessor _messageProcessor;
    private readonly IUserNotifier? _notifier;
    private readonly IScriptExecutionService _scriptExec;
    private readonly ISettingsService _settingsService;
    private readonly IUiThreadInvoker _uiThread;
    private readonly IUpdateService? _updateService;
    private bool _autoConnectEnabled;
    private string _connectionButtonText = "连接";
    private int _currentAddressIndex = 0;
    private double _currentWidth = 960;
    private int _failCount = 0;
    private bool _isRetrying = false;
    private int _selectedTabIndex;
    private string _sizeText = "100%";
    private bool _stopAutoConnect;
    private bool _suppressConnectionMessage;

    // ---------- 自动更新进度 UI 状态 ----------
    private bool _isUpdating;
    private double _updateProgress;       // 0-100
    private string _updateStatusText = "";
    private string _updateDownloadedMB = "";
    private string _updateTotalMB = "";
    private string _updateSpeed = "";

    private string _wsAddress = "ws://localhost:8080/Py";

    public MainWindowViewModel(
        IConnectionService connection,
        ISettingsService settingsService,
        string defaultPath,
        IDialogService dialogService,
        IMessageProcessor messageProcessor,
        IUiThreadInvoker uiThread,
        IUserNotifier? notifier = null,
        IUpdateService? updateService = null)
    {
        _connection = connection;
        _defaultPath = defaultPath;
        _settingsService = settingsService;
        _notifier = notifier;
        _uiThread = uiThread;
        _updateService = updateService;
        _scriptExec = new ScriptExecutionService(connection, defaultPath, notifier);
        _messageProcessor = messageProcessor;

        Others = new OthersViewModel(_scriptExec, _messageProcessor);
        Level = new LevelViewModel(_scriptExec, _messageProcessor);
        Resources = new ResourcesViewModel(_scriptExec, _messageProcessor);
        Plants = new PlantsViewModel(_scriptExec, _messageProcessor);
        Zombies = new ZombiesViewModel(_scriptExec, _messageProcessor);
        Spawn = new SpawnViewModel(_scriptExec, defaultPath, _messageProcessor, uiThread);
        Board = new BoardViewModel(_scriptExec, _messageProcessor);
        Challenge = new ChallengeViewModel(_scriptExec, _messageProcessor);
        Formation = new FormationViewModel(_scriptExec, defaultPath, _messageProcessor);
        Fun = new FunViewModel(_scriptExec, _messageProcessor);
        QMod = new QModViewModel(_scriptExec, defaultPath);

        Garden = new GardenViewModel(_scriptExec, _connection, dialogService, _messageProcessor);

        LoadSettings();

        _connection.ConnectionStateChanged += (s, connected) =>
        {
            ConnectionButtonText = connected ? "断开连接" : "连接";
            if(connected)
            {
                _failCount = 0;
                _ = _connection.SendAsync(Sharedstring.GetLogoDisplayString(!SuppressConnectionMessage));
                Log.Info($"已成功连接到{WsAddress}");
            }
        };

        _connection.ConnectionError += async (s, error) =>
        {
            await _uiThread.InvokeAsync(() =>
            {
                _notifier?.Warn("连接失败", "连接失败，请确认游戏是否已打开并允许联网权限。");
            });
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
        SizeUpCommand = new RelayCommand(_ => ChangeSize(true));
        SizeDownCommand = new RelayCommand(_ => ChangeSize(false));
    }

    public event EventHandler ShowSettingsDialog;

    /// <summary>
    /// 发现新版本时触发（包括启动自动检查和手动点击），让 View 层打开 UpdateWindow。
    /// 参数为 UpdateInfo；启动自动检查时设 isAuto=true，View 层可用 Show() 非阻塞弹出。
    /// </summary>
    public event EventHandler<UpdateInfoEventArgs>? ShowUpdateWindowRequested;

    public class UpdateInfoEventArgs(UpdateInfo info, bool isAuto) : EventArgs
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
            if(SetProperty(ref _selectedTabIndex, value))
            {
                // 仅在开启"允许自动更新按钮状态"时才自动发送脚本刷新开关状态
                if(!AllowAutoUpdateButtonStatus)
                    return;

                switch(value)
                {
                    case 0:
                        Others?.UpdateButtonStatusCommand?.Execute(null);
                        break;

                    case 1:
                        break;

                    case 2:
                        break;

                    case 3:
                        Plants?.UpdateButtonStatusCommand?.Execute(null);
                        break;

                    case 4:
                        Zombies?.UpdateButtonStatusCommand?.Execute(null);
                        break;

                    case 5:
                        Spawn?.UpdateButtonStatusCommand?.Execute(null);
                        break;

                    case 6:
                        Board?.UpdateButtonStatusCommand?.Execute(null);
                        break;

                    case 9:
                        Fun?.UpdateButtonStatusCommand?.Execute(null);
                        break;
                }
            }
        }
    }

    public ICommand SettingCommand { get; }

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

    public ICommand UpdateVersionCommand { get; }

    public string WsAddress
    {
        get => _wsAddress;
        set => SetProperty(ref _wsAddress, value);
    }

    public ZombiesViewModel Zombies { get; }

    public void ReloadSettingsFromService() => LoadSettings();

    public void SaveSettings()
    {
        _settingsService.Save();
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
        await _connection.ConnectAsync(WsAddress);
        _autoConnectTimer.Start();
    }

    private void ChangeSize(bool up)
    {
        int level = (int)Math.Round(CurrentWidth / 64.0);
        level = up ? level + 1 : Math.Max(1, level - 1);
        CurrentWidth = level * 64;
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
                _currentAddressIndex = (_currentAddressIndex + 1) % _addressList.Count;
                WsAddress = _addressList[_currentAddressIndex];
                Log.Info($"切换地址至: {WsAddress}");
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
        // WPF 端：让 View 层打开 UpdateWindow（有渠道选择 UI）
        // 先触发检查，查到新版本后 raise 事件
        _ = CheckAndApplyUpdateAsync(isManual: true);
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
