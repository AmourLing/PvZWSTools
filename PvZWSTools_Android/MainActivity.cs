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
using PvZWSTools_Android.Platform;
using Google.Android.Material.Navigation;
using PvZWSTools_Shared;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Android.Services;
using static PvZWSTools_Shared.Helpers.Sharedstring;

namespace PvZWSTools_Android;

[Activity(Name = "net.pvz.pvzwstools.MainActivity",
          Label = "@string/app_name",
          Theme = "@style/AppTheme.NoActionBar",
          MainLauncher = true)]
public class MainActivity:AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
{
    private AndroidAppSettings _appSettings;
    private AndroidX.AppCompat.App.AlertDialog _extractDialog;
    private bool _isConnected = false;
    private IMenuItem _settingsMenuItem;
    private string _settingsPath;
    private AndroidUpdateService _updateService;
    public static string AppFilesPath { get; private set; }

    public static MainActivity Instance { get; private set; }

    /// <summary>
    /// 抽屉里的一行对应共享清单里的一页，全部由 CatalogFragment 渲染；
    /// 只有"快捷脚本"留着自己的 Fragment —— 那一页在 Android 上还要编辑脚本参数网格，
    /// 共享清单里只有一个下拉。
    /// </summary>
    private static readonly Dictionary<int, Func<AndroidX.Fragment.App.Fragment>> NavFragmentMap = new()
    {
        { Resource.Id.nav_favorite, () => new FavoritesFragment() },
        { Resource.Id.nav_others, () => new CatalogFragment("杂项") },
        { Resource.Id.nav_level, () => new CatalogFragment("关卡") },
        { Resource.Id.nav_resources, () => new CatalogFragment("资源") },
        { Resource.Id.nav_plant, () => new CatalogFragment("植物") },
        { Resource.Id.nav_zombie, () => new CatalogFragment("僵尸") },
        { Resource.Id.nav_spawning, () => new CatalogFragment("出怪") },
        { Resource.Id.nav_board, () => new CatalogFragment("战场") },
        { Resource.Id.nav_challenge, () => new CatalogFragment("挑战") },
        { Resource.Id.nav_formation, () => new CatalogFragment("阵型") },
        { Resource.Id.nav_fun, () => new CatalogFragment("娱乐") },
        { Resource.Id.nav_script, () => new ScriptFragment() },
        { Resource.Id.nav_connect, () => new ConnectionFragment() },
        { Resource.Id.nav_console, () => new ConsoleFragment() },
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
            // 抽屉打开时，返回键先关闭抽屉
            drawer.CloseDrawer(GravityCompat.Start);
            return;
        }

