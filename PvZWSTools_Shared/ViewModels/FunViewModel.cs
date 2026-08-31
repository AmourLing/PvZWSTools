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

    public ICommand RandomVaseCommand => new RelayCommand(async _ =>
    {
        RandomVase = ButtonHelper.ToggleCheck(RandomVase);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Fun, "随机罐子",
            new Dictionary<string, string> { [Constants.Placeholders.RandomVaseCheck] = ButtonHelper.GetCheckValue(RandomVase) });
    });

    public ICommand RandomCardCommand => new RelayCommand(async _ =>
    {
        RandomCard = ButtonHelper.ToggleCheck(RandomCard);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Fun, "随机卡片",
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(RandomCard) });
    });

    public ICommand RandomPacketCommand => new RelayCommand(async _ =>
    {
        RandomPacket = ButtonHelper.ToggleCheck(RandomPacket);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Fun, "随机卡槽",
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(RandomPacket) });
    });
}
