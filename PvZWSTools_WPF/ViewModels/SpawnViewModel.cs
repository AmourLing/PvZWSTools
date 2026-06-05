using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Models;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels
{
    public class SpawnViewModel:ViewModelBase
    {
        private readonly string _defaultPath;
        private readonly IScriptExecutionService _scriptExec;
        private string _balloon = Constants.c_Symbol_Off;
        private string _bungee = Constants.c_Symbol_Off;
        private string _bungeeCheck = Constants.c_Symbol_Off;
        private string _catapult = Constants.c_Symbol_Off;
        private string _dancer = Constants.c_Symbol_Off;
        private string _digger = Constants.c_Symbol_Off;
        private string _dolphinRider = Constants.c_Symbol_Off;
        private string _door = Constants.c_Symbol_Off;
        private string _football = Constants.c_Symbol_Off;
        private string _gargantuar = Constants.c_Symbol_Off;
        private string _jackInTheBox = Constants.c_Symbol_Off;
        private string _jsonEditZombiesInWave = Constants.c_Symbol_Off;
        private string _ladder = Constants.c_Symbol_Off;
        private string _maxPoint = Constants.c_Symbol_Off;
        private string _newspaper = Constants.c_Symbol_Off;
        private string _pail = Constants.c_Symbol_Off;
        private string _pogo = Constants.c_Symbol_Off;
        private string _polevaulter = Constants.c_Symbol_Off;
        private string _redeyeCheck = Constants.c_Symbol_Off;
        private string _redeyeGargantuar = Constants.c_Symbol_Off;
        private string _snorkel = Constants.c_Symbol_Off;
        private string _stopSpawn = Constants.c_Symbol_Off;
        private string _trafficCone = Constants.c_Symbol_Off;
        private string _yeti = Constants.c_Symbol_Off;
        private string _zamboni = Constants.c_Symbol_Off;
        private string _zombieHealthMax = "0.65";
        private string _zombieHealthMin = "0.5";

        public SpawnViewModel(IScriptExecutionService scriptExec, string defaultPath)
        {
            _scriptExec = scriptExec;
            _defaultPath = defaultPath;
        }

        // 所有属性保持不变（略，与之前相同）
        public string Balloon
        { get => _balloon; set { _balloon = value; OnPropertyChanged(); } }

        public ICommand BalloonCommand => CreateSpawnToggleCommand("Balloon");

        public string Bungee
        { get => _bungee; set { _bungee = value; OnPropertyChanged(); } }

        public string BungeeCheck
        { get => _bungeeCheck; set { _bungeeCheck = value; OnPropertyChanged(); } }

        public ICommand BungeeCommand => CreateSpawnToggleCommand("Bungee");

        public ICommand BungeeHandleCommand => new RelayCommand(async _ =>
        {
            BungeeCheck = ButtonHelper.ToggleCheck(BungeeCheck);
            await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "蹦极红眼处理",
                new Dictionary<string, string>
                {
                    [Constants.Placeholders.BungeeCheck] = ButtonHelper.GetCheckValue(BungeeCheck),
                    [Constants.Placeholders.RedeyeCheck] = ButtonHelper.GetCheckValue(RedeyeCheck)
                });
        });

        public string Catapult
        { get => _catapult; set { _catapult = value; OnPropertyChanged(); } }

        public ICommand CatapultCommand => CreateSpawnToggleCommand("Catapult");

        public string Dancer
        { get => _dancer; set { _dancer = value; OnPropertyChanged(); } }

        public ICommand DancerCommand => CreateSpawnToggleCommand("Dancer");

        public string Digger
        { get => _digger; set { _digger = value; OnPropertyChanged(); } }

        public ICommand DiggerCommand => CreateSpawnToggleCommand("Digger");

        public string DolphinRider
        { get => _dolphinRider; set { _dolphinRider = value; OnPropertyChanged(); } }

        public ICommand DolphinRiderCommand => CreateSpawnToggleCommand("DolphinRider");

        public string Door
        { get => _door; set { _door = value; OnPropertyChanged(); } }

        public ICommand DoorCommand => CreateSpawnToggleCommand("Door");

        public string Football
        { get => _football; set { _football = value; OnPropertyChanged(); } }

        public ICommand FootballCommand => CreateSpawnToggleCommand("Football");

        public string Gargantuar
        { get => _gargantuar; set { _gargantuar = value; OnPropertyChanged(); } }

        public ICommand GargantuarCommand => CreateSpawnToggleCommand("Gargantuar");

        // 在 SpawnViewModel 类中，替换 GetZombieSpawnCommand 和 UpdateZombieStatesFromOutput
        public ICommand GetZombieSpawnCommand => new RelayCommand(async _ =>
        {
            try
            {
                string output = await _scriptExec.ExecuteWithResultAsync(Constants.SubFolders.Spawn, "获取当前出怪");
                UpdateZombieStatesFromOutput(output);
            }
            catch(Exception ex)
            {
                Log.Error($"获取当前出怪失败: {ex}");
            }
        });

        private void UpdateZombieStatesFromOutput(string output)
        {
            if(string.IsNullOrWhiteSpace(output))
            {
                Log.Warning("获取当前出怪返回空输出");
                return;
            }

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var line in lines)
            {
                int arrowIndex = line.IndexOf("=>");
                if(arrowIndex == -1)
                {
                    continue;
                }

                string zombieType = line.Substring(0, arrowIndex).Trim();
                string boolStr = line.Substring(arrowIndex + 2).Trim();

                if(!bool.TryParse(boolStr, out bool isAllowed))
                {
                    continue;
                }

                string newSymbol = isAllowed ? Constants.c_Symbol_On : Constants.c_Symbol_Off;

                string propName = zombieType switch
                {
                    "BackupDancer" => "Dancer",
                    _ => zombieType
                };

                var prop = GetType().GetProperty(propName);
                if(prop != null && prop.PropertyType == typeof(string))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        prop.SetValue(this, newSymbol);
                    });
                }
                else
                {
                }
            }
        }

        public string JackInTheBox
        { get => _jackInTheBox; set { _jackInTheBox = value; OnPropertyChanged(); } }

        public ICommand JackInTheBoxCommand => CreateSpawnToggleCommand("JackInTheBox");

        public ICommand JsonEditCommand => new RelayCommand(_ => { JsonEditZombiesInWave = ButtonHelper.ToggleCheck(JsonEditZombiesInWave); });

        public string JsonEditZombiesInWave
        { get => _jsonEditZombiesInWave; set { _jsonEditZombiesInWave = value; OnPropertyChanged(); } }

        public ICommand JsonEditZombiesInWaveCommand => new RelayCommand(_ => { JsonEditZombiesInWave = ButtonHelper.ToggleCheck(JsonEditZombiesInWave); });

        public string Ladder
        { get => _ladder; set { _ladder = value; OnPropertyChanged(); } }

        public ICommand LadderCommand => CreateSpawnToggleCommand("Ladder");

        public ICommand LimitTestCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "极限出怪测试"));
        public ICommand LoadJsonZombiesInWaveCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "载入json", new Dictionary<string, string> { [Constants.Placeholders.DefaultPath] = _defaultPath }));

        public string MaxPoint
        { get => _maxPoint; set { _maxPoint = value; OnPropertyChanged(); } }

        public ICommand MaxPointCommand => CreateToggleCommand(() => MaxPoint, "最大密度");

        public string Newspaper
        { get => _newspaper; set { _newspaper = value; OnPropertyChanged(); } }

        public ICommand NewspaperCommand => CreateSpawnToggleCommand("Newspaper");

        public string Pail
        { get => _pail; set { _pail = value; OnPropertyChanged(); } }

        public ICommand PailCommand => CreateSpawnToggleCommand("Pail");

        public string Pogo
        { get => _pogo; set { _pogo = value; OnPropertyChanged(); } }

        public ICommand PogoCommand => CreateSpawnToggleCommand("Pogo");

        public string Polevaulter
        { get => _polevaulter; set { _polevaulter = value; OnPropertyChanged(); } }

        public ICommand PolevaulterCommand => CreateSpawnToggleCommand("Polevaulter");

        public ICommand PrintZombieSpawnCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "打印场上僵尸"));

        public string RedeyeCheck
        { get => _redeyeCheck; set { _redeyeCheck = value; OnPropertyChanged(); } }

        public string RedeyeGargantuar
        { get => _redeyeGargantuar; set { _redeyeGargantuar = value; OnPropertyChanged(); } }

        public ICommand RedeyeGargantuarCommand => CreateSpawnToggleCommand("RedeyeGargantuar");

        public ICommand RedeyeHandleCommand => new RelayCommand(async _ =>
        {
            RedeyeCheck = ButtonHelper.ToggleCheck(RedeyeCheck);
            await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "蹦极红眼处理",
                new Dictionary<string, string>
                {
                    [Constants.Placeholders.BungeeCheck] = ButtonHelper.GetCheckValue(BungeeCheck),
                    [Constants.Placeholders.RedeyeCheck] = ButtonHelper.GetCheckValue(RedeyeCheck)
                });
        });

        public string Snorkel
        { get => _snorkel; set { _snorkel = value; OnPropertyChanged(); } }

        public ICommand SnorkelCommand => CreateSpawnToggleCommand("Snorkel");

        public string StopSpawn
        { get => _stopSpawn; set { _stopSpawn = value; OnPropertyChanged(); } }

        public ICommand StopSpawnCommand => CreateToggleCommand(() => StopSpawn, "暂停出怪");

        public ICommand ToggleSpawnCommand => new RelayCommand(async param =>
        {
            if(param is string zombieKey)
            {
                var prop = GetType().GetProperty(zombieKey);
                if(prop != null)
                {
                    var current = (string)prop.GetValue(this);
                    var newState = ButtonHelper.ToggleCheck(current);
                    prop.SetValue(this, newState);

                    string placeholder = string.Format(Constants.Placeholders.SpawnCheck, zombieKey.ToUpper());
                    await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "修改出怪",
                        new Dictionary<string, string> { [placeholder] = ButtonHelper.GetCheckValue(newState) });
                }
            }
        });

        public string TrafficCone
        { get => _trafficCone; set { _trafficCone = value; OnPropertyChanged(); } }

        public ICommand TrafficConeCommand => CreateSpawnToggleCommand("TrafficCone");

        public string Yeti
        { get => _yeti; set { _yeti = value; OnPropertyChanged(); } }

        public ICommand YetiCommand => CreateSpawnToggleCommand("Yeti");

        public string Zamboni
        { get => _zamboni; set { _zamboni = value; OnPropertyChanged(); } }

        public ICommand ZamboniCommand => CreateSpawnToggleCommand("Zamboni");

        public string ZombieHealthMax
        { get => _zombieHealthMax; set { _zombieHealthMax = value; OnPropertyChanged(); } }

        public string ZombieHealthMin
        { get => _zombieHealthMin; set { _zombieHealthMin = value; OnPropertyChanged(); } }

        public ICommand ZombieHealthToNextWaveCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "刷新血量", new Dictionary<string, string> { [Constants.Placeholders.Min] = ZombieHealthMin, [Constants.Placeholders.Max] = ZombieHealthMax }));

        public ICommand ZombiesInWaveCountCommand => new RelayCommand(async _ =>
        {
            string path = System.IO.Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_Options, Constants.JsonZombieFile);
            await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "波次出怪_数量", new Dictionary<string, string> { [Constants.Placeholders.Path] = path, [Constants.Placeholders.DefaultPath] = _defaultPath, [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(JsonEditZombiesInWave) });
        });

        public ICommand ZombiesInWaveIndexCommand => new RelayCommand(async _ =>
        {
            string path = System.IO.Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_Options, Constants.JsonZombieFile);
            await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "波次出怪_序号", new Dictionary<string, string> { [Constants.Placeholders.Path] = path, [Constants.Placeholders.DefaultPath] = _defaultPath, [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(JsonEditZombiesInWave) });
        });

        private ICommand CreateSpawnToggleCommand(string zombieKey)
        {
            return new RelayCommand(async _ =>
            {
                var prop = GetType().GetProperty(zombieKey);
                if(prop != null)
                {
                    var current = (string)prop.GetValue(this);
                    var newState = ButtonHelper.ToggleCheck(current);
                    prop.SetValue(this, newState);
                    try
                    {
                        string format = Constants.Placeholders.SpawnCheck;
                        string key = zombieKey.ToUpper();
                        string placeholder = string.Format(format, key);
                        await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "修改出怪",
                            new Dictionary<string, string> { [placeholder] = ButtonHelper.GetCheckValue(newState) });
                    }
                    catch(FormatException fe)
                    {
                        Log.Error($"FormatException: format='{Constants.Placeholders.SpawnCheck}', key='{zombieKey.ToUpper()}'");
                        Log.Error($"异常详情: {fe}");
                    }
                    catch(Exception ex)
                    {
                        Log.Error($"修改出怪失败: {ex}");
                    }
                }
            });
        }

        private ICommand CreateToggleCommand(Func<string> stateGetter, string scriptName)
        {
            return new RelayCommand(async _ =>
            {
                var current = stateGetter();
                var newState = ButtonHelper.ToggleCheck(current);
                if(scriptName == "暂停出怪") StopSpawn = newState;
                else if(scriptName == "最大密度") MaxPoint = newState;
                await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, scriptName,
                    new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(newState) });
            });
        }
    }
}
