using System;
using System.Windows.Input;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Models;
using PvZWSTools_WPF.Services;
using PvZWSTools_WPF.ViewModels;

public class SettingDialogViewModel:ViewModelBase
{
    private readonly AppSettings _originalSettings;
    private readonly ISettingsService _settingsService;
    private AppSettings _editableSettings;

    public SettingDialogViewModel(ISettingsService settingsService, string label1, string label2)
    {
        _settingsService = settingsService;
        _originalSettings = settingsService.Settings;
        _editableSettings = new AppSettings
        {
            AutoConnectEnabled = _originalSettings.AutoConnectEnabled,
            SuppressConnectionMessage = _originalSettings.SuppressConnectionMessage
        };

        Setting1Label = label1;
        Setting2Label = label2;

        OkCommand = new RelayCommand(_ => SaveAndClose());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public event EventHandler CancelRequestClose;

    public event EventHandler OkRequestClose;

    public ICommand CancelCommand { get; }
    public ICommand OkCommand { get; }

    public bool Setting1
    {
        get => _editableSettings.AutoConnectEnabled;
        set { _editableSettings.AutoConnectEnabled = value; OnPropertyChanged(); }
    }

    public string Setting1Label { get; }

    public bool Setting2
    {
        get => _editableSettings.SuppressConnectionMessage;
        set { _editableSettings.SuppressConnectionMessage = value; OnPropertyChanged(); }
    }

    public string Setting2Label { get; }

    private void Cancel()
    {
        CancelRequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void SaveAndClose()
    {
        _originalSettings.AutoConnectEnabled = _editableSettings.AutoConnectEnabled;
        _originalSettings.SuppressConnectionMessage = _editableSettings.SuppressConnectionMessage;
        _settingsService.Save();
        OkRequestClose?.Invoke(this, EventArgs.Empty);
    }
}
