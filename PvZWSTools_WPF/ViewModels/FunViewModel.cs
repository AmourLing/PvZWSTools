using System.Windows.Input;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels;

public class FunViewModel:ViewModelBase
{
    private readonly IScriptExecutionService _scriptExec;

    public FunViewModel(IScriptExecutionService scriptExec)
    {
        _scriptExec = scriptExec;
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
