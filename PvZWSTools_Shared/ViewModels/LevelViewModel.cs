using System.Collections.ObjectModel;
using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

public class LevelViewModel:ViewModelBase
{
    private static readonly IReadOnlyDictionary<string, string> _buttonMapping = new Dictionary<string, string>();

    private readonly IMessageProcessor _messageProcessor;
    private readonly IScriptExecutionService _scriptExec;

    private string _adventureInput = "1-1";

    private string _endlessFlagInput = "2025";

    private bool _modeDropdownToggleIsChecked;

    private string _modeInput = "冒险模式";

    private NameOption _modeSelected;

    public LevelViewModel(IScriptExecutionService scriptExec, IMessageProcessor messageProcessor)
    {
        _scriptExec = scriptExec;
        _messageProcessor = messageProcessor;
        if(_messageProcessor != null)
            _messageProcessor.ButtonStatusUpdated += OnButtonStatusUpdated;
        ModeOptions = OptionsLoader.Load(Helpers.Constants.JsonModeFile);
    }

    public string AdventureInput
    {
        get => _adventureInput;
        set => SetProperty(ref _adventureInput, value);
    }

    public string EndlessFlag
    {
        get => _endlessFlagInput;
        set => SetProperty(ref _endlessFlagInput, value);
    }

    public ICommand EnterNewLevelCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Level, "EnterNewLevel"));

    public bool ModeDropdownToggleIsChecked
    {
        get => _modeDropdownToggleIsChecked;
        set => SetProperty(ref _modeDropdownToggleIsChecked, value);
    }

    public string ModeInput
    {
        get => _modeInput;
        set => SetProperty(ref _modeInput, value);
    }

    public ObservableCollection<NameOption> ModeOptions { get; }

    public NameOption ModeSelected
    {
        get => _modeSelected;
        set
        {
            _modeSelected = value;
            if(value != null) ModeInput = value.Name;
            ModeDropdownToggleIsChecked = false;
            OnPropertyChanged();
        }
    }

    public ICommand PassThisLevelCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Level, "直接过关"));

    public ICommand SetEndlessFlagCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Level, "设置无尽旗数", new Dictionary<string, string> { [Constants.Placeholders.Flag] = EndlessFlag }));

    public ICommand SetModeCommand => new RelayCommand(async _ =>
    {
        string modeValue = NameOption.GetValue(ModeInput, ModeOptions);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Level, "混乱关卡",
            new Dictionary<string, string>
            {
                [Constants.Placeholders.GameMode] = modeValue,
                [Constants.Placeholders.AdventureNum] = AdventureInput
            });
    });

    private void OnButtonStatusUpdated(Dictionary<string, bool> statusDict)
    {
        UpdatePropertiesFromDict(statusDict, _buttonMapping);
    }
}
