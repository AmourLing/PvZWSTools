using System.Collections.ObjectModel;
using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

public class ZombiesViewModel:ViewModelBase
{
    private static readonly IReadOnlyDictionary<string, string> _buttonMapping = new Dictionary<string, string>()
    {
        ["NO_ICETRAP_CHECK"] = nameof(NoIceTrap),
        ["NOEXPLODE_CHECK"] = nameof(NoExplode),
        ["DROPPACKET_CHECK"] = nameof(DropPacket),
        ["INVINCZOMBIE_CHECK"] = nameof(InvincZombie),
        ["DRAW_ZOMBIE_HP_CHECK"] = nameof(DrawZombieHP),
        ["STOP_WALK_CHECK"] = nameof(StopWalk),
        ["NO_STEAL_CHECK"] = nameof(NoSteal),
        ["ALLOW_MINDCTRL"] = nameof(AllowMindCtrl),
        ["LIMIT_ZOMBIE_GET_DEBUFF"] = nameof(LimitZombieGetDebuff)
    };

    private readonly IMessageProcessor _messageProcessor;
    private readonly IScriptExecutionService _scriptExec;

    private string _allowMindCtrl = Constants.c_Symbol_Off;

    private string _allowMindCtrl_Name = "允许魅惑";

    private string _drawZombieHP = Constants.c_Symbol_Off;

    private string _drawZombieHP_Name = "僵尸血量显示";

    private string _dropPacket = Constants.c_Symbol_Off;

    private string _dropPacket_Name = "僵尸掉落卡片";

    private string _invincZombie = Constants.c_Symbol_Off;

    private string _invincZombie_Name = "僵尸无敌";

    private string _limitZombieGetDebuff = Constants.c_Symbol_Off;

    private string _limitZombieGetDebuff_Name = "限制减益";

    private string _noExplode = Constants.c_Symbol_Off;

    private string _noExplode_Name = "丑椒不爆";

    private string _noIceTrap = Constants.c_Symbol_Off;

    private string _noIceTrap_Name = "冰车无痕";

    private string _noSteal = Constants.c_Symbol_Off;

    private string _noSteal_Name = "小偷不偷";

    private string _setButtered_Name = "一键黄油效果";

    private string _setIceTrap_Name = "一键冰封效果";

    private string _setMindControl_Name = "一键魅惑效果";

    private string _setYuckyFace_Name = "一键大蒜效果";

    private string _stopWalk = Constants.c_Symbol_Off;

    private string _stopWalk_Name = "停滞不前";

    public ZombiesViewModel(IScriptExecutionService scriptExec, IMessageProcessor messageProcessor)
    {
        _scriptExec = scriptExec;
        _messageProcessor = messageProcessor;
        if(_messageProcessor != null)
            _messageProcessor.ButtonStatusUpdated += OnButtonStatusUpdated;
    }

    public string AllowMindCtrl
    {
        get => _allowMindCtrl;
        set => SetProperty(ref _allowMindCtrl, value);
    }

    public string DrawZombieHP
    {
        get => _drawZombieHP;
        set => SetProperty(ref _drawZombieHP, value);
    }

    public ICommand DrawZombieHPCommand => CreateToggleCommand(() => DrawZombieHP, v => DrawZombieHP = v, _drawZombieHP_Name);

    public string DropPacket
    {
        get => _dropPacket;
        set => SetProperty(ref _dropPacket, value);
    }

    public ICommand DropPacketCommand => CreateToggleCommand(() => DropPacket, v => DropPacket = v, _dropPacket_Name);

    public string InvincZombie
    {
        get => _invincZombie;
        set => SetProperty(ref _invincZombie, value);
    }

    public ICommand InvincZombieCommand => CreateToggleCommand(() => InvincZombie, v => InvincZombie = v, _invincZombie_Name);

    public string LimitZombieGetDebuff
    {
        get => _limitZombieGetDebuff;
        set => SetProperty(ref _limitZombieGetDebuff, value);
    }

    public string NoExplode
    {
        get => _noExplode;
        set => SetProperty(ref _noExplode, value);
    }

    public ICommand NoExplodeCommand => CreateToggleCommand(() => NoExplode, v => NoExplode = v, _noExplode_Name);

