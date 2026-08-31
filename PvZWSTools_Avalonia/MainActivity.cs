using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.Navigation;
using PvZWSTools_Shared;
using PvZWSTools_Avalonia.Helpers;
using static PvZWSTools_Shared.Sharedstring;

namespace PvZWSTools_Avalonia;

[Activity(Name = "net.pvz.pvzwstools.MainActivity",
          Label = "@string/app_name",
          Theme = "@style/AppTheme.NoActionBar",
          MainLauncher = true)]
public class MainActivity:AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
{
    public static WebSocketClient ws;
    private AppSettings _appSettings;
    private AndroidX.AppCompat.App.AlertDialog _extractDialog;
    private bool _isConnected = false;
    private IMenuItem _settingsMenuItem;
    private string _settingsPath;
    public static string AppFilesPath { get; private set; }

    public static MainActivity Instance { get; private set; }

    // 自动重连相关
    private Timer _reconnectTimer;
    private readonly object _reconnectLock = new object();

    private DateTime _lastReconnectAttempt = DateTime.MinValue;
    private const int RECONNECT_INTERVAL_MS = 3000;
    private bool _isReconnecting = false;

    /// <summary>
    /// 导航项 → (Fragment 工厂, 自动刷新按钮状态对应的控件子目录；null 表示切换时不自动刷新)。
    /// 新增界面只需在此注册，无需修改 OnNavigationItemSelected 逻辑。
    /// </summary>
    private static readonly Dictionary<int, (Func<AndroidX.Fragment.App.Fragment> Factory, string SubFolder)> NavFragmentMap = new()
    {
        { Resource.Id.nav_others, (() => new OthersFragment(), "杂项") },
        { Resource.Id.nav_level, (() => new LevelFragment(), null) },
        { Resource.Id.nav_resources, (() => new ResourcesFragment(), null) },
        { Resource.Id.nav_plant, (() => new PlantFragment(), "植物") },
        { Resource.Id.nav_zombie, (() => new ZombieFragment(), "僵尸") },
        { Resource.Id.nav_spawning, (() => new SpawningFragment(), "出怪") },
        { Resource.Id.nav_board, (() => new BoardFragment(), "战场") },
        { Resource.Id.nav_challenge, (() => new ChallengeFragment(), null) },
        { Resource.Id.nav_formation, (() => new FormationFragment(), null) },
        { Resource.Id.nav_fun, (() => new FunFragment(), "娱乐") },
        { Resource.Id.nav_script, (() => new ScriptFragment(), null) },
        { Resource.Id.nav_connect, (() => new ConnectionFragment(), null) },
    };

    public string GetLastWebSocketAddress()
    {
        return _appSettings?.LastWebSocketAddress ?? "ws://localhost:8080/Py";
    }

    public override void OnBackPressed()
    {
        DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
        if(drawer.IsDrawerOpen(GravityCompat.Start))
        {
            drawer.CloseDrawer(GravityCompat.Start);
        }
        else
        {
            base.OnBackPressed();
        }
    }

    public override bool OnCreateOptionsMenu(IMenu menu)
    {
        MenuInflater.Inflate(Resource.Menu.menu_main, menu);
        _settingsMenuItem = menu.FindItem(Resource.Id.action_settings);
        UpdateConnectionStatus(ws?.IsConnected ?? false);

        return true;
    }

    public bool OnNavigationItemSelected(IMenuItem item)
    {
        int id = item.ItemId;

        switch(id)
        {
            case Resource.Id.nav_settings:
                ShowSettingsDialog();
                return true;

            case Resource.Id.nav_updateversion:
                var uri = Android.Net.Uri.Parse(Sharedstring.BaseUpdateUrl);
                StartActivity(new Intent(Intent.ActionView, uri));
                return true;
        }

        if(!NavFragmentMap.TryGetValue(id, out var entry))
            return false;

        // 开启“允许自动更新按钮状态”时，切换界面自动刷新对应控件的按钮开关状态
        if(!string.IsNullOrEmpty(entry.SubFolder))
        {
            AutoUpdateButtonStatus(entry.SubFolder);
        }

        _ = SupportFragmentManager.BeginTransaction()
            .Replace(Resource.Id.content_frame, entry.Factory())
            .Commit();

        DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
        drawer.CloseDrawer(GravityCompat.Start);
        return true;
    }

    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        int id = item.ItemId;
        if(id == Resource.Id.action_settings)
        {
            ShowSettingsDialog();
            return true;
        }

