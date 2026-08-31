using System.Collections.ObjectModel;
using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

public class PlantsViewModel:ViewModelBase
{
    private static readonly IReadOnlyDictionary<string, string> _buttonMapping = new Dictionary<string, string>
    {
        ["MAGNET_CD_CHECK"] = nameof(CD_Magnet),
        ["CHOMPER_CD_CHECK"] = nameof(CD_Chomper),
        ["NO_CRATER_CHECK"] = nameof(NoCrater),
        ["NOSQUISH_CHECK"] = nameof(NoSquish),
        ["POTATO_CD_CHECK"] = nameof(CD_Potato),
        ["SUNSHROOM_CD_CHECK"] = nameof(CD_Sunshroom),
        ["COBCD_CHECK"] = nameof(CD_Cob),
        ["WAKEUP_CHECK"] = nameof(WakeUp),
        ["INVINCPLANT_CHECK"] = nameof(InvincPlant),
        ["DRAW_PLANT_HP_CHECK"] = nameof(DrawPlantHP),
        ["ONLY_BUTTER_CHECK"] = nameof(OnlyButter),
    };

    private readonly IMessageProcessor _messageProcessor;

    private readonly IScriptExecutionService _scriptExec;

    private string _CD_chomper = Constants.c_Symbol_Off;

    private string _CD_chomper_Name = "大嘴花准备时间";

    private string _CD_cob = Constants.c_Symbol_Off;

    private string _CD_cob_Name = "玉米炮准备时间";

    private string _CD_magnet = Constants.c_Symbol_Off;

    private string _CD_magnet_Name = "磁力菇准备时间";

    private string _CD_otherPlant_Name = "其他植物准备时间";

    private NameOption _CD_otherPlant_selected;

    private bool _CD_otherPlantDropdownToggleIsChecked;

    private string _CD_otherPlantInput = "豌豆射手";

    private string _CD_potato = Constants.c_Symbol_Off;

    private string _CD_potato_Name = "土豆雷准备时间";

    private string _CD_sunshroom = Constants.c_Symbol_Off;

    private string _CD_sunshroom_Name = "阳光菇准备时间";

    private string _drawPlantHP = Constants.c_Symbol_Off;

    private string _drawPlantHP_Name = "植物血量显示";

    private string _invincPlant = Constants.c_Symbol_Off;

    private string _invincPlant_Name = "植物无敌";

    private string _noCrater = Constants.c_Symbol_Off;

    private string _noCrater_Name = "核弹无坑";

    private string _noSquish = Constants.c_Symbol_Off;

    private string _noSquish_Name = "取消压扁";

    private string _onlyButter = Constants.c_Symbol_Off;

    private string _onlyButter_Name = "只投黄油";

    private string _wakeUp = Constants.c_Symbol_Off;

    private string _wakeUp_Name = "植物清醒";

    public PlantsViewModel(IScriptExecutionService scriptExec, IMessageProcessor messageProcessor)
    {
        _scriptExec = scriptExec;
        _messageProcessor = messageProcessor;
        if(_messageProcessor != null)
            _messageProcessor.ButtonStatusUpdated += OnButtonStatusUpdated;
        CD_OtherPlantOptions = OptionsLoader.Load(Constants.JsonPlantFile);
    }

    public string CD_Chomper
    {
        get => _CD_chomper;
        set => SetProperty(ref _CD_chomper, value);
    }

    public ICommand CD_ChomperCommand =>
        CreateToggleCommand(() => CD_Chomper, v => CD_Chomper = v, _CD_chomper_Name);

    public string CD_Cob
    {
        get => _CD_cob;
        set => SetProperty(ref _CD_cob, value);
    }

    public ICommand CD_CobCommand =>
        CreateToggleCommand(() => CD_Cob, v => CD_Cob = v, _CD_cob_Name);

    public string CD_Magnet
    {
        get => _CD_magnet;
        set => SetProperty(ref _CD_magnet, value);
    }

    public ICommand CD_MagnetCommand =>
        CreateToggleCommand(() => CD_Magnet, v => CD_Magnet = v, _CD_magnet_Name);