    public string NoIceTrap
    {
        get => _noIceTrap;
        set => SetProperty(ref _noIceTrap, value);
    }

    public ICommand NoIceTrapCommand => CreateToggleCommand(() => NoIceTrap, v => NoIceTrap = v, _noIceTrap_Name);

    public string NoSteal
    {
        get => _noSteal;
        set => SetProperty(ref _noSteal, value);
    }

    public ICommand NoStealCommand => CreateToggleCommand(() => NoSteal, v => NoSteal = v, _noSteal_Name);

    public ICommand SetButteredCommand => SetaCommand(_setButtered_Name);

    public ICommand SetIceTrapCommand => SetaCommand(_setIceTrap_Name);

    public ICommand SetMindControlCommand => SetaCommand(_setMindControl_Name);

    public ICommand SetYuckyFaceCommand => SetaCommand(_setYuckyFace_Name);

    /// <summary>大蒜/黄油/冰封/魅惑是同一个动作的四种取值（各自发一条"一键××效果"脚本），
    /// 在清单里本来占四行，和真正的开关混在一起看不出区别。收成一份选项 + 一个应用。</summary>
    public ObservableCollection<NameOption> ZombieStateOptions { get; } = new()
    {
        new NameOption { Name = "一键大蒜效果", Value = "garlic" },
        new NameOption { Name = "一键黄油效果", Value = "butter" },
        new NameOption { Name = "一键冰封效果", Value = "ice" },
        new NameOption { Name = "一键魅惑效果", Value = "mindcontrol" },
    };

    private string _zombieStateInput = "一键大蒜效果";

    private NameOption _selectedZombieState;

    public string ZombieStateInput
    {
        get => _zombieStateInput;
        set => SetProperty(ref _zombieStateInput, value);
    }

    public NameOption SelectedZombieState
    {
        get => _selectedZombieState;
        set
        {
            _selectedZombieState = value;
            if(value != null)
                ZombieStateInput = value.Name;
            OnPropertyChanged();
        }
    }

    public ICommand ApplyZombieStateCommand => new RelayCommand(_ => SetaCommand(ZombieStateInput).Execute(null));

    public string StopWalk
    {
        get => _stopWalk;
        set => SetProperty(ref _stopWalk, value);
    }

    public ICommand StopWalkCommand => CreateToggleCommand(() => StopWalk, v => StopWalk = v, _stopWalk_Name);

    public ICommand ToggleAllowMindCtrlCommand => CreateToggleCommand(() => AllowMindCtrl, v => AllowMindCtrl = v, _allowMindCtrl_Name, false);

    public ICommand ToggleLimitZombieGetDebuffCommand => CreateToggleCommand(() => LimitZombieGetDebuff, v => LimitZombieGetDebuff = v, _limitZombieGetDebuff_Name, false);

    public ICommand UpdateButtonStatusCommand => new RelayCommand(async _ =>
                                                                                                                                                                                        {
                                                                                                                                                                                            await _scriptExec.ExecuteAsync(Constants.SubFolders.Zombies, "GetButtonCheck");
                                                                                                                                                                                        });

    public ICommand SetaCommand(string scriptName)
    {
        return new RelayCommand(async _ =>
            await _scriptExec.ExecuteAsync(Constants.SubFolders.Zombies, scriptName,
            new Dictionary<string, string>
            {
                [Constants.Placeholders.MindCheck] = ButtonHelper.GetCheckValue(AllowMindCtrl),
                [Constants.Placeholders.LimitCheck] = ButtonHelper.GetCheckValue(LimitZombieGetDebuff)
            }));
    }

    private ICommand CreateToggleCommand(Func<string> stateGetter, Action<string> stateSetter, string scriptName, bool IsSendSync = true)
    {
        return new RelayCommand(async _ =>
        {
            var oldVal = stateGetter();
            var newState = ButtonHelper.ToggleCheck(oldVal);
            stateSetter(newState);
            if(IsSendSync)
            {
                bool sent = await _scriptExec.ExecuteAsync(Constants.SubFolders.Zombies, scriptName,
                    new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(newState) });
                if(!sent) stateSetter(oldVal);
            }
        });
    }

    private void OnButtonStatusUpdated(Dictionary<string, bool> statusDict)
    {
        UpdatePropertiesFromDict(statusDict, _buttonMapping);
    }
}
