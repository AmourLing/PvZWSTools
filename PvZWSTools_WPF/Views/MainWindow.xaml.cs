using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;
using PvZWSTools_Shared.ViewModels;
using PvZWSTools_WPF.Platform;
using PvZWSTools_WPF.Themes;
using PvZWSTools_WPF.Services;
using PvZWSTools_WPF.ViewModels;
using static PvZWSTools_Shared.Sharedstring;
using Lock = PvZWSTools_WPF.Helpers.Lock;

namespace PvZWSTools_WPF.Views;

public partial class MainWindow:Window
{
    private readonly MainWindowViewModel _viewModel;
    private ShellViewModel _shell = null!;
    private bool _isResizing = false;
    private readonly IUpdateService _updateService;
    private bool _isUpdateOnlyMode; // 过期后进入"仅更新模式"，防止自动检查重复弹 UpdateWindow

    public MainWindow()
    {
        InitializeComponent();
        UiThemeManager.RefreshWindowChrome(this); // 按模式设置字体/底色（经典=系统默认）

        Title = Title + "_" + CompileTime.GetCompileTime()?.ToString("yyyyMMdd");
        if(IsBetaVersion)
        {
            Title += "_Beta";
        }

        var uiThread = new WpfUiThreadInvoker(Dispatcher);
        var connection = new ConnectionService(uiThread);
        string defaultPath = AppContext.BaseDirectory;
        var settingsService = new SettingsService(defaultPath);
        var buttonStateService = new ButtonStateService(defaultPath);
        var messageProcessor = new MessageProcessor();
        var dialogService = new DialogService();

        // 自动更新服务（WPF 端实现：bat 重启替换）
        _updateService = new WpfUpdateService();

        _viewModel = new MainWindowViewModel(
            connection,
            settingsService,
            defaultPath,
            dialogService,
            messageProcessor,
            uiThread,
            new WpfUserNotifier(),
            _updateService,
            buttonStateService
        );
        _shell = new ShellViewModel(_viewModel);
        _shell.ScrollTopRequested += () => PageScroll.ScrollToVerticalOffset(0);
        DataContext = _shell;

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

        // 用户点击"状态管理"按钮 → 打开 StateWindow（保存/加载多组状态预设）
        _viewModel.ShowStateManagerRequested += (s, e) =>
        {
            try
            {
                var win = new StateWindow(
                    _viewModel.GetCurrentButtonStates,
                    _viewModel.LoadLastButtonStates,
                    _viewModel.GetDefaultButtonStates,
                    states => _viewModel.ApplyButtonStates(states))
                {
                    Owner = this
                };
                win.ShowDialog();
            }
            catch(Exception ex)
            {
                Log.Error($"打开状态管理窗口失败: {ex}");
                MessageBox.Show($"打开状态管理窗口失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        // 用户点击"获取更新"→ 直接打开 UpdateWindow，检查在窗口内进行（失败也有常驻网盘入口）
        _viewModel.OpenUpdateWindowRequested += (s, e) =>
        {
            var win = new UpdateWindow(_updateService)
            {
                Owner = this
            };
            win.ShowDialog();
        };

        // 启动时自动检查发现新版本 → 打开 UpdateWindow（专门的更新窗口，支持渠道选择）
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

        // 禁用主功能区（导航 + 内容），工具栏仍可用来更新
        try { if(MainContent != null) MainContent.IsEnabled = false; } catch { }
        try { if(NavList != null) NavList.IsEnabled = false; } catch { }

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

    // 打开 UI 选择界面（经典 UI / NewUI、黑夜 / 白天）
    private void UiButton_Click(object sender, RoutedEventArgs e)
    {
        new UiSelectWindow { Owner = this }.ShowDialog();
        UiThemeManager.RefreshWindowChrome(this);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _shell.Search = ((TextBox)sender).Text;
        _shell.ApplySearch();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if(_isResizing) return;
        _isResizing = true;

        try
        {
            if(e.WidthChanged)
            {
                _viewModel.UpdateSize(Width);
            }
        }
        finally
        {
            _isResizing = false;
        }
    }

    private ScrollViewer? _consoleScroll;

    /// <summary>控制台模板里的贴底只能写在代码里：换页签时模板会被拆掉重建，
    /// 所以订阅跟着 Loaded/Unloaded 走，不留着旧视图的引用。</summary>
    private void ConsoleScroll_Loaded(object sender, RoutedEventArgs e)
    {
        _consoleScroll = sender as ScrollViewer;
        _viewModel.Console.PropertyChanged += ConsoleLinesChanged;
        _consoleScroll?.ScrollToEnd();
    }

    private void ConsoleScroll_Unloaded(object sender, RoutedEventArgs e)
    {
        _consoleScroll = null;
        _viewModel.Console.PropertyChanged -= ConsoleLinesChanged;
    }

    /// <summary>关掉"自动滚动"时不抢用户正在读的位置。</summary>
    private void ConsoleLinesChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName != nameof(ConsoleViewModel.Shown)) return;
        if(_viewModel.Console.AutoScroll)
            _consoleScroll?.ScrollToEnd();
    }

    /// <summary>输入框里回车是换行（脚本常常不止一行），发送用 Ctrl+Enter。</summary>
    private void ConsoleInput_KeyDown(object sender, KeyEventArgs e)
    {
        if(e.Key != Key.Enter || (Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
        if(sender is TextBox box && box.Tag is ConsoleViewModel console)
            console.SendCommand.Execute(null);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // 窗口关闭时保存按钮状态与常用统计
        try { _viewModel.SaveButtonStates(); } catch { }
        try { _shell.SaveUsage(); } catch { }
        base.OnClosing(e);
    }
}
