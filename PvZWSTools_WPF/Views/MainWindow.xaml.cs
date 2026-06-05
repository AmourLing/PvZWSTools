using System;
using System.IO;
using System.Reflection;
using System.Windows;
using PvZWSTools_WPF.Services;
using PvZWSTools_WPF.ViewModels;

namespace PvZWSTools_WPF.Views
{
    public partial class MainWindow:Window
    {
        private readonly MainWindowViewModel _viewModel;

        private bool _isResizing = false;

        public MainWindow()
        {
            InitializeComponent();

            Title = Title + "_" + GetCompileTime();

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

        private string GetCompileTime()
        {
            AssemblyMetadataAttribute attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyMetadataAttribute>();
            DateTime buildTime = default(DateTime);
            if(attribute != null && attribute.Key == "BuildTimestamp")
            {
                buildTime = DateTime.Parse(attribute.Value);
            }
            return buildTime.ToString("yyyyMMdd");
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
}
