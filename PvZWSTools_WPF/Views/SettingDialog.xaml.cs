using System.Windows;
using PvZWSTools_WPF.Services;
using PvZWSTools_WPF.ViewModels;

namespace PvZWSTools_WPF.Views;

public partial class SettingDialog:Window
{
    private readonly SettingDialogViewModel _viewModel;
    private bool _setting1;
    private bool _setting2;

    public SettingDialog(ISettingsService settingsService, string str1, string str2)
    {
        InitializeComponent();
        _viewModel = new SettingDialogViewModel(settingsService, str1, str2);
        _viewModel.OkRequestClose += (s, e) =>
        {
            Setting1 = _viewModel.Setting1;
            Setting2 = _viewModel.Setting2;
            DialogResult = true;
            Close();
        };
        _viewModel.CancelRequestClose += (s, e) => { DialogResult = false; Close(); };
        DataContext = _viewModel;
    }

    public bool Setting1
    {
        get => _setting1;
        set { _setting1 = value; }
    }

    public bool Setting2
    {
        get => _setting2;
        set { _setting2 = value; }
    }
}
