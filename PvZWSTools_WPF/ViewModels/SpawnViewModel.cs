using System.Windows;
using System.Windows.Input;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels;

public class SpawnViewModel:ViewModelBase
{
    private readonly string _defaultPath;
    private readonly IScriptExecutionService _scriptExec;
    private string _bungeeCheck = Constants.c_Symbol_Off;
    private string _jsonEditZombiesInWave = Constants.c_Symbol_Off;
    private string _maxPoint = Constants.c_Symbol_Off;
    private string _redeyeCheck = Constants.c_Symbol_Off;
    private string _stopSpawn = Constants.c_Symbol_Off;
    private string _zombieBackupDancer = Constants.c_Symbol_Off;
    private string _zombieBalloon = Constants.c_Symbol_Off;
    private string _zombieBobsled = Constants.c_Symbol_Off;
    private string _zombieBoss = Constants.c_Symbol_Off;
    private string _zombieBungee = Constants.c_Symbol_Off;
    private string _zombieCatapult = Constants.c_Symbol_Off;
    private string _zombieDancer = Constants.c_Symbol_Off;
    private string _zombieDigger = Constants.c_Symbol_Off;
    private string _zombieDolphinRider = Constants.c_Symbol_Off;
    private string _zombieDoor = Constants.c_Symbol_Off;
    private string _zombieDuckyTube = Constants.c_Symbol_Off;
    private string _zombieFlag = Constants.c_Symbol_Off;
    private string _zombieFootball = Constants.c_Symbol_Off;
    private string _zombieFootballPremium = Constants.c_Symbol_Off;
    private string _zombieGargantuar = Constants.c_Symbol_Off;
    private string _zombieGatlingHead = Constants.c_Symbol_Off;
    private string _zombieHealthMax = "0.65";
    private string _zombieHealthMin = "0.5";
    private string _zombieImp = Constants.c_Symbol_Off;
    private string _zombieJackInTheBox = Constants.c_Symbol_Off;
    private string _zombieJalapenoHead = Constants.c_Symbol_Off;
    private string _zombieLadder = Constants.c_Symbol_Off;
    private string _zombieMonk = Constants.c_Symbol_Off;
    private string _zombieNewspaper = Constants.c_Symbol_Off;
    private string _zombieNinja = Constants.c_Symbol_Off;
    private string _zombieNormal = Constants.c_Symbol_Off;
    private string _zombiePail = Constants.c_Symbol_Off;
    private string _zombiePeaHead = Constants.c_Symbol_Off;
    private string _zombiePogo = Constants.c_Symbol_Off;
    private string _zombiePolevaulter = Constants.c_Symbol_Off;
    private string _zombiePropeller = Constants.c_Symbol_Off;
    private string _zombieRedeyeGargantuar = Constants.c_Symbol_Off;
    private string _zombieRedeyeRobotTitan = Constants.c_Symbol_Off;
    private string _zombieRobotTitan = Constants.c_Symbol_Off;
    private string _zombieSnorkel = Constants.c_Symbol_Off;
    private string _zombieSquashHead = Constants.c_Symbol_Off;
    private string _zombieTalisman = Constants.c_Symbol_Off;
    private string _zombieTallnutHead = Constants.c_Symbol_Off;
    private string _zombieTrafficCone = Constants.c_Symbol_Off;
    private string _zombieWallnutHead = Constants.c_Symbol_Off;
    private string _zombieYeti = Constants.c_Symbol_Off;
    private string _zombieZamboni = Constants.c_Symbol_Off;

    public SpawnViewModel(IScriptExecutionService scriptExec, string defaultPath)
    {
        _scriptExec = scriptExec;
        _defaultPath = defaultPath;
    }

    public string BungeeCheck
    { get => _bungeeCheck; set { _bungeeCheck = value; OnPropertyChanged(); } }

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

    public ICommand JsonEditCommand => new RelayCommand(_ => { JsonEditZombiesInWave = ButtonHelper.ToggleCheck(JsonEditZombiesInWave); });

    public string JsonEditZombiesInWave
    { get => _jsonEditZombiesInWave; set { _jsonEditZombiesInWave = value; OnPropertyChanged(); } }

    public ICommand JsonEditZombiesInWaveCommand => new RelayCommand(_ => { JsonEditZombiesInWave = ButtonHelper.ToggleCheck(JsonEditZombiesInWave); });

