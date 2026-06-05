using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Models;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels
{
    public class PlantsViewModel:ViewModelBase
    {
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

        public PlantsViewModel(IScriptExecutionService scriptExec)
        {
            _scriptExec = scriptExec;
            CD_OtherPlantOptions = OptionsLoader.Load(Constants.JsonPlantFile);
        }

        public string CD_Chomper
        { get => _CD_chomper; set { _CD_chomper = value; OnPropertyChanged(); } }

        public ICommand CD_ChomperCommand =>
            CreateToggleCommand(() => CD_Chomper, v => CD_Chomper = v, _CD_chomper_Name);

        public string CD_Cob
        { get => _CD_cob; set { _CD_cob = value; OnPropertyChanged(); } }

        public ICommand CD_CobCommand =>
            CreateToggleCommand(() => CD_Cob, v => CD_Cob = v, _CD_cob_Name);

        public string CD_Magnet
        { get => _CD_magnet; set { _CD_magnet = value; OnPropertyChanged(); } }

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
            set { _CD_otherPlantInput = value; OnPropertyChanged(); }
        }

        public bool CD_OtherPlantDropdownToggleIsChecked
        {
            get => _CD_otherPlantDropdownToggleIsChecked;
            set { _CD_otherPlantDropdownToggleIsChecked = value; OnPropertyChanged(); }
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
        { get => _CD_potato; set { _CD_potato = value; OnPropertyChanged(); } }

        public ICommand CD_PotatoCommand =>
            CreateToggleCommand(() => CD_Potato, v => CD_Potato = v, _CD_potato_Name);

        public string CD_Sunshroom
        { get => _CD_sunshroom; set { _CD_sunshroom = value; OnPropertyChanged(); } }

        public ICommand CD_SunshroomCommand =>
            CreateToggleCommand(() => CD_Sunshroom, v => CD_Sunshroom = v, _CD_sunshroom_Name);

        public string DrawPlantHP
        { get => _drawPlantHP; set { _drawPlantHP = value; OnPropertyChanged(); } }

        public ICommand DrawPlantHPCommand =>
            CreateToggleCommand(() => DrawPlantHP, v => DrawPlantHP = v, _drawPlantHP_Name);

        public string InvincPlant
        { get => _invincPlant; set { _invincPlant = value; OnPropertyChanged(); } }

        public ICommand InvincPlantCommand =>
            CreateToggleCommand(() => InvincPlant, v => InvincPlant = v, _invincPlant_Name);

        public string NoCrater
        { get => _noCrater; set { _noCrater = value; OnPropertyChanged(); } }

        public ICommand NoCraterCommand =>
            CreateToggleCommand(() => NoCrater, v => NoCrater = v, _noCrater_Name);

        public string NoSquish
        { get => _noSquish; set { _noSquish = value; OnPropertyChanged(); } }

        public ICommand NoSquishCommand =>
            CreateToggleCommand(() => NoSquish, v => NoSquish = v, _noSquish_Name);

        public string OnlyButter
        { get => _onlyButter; set { _onlyButter = value; OnPropertyChanged(); } }

        public ICommand OnlyButterCommand =>
            CreateToggleCommand(() => OnlyButter, v => OnlyButter = v, _onlyButter_Name);

        public string WakeUp
        { get => _wakeUp; set { _wakeUp = value; OnPropertyChanged(); } }

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
    }
}
