using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

public class SpawnViewModel:ViewModelBase
{
    private static readonly IReadOnlyDictionary<string, string> _buttonMapping = new Dictionary<string, string>
    {
        ["BUNGEE_FLAG_CHECK"] = nameof(BungeeCheck),
        ["REDEYE_FLAG_CHECK"] = nameof(RedeyeCheck),
        ["STOP_SPAWN_CHECK"] = nameof(StopSpawn),
        ["MAXPOINT_CHECK"] = nameof(MaxPoint)
    };

    private readonly string _defaultPath;
    private readonly IMessageProcessor _messageProcessor;
    private readonly IScriptExecutionService _scriptExec;
    private readonly IUiThreadInvoker _uiThread;
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

    public SpawnViewModel(IScriptExecutionService scriptExec, string defaultPath, IMessageProcessor messageProcessor, IUiThreadInvoker uiThread)
    {
        _scriptExec = scriptExec;
        _defaultPath = defaultPath;
        _messageProcessor = messageProcessor;
        _uiThread = uiThread;
        if(_messageProcessor != null)
            _messageProcessor.ButtonStatusUpdated += OnButtonStatusUpdated;
    }

    public string BungeeCheck
    {
        get => _bungeeCheck;
        set => SetProperty(ref _bungeeCheck, value);
    }

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
    {
        get => _jsonEditZombiesInWave;
        set => SetProperty(ref _jsonEditZombiesInWave, value);
    }

    public ICommand JsonEditZombiesInWaveCommand => new RelayCommand(_ => { JsonEditZombiesInWave = ButtonHelper.ToggleCheck(JsonEditZombiesInWave); });