    public ICommand LimitTestCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "极限出怪测试"));

    public ICommand LoadJsonZombiesInWaveCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "载入json", new Dictionary<string, string> { [Constants.Placeholders.DefaultPath] = _defaultPath }));

    public string MaxPoint
    { get => _maxPoint; set { _maxPoint = value; OnPropertyChanged(); } }

    public ICommand MaxPointCommand => CreateToggleCommand(() => MaxPoint, "最大密度");

    public ICommand PrintZombieSpawnCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "打印场上僵尸"));

    public string RedeyeCheck
    { get => _redeyeCheck; set { _redeyeCheck = value; OnPropertyChanged(); } }

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

    // 10. BackupDancer
    public string ZombieBackupDancer
    {
        get => _zombieBackupDancer;
        set { _zombieBackupDancer = value; OnPropertyChanged(); }
    }

    public ICommand ZombieBackupDancerCommand => CreateSpawnToggleCommand("ZombieBackupDancer");

    // 17. Balloon
    public string ZombieBalloon
    {
        get => _zombieBalloon;
        set { _zombieBalloon = value; OnPropertyChanged(); }
    }

    public ICommand ZombieBalloonCommand => CreateSpawnToggleCommand("ZombieBalloon");

    // 14. Bobsled
    public string ZombieBobsled
    {
        get => _zombieBobsled;
        set { _zombieBobsled = value; OnPropertyChanged(); }
    }

    public ICommand ZombieBobsledCommand => CreateSpawnToggleCommand("ZombieBobsled");

    // 26. Boss
    public string ZombieBoss
    {
        get => _zombieBoss;
        set { _zombieBoss = value; OnPropertyChanged(); }
    }

    public ICommand ZombieBossCommand => CreateSpawnToggleCommand("ZombieBoss");

    // 21. Bungee
    public string ZombieBungee
    {
        get => _zombieBungee;
        set { _zombieBungee = value; OnPropertyChanged(); }
    }

    public ICommand ZombieBungeeCommand => CreateSpawnToggleCommand("ZombieBungee");

    // 23. Catapult
    public string ZombieCatapult
    {
        get => _zombieCatapult;
        set { _zombieCatapult = value; OnPropertyChanged(); }
    }

    public ICommand ZombieCatapultCommand => CreateSpawnToggleCommand("ZombieCatapult");

    // 9. Dancer
    public string ZombieDancer
    {
        get => _zombieDancer;
        set { _zombieDancer = value; OnPropertyChanged(); }
    }

    public ICommand ZombieDancerCommand => CreateSpawnToggleCommand("ZombieDancer");

    // 18. Digger
    public string ZombieDigger
    {
        get => _zombieDigger;
        set { _zombieDigger = value; OnPropertyChanged(); }
    }

    public ICommand ZombieDiggerCommand => CreateSpawnToggleCommand("ZombieDigger");

    // 15. DolphinRider
    public string ZombieDolphinRider
    {
        get => _zombieDolphinRider;
        set { _zombieDolphinRider = value; OnPropertyChanged(); }
    }

    public ICommand ZombieDolphinRiderCommand => CreateSpawnToggleCommand("ZombieDolphinRider");

    // 7. Door
    public string ZombieDoor
    {
        get => _zombieDoor;
        set { _zombieDoor = value; OnPropertyChanged(); }
    }

    public ICommand ZombieDoorCommand => CreateSpawnToggleCommand("ZombieDoor");

    // 11. DuckyTube
    public string ZombieDuckyTube
    {
        get => _zombieDuckyTube;
        set { _zombieDuckyTube = value; OnPropertyChanged(); }
    }

    public ICommand ZombieDuckyTubeCommand => CreateSpawnToggleCommand("ZombieDuckyTube");

    // 2. Flag
    public string ZombieFlag
    {
        get => _zombieFlag;
        set { _zombieFlag = value; OnPropertyChanged(); }
    }

    public ICommand ZombieFlagCommand => CreateSpawnToggleCommand("ZombieFlag");

    // 8. Football
    public string ZombieFootball
    {
        get => _zombieFootball;
        set { _zombieFootball = value; OnPropertyChanged(); }
    }

    public ICommand ZombieFootballCommand => CreateSpawnToggleCommand("ZombieFootball");

    // 37. FootballPremium
    public string ZombieFootballPremium
    {
        get => _zombieFootballPremium;
        set { _zombieFootballPremium = value; OnPropertyChanged(); }
    }

    public ICommand ZombieFootballPremiumCommand => CreateSpawnToggleCommand("ZombieFootballPremium");

    // 24. Gargantuar
    public string ZombieGargantuar
    {
        get => _zombieGargantuar;
        set { _zombieGargantuar = value; OnPropertyChanged(); }
    }

    public ICommand ZombieGargantuarCommand => CreateSpawnToggleCommand("ZombieGargantuar");

    // 30. GatlingHead
    public string ZombieGatlingHead
    {
        get => _zombieGatlingHead;
        set { _zombieGatlingHead = value; OnPropertyChanged(); }
    }

    public ICommand ZombieGatlingHeadCommand => CreateSpawnToggleCommand("ZombieGatlingHead");

    public string ZombieHealthMax
    { get => _zombieHealthMax; set { _zombieHealthMax = value; OnPropertyChanged(); } }

    public string ZombieHealthMin
    { get => _zombieHealthMin; set { _zombieHealthMin = value; OnPropertyChanged(); } }

    public ICommand ZombieHealthToNextWaveCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "刷新血量", new Dictionary<string, string> { [Constants.Placeholders.Min] = ZombieHealthMin, [Constants.Placeholders.Max] = ZombieHealthMax }));

    // 25. Imp
    public string ZombieImp
    {
        get => _zombieImp;
        set { _zombieImp = value; OnPropertyChanged(); }
    }

    public ICommand ZombieImpCommand => CreateSpawnToggleCommand("ZombieImp");

    // 16. JackInTheBox
    public string ZombieJackInTheBox
    {
        get => _zombieJackInTheBox;
        set { _zombieJackInTheBox = value; OnPropertyChanged(); }
    }

    public ICommand ZombieJackInTheBoxCommand => CreateSpawnToggleCommand("ZombieJackInTheBox");

    // 29. JalapenoHead
    public string ZombieJalapenoHead
    {
        get => _zombieJalapenoHead;
        set { _zombieJalapenoHead = value; OnPropertyChanged(); }
    }

    public ICommand ZombieJalapenoHeadCommand => CreateSpawnToggleCommand("ZombieJalapenoHead");

    // 22. Ladder
    public string ZombieLadder
    {
        get => _zombieLadder;
        set { _zombieLadder = value; OnPropertyChanged(); }
    }

    public ICommand ZombieLadderCommand => CreateSpawnToggleCommand("ZombieLadder");

    // 36. Monk
    public string ZombieMonk
    {
        get => _zombieMonk;
        set { _zombieMonk = value; OnPropertyChanged(); }
    }

    public ICommand ZombieMonkCommand => CreateSpawnToggleCommand("ZombieMonk");

    // 6. Newspaper
    public string ZombieNewspaper
    {
        get => _zombieNewspaper;
        set { _zombieNewspaper = value; OnPropertyChanged(); }
    }

    public ICommand ZombieNewspaperCommand => CreateSpawnToggleCommand("ZombieNewspaper");

    // 38. Ninja
    public string ZombieNinja
    {
        get => _zombieNinja;
        set { _zombieNinja = value; OnPropertyChanged(); }
    }

    public ICommand ZombieNinjaCommand => CreateSpawnToggleCommand("ZombieNinja");

    // 1. Normal
    public string ZombieNormal
    {
        get => _zombieNormal;
        set { _zombieNormal = value; OnPropertyChanged(); }
    }

    public ICommand ZombieNormalCommand => CreateSpawnToggleCommand("ZombieNormal");

    // 5. Pail
    public string ZombiePail
    {
        get => _zombiePail;
        set { _zombiePail = value; OnPropertyChanged(); }
    }

    public ICommand ZombiePailCommand => CreateSpawnToggleCommand("ZombiePail");

    // 27. PeaHead
    public string ZombiePeaHead
    {
        get => _zombiePeaHead;
        set { _zombiePeaHead = value; OnPropertyChanged(); }
    }

    public ICommand ZombiePeaHeadCommand => CreateSpawnToggleCommand("ZombiePeaHead");

    // 19. Pogo
    public string ZombiePogo
    {
        get => _zombiePogo;
        set { _zombiePogo = value; OnPropertyChanged(); }
    }

    public ICommand ZombiePogoCommand => CreateSpawnToggleCommand("ZombiePogo");

    // 4. Polevaulter
    public string ZombiePolevaulter
    {
        get => _zombiePolevaulter;
        set { _zombiePolevaulter = value; OnPropertyChanged(); }
    }

    public ICommand ZombiePolevaulterCommand => CreateSpawnToggleCommand("ZombiePolevaulter");

    // 40. Propeller
    public string ZombiePropeller
    {
        get => _zombiePropeller;
        set { _zombiePropeller = value; OnPropertyChanged(); }
    }

    public ICommand ZombiePropellerCommand => CreateSpawnToggleCommand("ZombiePropeller");

    // 33. RedeyeGargantuar
    public string ZombieRedeyeGargantuar
    {
        get => _zombieRedeyeGargantuar;
        set { _zombieRedeyeGargantuar = value; OnPropertyChanged(); }
    }

    public ICommand ZombieRedeyeGargantuarCommand => CreateSpawnToggleCommand("ZombieRedeyeGargantuar");

    // 35. RedeyeRobotTitan
    public string ZombieRedeyeRobotTitan
    {
        get => _zombieRedeyeRobotTitan;
        set { _zombieRedeyeRobotTitan = value; OnPropertyChanged(); }
    }

    public ICommand ZombieRedeyeRobotTitanCommand => CreateSpawnToggleCommand("ZombieRedeyeRobotTitan");

    // 34. RobotTitan
    public string ZombieRobotTitan
    {
        get => _zombieRobotTitan;
        set { _zombieRobotTitan = value; OnPropertyChanged(); }
    }

    public ICommand ZombieRobotTitanCommand => CreateSpawnToggleCommand("ZombieRobotTitan");

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

    // 12. Snorkel
    public string ZombieSnorkel
    {
        get => _zombieSnorkel;
        set { _zombieSnorkel = value; OnPropertyChanged(); }
    }

    public ICommand ZombieSnorkelCommand => CreateSpawnToggleCommand("ZombieSnorkel");

    // 31. SquashHead
    public string ZombieSquashHead
    {
        get => _zombieSquashHead;
        set { _zombieSquashHead = value; OnPropertyChanged(); }
    }

    public ICommand ZombieSquashHeadCommand => CreateSpawnToggleCommand("ZombieSquashHead");

    // 39. Talisman
    public string ZombieTalisman
    {
        get => _zombieTalisman;
        set { _zombieTalisman = value; OnPropertyChanged(); }
    }

    public ICommand ZombieTalismanCommand => CreateSpawnToggleCommand("ZombieTalisman");

    // 32. TallnutHead
    public string ZombieTallnutHead
    {
        get => _zombieTallnutHead;
        set { _zombieTallnutHead = value; OnPropertyChanged(); }
    }

    public ICommand ZombieTallnutHeadCommand => CreateSpawnToggleCommand("ZombieTallnutHead");

    // 3. TrafficCone
    public string ZombieTrafficCone
    {
        get => _zombieTrafficCone;
        set { _zombieTrafficCone = value; OnPropertyChanged(); }
    }

    public ICommand ZombieTrafficConeCommand => CreateSpawnToggleCommand("ZombieTrafficCone");

    // 28. WallnutHead
    public string ZombieWallnutHead
    {
        get => _zombieWallnutHead;
        set { _zombieWallnutHead = value; OnPropertyChanged(); }
    }

    public ICommand ZombieWallnutHeadCommand => CreateSpawnToggleCommand("ZombieWallnutHead");

    // 20. Yeti
    public string ZombieYeti
    {
        get => _zombieYeti;
        set { _zombieYeti = value; OnPropertyChanged(); }
    }

    public ICommand ZombieYetiCommand => CreateSpawnToggleCommand("ZombieYeti");

    // 13. Zamboni
    public string ZombieZamboni
    {
        get => _zombieZamboni;
        set { _zombieZamboni = value; OnPropertyChanged(); }
    }

    public ICommand ZombieZamboniCommand => CreateSpawnToggleCommand("ZombieZamboni");

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

    private void UpdateZombieStatesFromOutput(string output)
    {
        if(string.IsNullOrWhiteSpace(output)) return;

        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach(var line in lines)
        {
            int arrowIndex = line.IndexOf("=>");
            if(arrowIndex == -1) continue;

            string zombieType = line.Substring(0, arrowIndex).Trim();
            string boolStr = line.Substring(arrowIndex + 2).Trim();

            if(!bool.TryParse(boolStr, out bool isAllowed)) continue;

            string newSymbol = isAllowed ? Constants.c_Symbol_On : Constants.c_Symbol_Off;
            string propName = "Zombie" + zombieType;   // 直接拼接

            var prop = GetType().GetProperty(propName);
            if(prop != null && prop.PropertyType == typeof(string))
            {
                Application.Current.Dispatcher.Invoke(() => prop.SetValue(this, newSymbol));
            }
        }
    }
}