        // 抽屉关闭时，弹出退出确认，避免误触直接退出
        new AndroidX.AppCompat.App.AlertDialog.Builder(this)
            .SetTitle(Loc.T("退出应用"))
            .SetMessage(Loc.T("确定要退出吗？当前设置已自动保存。"))
            .SetPositiveButton(Loc.T("退出"), (sender, e) => SafeExit())
            .SetNegativeButton(Loc.T("取消"), (IDialogInterfaceOnClickListener)null)
            .Show();
    }

    /// <summary>
    /// 安全退出：显式保存"上次状态" → 断开连接 → 结束 Activity。
    /// </summary>
    private void SafeExit()
    {
        try
        {
            Log.Info("用户确认退出，执行安全退出");
            StopStateSaveTimer();
            PersistButtonStates();
            AppServices.Connection?.Disconnect();
        }
        catch(Exception ex)
        {
            Log.Error($"安全退出时发生异常: {ex.Message}");
        }
        Finish();
    }

    public override bool OnCreateOptionsMenu(IMenu menu)
    {
        MenuInflater.Inflate(Resource.Menu.menu_main, menu);
        _settingsMenuItem = menu.FindItem(Resource.Id.action_settings);
        UpdateConnectionStatus(AppServices.IsConnected);

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

            case Resource.Id.nav_statemanage:
                StateManageDialog.Show(this);
                return true;

            case Resource.Id.nav_updateversion:
                _ = CheckForUpdatesAsync(isManual: true);
                return true;
        }

        if(!NavFragmentMap.TryGetValue(id, out var factory))
            return false;

        var fragment = factory();
        _ = SupportFragmentManager.BeginTransaction()
            .Replace(Resource.Id.content_frame, fragment)
            .Commit();

        // 开启"允许自动更新按钮状态"时，切页顺带向游戏要一次真实开关状态。
        // 走共享层的同一个入口：回包由 MessageProcessor 解析，桌面和手机看到的是同一份结果。
        if(fragment is CatalogFragment page)
            AppServices.Root?.RefreshButtonStatesForPage(page.PageTitle);

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
                _ = settingsMenuItem.SetTitle(Loc.T(GetString(Resource.String.ws_connected)));
            }
            else
            {
                _ = settingsMenuItem.SetTitle(Loc.T(GetString(Resource.String.ws_disconnected)));
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

        // 连接成功后把开启的开关同步给游戏，由 MainWindowViewModel 自己做，
        // 这里只负责把状态反映到界面上。

        RunOnUiThread(() =>
        {
            if(_settingsMenuItem != null)
            {
                if(isConnected)
                {
                    _ = _settingsMenuItem.SetTitle(Loc.T(GetString(Resource.String.ws_connected)));
                }
                else
                {
                    _ = _settingsMenuItem.SetTitle(Loc.T(GetString(Resource.String.ws_disconnected)));
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

        // 日志要落文件：安卓这边显式指到外部存储的 配置文件\Log，
        // 和桌面端同一套相对布局，用户在文件管理器里就能翻到
        Log.Initialize(Path.Combine(configPath, Constants.Folder_Log));
        Log.Info("MainActivity 启动");
        Log.Info($"存储路径: {AppFilesPath}");

        _settingsPath = Path.Combine(configPath, "setting.json");
        _appSettings = AndroidAppSettings.Load(_settingsPath);

        // 语种要比重建清单早定：UnitCatalog 在 AppServices.Initialize 里拼标签。
        // 抽屉标题是从 XML 读出来的中文，就地过一遍文案表。
        Loc.SetLanguage(_appSettings.Language);
        LocalizeDrawer();

        StartStateSaveTimer();

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

    protected override void OnStop()
    {
        base.OnStop();

        // 状态管理：退后台/关闭时把当前状态保存为"上次状态"。
        // 放在 OnStop（而非仅 OnDestroy）：从最近任务划掉应用时 OnDestroy 可能不触发，
        // 导致磁盘上的上次状态永远不更新。
        PersistButtonStates();
    }

    protected override void OnDestroy()
    {
        Log.Info("MainActivity 销毁");

        // 停止状态保存定时器
        StopStateSaveTimer();

        // 状态管理：把当前状态保存为"上次状态"（OnStop 已保存过，此处兜底）
        PersistButtonStates();

        Instance = null;

        // 先断开 WebSocket
        AppServices.Connection?.Disconnect();

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
        // 常用统计是"点一次记一笔"，只有这份计数值得退到后台时整份覆写一次；
        // 收藏改动小又容易丢，ToggleFavorite 里当场就落盘了。
        AppServices.Favorites?.SaveUsage();
    }

    /// <summary>
    /// 从设置界面回来时，让 VM 重读一次设置（自动连接、是否发连接提醒都在那边）。
    /// </summary>
    private void ApplySettings()
    {
        AppServices.Root?.ReloadSettingsFromService();
    }

    /// <summary>beta 包过期后先弹密码框：对了就照常往下走。WPF 那边同一件事在
    /// Helpers\Lock.cs，规则共用共享层的 <see cref="BetaLock"/>。</summary>
    private void EnsureBetaAccess()
    {
        if(!BetaLock.IsExpired())
        {
            Log.Info($"程序有效期至 {BetaLock.ExpirationDateText()}，剩余 {BetaLock.RemainingDays()} 天");
            return;
        }

        Log.Info($"程序已过期（有效期至 {BetaLock.ExpirationDateText()}），需要密码验证");
        ShowPasswordDialog(BetaLock.MaxAttempts);
    }

    /// <summary>连错 attemptsLeft 次就退出。"检查更新"是打开下载页后退出——安卓没有
    /// WPF 那种"仅更新模式"（那要把每个 Fragment 挨个裁掉），先把人送到新版本跟前。</summary>
    private void ShowPasswordDialog(int attemptsLeft)
    {
        var input = new EditText(this)
        {
            InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword
        };
        _ = new AndroidX.AppCompat.App.AlertDialog.Builder(this)
            .SetTitle(Loc.T("程序已过期"))
            .SetMessage(Loc.T("程序已超过使用期限，请输入密码继续使用"))
            .SetView(input)
            .SetCancelable(false)
            .SetPositiveButton(Loc.T("确定"), (_, _) =>
            {
                if(BetaLock.VerifyPassword(input.Text))
                {
                    Log.Info("密码验证成功，继续启动程序");
                    return;
                }

                int left = attemptsLeft - 1;
                if(left <= 0)
                {
                    Log.Info("密码验证失败次数过多，程序退出");
                    Finish();
                    return;
                }

                Toast.MakeText(this, Loc.F("密码错误，还剩{0}次机会", left), ToastLength.Short)?.Show();
                ShowPasswordDialog(left);
            })
            .SetNeutralButton(Loc.T("检查更新"), (_, _) =>
            {
                Log.Info("用户选择检查更新，打开下载页后退出");
                try
                {
                    StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(BaseUpdateUrl)));
                }
                catch(Exception ex)
                {
                    Log.Error("打开更新地址失败", ex);
                }
                Finish();
            })
            .Show();
    }

    private void FinishInitialization()
    {
        try
        {
            // 共享 ViewModel 图 + 功能清单；必须在 配置文件 解压完之后，否则读不到选项 json
            Platform.AppServices.Initialize(this, AppFilesPath, _appSettings, _settingsPath);

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
                var versionSuffix = CompileTime.GetCompileTime()?.ToString("yyyyMMdd") ?? "未知";
                string title = Loc.F("当前版本{0}", versionSuffix);
                if(IsBetaVersion)
                {
                    title += "-beta";
                }
                _ = versionItem.SetTitle(title);
            }

            // beta 包的准入锁：和 WPF 的 App.OnStartup 用同一套规则（共享层 BetaLock）。
            // 放在自动检查更新之前——先解决"进不进得去"，别让两个框叠着弹。
            if(IsBetaVersion)
            {
                EnsureBetaAccess();
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

            // 唯一一条连接：状态变化由共享层的 ConnectionService 报上来，
            // 自动重连、连上后发 logo 都在 MainWindowViewModel 里，这里只负责刷 UI。
            AppServices.Connection.ConnectionStateChanged += (_, connected) =>
                UpdateConnectionStatus(connected);

            ApplySettings();
        }
        catch(Exception ex)
        {
            Log.Error("应用初始化失败", ex);
            Toast.MakeText(this, $"应用初始化失败: {ex.Message}", ToastLength.Long).Show();
        }
    }

    // --- 状态定时保存 ---

    /// <summary>状态定时保存周期（毫秒）。</summary>
    private const int StateSaveIntervalMs = 15000;
    private Timer? _stateSaveTimer;

    /// <summary>
    /// 启动状态定时保存：强杀进程（多任务清理/force-stop）不触发 OnStop/OnDestroy，
    /// 周期性保存保证改动最多丢失一个周期。
    /// </summary>
    private void StartStateSaveTimer()
    {
        if(_stateSaveTimer == null)
        {
            Log.Info("[MainActivity] 启动状态定时保存（每 15 秒）");
            _stateSaveTimer = new Timer(StateSaveCallback, null, StateSaveIntervalMs, StateSaveIntervalMs);
        }
    }

    private void StopStateSaveTimer()
    {
        _stateSaveTimer?.Dispose();
        _stateSaveTimer = null;
    }

    /// <summary>
    /// 把当前各界面状态存成"上次状态"。开关现在住在共享 ViewModel 的属性上，
    /// 所以直接让 VM 导出并落盘，不再需要先从 Fragment 的 Map 里回捞一次。
    /// 定时器和 AppServices.Initialize 有先后，Root 可能还没建好，这里要判空。
    /// </summary>
    private void PersistButtonStates()
    {
        try
        {
            AppServices.Root?.SaveButtonStates();
        }
        catch(Exception ex)
        {
            Log.Error($"保存按钮状态失败: {ex.Message}");
        }
    }

    private void StateSaveCallback(object state)
    {
        RunOnUiThread(PersistButtonStates);
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
        _ = builder.SetTitle(Loc.T("正在准备资源文件"));
        _ = builder.SetMessage(Loc.T("正在解压必要资源，请稍候..."));
        _ = builder.SetCancelable(false);

        ProgressBar progressBar = new ProgressBar(this);
        progressBar.Indeterminate = true;
        _ = builder.SetView(progressBar);

        _extractDialog = builder.Create();
        _extractDialog.Show();
    }

    // --- 自动更新逻辑 ---

    /// <summary>
    /// 检查更新入口。手动触发时先弹更新对话框（网盘渠道常驻可跳），检查在对话框内进行：
    /// 查到新版本填充版本信息，失败/无更新则在对话框内提示；
    /// 启动自动检查仍是查到新版本才弹窗，静默失败。
    /// </summary>
    private async Task CheckForUpdatesAsync(bool isManual)
    {
        if(_updateService == null) return;

        // 非手动模式且未开启"启动时自动检查更新"则跳过
        if(!isManual && (_appSettings == null || !_appSettings.AutoCheckUpdateEnabled))
            return;

        UpdateInfo? preFetched = null;
        if(!isManual)
        {
            // 启动自动检查：查到新版本才弹窗，否则静默
            preFetched = await _updateService.CheckForUpdatesAsync(PvZWSTools_Shared.Helpers.Sharedstring.AssetNameAndroid);
            if(preFetched == null || !preFetched.IsNewerThan(_updateService.CurrentVersion))
                return;
        }

        var (choice, netdisk, info) = await ShowUpdateDialogAsync(preFetched);
        if(choice == UpdateSource.None) return;

        // 网盘渠道：打开浏览器跳转，用户手动下载 APK 后安装
        if(choice == UpdateSource.Netdisk && netdisk != null)
        {
            OpenNetdiskPage(netdisk);
            return;
        }

        // GitHub/Gitee：DownloadUpdateAsync 按 Source 排的渠道优先、另一源兜底
        info!.Source = choice == UpdateSource.Gitee ? "gitee" : "github";

        await DownloadAndInstallAsync(info);
    }

    private enum UpdateSource { None, Github, Gitee, Netdisk }

    /// <summary>
    /// 显示带渠道选择的更新对话框。info 为 null（手动入口）时先弹窗、由对话框内继续检查：
    /// 查到新版本填充版本信息，否则在对话框内提示"已是最新/检查失败"；
    /// 无论检查结果如何，网盘渠道都常驻可跳。返回用户选择的渠道（None=取消）、
    /// 网盘渠道对应的分享链接，以及检查到的版本信息（GitHub/Gitee 下载用）。
    /// </summary>
    private Task<(UpdateSource Source, NetdiskChannel? Netdisk, UpdateInfo? Info)> ShowUpdateDialogAsync(UpdateInfo? info)
    {
        var tcs = new TaskCompletionSource<(UpdateSource, NetdiskChannel?, UpdateInfo?)>();
        RunOnUiThread(() =>
        {
            var dialogView = LayoutInflater.From(this)!.Inflate(Resource.Layout.update_dialog, null);
            AndroidUi.LocalizeTexts(dialogView);          // 渠道说明这些文字写在布局 XML 里

            var currentVerText = dialogView.FindViewById<TextView>(Resource.Id.current_version_text)!;
            currentVerText.Text = _updateService!.CurrentVersionDisplay;

            var newTagText = dialogView.FindViewById<TextView>(Resource.Id.new_version_tag)!;
            var sizeText = dialogView.FindViewById<TextView>(Resource.Id.new_version_size)!;
            var notesText = dialogView.FindViewById<TextView>(Resource.Id.release_notes)!;
            var statusTitle = dialogView.FindViewById<TextView>(Resource.Id.update_status_title)!;

            var radioGroup = dialogView.FindViewById<RadioGroup>(Resource.Id.source_radio_group)!;
            var radioGithub = dialogView.FindViewById<RadioButton>(Resource.Id.radio_github)!;
            var radioGitee = dialogView.FindViewById<RadioButton>(Resource.Id.radio_gitee)!;

            // 网盘渠道不进 Release，是固定分享链接；按 readme「下载」一节的顺序插到 GitHub/Gitee 前面
            var netdiskRadios = new List<(NetdiskChannel Channel, RadioButton Radio)>();
            foreach(var channel in NetdiskChannel.All)
            {
                var radio = new RadioButton(this)
                {
                    Text = channel.Display,
                    TextSize = 13f
                };
                // 按已插入数量定位下标（0,1,2…），逐个排在 GitHub/Gitee 之前
                radioGroup.AddView(radio, netdiskRadios.Count);
                netdiskRadios.Add((channel, radio));
            }

            // 检查结果填充对话框；GitHub/Gitee 直链到手才启用对应单选钮
            UpdateInfo? found = info;
            void ApplyInfo(UpdateInfo i)
            {
                found = i;
                statusTitle.Text = Loc.T("发现新版本！");
                statusTitle.SetTextColor(Android.Graphics.Color.ParseColor("#2E7D32"));
                newTagText.Text = i.TagName;
                sizeText.Text = i.Size.HasValue ? $"（{i.Size.Value / 1048576.0:F1} MB）" : "";
                notesText.Text = string.IsNullOrWhiteSpace(i.ReleaseNotes) ? "暂无更新说明" : i.ReleaseNotes;
                radioGithub.Enabled = !string.IsNullOrEmpty(i.GithubUrl);
                radioGitee.Enabled = !string.IsNullOrEmpty(i.GiteeUrl);
            }

            if(found != null)
            {
                ApplyInfo(found);
            }
            else
            {
                statusTitle.Text = Loc.T("正在检查更新...");
                statusTitle.SetTextColor(Android.Graphics.Color.ParseColor("#555555"));
                notesText.Text = Loc.T("正在从 GitHub / Gitee 检查新版本，请稍候。\n\n无论检查结果如何，都可以随时通过下方网盘渠道手动下载。");

                _ = CheckInBackgroundAsync();
                async Task CheckInBackgroundAsync()
                {
                    var result = await _updateService!.CheckForUpdatesAsync(PvZWSTools_Shared.Helpers.Sharedstring.AssetNameAndroid);
                    RunOnUiThread(() =>
                    {
                        if(result == null)
                        {
                            statusTitle.Text = Loc.T("检查更新失败");
                            statusTitle.SetTextColor(Android.Graphics.Color.ParseColor("#C62828"));
                            notesText.Text = Loc.T("无法从 GitHub / Gitee 获取版本信息，请稍后重试，或通过下方网盘渠道手动下载查看。");
                        }
                        else if(!result.IsNewerThan(_updateService!.CurrentVersion))
                        {
                            statusTitle.Text = Loc.T("当前已是最新版本");
                            statusTitle.SetTextColor(Android.Graphics.Color.ParseColor("#2E7D32"));
                            newTagText.Text = result.TagName;
                            notesText.Text = Loc.T("服务器最新版本如上，仍可通过下方网盘渠道手动查看。");
                        }
                        else
                        {
                            ApplyInfo(result);
                        }
                    });
                }
            }

            // 默认选夸克网盘（网盘列表首位，固定分享链接恒可用）；GitHub/Gitee 仍可手动选
            netdiskRadios[0].Radio.Checked = true;

            var dialog = new AndroidX.AppCompat.App.AlertDialog.Builder(this)
                .SetTitle(Loc.T("检查更新"))
                .SetView(dialogView)
                .SetCancelable(true)
                .SetPositiveButton(Loc.T("下载并更新"), (_, _) =>
                {
                    if(radioGithub.Checked && radioGithub.Enabled)
                    {
                        tcs.TrySetResult((UpdateSource.Github, null, found));
                        return;
                    }
                    if(radioGitee.Checked && radioGitee.Enabled)
                    {
                        tcs.TrySetResult((UpdateSource.Gitee, null, found));
                        return;
                    }
                    foreach(var (channel, radio) in netdiskRadios)
                    {
                        if(radio.Checked)
                        {
                            tcs.TrySetResult((UpdateSource.Netdisk, channel, found));
                            return;
                        }
                    }
                    tcs.TrySetResult((UpdateSource.None, null, found));
                })
                .SetNegativeButton(Loc.T("取消"), (_, _) => tcs.TrySetResult((UpdateSource.None, null, found)))
                .Create();

            dialog.Show();
        });
        return tcs.Task;
    }

    /// <summary>
    /// 打开网盘分享链接（调起浏览器或对应网盘 App）。
    /// </summary>
    private void OpenNetdiskPage(NetdiskChannel netdisk)
    {
        string codeText = !string.IsNullOrEmpty(netdisk.ExtractCode)
            ? $"提取码：{netdisk.ExtractCode}\n\n"
            : "";

        RunOnUiThread(() =>
        {
            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this)
                .SetTitle($"打开{netdisk.Name}")
                .SetMessage($"{codeText}即将打开浏览器，请手动下载 APK 后安装。\n\n下载完成后，关闭本程序，安装新 APK 即可。")
                .SetPositiveButton(Loc.T("打开浏览器"), (_, _) =>
                {
                    try
                    {
                        var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(netdisk.Url));
                        StartActivity(intent);
                    }
                    catch(Exception ex)
                    {
                        Log.Error($"打开{netdisk.Name}失败", ex);
                        Toast.MakeText(this, Loc.T("无法打开浏览器，请手动复制链接"), ToastLength.Long).Show();
                    }
                })
                .SetNegativeButton(Loc.T("取消"), (_, _) => { });
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
            builder.SetTitle(Loc.T("正在下载更新"));
            builder.SetMessage(Loc.T("正在后台下载更新包，请稍候..."));
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
                Toast.MakeText(this, Loc.T("下载更新包失败，请稍后重试"), ToastLength.Long).Show());
            return;
        }

        bool applied = await _updateService.ApplyUpdateAsync(downloaded);
        if(!applied)
        {
            RunOnUiThread(() =>
                Toast.MakeText(this, Loc.T("应用更新失败，请前往发布页手动下载"), ToastLength.Long).Show());
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

        var chkAutoConnect = CreateSettingCheckBox(this, Loc.T("允许自动连接"), _appSettings.AutoConnectEnabled, 30);
        var chkShowNotification = CreateSettingCheckBox(this, Loc.T("取消连接提醒"), _appSettings.SuppressConnectionMessage, 10);
        var chkAutoUpdateButtonStatus = CreateSettingCheckBox(this, Loc.T("允许自动更新按钮状态"), _appSettings.AllowAutoUpdateButtonStatus, 10);
        var chkAutoCheckUpdate = CreateSettingCheckBox(this, Loc.T("启动时自动检查更新"), _appSettings.AutoCheckUpdateEnabled, 10);
        var chkAutoApplyLastState = CreateSettingCheckBox(this, Loc.T("自动应用上次配置"), _appSettings.AutoApplyLastState, 10);

        // 语言名故意不翻译：这一组就是切语言的入口，两种语言下都显示各自本名才不会找不到自己。
        var rbLangZh = new RadioButton(this) { Text = "简体中文" };
        var rbLangEn = new RadioButton(this) { Text = "English" };
        rbLangZh.Checked = !Loc.IsEnglish;
        rbLangEn.Checked = Loc.IsEnglish;
        var langGroup = new RadioGroup(this) { Orientation = Orientation.Vertical };
        langGroup.AddView(rbLangZh);
        langGroup.AddView(rbLangEn);
        var txtLangLabel = new TextView(this) { Text = Loc.T("界面语言（改后要重启）"), TextSize = 16 };
        txtLangLabel.SetPadding(0, 20, 0, 6);

        var txtWsAddressLabel = new TextView(this)
        {
            Text = Loc.T("WebSocket地址:"),
            TextSize = 16
        };
        txtWsAddressLabel.SetPadding(0, 20, 0, 10);

        var txtWsAddress = new EditText(this)
        {
            Text = _appSettings.LastWebSocketAddress,
            Hint = Loc.T("请输入WebSocket地址")
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
        layout.AddView(chkAutoApplyLastState);
        layout.AddView(txtLangLabel);
        layout.AddView(langGroup);
        layout.AddView(txtWsAddressLabel);
        layout.AddView(txtWsAddress);

        var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        _ = builder.SetTitle(Loc.T("设置"));
        _ = builder.SetView(layout);

        _ = builder.SetPositiveButton(Loc.T("确定"), (sender, e) =>
        {
            _appSettings.AutoConnectEnabled = chkAutoConnect.Checked;
            _appSettings.SuppressConnectionMessage = chkShowNotification.Checked;
            _appSettings.AllowAutoUpdateButtonStatus = chkAutoUpdateButtonStatus.Checked;
            _appSettings.AutoCheckUpdateEnabled = chkAutoCheckUpdate.Checked;
            _appSettings.AutoApplyLastState = chkAutoApplyLastState.Checked;
            var address = txtWsAddress.Text?.Trim();
            if(!string.IsNullOrEmpty(address))
            {
                _appSettings.LastWebSocketAddress = address;
            }

            string wantedLang = rbLangEn.Checked ? Loc.En : Loc.Zh;
            bool relaunch = wantedLang != (string.IsNullOrEmpty(_appSettings.Language) ? Loc.Zh : _appSettings.Language);
            _appSettings.Language = wantedLang;

            _appSettings.Save(_settingsPath);
            ApplySettings(); // 应用新设置

            Toast.MakeText(this, Loc.T("设置已保存"), ToastLength.Short).Show();
            if(relaunch)
                RestartForNewLanguage();   // 清单标签是启动时拼好的，换语种只能重来一遍
        });

        _ = builder.SetNegativeButton(Loc.T("取消"), (Android.Content.IDialogInterfaceOnClickListener)null);

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

    /// <summary>抽屉那 17 条标题在 activity_main_drawer.xml 里是写死的中文。
    /// 翻页比对用的是 Resource.Id 而不是标题文字，所以翻译它们不影响选页回调。</summary>
    private void LocalizeDrawer()
    {
        var nav = FindViewById<NavigationView>(Resource.Id.nav_view);
        if(nav == null)
            return;
        for(int i = 0; i < nav.HeaderCount; i++)
            AndroidUi.LocalizeTexts(nav.GetHeaderView(i));

        var menu = nav.Menu;
        if(menu == null)
            return;
        for(int i = 0; i < menu.Size(); i++)
        {
            var item = menu.GetItem(i);
            string title = item?.TitleFormatted?.ToString();
            if(item != null && !string.IsNullOrEmpty(title))
                item.SetTitle(Loc.T(title));

            var sub = item.SubMenu;
            if(sub == null)
                continue;
            // ISubMenu 没有读标题的 getter，但分组头就是父项那条标题，所以拿刚读到的原文喂它
            if(!string.IsNullOrEmpty(title))
                sub.SetHeaderTitle(Loc.T(title));
            for(int j = 0; j < sub.Size(); j++)
            {
                var child = sub.GetItem(j);
                string childTitle = child?.TitleFormatted?.ToString();
                if(child != null && !string.IsNullOrEmpty(childTitle))
                    child.SetTitle(Loc.T(childTitle));
            }
        }
    }

    /// <summary>头部布局里的说明文字走 @string，平台按设备语言取值、不跟应用内语种，
    /// 所以 inflate 完再就地过一遍文案表。</summary>
    private static void LocalizeTexts(Android.Views.View v)
    {
        if(v == null)
            return;
        if(v is Android.Widget.TextView tv && !string.IsNullOrEmpty(tv.Text))
            tv.Text = Loc.T(tv.Text);
        if(v is Android.Views.ViewGroup group)
            for(int i = 0; i < group.ChildCount; i++)
                LocalizeTexts(group.GetChildAt(i));
    }
    /// <summary>换语种要重启：静态的 AppServices 里攥着按旧语种拼好的清单，
    /// 只 Recreate 一个 Activity 换不掉它。</summary>
    private void RestartForNewLanguage()
    {
        var intent = PackageManager?.GetLaunchIntentForPackage(PackageName ?? "");
        if(intent != null)
        {
            intent.AddFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
            StartActivity(intent);
        }
        Finish();
        Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
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