    public ICommand LimitTestCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "极限出怪测试"));

    public ICommand LoadJsonZombiesInWaveCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "载入json", new Dictionary<string, string> { [Constants.Placeholders.DefaultPath] = _defaultPath }));

    public string MaxPoint
    {
        get => _maxPoint;
        set => SetProperty(ref _maxPoint, value);
    }

    public ICommand MaxPointCommand => CreateToggleCommand(() => MaxPoint, "最大密度");

    public ICommand PrintZombieSpawnCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "打印场上僵尸"));

    public string RedeyeCheck
    {
        get => _redeyeCheck;
        set => SetProperty(ref _redeyeCheck, value);
    }

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
    {
        get => _stopSpawn;
        set => SetProperty(ref _stopSpawn, value);
    }

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

    public ICommand UpdateButtonStatusCommand => new RelayCommand(async _ =>
        {
            await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "GetButtonCheck");
        });

    public string ZombieBackupDancer
    {
        get => _zombieBackupDancer;
        set => SetProperty(ref _zombieBackupDancer, value);
    }

    public ICommand ZombieBackupDancerCommand => CreateSpawnToggleCommand("ZombieBackupDancer");

    public string ZombieBalloon
    {
        get => _zombieBalloon;
        set => SetProperty(ref _zombieBalloon, value);
    }

    public ICommand ZombieBalloonCommand => CreateSpawnToggleCommand("ZombieBalloon");

    public string ZombieBobsled
    {
        get => _zombieBobsled;
        set => SetProperty(ref _zombieBobsled, value);
    }

    public ICommand ZombieBobsledCommand => CreateSpawnToggleCommand("ZombieBobsled");

    public string ZombieBoss
    {
        get => _zombieBoss;
        set => SetProperty(ref _zombieBoss, value);
    }

    public ICommand ZombieBossCommand => CreateSpawnToggleCommand("ZombieBoss");

    public string ZombieBungee
    {
        get => _zombieBungee;
        set => SetProperty(ref _zombieBungee, value);
    }

    public ICommand ZombieBungeeCommand => CreateSpawnToggleCommand("ZombieBungee");

    public string ZombieCatapult
    {
        get => _zombieCatapult;
        set => SetProperty(ref _zombieCatapult, value);
    }

    public ICommand ZombieCatapultCommand => CreateSpawnToggleCommand("ZombieCatapult");

    public string ZombieDancer
    {
        get => _zombieDancer;
        set => SetProperty(ref _zombieDancer, value);
    }

    public ICommand ZombieDancerCommand => CreateSpawnToggleCommand("ZombieDancer");

    public string ZombieDigger
    {
        get => _zombieDigger;
        set => SetProperty(ref _zombieDigger, value);
    }

    public ICommand ZombieDiggerCommand => CreateSpawnToggleCommand("ZombieDigger");

    public string ZombieDolphinRider
    {
        get => _zombieDolphinRider;
        set => SetProperty(ref _zombieDolphinRider, value);
    }

    public ICommand ZombieDolphinRiderCommand => CreateSpawnToggleCommand("ZombieDolphinRider");

    public string ZombieDoor
    {
        get => _zombieDoor;
        set => SetProperty(ref _zombieDoor, value);
    }

    public ICommand ZombieDoorCommand => CreateSpawnToggleCommand("ZombieDoor");

    public string ZombieDuckyTube
    {
        get => _zombieDuckyTube;
        set => SetProperty(ref _zombieDuckyTube, value);
    }

    public ICommand ZombieDuckyTubeCommand => CreateSpawnToggleCommand("ZombieDuckyTube");

    public string ZombieFlag
    {
        get => _zombieFlag;
        set => SetProperty(ref _zombieFlag, value);
    }

    public ICommand ZombieFlagCommand => CreateSpawnToggleCommand("ZombieFlag");

    public string ZombieFootball
    {
        get => _zombieFootball;
        set => SetProperty(ref _zombieFootball, value);
    }

    public ICommand ZombieFootballCommand => CreateSpawnToggleCommand("ZombieFootball");

    public string ZombieFootballPremium
    {
        get => _zombieFootballPremium;
        set => SetProperty(ref _zombieFootballPremium, value);
    }

    public ICommand ZombieFootballPremiumCommand => CreateSpawnToggleCommand("ZombieFootballPremium");

    public string ZombieGargantuar
    {
        get => _zombieGargantuar;
        set => SetProperty(ref _zombieGargantuar, value);
    }

    public ICommand ZombieGargantuarCommand => CreateSpawnToggleCommand("ZombieGargantuar");

    public string ZombieGatlingHead
    {
        get => _zombieGatlingHead;
        set => SetProperty(ref _zombieGatlingHead, value);
    }

    public ICommand ZombieGatlingHeadCommand => CreateSpawnToggleCommand("ZombieGatlingHead");

    public string ZombieHealthMax
    {
        get => _zombieHealthMax;
        set => SetProperty(ref _zombieHealthMax, value);
    }

    public string ZombieHealthMin
    {
        get => _zombieHealthMin;
        set => SetProperty(ref _zombieHealthMin, value);
    }

    public ICommand ZombieHealthToNextWaveCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "刷新血量", new Dictionary<string, string> { [Constants.Placeholders.Min] = ZombieHealthMin, [Constants.Placeholders.Max] = ZombieHealthMax }));

    public string ZombieImp
    {
        get => _zombieImp;
        set => SetProperty(ref _zombieImp, value);
    }

    public ICommand ZombieImpCommand => CreateSpawnToggleCommand("ZombieImp");

    public string ZombieJackInTheBox
    {
        get => _zombieJackInTheBox;
        set => SetProperty(ref _zombieJackInTheBox, value);
    }

    public ICommand ZombieJackInTheBoxCommand => CreateSpawnToggleCommand("ZombieJackInTheBox");

    public string ZombieJalapenoHead
    {
        get => _zombieJalapenoHead;
        set => SetProperty(ref _zombieJalapenoHead, value);
    }

    public ICommand ZombieJalapenoHeadCommand => CreateSpawnToggleCommand("ZombieJalapenoHead");

    public string ZombieLadder
    {
        get => _zombieLadder;
        set => SetProperty(ref _zombieLadder, value);
    }

    public ICommand ZombieLadderCommand => CreateSpawnToggleCommand("ZombieLadder");

    public string ZombieMonk
    {
        get => _zombieMonk;
        set => SetProperty(ref _zombieMonk, value);
    }

    public ICommand ZombieMonkCommand => CreateSpawnToggleCommand("ZombieMonk");

    public string ZombieNewspaper
    {
        get => _zombieNewspaper;
        set => SetProperty(ref _zombieNewspaper, value);
    }

    public ICommand ZombieNewspaperCommand => CreateSpawnToggleCommand("ZombieNewspaper");

    public string ZombieNinja
    {
        get => _zombieNinja;
        set => SetProperty(ref _zombieNinja, value);
    }

    public ICommand ZombieNinjaCommand => CreateSpawnToggleCommand("ZombieNinja");

    public string ZombieNormal
    {
        get => _zombieNormal;
        set => SetProperty(ref _zombieNormal, value);
    }

    public ICommand ZombieNormalCommand => CreateSpawnToggleCommand("ZombieNormal");

    public string ZombiePail
    {
        get => _zombiePail;
        set => SetProperty(ref _zombiePail, value);
    }

    public ICommand ZombiePailCommand => CreateSpawnToggleCommand("ZombiePail");

    public string ZombiePeaHead
    {
        get => _zombiePeaHead;
        set => SetProperty(ref _zombiePeaHead, value);
    }

    public ICommand ZombiePeaHeadCommand => CreateSpawnToggleCommand("ZombiePeaHead");

    public string ZombiePogo
    {
        get => _zombiePogo;
        set => SetProperty(ref _zombiePogo, value);
    }

    public ICommand ZombiePogoCommand => CreateSpawnToggleCommand("ZombiePogo");

    public string ZombiePolevaulter
    {
        get => _zombiePolevaulter;
        set => SetProperty(ref _zombiePolevaulter, value);
    }

    public ICommand ZombiePolevaulterCommand => CreateSpawnToggleCommand("ZombiePolevaulter");

    public string ZombiePropeller
    {
        get => _zombiePropeller;
        set => SetProperty(ref _zombiePropeller, value);
    }

    public ICommand ZombiePropellerCommand => CreateSpawnToggleCommand("ZombiePropeller");

    public string ZombieRedeyeGargantuar
    {
        get => _zombieRedeyeGargantuar;
        set => SetProperty(ref _zombieRedeyeGargantuar, value);
    }

    public ICommand ZombieRedeyeGargantuarCommand => CreateSpawnToggleCommand("ZombieRedeyeGargantuar");

    public string ZombieRedeyeRobotTitan
    {
        get => _zombieRedeyeRobotTitan;
        set => SetProperty(ref _zombieRedeyeRobotTitan, value);
    }

    public ICommand ZombieRedeyeRobotTitanCommand => CreateSpawnToggleCommand("ZombieRedeyeRobotTitan");

    public string ZombieRobotTitan
    {
        get => _zombieRobotTitan;
        set => SetProperty(ref _zombieRobotTitan, value);
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

    public string ZombieSnorkel
    {
        get => _zombieSnorkel;
        set => SetProperty(ref _zombieSnorkel, value);
    }

    public ICommand ZombieSnorkelCommand => CreateSpawnToggleCommand("ZombieSnorkel");

    public string ZombieSquashHead
    {
        get => _zombieSquashHead;
        set => SetProperty(ref _zombieSquashHead, value);
    }

    public ICommand ZombieSquashHeadCommand => CreateSpawnToggleCommand("ZombieSquashHead");

    public string ZombieTalisman
    {
        get => _zombieTalisman;
        set => SetProperty(ref _zombieTalisman, value);
    }

    public ICommand ZombieTalismanCommand => CreateSpawnToggleCommand("ZombieTalisman");

    public string ZombieTallnutHead
    {
        get => _zombieTallnutHead;
        set => SetProperty(ref _zombieTallnutHead, value);
    }

    public ICommand ZombieTallnutHeadCommand => CreateSpawnToggleCommand("ZombieTallnutHead");

    public string ZombieTrafficCone
    {
        get => _zombieTrafficCone;
        set => SetProperty(ref _zombieTrafficCone, value);
    }

    public ICommand ZombieTrafficConeCommand => CreateSpawnToggleCommand("ZombieTrafficCone");

    public string ZombieWallnutHead
    {
        get => _zombieWallnutHead;
        set => SetProperty(ref _zombieWallnutHead, value);
    }

    public ICommand ZombieWallnutHeadCommand => CreateSpawnToggleCommand("ZombieWallnutHead");

    public string ZombieYeti
    {
        get => _zombieYeti;
        set => SetProperty(ref _zombieYeti, value);
    }

    public ICommand ZombieYetiCommand => CreateSpawnToggleCommand("ZombieYeti");

    public string ZombieZamboni
    {
        get => _zombieZamboni;
        set => SetProperty(ref _zombieZamboni, value);
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

    private void OnButtonStatusUpdated(Dictionary<string, bool> statusDict)
    {
        UpdatePropertiesFromDict(statusDict, _buttonMapping);
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
            string propName = "Zombie" + zombieType;

            var prop = GetType().GetProperty(propName);
            if(prop != null && prop.PropertyType == typeof(string))
            {
                _uiThread.Invoke(() => prop.SetValue(this, newSymbol));
            }
        }
    }
}
