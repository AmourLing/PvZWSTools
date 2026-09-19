using System.Collections.ObjectModel;
using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

public class OthersViewModel:ViewModelBase
{
    private static readonly IReadOnlyDictionary<string, string> _buttonMapping = new Dictionary<string, string>
    {
        ["AUTO_FERTILIZER_BUGSPRAY_CHECK"] = nameof(AutoFertilizerBugSpray),
        ["CLEARVASE_CHECK"] = nameof(ClearVase),
        ["RUNWHILELOCKED_CHECK"] = nameof(RunWhileLocked),
        ["CLEARFOG_CHECK"] = nameof(ClearFog),
        ["NO_CD_PLANTING_CHECK"] = nameof(NoCDPlanting),
        ["NO_COST_PLANTING_CHECK"] = nameof(NoCostPlanting),
        ["IS_REMOVE_COVERLAYER"] = nameof(RemoveCoverLayer),
        ["BIGSUN_CHECK"] = nameof(BigSun),
        ["AUTO_WATERING_CHECK"] = nameof(AutoWatering),
        ["AUTO_COLLECT_CHECK"] = nameof(AutoCollect),
    };

    private readonly IMessageProcessor _messageProcessor;
    private readonly IScriptExecutionService _scriptExec;

    private string _autoCollect = Constants.c_Symbol_Off;
    private string _autoCollectName = "自动收集";

    private string _autoFertilizerBugSpray = Constants.c_Symbol_Off;
    private string _autoFertilizerBugSprayName = "补充肥料杀虫剂";

    private string _autoWatering = Constants.c_Symbol_Off;
    private string _autoWateringName = "自动浇水";

    private string _bigSun = Constants.c_Symbol_Off;
    private string _bigSunName = "阳光增值";

    private string _clearFog = Constants.c_Symbol_Off;
    private string _clearFogName = "清除迷雾";

    private string _clearVase = Constants.c_Symbol_Off;
    private string _clearVaseName = "罐子透视";

    private string _noCDPlanting = Constants.c_Symbol_Off;
    private string _noCDPlantingName = "取消冷却";

    private string _noCostPlanting = Constants.c_Symbol_Off;
    private string _noCostPlantingName = "取消阳光";

    private string _removeCoverLayer = Constants.c_Symbol_Off;
    private string _removeCoverLayerName = "去除遮挡";
    private string _runWhileLocked = Constants.c_Symbol_Off;
    private string _runWhileLockedName = "后台运行";
    private string _setTreeHeight = "1437";
    private string _setTreeHeightName = "智慧树高度";

    public OthersViewModel(IScriptExecutionService scriptExec, IMessageProcessor messageProcessor)
    {
        _scriptExec = scriptExec;
        _messageProcessor = messageProcessor;
        if(_messageProcessor != null)
            _messageProcessor.ButtonStatusUpdated += OnButtonStatusUpdated;
        GameRunSpeedOptions = OptionsLoader.Load(Helpers.Constants.JsonGameRunSpeedFile);
    }

    public string AutoCollect
    {
        get => _autoCollect;
        set => SetProperty(ref _autoCollect, value);
    }

