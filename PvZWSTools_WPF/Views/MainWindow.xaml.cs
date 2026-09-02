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
        var updateService = new PvZWSTools_WPF.Services.WpfUpdateService();

        _viewModel = new MainWindowViewModel(
            connection,
            settingsService,
            defaultPath,
            dialogService,
            messageProcessor,
            uiThread,
            new WpfUserNotifier(),
            updateService
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

        DataContext = _viewModel;
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
