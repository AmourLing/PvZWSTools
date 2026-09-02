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
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;
using PvZWSTools_Avalonia.Helpers;
using PvZWSTools_Avalonia.Services;
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
    private AndroidUpdateService _updateService;
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
                _ = CheckForUpdatesAsync(isManual: true);
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

        _updateService = new AndroidUpdateService(this);

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

            // 启动时自动检查更新（受 AutoCheckUpdateEnabled 控制）
            if(_updateService != null)
            {
                _ = CheckForUpdatesAsync(isManual: false);
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

    // --- 自动更新逻辑 ---

    /// <summary>
    /// 检查更新：调 GitHub/Gitee Release → 比较版本号 → 弹出更新对话框（带渠道选择）。
    /// </summary>
    private async Task CheckForUpdatesAsync(bool isManual)
    {
        if(_updateService == null) return;

        // 非手动模式且未开启"启动时自动检查更新"则跳过
        if(!isManual && (_appSettings == null || !_appSettings.AutoCheckUpdateEnabled))
            return;

        var info = await _updateService.CheckForUpdatesAsync(Sharedstring.AssetNameAndroid);
        if(info == null)
        {
            if(isManual)
            {
                RunOnUiThread(() =>
                    Toast.MakeText(this, "检查更新失败，请稍后重试", ToastLength.Short).Show());
            }
            return;
        }

        if(!info.IsNewerThan(_updateService.CurrentVersion))
        {
            if(isManual)
            {
                RunOnUiThread(() =>
                    Toast.MakeText(this, $"当前已是最新版本（{info.TagName}）", ToastLength.Short).Show());
            }
            return;
        }

        // 弹出带渠道选择的更新对话框
        var choice = await ShowUpdateDialogAsync(info);
        if(choice == UpdateSource.None) return;

        // 百度网盘：打开浏览器跳转
        if(choice == UpdateSource.Baidu)
        {
            OpenBaiduNetdisk(info);
            return;
        }

        // GitHub/Gitee：根据选择调整下载优先级
        if(choice == UpdateSource.Gitee && !string.IsNullOrEmpty(info.DownloadUrlFallback))
        {
            var githubUrl = info.DownloadUrl;
            info.DownloadUrl = info.DownloadUrlFallback;
            info.DownloadUrlFallback = githubUrl;
            info.Source = "gitee";
        }
        else
        {
            info.Source = "github";
        }

        await DownloadAndInstallAsync(info);
    }

    private enum UpdateSource { None, Github, Gitee, Baidu }

    /// <summary>
    /// 显示带渠道选择的更新对话框。返回用户选择的渠道（None=取消）。
    /// </summary>
    private Task<UpdateSource> ShowUpdateDialogAsync(UpdateInfo info)
    {
        var tcs = new TaskCompletionSource<UpdateSource>();
        RunOnUiThread(() =>
        {
            var dialogView = LayoutInflater.From(this)!.Inflate(Resource.Layout.update_dialog, null);

            // 填充版本信息
            var currentVerText = dialogView.FindViewById<TextView>(Resource.Id.current_version_text)!;
            currentVerText.Text = _updateService!.CurrentVersionDisplay;

            var newTagText = dialogView.FindViewById<TextView>(Resource.Id.new_version_tag)!;
            newTagText.Text = info.TagName;

            var sizeText = dialogView.FindViewById<TextView>(Resource.Id.new_version_size)!;
            sizeText.Text = info.Size.HasValue ? $"（{info.Size.Value / 1048576.0:F1} MB）" : "";

            var notesText = dialogView.FindViewById<TextView>(Resource.Id.release_notes)!;
            notesText.Text = string.IsNullOrWhiteSpace(info.ReleaseNotes) ? "暂无更新说明" : info.ReleaseNotes;

            // 渠道可用性
            bool hasGithub = !string.IsNullOrEmpty(info.DownloadUrl);
            bool hasGitee = !string.IsNullOrEmpty(info.DownloadUrlFallback);
            bool hasBaidu = !string.IsNullOrEmpty(info.DownloadUrlBaidu);

            var radioGithub = dialogView.FindViewById<RadioButton>(Resource.Id.radio_github)!;
            var radioGitee = dialogView.FindViewById<RadioButton>(Resource.Id.radio_gitee)!;
            var radioBaidu = dialogView.FindViewById<RadioButton>(Resource.Id.radio_baidu)!;

            radioGithub.Enabled = hasGithub;
            radioGitee.Enabled = hasGitee;
            radioBaidu.Enabled = hasBaidu;

            // 默认选有可用的渠道（优先 GitHub → Gitee → 百度网盘）
            if(hasGithub) radioGithub.Checked = true;
            else if(hasGitee) radioGitee.Checked = true;
            else if(hasBaidu) radioBaidu.Checked = true;

            var dialog = new AndroidX.AppCompat.App.AlertDialog.Builder(this)
                .SetTitle("检查更新")
                .SetView(dialogView)
                .SetCancelable(true)
                .SetPositiveButton("下载并更新", (_, _) =>
                {
                    UpdateSource choice = UpdateSource.None;
                    if(radioGithub.Checked && hasGithub) choice = UpdateSource.Github;
                    else if(radioGitee.Checked && hasGitee) choice = UpdateSource.Gitee;
                    else if(radioBaidu.Checked && hasBaidu) choice = UpdateSource.Baidu;
                    tcs.TrySetResult(choice);
                })
                .SetNegativeButton("取消", (_, _) => tcs.TrySetResult(UpdateSource.None))
                .Create();

            dialog.Show();
        });
        return tcs.Task;
    }

    /// <summary>
    /// 打开百度网盘链接（调起浏览器或百度网盘 App）。
    /// </summary>
    private void OpenBaiduNetdisk(UpdateInfo info)
    {
        if(string.IsNullOrEmpty(info.DownloadUrlBaidu)) return;

        string codeText = !string.IsNullOrEmpty(info.BaiduExtractCode)
            ? $"提取码：{info.BaiduExtractCode}\n\n"
            : "";

        RunOnUiThread(() =>
        {
            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this)
                .SetTitle("打开百度网盘")
                .SetMessage($"{codeText}即将打开浏览器，请手动下载 APK 后安装。\n\n下载完成后，关闭本程序，安装新 APK 即可。")
                .SetPositiveButton("打开浏览器", (_, _) =>
                {
                    try
                    {
                        var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(info.DownloadUrlBaidu));
                        StartActivity(intent);
                    }
                    catch(Exception ex)
                    {
                        Log.Error("打开百度网盘失败", ex);
                        Toast.MakeText(this, "无法打开浏览器，请手动复制链接", ToastLength.Long).Show();
                    }
                })
                .SetNegativeButton("取消", (_, _) => { });
            builder.Create().Show();
        });
    }

    /// <summary>
    /// 下载并安装 APK（GitHub / Gitee 渠道）。
    /// </summary>
    private async Task DownloadAndInstallAsync(UpdateInfo info)
    {
        // 显示下载进度对话框
        var progress = new Progress<PvZWSTools_Shared.Models.DownloadProgress>(p =>
        {
            RunOnUiThread(() =>
            {
                if(_extractDialog != null && _extractDialog.IsShowing)
                {
                    int pct = p.Percentage ?? 0;
                    string speedText = p.BytesPerSecond.HasValue ? $"（{FormatSpeed(p.BytesPerSecond.Value)}）" : "";
                    string totalText = p.TotalBytes.HasValue ? $" / {p.TotalBytes.Value / 1048576.0:F1} MB" : "";
                    _extractDialog.SetMessage($"正在下载更新包 {pct}%{totalText}{speedText}...");
                }
            });
        });

        RunOnUiThread(() =>
        {
            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            builder.SetTitle("正在下载更新");
            builder.SetMessage("正在后台下载更新包，请稍候...");
            builder.SetCancelable(false);
            ProgressBar progressBar = new ProgressBar(this) { Indeterminate = true };
            builder.SetView(progressBar);
            _extractDialog = builder.Create();
            _extractDialog.Show();
        });

        string? downloaded = null;
        try
        {
            downloaded = await _updateService!.DownloadUpdateAsync(info, progress);
        }
        catch(Exception ex)
        {
            Log.Error("下载更新失败", ex);
        }
        finally
        {
            RunOnUiThread(HideExtractDialog);
        }

        if(string.IsNullOrEmpty(downloaded))
        {
            RunOnUiThread(() =>
                Toast.MakeText(this, "下载更新包失败，请稍后重试", ToastLength.Long).Show());
            return;
        }

        bool applied = await _updateService.ApplyUpdateAsync(downloaded);
        if(!applied)
        {
            RunOnUiThread(() =>
                Toast.MakeText(this, "应用更新失败，请前往发布页手动下载", ToastLength.Long).Show());
        }
        // 应用成功时系统安装器已弹起
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
        var chkAutoCheckUpdate = CreateSettingCheckBox(this, "启动时自动检查更新", _appSettings.AutoCheckUpdateEnabled, 10);

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
        layout.AddView(chkAutoCheckUpdate);
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
            _appSettings.AutoCheckUpdateEnabled = chkAutoCheckUpdate.Checked;
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

    private static string FormatSpeed(double bytesPerSecond)
    {
        if(bytesPerSecond >= 1048576)
            return $"{bytesPerSecond / 1048576.0:F1} MB/s";
        if(bytesPerSecond >= 1024)
            return $"{bytesPerSecond / 1024.0:F1} KB/s";
        return $"{bytesPerSecond} B/s";
    }
}
