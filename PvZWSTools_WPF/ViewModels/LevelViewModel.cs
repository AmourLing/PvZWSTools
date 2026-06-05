using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Models;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels
{
    public class LevelViewModel:ViewModelBase
    {
        private readonly IScriptExecutionService _scriptExec;

        private string _adventureInput = "1-1";

        private string _endlessFlagInput = "2025";

        private bool _modeDropdownToggleIsChecked;

        private string _modeInput = "冒险模式";

        private NameOption _modeSelected;

        public LevelViewModel(IScriptExecutionService scriptExec)
        {
            _scriptExec = scriptExec;

            ModeOptions = OptionsLoader.Load(Constants.JsonModeFile);
        }

        public string AdventureInput
        {
            get => _adventureInput;
            set { _adventureInput = value; OnPropertyChanged(); }
        }

        public string EndlessFlag
        {
            get => _endlessFlagInput;
            set { _endlessFlagInput = value; OnPropertyChanged(); }
        }

        public ICommand EnterNewLevelCommand => new RelayCommand(async _ =>
                    await _scriptExec.ExecuteAsync(Constants.SubFolders.Level, "EnterNewLevel"));

        public bool ModeDropdownToggleIsChecked
        {
            get => _modeDropdownToggleIsChecked;
            set { _modeDropdownToggleIsChecked = value; OnPropertyChanged(); }
        }

        public string ModeInput
        {
            get => _modeInput;
            set { _modeInput = value; OnPropertyChanged(); }
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

        public ICommand PassThisLevelCommand => new RelayCommand(async _ =>
                    await _scriptExec.ExecuteAsync(Constants.SubFolders.Level, "直接过关"));

        public ICommand SetEndlessFlagCommand => new RelayCommand(async _ =>
                    await _scriptExec.ExecuteAsync(Constants.SubFolders.Level, "设置无尽旗数",
                        new Dictionary<string, string> { [Constants.Placeholders.Flag] = EndlessFlag }));

        // 命令
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
    }
}
