using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels
{
    public class ChallengeViewModel:ViewModelBase
    {
        private readonly IScriptExecutionService _scriptExec;

        private string _beghouled = "2";

        private string _column = "2";

        private string _conveyorBelt = "2";

        private string _iZombie = "2";

        private string _lastStand = "2";

        private string _portal = "2";

        private string _rain = "2";

        private string _scaryPotter = "2";

        private string _slotMachine = "2";

        private string _speed = "2";

        private string _squirrel = "2";

        private string _stormyNight = "2";

        private string _whackAZombie = "2";

        public ChallengeViewModel(IScriptExecutionService scriptExec)
        {
            _scriptExec = scriptExec;
        }

        public string Beghouled
        { get => _beghouled; set { _beghouled = value; OnPropertyChanged(); } }

        public ICommand BeghouledCommand => new RelayCommand(async _ =>
        {
            Beghouled = ButtonHelper.ToggleChallenge(Beghouled);
            await UpdateOthersAsync();
        });

        public string Column
        { get => _column; set { _column = value; OnPropertyChanged(); } }

        public ICommand ColumnCommand => CreateChallengeCommand(() => Column, "排山倒海");

        public string ConveyorBelt
        { get => _conveyorBelt; set { _conveyorBelt = value; OnPropertyChanged(); } }

        public ICommand ConveyorBeltCommand => CreateChallengeCommand(() => ConveyorBelt, "传送带");

        public string IZombie
        { get => _iZombie; set { _iZombie = value; OnPropertyChanged(); } }

        public ICommand IZombieCommand => CreateChallengeCommand(() => IZombie, "IZombie");

        public string LastStand
        { get => _lastStand; set { _lastStand = value; OnPropertyChanged(); } }

        public ICommand LastStandCommand => new RelayCommand(async _ =>
        {
            LastStand = ButtonHelper.ToggleChallenge(LastStand);
            await UpdateOthersAsync();
        });

        public string Portal
        { get => _portal; set { _portal = value; OnPropertyChanged(); } }

        public ICommand PortalCommand => new RelayCommand(async _ =>
        {
            Portal = ButtonHelper.ToggleChallenge(Portal);
            await UpdateOthersAsync();
        });

        public string Rain
        { get => _rain; set { _rain = value; OnPropertyChanged(); } }

        public ICommand RainCommand => new RelayCommand(async _ =>
        {
            Rain = ButtonHelper.ToggleChallenge(Rain);
            await UpdateOthersAsync();
        });

        public string ScaryPotter
        { get => _scaryPotter; set { _scaryPotter = value; OnPropertyChanged(); } }

        public ICommand ScaryPotterCommand => CreateChallengeCommand(() => ScaryPotter, "砸罐子");

        public string SlotMachine
        { get => _slotMachine; set { _slotMachine = value; OnPropertyChanged(); } }

        public ICommand SlotMachineCommand => CreateChallengeCommand(() => SlotMachine, "老虎机");

        public string Speed
        { get => _speed; set { _speed = value; OnPropertyChanged(); } }

        public ICommand SpeedCommand => new RelayCommand(async _ =>
        {
            Speed = ButtonHelper.ToggleChallenge(Speed);
            await UpdateOthersAsync();
        });

        public string Squirrel
        { get => _squirrel; set { _squirrel = value; OnPropertyChanged(); } }

        public ICommand SquirrelCommand => CreateChallengeCommand(() => Squirrel, "松鼠");

        public string StormyNight
        { get => _stormyNight; set { _stormyNight = value; OnPropertyChanged(); } }

        public ICommand StormyNightCommand => CreateChallengeCommand(() => StormyNight, "风暴");

        public string WhackAZombie
        { get => _whackAZombie; set { _whackAZombie = value; OnPropertyChanged(); } }

        public ICommand WhackAZombieCommand => CreateChallengeCommand(() => WhackAZombie, "砸僵尸");

        private ICommand CreateChallengeCommand(System.Func<string> stateGetter, string scriptName)
        {
            return new RelayCommand(async _ =>
            {
                var current = stateGetter();
                var newState = ButtonHelper.ToggleChallenge(current);
                if(scriptName == "风暴") StormyNight = newState;
                else if(scriptName == "传送带") ConveyorBelt = newState;
                else if(scriptName == "砸罐子") ScaryPotter = newState;
                else if(scriptName == "砸僵尸") WhackAZombie = newState;
                else if(scriptName == "IZombie") IZombie = newState;
                else if(scriptName == "老虎机") SlotMachine = newState;
                else if(scriptName == "松鼠") Squirrel = newState;
                else if(scriptName == "排山倒海") Column = newState;

                if(scriptName == "风暴" || scriptName == "传送带" || scriptName == "砸罐子" ||
                    scriptName == "砸僵尸" || scriptName == "IZombie" || scriptName == "老虎机" ||
                    scriptName == "松鼠" || scriptName == "排山倒海")
                {
                    await _scriptExec.ExecuteAsync(Constants.SubFolders.Challenge, scriptName,
                        new Dictionary<string, string> { [Constants.Placeholders.Check] = newState });
                }
                else
                {
                    await UpdateOthersAsync();
                }
            });
        }

        private async Task UpdateOthersAsync()
        {
            await _scriptExec.ExecuteAsync(Constants.SubFolders.Challenge, "其他挑战",
                new Dictionary<string, string>
                {
                    ["{RAIN_CHECK}"] = Rain,
                    ["{BEGHOULED_CHECK}"] = Beghouled,
                    ["{SPEED_CHECK}"] = Speed,
                    ["{PORTALCOMBAT_CHECK}"] = Portal,
                    ["{LAST_STAND_CHECK}"] = LastStand
                });
        }
    }
}
