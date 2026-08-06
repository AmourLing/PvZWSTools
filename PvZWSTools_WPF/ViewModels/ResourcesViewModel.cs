using System.Collections.ObjectModel;
using System.Windows.Input;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Models;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels;

public class ResourcesViewModel:ViewModelBase
{
    private readonly IScriptExecutionService _scriptExec;

    private bool _damageDropdownToggleIsChecked;
    private string _damageInput;
    private string _damageInput2;
    private string _damageName = "设置伤害";
    private NameOption _damageSelected;

    private bool _healthDropdownToggleIsChecked;
    private string _healthInput;
    private string _healthInput2;
    private string _healthName = "设置血量";
    private NameOption _healthSelected;

    private string _moneyCount = "99999";
    private string _moneyCountLimit = "999999";
    private string _moneyCountLimitName = "设置金钱上限";
    private string _moneyCountName = "设置金钱";

    private string _sunCount = "8000";
    private string _sunCountLimit = "99990";
    private string _sunCountLimitName = "设置阳光上限";
    private string _sunCountName = "设置阳光";

    private bool _timeDropdownToggleIsChecked;
    private string _timeInput;
    private string _timeInput2;
    private string _timeName = "设置时间";
    private NameOption _timeSelected;

    private bool _valueDropdownToggleIsChecked;
    private string _valueInput;
    private string _valueInput2;
    private string _valueName = "设置价值";
    private NameOption _valueSelected;

    public ResourcesViewModel(IScriptExecutionService scriptExec)
    {
        _scriptExec = scriptExec;

        ValueOptions = OptionsLoader.Load(Constants.JsonValueFile);
        DamageOptions = OptionsLoader.Load(Constants.JsonDamageFile);
        HealthOptions = OptionsLoader.Load(Constants.JsonHealthFile);
        TimeOptions = OptionsLoader.Load(Constants.JsonTimeFile);
    }

    public bool DamageDropdownToggleIsChecked
    {
        get => _damageDropdownToggleIsChecked;
        set { _damageDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string DamageInput
    {
        get => _damageInput;
        set { _damageInput = value; OnPropertyChanged(); }
    }

    public string DamageInput2
    {
        get => _damageInput2;
        set { _damageInput2 = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> DamageOptions { get; }

    public NameOption DamageSelected
    {
        get => _damageSelected;
        set
        {
            _damageSelected = value;

            if(value != null)
            {
                DamageInput = value.Name;
                DamageInput2 = value.Default ?? string.Empty;
            }
            DamageDropdownToggleIsChecked = false;
            OnPropertyChanged();
        }
    }

    public ICommand DamageSetCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Resources, _damageName,
            new Dictionary<string, string>
            {
                [Constants.Placeholders.Damage] = NameOption.GetValue(DamageInput, DamageOptions),
                [Constants.Placeholders.Damage2] = DamageInput2
            }));

    public bool HealthDropdownToggleIsChecked
    {
        get => _healthDropdownToggleIsChecked;
        set { _healthDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string HealthInput
    {
        get => _healthInput;
        set { _healthInput = value; OnPropertyChanged(); }
    }

    public string HealthInput2
    {
        get => _healthInput2;
        set { _healthInput2 = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> HealthOptions { get; }

    public NameOption HealthSelected
    {
        get => _healthSelected;
        set
        {
            _healthSelected = value;

            if(value != null)
            {
                HealthInput = value.Name;
                HealthInput2 = value.Default ?? string.Empty;
            }
            HealthDropdownToggleIsChecked = false;
            OnPropertyChanged();
        }
    }

    public ICommand HealthSetCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Resources, _healthName,
            new Dictionary<string, string>
            {
                [Constants.Placeholders.Health] = NameOption.GetValue(HealthInput, HealthOptions),
                [Constants.Placeholders.Health2] = HealthInput2
            }));

    public string MoneyCount
    {
        get => _moneyCount;
        set { _moneyCount = value; OnPropertyChanged(); }
    }

    public string MoneyCountLimit
    {
        get => _moneyCountLimit;
        set { _moneyCountLimit = value; OnPropertyChanged(); }
    }

    public ICommand MoneyLimitSetCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Resources, _moneyCountLimitName,
            new Dictionary<string, string> { [Constants.Placeholders.CoinLimit] = MoneyCountLimit }));

    public ICommand MoneySetCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Resources, _moneyCountName,
            new Dictionary<string, string> { [Constants.Placeholders.Coin] = MoneyCount }));

    public string SunCount
    {
        get => _sunCount;
        set { _sunCount = value; OnPropertyChanged(); }
    }

    public string SunCountLimit
    {
        get => _sunCountLimit;
        set { _sunCountLimit = value; OnPropertyChanged(); }
    }

    public ICommand SunLimitSetCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Resources, _sunCountLimitName,
            new Dictionary<string, string> { [Constants.Placeholders.SunMoneyLimit] = SunCountLimit }));

    // 命令
    public ICommand SunSetCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Resources, _sunCountName,
            new Dictionary<string, string> { [Constants.Placeholders.SunMoney] = SunCount }));

    public bool TimeDropdownToggleIsChecked
    {
        get => _timeDropdownToggleIsChecked;
        set { _timeDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string TimeInput
    {
        get => _timeInput;
        set { _timeInput = value; OnPropertyChanged(); }
    }

    public string TimeInput2
    {
        get => _timeInput2;
        set { _timeInput2 = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> TimeOptions { get; }

    public NameOption TimeSelected
    {
        get => _timeSelected;
        set
        {
            _timeSelected = value;

            if(value != null)
            {
                TimeInput = value.Name;
                TimeInput2 = value.Default ?? string.Empty;
            }
            TimeDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public ICommand TimeSetCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Resources, _timeName,
            new Dictionary<string, string>
            {
                [Constants.Placeholders.Time] = NameOption.GetValue(TimeInput, TimeOptions),
                [Constants.Placeholders.Time2] = TimeInput2
            }));

    public bool ValueDropdownToggleIsChecked
    {
        get => _valueDropdownToggleIsChecked;
        set { _valueDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string ValueInput
    {
        get => _valueInput;
        set { _valueInput = value; OnPropertyChanged(); }
    }

    public string ValueInput2
    {
        get => _valueInput2;
        set { _valueInput2 = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> ValueOptions { get; }

    public NameOption ValueSelected
    {
        get => _valueSelected;
        set
        {
            _valueSelected = value;
            OnPropertyChanged();
            if(value != null)
            {
                ValueInput = value.Name;
                ValueInput2 = value.Default ?? string.Empty;
            }
            ValueDropdownToggleIsChecked = false;
        }
    }

    public ICommand ValueSetCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Resources, _valueName,
            new Dictionary<string, string>
            {
                [Constants.Placeholders.Value] = NameOption.GetValue(ValueInput, ValueOptions),
                [Constants.Placeholders.Value2] = ValueInput2
            }));
}