    public ICommand CD_OtherCommand => new RelayCommand(async _ =>
    {
        string seedType = NameOption.GetValue(CD_OtherInput, CD_OtherPlantOptions);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Plants, _CD_otherPlant_Name,
            new Dictionary<string, string> { [Constants.Placeholders.SeedType] = seedType });
    });

    public string CD_OtherInput
    {
        get => _CD_otherPlantInput;
        set => SetProperty(ref _CD_otherPlantInput, value);
    }

    public bool CD_OtherPlantDropdownToggleIsChecked
    {
        get => _CD_otherPlantDropdownToggleIsChecked;
        set => SetProperty(ref _CD_otherPlantDropdownToggleIsChecked, value);
    }

    public ObservableCollection<NameOption> CD_OtherPlantOptions { get; }

    public NameOption CD_OtherPlantSelected
    {
        get => _CD_otherPlant_selected;
        set
        {
            _CD_otherPlant_selected = value;
            if(value != null)
                CD_OtherInput = value.Name;
            CD_OtherPlantDropdownToggleIsChecked = false;
            OnPropertyChanged();
        }
    }

    public string CD_Potato
    {
        get => _CD_potato;
        set => SetProperty(ref _CD_potato, value);
    }

    public ICommand CD_PotatoCommand =>
        CreateToggleCommand(() => CD_Potato, v => CD_Potato = v, _CD_potato_Name);

    public string CD_Sunshroom
    {
        get => _CD_sunshroom;
        set => SetProperty(ref _CD_sunshroom, value);
    }

    public ICommand CD_SunshroomCommand =>
        CreateToggleCommand(() => CD_Sunshroom, v => CD_Sunshroom = v, _CD_sunshroom_Name);

    public string DrawPlantHP
    {
        get => _drawPlantHP;
        set => SetProperty(ref _drawPlantHP, value);
    }

    public ICommand DrawPlantHPCommand =>
        CreateToggleCommand(() => DrawPlantHP, v => DrawPlantHP = v, _drawPlantHP_Name);

    public string InvincPlant
    {
        get => _invincPlant;
        set => SetProperty(ref _invincPlant, value);
    }

    public ICommand InvincPlantCommand =>
        CreateToggleCommand(() => InvincPlant, v => InvincPlant = v, _invincPlant_Name);

    public string NoCrater
    {
        get => _noCrater;
        set => SetProperty(ref _noCrater, value);
    }

    public ICommand NoCraterCommand =>
        CreateToggleCommand(() => NoCrater, v => NoCrater = v, _noCrater_Name);

    public string NoSquish
    {
        get => _noSquish;
        set => SetProperty(ref _noSquish, value);
    }

    public ICommand NoSquishCommand =>
        CreateToggleCommand(() => NoSquish, v => NoSquish = v, _noSquish_Name);

    public string OnlyButter
    {
        get => _onlyButter;
        set => SetProperty(ref _onlyButter, value);
    }

    public ICommand OnlyButterCommand =>
        CreateToggleCommand(() => OnlyButter, v => OnlyButter = v, _onlyButter_Name);

    public ICommand UpdateButtonStatusCommand => new RelayCommand(async _ =>
    {
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Plants, "GetButtonCheck");
    });

    public string WakeUp
    {
        get => _wakeUp;
        set => SetProperty(ref _wakeUp, value);
    }

    public ICommand WakeUpCommand =>
        CreateToggleCommand(() => WakeUp, v => WakeUp = v, _wakeUp_Name);

    private ICommand CreateToggleCommand(Func<string> stateGetter, Action<string> stateSetter, string scriptName)
    {
        return new RelayCommand(async _ =>
        {
            var current = stateGetter();
            var newState = ButtonHelper.ToggleCheck(current);
            stateSetter(newState);
            await _scriptExec.ExecuteAsync(Constants.SubFolders.Plants, scriptName,
                new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(newState) });
        });
    }

    private void OnButtonStatusUpdated(Dictionary<string, bool> statusDict)
    {
        UpdatePropertiesFromDict(statusDict, _buttonMapping);
    }
}
