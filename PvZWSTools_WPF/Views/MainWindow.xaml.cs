using System.IO;
using System.Windows;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;
using PvZWSTools_Shared.ViewModels;
using PvZWSTools_WPF.Platform;
using PvZWSTools_WPF.Services;
using static PvZWSTools_Shared.Sharedstring;

namespace PvZWSTools_WPF.Views;

public partial class MainWindow:Window
{
    private readonly MainWindowViewModel _viewModel;
    private bool _isResizing = false;
    private readonly IUpdateService _updateService;
    private bool _isUpdateOnlyMode; // 过期后进入"仅更新模式"，防止自动检查重复弹 UpdateWindow

    public MainWindow()
    {
        InitializeComponent();

        Title = Title + "_" + CompileTime.GetCompileTime()?.ToString("yyyyMMdd");
        if(IsBetaVersion)
        {
            Title += "_Beta";
        }
#if DEBUG // 调试模式下隐藏花园编辑页面
        GardenPage.Visibility = Visibility.Visible;
#endif

        var uiThread = new WpfUiThreadInvoker(Dispatcher);
        var connection = new ConnectionService(uiThread);
        string defaultPath = Directory.GetCurrentDirectory();
        var settingsService = new SettingsService(defaultPath);
        var messageProcessor = new MessageProcessor();
        var dialogService = new DialogService();

        // 自动更新服务（WPF 端实现：bat 重启替换）
        _updateService = new PvZWSTools_WPF.Services.WpfUpdateService();

        _viewModel = new MainWindowViewModel(
            connection,
            settingsService,
            defaultPath,
            dialogService,
            messageProcessor,
            uiThread,
            new WpfUserNotifier(),
            _updateService
        );

        // 启动后异步检查更新（受 AutoCheckUpdateEnabled 控制）
        _ = _viewModel.CheckAndApplyUpdateAsync(isManual: false);

        _viewModel.ShowSettingsDialog += (s, e) =>
        {
            var dialog = new SettingDialog(settingsService)
            {
                Owner = this
            };
            if(dialog.ShowDialog() == true)
            {
                _viewModel.ReloadSettingsFromService();
            }
        };

        // 用户点击"获取更新"或启动时自动检查发现新版本 → 打开 UpdateWindow（专门的更新窗口，支持渠道选择）
        _viewModel.ShowUpdateWindowRequested += (s, e) =>
        {
            // 过期"仅更新模式"下，自动检查的 UpdateWindow 已由 Loaded 事件弹出，这里跳过
            if(_isUpdateOnlyMode && e.IsAuto) return;

            var win = new UpdateWindow(_updateService, e.Info)
            {
                Owner = this
            };
            if(e.IsAuto)
            {
                // 启动自动检查：非阻塞弹出，不打断用户操作
                win.Show();
            }
            else
            {
                // 用户主动点击：阻塞弹窗
                win.ShowDialog();
            }
        };

        DataContext = _viewModel;
    }

    /// <summary>
    /// 仅更新模式：标题提示 + 自动弹出 UpdateWindow 引导更新。
    /// 用于程序已过期但用户选择"检查更新"的场景。
    /// </summary>
    public void EnterUpdateOnlyMode()
    {
        _isUpdateOnlyMode = true;

        // 标题提示
        Title = "PvZWSTools — 仅更新模式（程序已过期）";

        // 禁用主功能 TabControl
        try { if(MainTabControl != null) MainTabControl.IsEnabled = false; } catch { }

        // 自动弹出 UpdateWindow 引导更新
        Loaded += async (_, _) =>
        {
            await System.Windows.Threading.Dispatcher.Yield();
            var win = new UpdateWindow(_updateService)
            {
                Owner = this
            };
            win.ShowDialog();
        };
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if(_isResizing) return;
        _isResizing = true;

        try
        {
            if(e.WidthChanged)
            {
                Height = Width / 1.6;
                _viewModel.UpdateSize(Width);
            }
        }
        finally
        {
            _isResizing = false;
        }
    }
}
