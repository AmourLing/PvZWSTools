using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Models;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels
{
    public class ZombiesViewModel:ViewModelBase
    {
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

        public ZombiesViewModel(IScriptExecutionService scriptExec)
        {
            _scriptExec = scriptExec;
        }

        public string AllowMindCtrl
        {
            get => _allowMindCtrl;
            set { _allowMindCtrl = value; OnPropertyChanged(); }
        }

        public string DrawZombieHP
        {
            get => _drawZombieHP;
            set { _drawZombieHP = value; OnPropertyChanged(); }
        }

        public ICommand DrawZombieHPCommand => CreateToggleCommand(() => DrawZombieHP, v => DrawZombieHP = v, _drawZombieHP_Name);

        public string DropPacket
        {
            get => _dropPacket;
            set { _dropPacket = value; OnPropertyChanged(); }
        }

        public ICommand DropPacketCommand => CreateToggleCommand(() => DropPacket, v => DropPacket = v, _dropPacket_Name);

        public string InvincZombie
        {
            get => _invincZombie;
            set { _invincZombie = value; OnPropertyChanged(); }
        }

        public ICommand InvincZombieCommand => CreateToggleCommand(() => InvincZombie, v => InvincZombie = v, _invincZombie_Name);

        public string LimitZombieGetDebuff
        {
            get => _limitZombieGetDebuff;
            set { _limitZombieGetDebuff = value; OnPropertyChanged(); }
        }

        public string NoExplode
        {
            get => _noExplode;
            set { _noExplode = value; OnPropertyChanged(); }
        }

        public ICommand NoExplodeCommand => CreateToggleCommand(() => NoExplode, v => NoExplode = v, _noExplode_Name);

        public string NoIceTrap
        {
            get => _noIceTrap;
            set { _noIceTrap = value; OnPropertyChanged(); }
        }

        public ICommand NoIceTrapCommand => CreateToggleCommand(() => NoIceTrap, v => NoIceTrap = v, _noIceTrap_Name);

        public string NoSteal
        {
            get => _noSteal;
            set { _noSteal = value; OnPropertyChanged(); }
        }

        public ICommand NoStealCommand => CreateToggleCommand(() => NoSteal, v => NoSteal = v, _noSteal_Name);
        public ICommand SetButteredCommand => SetaCommand(_setButtered_Name);

        public ICommand SetIceTrapCommand => SetaCommand(_setIceTrap_Name);

        public ICommand SetMindControlCommand => SetaCommand(_setMindControl_Name);

        public ICommand SetYuckyFaceCommand => SetaCommand(_setYuckyFace_Name);

        public string StopWalk
        {
            get => _stopWalk;
            set { _stopWalk = value; OnPropertyChanged(); }
        }

        public ICommand StopWalkCommand => CreateToggleCommand(() => StopWalk, v => StopWalk = v, _stopWalk_Name);

        public ICommand ToggleAllowMindCtrlCommand => CreateToggleCommand(() => AllowMindCtrl, v => AllowMindCtrl = v, _allowMindCtrl_Name, false);

        public ICommand ToggleLimitZombieGetDebuffCommand => CreateToggleCommand(() => LimitZombieGetDebuff, v => LimitZombieGetDebuff = v, _limitZombieGetDebuff_Name, false);

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
                var current = stateGetter();
                var newState = ButtonHelper.ToggleCheck(current);
                stateSetter(newState);
                if(IsSendSync)
                {
                    await _scriptExec.ExecuteAsync(Constants.SubFolders.Zombies, scriptName,
                        new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(newState) });
                }
            });
        }
    }
}
