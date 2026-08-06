using System.IO;
using System.Windows;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Services;
using PvZWSTools_WPF.ViewModels;
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
        var connection = new ConnectionService(Dispatcher);
        string defaultPath = Directory.GetCurrentDirectory();
        var settingsService = new SettingsService(defaultPath);

        _viewModel = new MainWindowViewModel(connection, settingsService, defaultPath);

        _viewModel.ShowSettingsDialog += (s, e) =>
        {
            var dialog = new SettingDialog(
                settingsService,
                "允许自动连接",
                "取消发送连接提醒"
                );
            dialog.Owner = this;
            _ = dialog.ShowDialog();
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
            }
            double percentage = (Width / 640.0) * 100;
            _viewModel.SizeText = $"{percentage:F0}%";
        }
        finally
        {
            _isResizing = false;
        }
    }
}
