using System.Windows;
using PvZWSTools_Shared.Services;
using PvZWSTools_Shared.ViewModels;

namespace PvZWSTools_WPF.Views;

public partial class SettingDialog:Window
{
    private readonly SettingDialogViewModel _viewModel;

    public SettingDialog(ISettingsService settingsService)
    {
        InitializeComponent();
        _viewModel = new SettingDialogViewModel(settingsService);
        _viewModel.OkRequestClose += (s, e) =>
        {
            DialogResult = true;
            Close();
        };
        _viewModel.CancelRequestClose += (s, e) => { DialogResult = false; Close(); };
        DataContext = _viewModel;
    }
}
