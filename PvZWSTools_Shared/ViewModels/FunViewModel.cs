using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

public class FunViewModel:ViewModelBase
{
    private readonly IScriptExecutionService _scriptExec;

    private static readonly IReadOnlyDictionary<string, string> _buttonMapping = new Dictionary<string, string>
    {
        ["RANDOM_VASE_CHECK"] = nameof(RandomVase),
        ["RANDOM_PACKET_CHECK"] = nameof(RandomPacket),
        ["RANDOM_CARD_CHECK"] = nameof(RandomCard),
        ["GLOVE_ALWAYS_CHECK"] = nameof(GloveAlways),
        ["ALWAYS_FUSION_MODE_CHECK"] = nameof(AlwaysFusionMode),
        ["ALWAYS_HAS_TRASHCAN_CHECK"] = nameof(AlwaysHasTrashcan),
        ["PURPLE_DIRECT_CHECK"] = nameof(PurpleDirectPlant),
    };

    public ICommand UpdateButtonStatusCommand => new RelayCommand(async _ =>
    {
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Fun, "GetButtonCheck");
    });

    private readonly IMessageProcessor _messageProcessor;

    private void OnButtonStatusUpdated(Dictionary<string, bool> statusDict)
    {
        UpdatePropertiesFromDict(statusDict, _buttonMapping);
    }

    public FunViewModel(IScriptExecutionService scriptExec, IMessageProcessor messageProcessor)
    {
        _scriptExec = scriptExec;
        _messageProcessor = messageProcessor;
        if(_messageProcessor != null)
            _messageProcessor.ButtonStatusUpdated += OnButtonStatusUpdated;
    }

    private string _randomVase = Constants.c_Symbol_Off;

    public string RandomVase
    {
        get => _randomVase;
        set { _randomVase = value; OnPropertyChanged(); }
    }

    private string _randomCard = Constants.c_Symbol_Off;

    public string RandomCard
    {
        get => _randomCard;
        set { _randomCard = value; OnPropertyChanged(); }
    }

    private string _randomPacket = Constants.c_Symbol_Off;

    public string RandomPacket
    {
        get => _randomPacket;
        set { _randomPacket = value; OnPropertyChanged(); }
    }
    private string _gloveAlways = Constants.c_Symbol_Off;

    public string GloveAlways
    {
        get => _gloveAlways;
        set { _gloveAlways = value; OnPropertyChanged(); }
    }

    private string _alwaysFusionMode = Constants.c_Symbol_Off;

    public string AlwaysFusionMode
    {
        get => _alwaysFusionMode;
        set { _alwaysFusionMode = value; OnPropertyChanged(); }
    }
    private string _alwaysHasTrashcan = Constants.c_Symbol_Off;

    public string AlwaysHasTrashcan
    {
        get => _alwaysHasTrashcan;
        set { _alwaysHasTrashcan = value; OnPropertyChanged(); }
    }

    private string _purpleDirectPlant = Constants.c_Symbol_Off;

    public string PurpleDirectPlant
    {
        get => _purpleDirectPlant;
        set { _purpleDirectPlant = value; OnPropertyChanged(); }
    }
    public ICommand RandomVaseCommand => new RelayCommand(async _ =>
    {
        var __old = RandomVase;
        RandomVase = ButtonHelper.ToggleCheck(RandomVase);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Fun, "随机罐子",
            new Dictionary<string, string> { [Constants.Placeholders.RandomVaseCheck] = ButtonHelper.GetCheckValue(RandomVase) }))
            RandomVase = __old;
    });

    public ICommand RandomCardCommand => new RelayCommand(async _ =>
    {
        var __old = RandomCard;
        RandomCard = ButtonHelper.ToggleCheck(RandomCard);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Fun, "随机卡片",
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(RandomCard) }))
            RandomCard = __old;
    });

    public ICommand RandomPacketCommand => new RelayCommand(async _ =>
    {
        var __old = RandomPacket;
        RandomPacket = ButtonHelper.ToggleCheck(RandomPacket);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Fun, "随机卡槽",
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(RandomPacket) }))
            RandomPacket = __old;
    });

    public ICommand GloveAlwaysCommand => new RelayCommand(async _ =>
    {
        var __old = GloveAlways;
        GloveAlways = ButtonHelper.ToggleCheck(GloveAlways);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Fun, "手套常驻",
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(GloveAlways) }))
            GloveAlways = __old;
    });
    public ICommand AlwaysFusionModeCommand => new RelayCommand(async _ =>
    {
        var __old = AlwaysFusionMode;
        AlwaysFusionMode = ButtonHelper.ToggleCheck(AlwaysFusionMode);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Fun, "融合常驻",
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(AlwaysFusionMode) }))
            AlwaysFusionMode = __old;
    });

    public ICommand AlwaysHasTrashcanCommand => new RelayCommand(async _ =>
    {
        var __old = AlwaysHasTrashcan;
        AlwaysHasTrashcan = ButtonHelper.ToggleCheck(AlwaysHasTrashcan);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Fun, "垃圾桶常驻",
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(AlwaysHasTrashcan) }))
            AlwaysHasTrashcan = __old;
    });

    public ICommand PurpleDirectPlantCommand => new RelayCommand(async _ =>
    {
        var __old = PurpleDirectPlant;
        PurpleDirectPlant = ButtonHelper.ToggleCheck(PurpleDirectPlant);
        if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Fun, "紫卡直接种植",
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(PurpleDirectPlant) }))
            PurpleDirectPlant = __old;
    });
}
