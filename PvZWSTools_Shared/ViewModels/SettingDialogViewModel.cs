using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

/// <summary>
/// 设置对话框中的单个设置项（通过反射从 AppSettings 自动注册）
/// </summary>
public class SettingItem:ViewModelBase
{
    public string Label { get; set; }

    public PropertyInfo Property { get; set; }

    private bool _value;
    public bool Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }
}

public class SettingDialogViewModel:ViewModelBase
{
    private readonly AppSettings _originalSettings;
    private readonly ISettingsService _settingsService;

    public SettingDialogViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _originalSettings = settingsService.Settings;

        // 通过反射自动注册所有带 [Setting] 特性的布尔设置项，新增设置时无需修改本类
        foreach(var prop in AppSettings.SettingProperties)
        {
            Settings.Add(new SettingItem
            {
                Label = prop.GetCustomAttribute<SettingAttribute>()?.Label ?? prop.Name,
                Property = prop,
                Value = (bool)prop.GetValue(_originalSettings)
            });
        }

        OkCommand = new RelayCommand(_ => SaveAndClose());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public event EventHandler CancelRequestClose;

    public event EventHandler OkRequestClose;

    public ICommand CancelCommand { get; }
    public ICommand OkCommand { get; }

    public ObservableCollection<SettingItem> Settings { get; } = new ObservableCollection<SettingItem>();

    private void Cancel()
    {
        CancelRequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void SaveAndClose()
    {
        foreach(var item in Settings)
        {
            item.Property.SetValue(_originalSettings, item.Value);
        }
        _settingsService.Save();
        OkRequestClose?.Invoke(this, EventArgs.Empty);
    }
}