    public ICommand AutoCollectCommand => new RelayCommand(async _ =>
    {
        var __old = AutoCollect;
        AutoCollect = ButtonHelper.ToggleCheck(AutoCollect);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _autoCollectName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(AutoCollect) }))
            AutoCollect = __old;
    });

    public string AutoFertilizerBugSpray
    {
        get => _autoFertilizerBugSpray;
        set => SetProperty(ref _autoFertilizerBugSpray, value);
    }

    public ICommand AutoFertilizerBugSprayCommand => new RelayCommand(async _ =>
    {
        var __old = AutoFertilizerBugSpray;
        AutoFertilizerBugSpray = ButtonHelper.ToggleCheck(AutoFertilizerBugSpray);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _autoFertilizerBugSprayName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(AutoFertilizerBugSpray) }))
            AutoFertilizerBugSpray = __old;
    });

    public string AutoWatering
    {
        get => _autoWatering;
        set => SetProperty(ref _autoWatering, value);
    }

    public ICommand AutoWateringCommand => new RelayCommand(async _ =>
    {
        var __old = AutoWatering;
        AutoWatering = ButtonHelper.ToggleCheck(AutoWatering);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _autoWateringName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(AutoWatering) }))
            AutoWatering = __old;
    });
    private bool _gameRunSpeedDropdownToggleIsChecked;
    public bool GameRunSpeedDropdownToggleIsChecked
    {
        get => _gameRunSpeedDropdownToggleIsChecked;
        set => SetProperty(ref _gameRunSpeedDropdownToggleIsChecked, value);
    }

    public ObservableCollection<NameOption> GameRunSpeedOptions { get; }
    private NameOption _gameRunSpeedSelected;
    public string GameRunSpeedInput
    {
        get => _gameRunSpeedInput;
        set => SetProperty(ref _gameRunSpeedInput, value);
    }
    private string _gameRunSpeedInput = "1";
    public NameOption GameRunSpeedSelected
    {
        get => _gameRunSpeedSelected;
        set
        {
            _gameRunSpeedSelected = value;
            if(value != null) GameRunSpeedInput = value.Name;
            GameRunSpeedDropdownToggleIsChecked = false;
            OnPropertyChanged();
        }
    }
    public ICommand GameRunSpeedCommand => new RelayCommand(async _ =>
    {
        string Value = NameOption.GetValue(GameRunSpeedInput, GameRunSpeedOptions);
        _ = await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, "游戏速度",
            new Dictionary<string, string>
            {
                [Constants.Placeholders.GameRunSpeed] = Value,
            });
    });
    public string BigSun
    {
        get => _bigSun;
        set => SetProperty(ref _bigSun, value);
    }

    public ICommand BigSunCommand => new RelayCommand(async _ =>
    {
        var __old = BigSun;
        BigSun = ButtonHelper.ToggleCheck(BigSun);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _bigSunName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(BigSun) }))
            BigSun = __old;
    });

    public string ClearFog
    {
        get => _clearFog;
        set => SetProperty(ref _clearFog, value);
    }

    public ICommand ClearFogCommand => new RelayCommand(async _ =>
    {
        var __old = ClearFog;
        ClearFog = ButtonHelper.ToggleCheck(ClearFog);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _clearFogName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(ClearFog) }))
            ClearFog = __old;
    });

    public string ClearVase
    {
        get => _clearVase;
        set => SetProperty(ref _clearVase, value);
    }

    public ICommand ClearVaseCommand => new RelayCommand(async _ =>
    {
        var __old = ClearVase;
        ClearVase = ButtonHelper.ToggleCheck(ClearVase);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _clearVaseName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(ClearVase) }))
            ClearVase = __old;
    });

    public string NoCDPlanting
    {
        get => _noCDPlanting;
        set => SetProperty(ref _noCDPlanting, value);
    }

    public ICommand NoCDPlantingCommand => new RelayCommand(async _ =>
    {
        var __old = NoCDPlanting;
        NoCDPlanting = ButtonHelper.ToggleCheck(NoCDPlanting);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _noCDPlantingName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(NoCDPlanting) }))
            NoCDPlanting = __old;
    });

    public string NoCostPlanting
    {
        get => _noCostPlanting;
        set => SetProperty(ref _noCostPlanting, value);
    }

    public ICommand NoCostPlantingCommand => new RelayCommand(async _ =>
    {
        var __old = NoCostPlanting;
        NoCostPlanting = ButtonHelper.ToggleCheck(NoCostPlanting);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _noCostPlantingName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(NoCostPlanting) }))
            NoCostPlanting = __old;
    });

    public string RemoveCoverLayer
    {
        get => _removeCoverLayer;
        set => SetProperty(ref _removeCoverLayer, value);
    }

    public ICommand RemoveCoverLayerCommand => new RelayCommand(async _ =>
    {
        var __old = RemoveCoverLayer;
        RemoveCoverLayer = ButtonHelper.ToggleCheck(RemoveCoverLayer);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _removeCoverLayerName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(RemoveCoverLayer) }))
            RemoveCoverLayer = __old;
    });

    public string RunWhileLocked
    {
        get => _runWhileLocked;
        set => SetProperty(ref _runWhileLocked, value);
    }

    public ICommand RunWhileLockedCommand => new RelayCommand(async _ =>
    {
        var __old = RunWhileLocked;
        RunWhileLocked = ButtonHelper.ToggleCheck(RunWhileLocked);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _runWhileLockedName,
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(RunWhileLocked) }))
            RunWhileLocked = __old;
    });

    public string SetTreeHeight
    {
        get => _setTreeHeight;
        set => SetProperty(ref _setTreeHeight, value);
    }

    public ICommand SetTreeHeightCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, _setTreeHeightName,
            new Dictionary<string, string>
            {
                [Constants.Placeholders.TreeHeight] = SetTreeHeight
            }));

    public ICommand UpdateButtonStatusCommand => new RelayCommand(async _ =>
    {
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Others, "GetButtonCheck");
    });

    public void Dispose()
    {
        if(_messageProcessor != null)
            _messageProcessor.ButtonStatusUpdated -= OnButtonStatusUpdated;
    }

    private void OnButtonStatusUpdated(Dictionary<string, bool> statusDict)
    {
        UpdatePropertiesFromDict(statusDict, _buttonMapping);
    }
}
