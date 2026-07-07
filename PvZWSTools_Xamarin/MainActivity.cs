using System;
using System.IO;
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
using static PvZWSTools_Shared.Sharedstring;

namespace PvZWSTools_Xamarin;

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
        AndroidX.Fragment.App.Fragment fragment = null;
        switch(id)
        {
            case Resource.Id.nav_others:
                fragment = new OthersFragment();
                break;

            case Resource.Id.nav_level:
                fragment = new LevelFragment();
                break;

            case Resource.Id.nav_resources:
                fragment = new ResourcesFragment();
                break;

            case Resource.Id.nav_plant:
                fragment = new PlantFragment();
                break;

            case Resource.Id.nav_zombie:
                fragment = new ZombieFragment();
                break;

            case Resource.Id.nav_spawning:
                fragment = new SpawningFragment();
                break;

            case Resource.Id.nav_board:
                fragment = new BoardFragment();
                break;

            case Resource.Id.nav_challenge:
                fragment = new ChallengeFragment();
                break;

            case Resource.Id.nav_setups:
                fragment = new SetupsFragment();
                break;

            case Resource.Id.nav_fun:
                fragment = new FunFragment();
                break;

            case Resource.Id.nav_script:
                fragment = new ScriptFragment();
                break;

            case Resource.Id.nav_connect:
                fragment = new ConnectionFragment();
                break;

            case Resource.Id.nav_settings:
                ShowSettingsDialog();
                return true;

            case Resource.Id.nav_updateversion:
                var uri = Android.Net.Uri.Parse(Sharedstring.BaseUpdateUrl);
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
                return true;

            default:
                return false;
        }
        if(fragment != null)
        {
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.content_frame, fragment)
                .Commit();
        }
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
        // 每次准备菜单时更新连接状态
        var settingsMenuItem = menu.FindItem(Resource.Id.action_settings);
        if(settingsMenuItem != null)
        {
            if(_isConnected)
            {
                settingsMenuItem.SetTitle(Resource.String.ws_connected);
            }
            else
            {
                settingsMenuItem.SetTitle(Resource.String.ws_disconnected);
            }
        }
        return base.OnPrepareOptionsMenu(menu);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {
        Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    // 保存WebSocket地址到设置
    public void SaveWebSocketAddress(string address)
    {
        if(_appSettings != null && !string.IsNullOrWhiteSpace(address))
        {
            _appSettings.LastWebSocketAddress = address;
            _appSettings.Save(_settingsPath);
        }
    }

    // 获取是否显示连接提醒
    public bool ShouldShowConnectionNotification()
    {
        return _appSettings?.ShowConnectionNotification ?? true;
    }

    // 更新连接状态的方法
    public void UpdateConnectionStatus(bool isConnected)
    {
        _isConnected = isConnected;

        RunOnUiThread(() =>
        {
            // 方法1：直接更新保存的菜单项引用
            if(_settingsMenuItem != null)
            {
                if(isConnected)
                {
                    _settingsMenuItem.SetTitle(Resource.String.ws_connected);
                }
                else
                {
                    _settingsMenuItem.SetTitle(Resource.String.ws_disconnected);
                }
            }
        });
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        Instance = this;

        Xamarin.Essentials.Platform.Init(this, savedInstanceState);
        SetContentView(Resource.Layout.activity_main);

        AppFilesPath = Application.Context.GetExternalFilesDir(null).AbsolutePath;

        string configPath = Path.Combine(AppFilesPath, "配置文件");
        if(!Directory.Exists(configPath))
            Directory.CreateDirectory(configPath);

        string logPath = Path.Combine(configPath, "Log");
        if(!Directory.Exists(logPath))
            Directory.CreateDirectory(logPath);

        LogHelper.Initialize(configPath);
        LogHelper.Log("MainActivity 启动 (私有存储)");

        // 设置文件路径
        _settingsPath = Path.Combine(configPath, "setting.json");
        _appSettings = AppSettings.Load(_settingsPath);

        // 显示解压提示对话框
        ShowExtractDialog();

        // 异步初始化资源管理器
        Task.Run(async () =>
        {
            try
            {
                // 初始化资源管理器（这会解压资源文件）
                ResourceManager.Initialize();

                // 回到UI线程更新界面
                RunOnUiThread(() =>
                {
                    // 关闭解压对话框
                    HideExtractDialog();

                    // 继续正常的主界面初始化
                    FinishInitialization();
                });
            }
            catch(Exception ex)
            {
                RunOnUiThread(() =>
                {
                    HideExtractDialog();
                    Toast.MakeText(this, $"资源初始化失败: {ex.Message}", ToastLength.Long).Show();

                    // 如果资源初始化失败，仍然尝试继续
                    FinishInitialization();
                });
            }
        });
    }

    protected override void OnDestroy()
    {
        Instance = null;
        ws?.Dispose();
        base.OnDestroy();
    }

    protected override void OnResume()
    {
        base.OnResume();

        // 应用设置
        ApplySettings();
    }

    // 应用设置
    private void ApplySettings()
    {
        if(ws != null)
        {
            ws.EnableAutoConnect(_appSettings.AutoConnect);
        }
    }

    private void FinishInitialization()
    {
        // 继续完成正常的初始化流程
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
                var compileTime = Helpers.CompileTimeHelper.GetCompileTime();
                string versionSuffix = compileTime?.ToString("yyyyMMdd") ?? "未知";
                string title = $"当前版本{versionSuffix}";
                if(IsBetaVersion)
                {
                    title += "-beta";
                }
                _ = versionItem.SetTitle(title);
            }

            if(SupportFragmentManager.BackStackEntryCount == 0)
            {
                SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.content_frame, new ConnectionFragment())
                    .Commit();
            }

            Task.Run(() =>
            {
                // 初始化WebSocketClient，启用自动连接
                ws = new WebSocketClient((isConnected) =>
                {
                    // 连接状态变化的回调
                    RunOnUiThread(() =>
                    {
                        UpdateConnectionStatus(isConnected);
                    });
                });

                // 应用设置
                RunOnUiThread(() =>
                {
                    ApplySettings();
                });
            });
        }
        catch(Exception ex)
        {
            Toast.MakeText(this, $"应用初始化失败: {ex.Message}", ToastLength.Long).Show();
        }
    }

    // 帮助方法：检查ReconnectInterval属性是否存在
    private System.Reflection.PropertyInfo GetReconnectIntervalProperty()
    {
        return typeof(AppSettings).GetProperty("ReconnectInterval");
    }

    private void HideExtractDialog()
    {
        if(_extractDialog != null && _extractDialog.IsShowing)
        {
            _extractDialog.Dismiss();
            _extractDialog = null;
        }
    }

    // 帮助方法：设置ReconnectInterval属性
    private void SetReconnectInterval(AppSettings settings, int value)
    {
        var property = GetReconnectIntervalProperty();
        if(property != null)
        {
            property.SetValue(settings, value);
        }
    }

    private void ShowExtractDialog()
    {
        // 创建解压提示对话框
        var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        builder.SetTitle("正在准备资源文件");
        builder.SetMessage("正在解压必要资源，请稍候...");
        builder.SetCancelable(false); // 不允许用户取消

        // 创建一个进度条
        ProgressBar progressBar = new ProgressBar(this);
        progressBar.Indeterminate = true;
        builder.SetView(progressBar);

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
        var chkAutoConnect = new CheckBox(this)
        {
            Text = "允许自动连接",
            Checked = _appSettings.AutoConnect
        };
        chkAutoConnect.SetTextSize(Android.Util.ComplexUnitType.Sp, 16);
        chkAutoConnect.SetPadding(0, 0, 0, 30);
        var chkShowNotification = new CheckBox(this)
        {
            Text = "显示连接提醒",
            Checked = _appSettings.ShowConnectionNotification
        };
        chkShowNotification.SetTextSize(Android.Util.ComplexUnitType.Sp, 16);
        chkShowNotification.SetPadding(0, 0, 0, 10);
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
        var txtReconnectIntervalLabel = new TextView(this)
        {
            Text = "重连间隔(毫秒):",
            TextSize = 16
        };
        txtReconnectIntervalLabel.SetPadding(0, 20, 0, 10);

        var txtReconnectInterval = new EditText(this)
        {
            Text = "250",
            InputType = Android.Text.InputTypes.ClassNumber
        };
        txtReconnectInterval.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
        txtReconnectInterval.SetPadding(10, 10, 10, 10);
        var gradientDrawable2 = new GradientDrawable();
        gradientDrawable2.SetCornerRadius(8f);
        gradientDrawable2.SetStroke(2, Android.Graphics.Color.LightGray);
        gradientDrawable2.SetColor(Android.Graphics.Color.White);
        txtReconnectInterval.Background = gradientDrawable2;
        layout.AddView(chkAutoConnect);
        layout.AddView(chkShowNotification);
        layout.AddView(txtWsAddressLabel);
        layout.AddView(txtWsAddress);
        layout.AddView(txtReconnectIntervalLabel);
        layout.AddView(txtReconnectInterval);

        var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        builder.SetTitle("设置");
        builder.SetView(layout);

        builder.SetPositiveButton("确定", (sender, e) =>
        {
            _appSettings.AutoConnect = chkAutoConnect.Checked;
            _appSettings.ShowConnectionNotification = chkShowNotification.Checked;
            var address = txtWsAddress.Text?.Trim();
            if(!string.IsNullOrEmpty(address))
            {
                _appSettings.LastWebSocketAddress = address;
            }

            if(int.TryParse(txtReconnectInterval.Text, out int interval) && interval > 0)
            {
                if(GetReconnectIntervalProperty() != null)
                {
                    SetReconnectInterval(_appSettings, interval);
                }
            }

            _appSettings.Save(_settingsPath);

            ApplySettings();

            Toast.MakeText(this, "设置已保存", ToastLength.Short).Show();
        });

        builder.SetNegativeButton("取消", (Android.Content.IDialogInterfaceOnClickListener)null);

        var dialog = builder.Create();
        dialog.Show();
    }
}
