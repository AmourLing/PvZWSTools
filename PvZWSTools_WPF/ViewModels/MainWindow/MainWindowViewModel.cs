using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PvZWSTools_Shared;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Models;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels;

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

    private int _currentAddressIndex = 0;
    private int _failCount = 0;
    private bool _isRetrying = false;  // 防止重入

    private readonly IConnectionService _connection;
    private readonly IScriptExecutionService _scriptExec;
    private readonly string _defaultPath;
    private readonly DispatcherTimer _autoConnectTimer;
    private bool _stopAutoConnect;
    private bool _autoConnectEnabled;

    public bool AutoConnectEnabled
    {
        get => _autoConnectEnabled;
        set
        {
            _autoConnectEnabled = value;
            OnPropertyChanged();
        }
    }

    private bool _suppressConnectionMessage;

    public bool SuppressConnectionMessage
    {
        get => _suppressConnectionMessage;
        set
        {
            _suppressConnectionMessage = value;
            OnPropertyChanged();
        }
    }

    private string _wsAddress = "ws://localhost:8080/Py";

    public string WsAddress
    {
        get => _wsAddress;
        set
        {
            _wsAddress = value;
            OnPropertyChanged();
        }
    }

    private string _connectionButtonText = "连接";

    public string ConnectionButtonText
    {
        get => _connectionButtonText;
        set
        {
            _connectionButtonText = value;
            OnPropertyChanged();
        }
    }

    private string _sizeText = "100%";

    public string SizeText
    {
        get => _sizeText;
        set
        {
            _sizeText = value;
            OnPropertyChanged();
        }
    }

    public OthersViewModel Others { get; }

    public LevelViewModel Level { get; }
    public ResourcesViewModel Resources { get; }
    public PlantsViewModel Plants { get; }
    public ZombiesViewModel Zombies { get; }
    public SpawnViewModel Spawn { get; }
    public BoardViewModel Board { get; }
    public ChallengeViewModel Challenge { get; }
    public FormationViewModel Formation { get; }
    public FunViewModel Fun { get; }

    public QModViewModel QMod { get; }

    public ICommand ConnectCommand { get; }

    public ICommand OpenPathCommand { get; }
    public ICommand UpdateVersionCommand { get; }
    public ICommand SettingCommand { get; }
    public ICommand SizeUpCommand { get; }
    public ICommand SizeDownCommand { get; }
    public GardenViewModel Garden { get; }

    private readonly ISettingsService _settingsService;

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

    public MainWindowViewModel(IConnectionService connection, ISettingsService settingsService, string defaultPath)
    {
        _connection = connection;
        _defaultPath = defaultPath;
        _settingsService = settingsService;
        _scriptExec = new ScriptExecutionService(connection, defaultPath);

        Others = new OthersViewModel(_scriptExec);
        Level = new LevelViewModel(_scriptExec);
        Resources = new ResourcesViewModel(_scriptExec);
        Plants = new PlantsViewModel(_scriptExec);
        Zombies = new ZombiesViewModel(_scriptExec);
        Spawn = new SpawnViewModel(_scriptExec, defaultPath);
        Board = new BoardViewModel(_scriptExec);
        Challenge = new ChallengeViewModel(_scriptExec);
        Formation = new FormationViewModel(_scriptExec, defaultPath);
        Fun = new FunViewModel(_scriptExec);
        QMod = new QModViewModel(_scriptExec, defaultPath);

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
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _ = MessageBox.Show("连接失败，请确认游戏是否已打开并允许联网权限。", "连接失败", MessageBoxButton.OK, MessageBoxImage.Warning);
            });
            Log.Error(error);
            await HandleConnectionErrorAsync();
        };
        _connection.MessageReceived += OnMessageReceived;

        _autoConnectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _autoConnectTimer.Tick += AutoConnectTimer_Tick;
        _autoConnectTimer.Start();

        ConnectCommand = new RelayCommand(_ => ToggleConnection());
        OpenPathCommand = new RelayCommand(_ => OpenPath());
        UpdateVersionCommand = new RelayCommand(_ => UpdateVersion());
        SettingCommand = new RelayCommand(_ => OpenSettings());
        SizeUpCommand = new RelayCommand(_ => ChangeSize(true));
        SizeDownCommand = new RelayCommand(_ => ChangeSize(false));

        Garden = new GardenViewModel(_scriptExec, _connection);
    }

    private void OnMessageReceived(object sender, string message)
    {
        try
        {
            var jo = JObject.Parse(message);
            string eventType = jo["eventtype"]?.Value<string>();
            if(eventType == "output")
            {
                var output = jo.ToObject<WSEvents.OutputEvent>();
                Log.Info($"[输出] {output.msg}");
            }
            else if(eventType == "execution")
            {
                var exec = jo.ToObject<WSEvents.ExecutionEvent>();
                if(exec.statuscode == WSEvents.ExecutionEventResult.error)
                {
                    Log.Error($"[执行错误] {exec.errortype}: {exec.result}");
                }
                else
                {
                    Log.Info($"[执行结果] {exec.result}");
                }
            }
            else
            {
                Log.Info($"[未知事件] {message}");
            }
        }
        catch(Exception ex)
        {
            Log.Error($"[消息解析失败] {ex.Message}");
        }
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

    private async void AutoConnectTimer_Tick(object sender, EventArgs e)
    {
        if(_stopAutoConnect || !AutoConnectEnabled || _connection.IsConnected || string.IsNullOrWhiteSpace(WsAddress))
            return;

        _autoConnectTimer.Stop();
        Log.Info("自动连接中...");
        await _connection.ConnectAsync(WsAddress);
        _autoConnectTimer.Start();
    }

    private void LoadSettings()
    {
        string path = Path.Combine(_defaultPath, Constants.Folder_Need, "setting.json");
        if(File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                var settings = JsonConvert.DeserializeObject<AppSettings>(json);
                AutoConnectEnabled = settings.AutoConnectEnabled;
                SuppressConnectionMessage = settings.SuppressConnectionMessage;
            }
            catch { }
        }
    }

    public void SaveSettings()
    {
        try
        {
            var settings = new AppSettings
            {
                AutoConnectEnabled = AutoConnectEnabled,
                SuppressConnectionMessage = SuppressConnectionMessage
            };
            string dir = Path.Combine(_defaultPath, Constants.Folder_Need);
            _ = Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "setting.json");
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(path, json);
            OnPropertyChanged();

            Log.Info($"setting保存成功");
        }
        catch(Exception ex)
        {
            Log.Error($"setting保存失败：{ex}");
        }
    }

    private void OpenPath()
    {
        string path = Path.Combine(_defaultPath, Constants.Folder_Need);
        if(Directory.Exists(path))
            _ = Process.Start("explorer.exe", path);
    }

    private void UpdateVersion()
    {
        _ = Process.Start(new ProcessStartInfo(Sharedstring.BaseUpdateUrl) { UseShellExecute = true });
    }

    private void OpenSettings()
    {
        ShowSettingsDialog?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler ShowSettingsDialog;

    private double _currentWidth = 960;

    public double CurrentWidth
    {
        get => _currentWidth;
        set
        {
            _currentWidth = value;
            OnPropertyChanged();
        }
    }

    private void ChangeSize(bool up)
    {
        int level = (int)Math.Round(CurrentWidth / 64.0);
        level = up ? level + 1 : Math.Max(1, level - 1);
        CurrentWidth = level * 64;
    }
}