        return base.OnOptionsItemSelected(item);
    }

    public override bool OnPrepareOptionsMenu(IMenu menu)
    {
        var settingsMenuItem = menu.FindItem(Resource.Id.action_settings);
        if(settingsMenuItem != null)
        {
            if(_isConnected)
            {
                _ = settingsMenuItem.SetTitle(Resource.String.ws_connected);
            }
            else
            {
                _ = settingsMenuItem.SetTitle(Resource.String.ws_disconnected);
            }
        }
        return base.OnPrepareOptionsMenu(menu);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    public void SaveWebSocketAddress(string address)
    {
        if(_appSettings != null && !string.IsNullOrWhiteSpace(address))
        {
            _appSettings.LastWebSocketAddress = address;
            _appSettings.Save(_settingsPath);
        }
    }

    public void UpdateConnectionStatus(bool isConnected)
    {
        _isConnected = isConnected;

        RunOnUiThread(() =>
        {
            if(_settingsMenuItem != null)
            {
                if(isConnected)
                {
                    _ = _settingsMenuItem.SetTitle(Resource.String.ws_connected);
                }
                else
                {
                    _ = _settingsMenuItem.SetTitle(Resource.String.ws_disconnected);
                }
            }

            // 通知当前显示的 ConnectionFragment 更新 UI
            var currentFragment = SupportFragmentManager.FindFragmentById(Resource.Id.content_frame);
            if(currentFragment is ConnectionFragment connFragment)
            {
                connFragment.NotifyConnectionStatusChanged(isConnected);
            }
        });
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        Instance = this;

        SetContentView(Resource.Layout.activity_main);

        AppFilesPath = Application.Context.GetExternalFilesDir(null).AbsolutePath;

        string configPath = Path.Combine(AppFilesPath, "配置文件");
        if(!Directory.Exists(configPath))
            _ = Directory.CreateDirectory(configPath);

        Log.Initialize(configPath);
        Log.Info("MainActivity 启动");
        Log.Info($"存储路径: {AppFilesPath}");

        _settingsPath = Path.Combine(configPath, "setting.json");
        _appSettings = AppSettings.Load(_settingsPath);

        ShowExtractDialog();

        _ = Task.Run(async () =>
        {
            try
            {
                ResourceManager.Initialize();
                RunOnUiThread(() =>
                {
                    HideExtractDialog();
                    FinishInitialization();
                });
            }
            catch(Exception ex)
            {
                Log.Error("资源初始化失败", ex);
                RunOnUiThread(() =>
                {
                    HideExtractDialog();
                    Toast.MakeText(this, $"资源初始化失败: {ex.Message}", ToastLength.Long).Show();
                    FinishInitialization();
                });
            }
        });
    }

    protected override void OnDestroy()
    {
        Log.Info("MainActivity 销毁");
        Instance = null;

        // 停止重连定时器
        StopReconnectTimer();

        // 先断开 WebSocket
        ws?.Dispose();

        // 最后关闭日志流
        Log.Shutdown();

        base.OnDestroy();
    }

    protected override void OnResume()
    {
        base.OnResume();
        ApplySettings();
    }

    protected override void OnPause()
    {
        base.OnPause();
        // 可选：如果在后台不想重连，可以暂停定时器
        // StopReconnectTimer();
    }

    private void ApplySettings()
    {
        if(ws != null)
        {
            ws.EnableSuppressConnectionMessage(_appSettings.SuppressConnectionMessage);

            // 根据设置启用或禁用自动重连
            if(_appSettings.AutoConnectEnabled)
            {
                StartReconnectTimer();
            }
            else
            {
                StopReconnectTimer();
            }
        }
    }

    /// <summary>
    /// 根据"允许自动更新按钮状态"设置，在切换界面时自动发送 GetButtonCheck 脚本刷新按钮开关状态。
    /// </summary>
    /// <param name="subFolder">控件子目录（杂项/植物/僵尸/出怪/战场/娱乐）</param>
    private void AutoUpdateButtonStatus(string subFolder)
    {
        if(_appSettings == null || !_appSettings.AllowAutoUpdateButtonStatus)
            return;
        if(ws == null || !ws.IsConnected)
            return;

        try
        {
            // 复用 OnCreate 中已确定的静态应用文件路径，避免重复查询外部存储目录
            var filepath = Path.Combine(AppFilesPath, "配置文件", "控件", subFolder, "GetButtonCheck.py");
            if(!File.Exists(filepath)) return;

            ws.Send(File.ReadAllText(filepath));
        }
        catch(Exception ex)
        {
            Log.Error($"自动更新按钮状态失败({subFolder})", ex);
        }
    }

    private void FinishInitialization()
    {
        try
        {
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            var menu = navigationView.Menu;
            var versionItem = menu.FindItem(Resource.Id.nav_versioninfo);
            if(versionItem != null)
            {
                var versionSuffix = CompileTimeHelper.GetCompileTimeString("yyyyMMdd");
                string title = $"当前版本{versionSuffix}";
                if(IsBetaVersion)
                {
                    title += "-beta";
                }
                _ = versionItem.SetTitle(title);
            }

            if(SupportFragmentManager.BackStackEntryCount == 0)
            {
                _ = SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.content_frame, new ConnectionFragment())
                    .Commit();
            }

            _ = Task.Run(() =>
            {
                ws = new WebSocketClient((isConnected) =>
                {
                    RunOnUiThread(() =>
                    {
                        UpdateConnectionStatus(isConnected);
                    });
                });

                RunOnUiThread(() =>
                {
                    ApplySettings();
                });
            });
        }
        catch(Exception ex)
        {
            Log.Error("应用初始化失败", ex);
            Toast.MakeText(this, $"应用初始化失败: {ex.Message}", ToastLength.Long).Show();
        }
    }

    // --- 自动重连逻辑 ---

    private void StartReconnectTimer()
    {
        if(_reconnectTimer == null)
        {
            Log.Info("[MainActivity] 启动自动重连定时器");
            _reconnectTimer = new Timer(ReconnectCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
        }
    }

    private void StopReconnectTimer()
    {
        if(_reconnectTimer != null)
        {
            Log.Info("[MainActivity] 停止自动重连定时器");
            _reconnectTimer.Dispose();
            _reconnectTimer = null;
        }
    }

    private void ReconnectCallback(object state)
    {
        if(!_appSettings.AutoConnectEnabled || _isReconnecting || _isConnected || ws == null)
            return;

        string address = GetLastWebSocketAddress();
        if(string.IsNullOrWhiteSpace(address))
            return;

        if((DateTime.Now - _lastReconnectAttempt).TotalMilliseconds < RECONNECT_INTERVAL_MS)
            return;

        lock(_reconnectLock)
        {
            if(_isReconnecting || _isConnected) return;

            _isReconnecting = true;
            _lastReconnectAttempt = DateTime.Now;
        }

        Log.Info($"[MainActivity] 自动重连尝试: {address}");

        _ = Task.Run(() =>
        {
            try
            {
                ws.Connect(address);
            }
            catch(Exception ex)
            {
                Log.Error($"[MainActivity] 自动重连异常: {ex.Message}");
            }
            finally
            {
                _isReconnecting = false;
            }
        });
    }

    // --- 对话框逻辑 ---

    private void HideExtractDialog()
    {
        if(_extractDialog != null && _extractDialog.IsShowing)
        {
            _extractDialog.Dismiss();
            _extractDialog = null;
        }
    }

    private void ShowExtractDialog()
    {
        var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        _ = builder.SetTitle("正在准备资源文件");
        _ = builder.SetMessage("正在解压必要资源，请稍候...");
        _ = builder.SetCancelable(false);

        ProgressBar progressBar = new ProgressBar(this);
        progressBar.Indeterminate = true;
        _ = builder.SetView(progressBar);

        _extractDialog = builder.Create();
        _extractDialog.Show();
    }

    private void ShowSettingsDialog()
    {
        var layout = new LinearLayout(this);
        layout.Orientation = Orientation.Vertical;
        layout.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent);
        layout.SetPadding(50, 30, 50, 30);

        var chkAutoConnect = CreateSettingCheckBox(this, "允许自动连接", _appSettings.AutoConnectEnabled, 30);
        var chkShowNotification = CreateSettingCheckBox(this, "取消连接提醒", _appSettings.SuppressConnectionMessage, 10);
        var chkAutoUpdateButtonStatus = CreateSettingCheckBox(this, "允许自动更新按钮状态", _appSettings.AllowAutoUpdateButtonStatus, 10);

        var txtWsAddressLabel = new TextView(this)
        {
            Text = "WebSocket地址:",
            TextSize = 16
        };
        txtWsAddressLabel.SetPadding(0, 20, 0, 10);

        var txtWsAddress = new EditText(this)
        {
            Text = _appSettings.LastWebSocketAddress,
            Hint = "请输入WebSocket地址"
        };
        txtWsAddress.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
        txtWsAddress.SetPadding(10, 10, 10, 10);

        var gradientDrawable = new GradientDrawable();
        gradientDrawable.SetCornerRadius(8f);
        gradientDrawable.SetStroke(2, Android.Graphics.Color.LightGray);
        gradientDrawable.SetColor(Android.Graphics.Color.White);
        txtWsAddress.Background = gradientDrawable;

        layout.AddView(chkAutoConnect);
        layout.AddView(chkShowNotification);
        layout.AddView(chkAutoUpdateButtonStatus);
        layout.AddView(txtWsAddressLabel);
        layout.AddView(txtWsAddress);

        var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        _ = builder.SetTitle("设置");
        _ = builder.SetView(layout);

        _ = builder.SetPositiveButton("确定", (sender, e) =>
        {
            _appSettings.AutoConnectEnabled = chkAutoConnect.Checked;
            _appSettings.SuppressConnectionMessage = chkShowNotification.Checked;
            _appSettings.AllowAutoUpdateButtonStatus = chkAutoUpdateButtonStatus.Checked;
            var address = txtWsAddress.Text?.Trim();
            if(!string.IsNullOrEmpty(address))
            {
                _appSettings.LastWebSocketAddress = address;
            }

            _appSettings.Save(_settingsPath);
            ApplySettings(); // 应用新设置

            Toast.MakeText(this, "设置已保存", ToastLength.Short).Show();
        });

        _ = builder.SetNegativeButton("取消", (Android.Content.IDialogInterfaceOnClickListener)null);

        var dialog = builder.Create();
        dialog.Show();
    }

    /// <summary>
    /// 创建设置对话框中的复选框（统一 16sp 字号与底部间距）。
    /// </summary>
    private static CheckBox CreateSettingCheckBox(Activity activity, string text, bool isChecked, int bottomPadding)
    {
        var checkBox = new CheckBox(activity)
        {
            Text = text,
            Checked = isChecked
        };
        checkBox.SetTextSize(Android.Util.ComplexUnitType.Sp, 16);
        checkBox.SetPadding(0, 0, 0, bottomPadding);
        return checkBox;
    }
}
