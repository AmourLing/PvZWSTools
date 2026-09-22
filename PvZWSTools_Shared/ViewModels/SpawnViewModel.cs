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
        ["MAXPOINT_CHECK"] = nameof(MaxPoint),
    };

    private readonly string _defaultPath;
    private readonly IDialogService _dialogService;
    private readonly IMessageProcessor _messageProcessor;
    private readonly IScriptExecutionService _scriptExec;
    private readonly IUiThreadInvoker _uiThread;
    private string _bungeeCheck = Constants.c_Symbol_Off;
    private string _jsonEditZombiesInWave = Constants.c_Symbol_Off;
    private string _maxPoint = Constants.c_Symbol_Off;
    private string _redeyeCheck = Constants.c_Symbol_Off;
    private string _stopSpawn = Constants.c_Symbol_Off;
    private string _syncSpawnList = Constants.c_Symbol_Off;
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

    private string _nextWave = Constants.c_Symbol_Off;
    private string _nextWave_Name = "下一波";
    private bool _isEditingWaveJson;

    public SpawnViewModel(IScriptExecutionService scriptExec, string defaultPath, IMessageProcessor messageProcessor, IUiThreadInvoker uiThread, IDialogService dialogService)
    {
        _scriptExec = scriptExec;
        _defaultPath = defaultPath;
        _messageProcessor = messageProcessor;
        _uiThread = uiThread;
        _dialogService = dialogService;
        if(_messageProcessor != null)
        {
            _messageProcessor.ButtonStatusUpdated += OnButtonStatusUpdated;
            _messageProcessor.OutputReceived += OnGameOutput;
        }
    }

    public string BungeeCheck
    {
        get => _bungeeCheck;
        set => SetProperty(ref _bungeeCheck, value);
    }

    public ICommand BungeeHandleCommand => new RelayCommand(async _ =>
        {
            var __old = BungeeCheck;
            BungeeCheck = ButtonHelper.ToggleCheck(BungeeCheck);
            if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "蹦极红眼处理",
                new Dictionary<string, string>
                {
                    [Constants.Placeholders.BungeeCheck] = ButtonHelper.GetCheckValue(BungeeCheck),
                    [Constants.Placeholders.RedeyeCheck] = ButtonHelper.GetCheckValue(RedeyeCheck)
                }))
                BungeeCheck = __old;
        });

    /// <summary>经典页那一下"点一次问一次"的拉取，留着不动；新界面改用了下面的开关。</summary>
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

    /// <summary>同步出怪列表：开=让游戏在换关/初始化/读档时主动把 mZombieAllowed 推过来，
    /// 不再是宿主点一次问一次。见 控件/出怪/同步出怪列表.py。</summary>
    public string SyncSpawnList
    {
        get => _syncSpawnList;
        set => SetProperty(ref _syncSpawnList, value);
    }

    public ICommand SyncSpawnListCommand => CreateToggleCommand(() => SyncSpawnList, v => SyncSpawnList = v, "同步出怪列表");

    public ICommand JsonEditCommand => new RelayCommand(_ => { JsonEditZombiesInWave = ButtonHelper.ToggleCheck(JsonEditZombiesInWave); });

    public string JsonEditZombiesInWave
    {
        get => _jsonEditZombiesInWave;
        set => SetProperty(ref _jsonEditZombiesInWave, value);
    }

    public ICommand JsonEditZombiesInWaveCommand => new RelayCommand(_ => { JsonEditZombiesInWave = ButtonHelper.ToggleCheck(JsonEditZombiesInWave); });

    public ICommand LimitTestCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "极限出怪测试"));

    public ICommand LoadJsonZombiesInWaveCommand => new RelayCommand(async _ =>
        {
            string wavePath = GetSpawnWaveFilePath();
            string? waveBase64 = await ScriptPayload.ReadFileAsBase64Async(wavePath);
            if(string.IsNullOrEmpty(waveBase64))
            {
                Log.Error($"波次出怪文件不存在：{wavePath}，请先执行「波次出怪(数量)」导出。");
                return;
            }

            await SendWaveJsonAsync(waveBase64);
        });

    public string MaxPoint
    {
        get => _maxPoint;
        set => SetProperty(ref _maxPoint, value);
    }

    public ICommand MaxPointCommand => CreateToggleCommand(() => MaxPoint, v => MaxPoint = v, "最大密度");

    public ICommand PrintZombieSpawnCommand => new RelayCommand(async _ => await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "打印场上僵尸"));

    public string RedeyeCheck
    {
        get => _redeyeCheck;
        set => SetProperty(ref _redeyeCheck, value);
    }

    public ICommand RedeyeHandleCommand => new RelayCommand(async _ =>
        {
            var __old = RedeyeCheck;
            RedeyeCheck = ButtonHelper.ToggleCheck(RedeyeCheck);
            if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "蹦极红眼处理",
                new Dictionary<string, string>
                {
                    [Constants.Placeholders.BungeeCheck] = ButtonHelper.GetCheckValue(BungeeCheck),
                    [Constants.Placeholders.RedeyeCheck] = ButtonHelper.GetCheckValue(RedeyeCheck)
                }))
                RedeyeCheck = __old;
        });
    public string NextWave
    {
        get => _nextWave;
        set => SetProperty(ref _nextWave, value);
    }

    public ICommand NextWaveCommand => CreateToggleCommand(() => NextWave, v => NextWave = v, "下一波");
    public string StopSpawn
    {
        get => _stopSpawn;
        set => SetProperty(ref _stopSpawn, value);
    }

    public ICommand StopSpawnCommand => CreateToggleCommand(() => StopSpawn, v => StopSpawn = v, "暂停出怪");

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

    /// <summary>波次出怪 JSON 的存放目录：配置文件/出怪/。</summary>
    private string GetSpawnWaveDir() =>
        System.IO.Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_SpawnWave);

    private string GetSpawnWaveFilePath() =>
        System.IO.Path.Combine(GetSpawnWaveDir(), Constants.JsonWaveFile);

    /// <summary>僵尸名称映射表（配置文件/选项/僵尸.json）的 Base64，内联给脚本用。</summary>
    private async Task<string> ReadZombieNameMapBase64Async() =>
        await ScriptPayload.ReadFileAsBase64Async(
            System.IO.Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_Options, Constants.JsonZombieFile))
            ?? string.Empty;

    public ICommand ZombiesInWaveCountCommand => new RelayCommand(async _ =>
        {
            string checkValue = ButtonHelper.GetCheckValue(JsonEditZombiesInWave);
            var placeholders = new Dictionary<string, string>
            {
                [Constants.Placeholders.ZombieJsonBase64] = await ReadZombieNameMapBase64Async(),
                [Constants.Placeholders.Check] = checkValue
            };

            if(checkValue != Constants.c_Value_Checked)
            {
                await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "波次出怪_数量", placeholders);
                return;
            }

            try
            {
                string output = await _scriptExec.ExecuteWithResultAsync(Constants.SubFolders.Spawn, "波次出怪_数量", placeholders);
                string? waveBase64 = ScriptPayload.ExtractBase64(output, Constants.Markers.WaveJsonStart, Constants.Markers.WaveJsonEnd);
                if(string.IsNullOrEmpty(waveBase64))
                {
                    Log.Error($"未能从脚本输出中提取波次出怪数据，请确认当前在关卡内且已开启 json 编辑。输出：{output}");
                    return;
                }

                string savedPath = await ScriptPayload.WriteBase64ToAsync(GetSpawnWaveDir(), Constants.JsonWaveFile, waveBase64);
                Log.Info($"波次出怪数据已保存到 {savedPath}");
                OpenWithExternalEditor(savedPath);
            }
            catch(Exception ex)
            {
                Log.Error($"导出波次出怪失败：{ex}");
            }
        });

    /// <summary>波次出怪(数量)：导出当前关卡波次表 → 应用内编辑 → 确认即写回游戏。
    /// 一个功能一次点完，不再拆成"先勾 json 编辑、再点载入"三步。</summary>
    public ICommand ZombiesInWaveEditCommand => new RelayCommand(async _ =>
        {
            if(_isEditingWaveJson) return;
            _isEditingWaveJson = true;

            try
            {
                var placeholders = new Dictionary<string, string>
                {
                    [Constants.Placeholders.ZombieJsonBase64] = await ReadZombieNameMapBase64Async(),
                    [Constants.Placeholders.Check] = Constants.c_Value_Checked
                };

                string output = await _scriptExec.ExecuteWithResultAsync(Constants.SubFolders.Spawn, "波次出怪_数量", placeholders);
                string? waveBase64 = ScriptPayload.ExtractBase64(output, Constants.Markers.WaveJsonStart, Constants.Markers.WaveJsonEnd);
                if(string.IsNullOrEmpty(waveBase64))
                {
                    // 分两种情况报：完全没回传（没连上/超时/不在关卡），和回了但没带数据。
                    Log.Error(string.IsNullOrEmpty(output)
                        ? "没收到波次出怪回传，请确认已连接游戏且当前在关卡内。"
                        : $"回传里没有波次出怪数据（缺 WAVE_JSON 标记）。输出：{output}");
                    return;
                }

                // 先把原样导出的表落盘，编辑中途退出也留得住关卡现状。
                _ = await ScriptPayload.WriteBase64ToAsync(GetSpawnWaveDir(), Constants.JsonWaveFile, waveBase64);

                var editor = new WaveJsonEditorViewModel(ScriptPayload.DecodeUtf8(waveBase64));
                if(!await _dialogService.ShowDialogAsync(editor)) return;

                string editedBase64 = ScriptPayload.EncodeUtf8(editor.Json);
                _ = await ScriptPayload.WriteBase64ToAsync(GetSpawnWaveDir(), Constants.JsonWaveFile, editedBase64);
                if(editor.SaveOnly)
                    Log.Info("波次表只保存、未载入游戏。进关后点「载入已存波次表」可以灌回去。");
                else
                    await SendWaveJsonAsync(editedBase64);
            }
            catch(Exception ex)
            {
                Log.Error($"波次出怪编辑失败：{ex}");
            }
            finally
            {
                _isEditingWaveJson = false;
            }
        });

    private Task SendWaveJsonAsync(string waveBase64) =>
        _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, "载入json",
            new Dictionary<string, string> { [Constants.Placeholders.WaveJsonBase64] = waveBase64 });

    /// <summary>经典页那条"勾了 json 编辑再点"的老路子：Windows 交给系统关联程序打开，Android 没有对应程序，只落盘。</summary>
    private static void OpenWithExternalEditor(string path)
    {
#if ANDROID
        _ = path;
#else
        try
        {
            _ = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch(Exception ex)
        {
            Log.Warning($"已保存到 {path}，但无法自动打开：{ex.Message}");
        }
#endif
    }

    /// <summary>波次出怪(序号)：让脚本把清单用 Base64 包一层再回传（CHECK=2），
    /// 宿主解出来打进日志/控制台。明文回传要走标准输出那条通道，
    /// 中文在那之前就可能变成 U+FFFD；包一层两端都稳，桌面和手机读到的完全一样。</summary>
    public ICommand ZombiesInWaveIndexCommand => new RelayCommand(async _ =>
        {
            var placeholders = new Dictionary<string, string>
            {
                [Constants.Placeholders.ZombieJsonBase64] = await ReadZombieNameMapBase64Async(),
                [Constants.Placeholders.Check] = Constants.c_Value_Base64Text
            };

            string output = await _scriptExec.ExecuteWithResultAsync(Constants.SubFolders.Spawn, "波次出怪_序号", placeholders);
            string? listing = ScriptPayload.ExtractBase64(output, Constants.Markers.WaveListStart, Constants.Markers.WaveListEnd);
            if(string.IsNullOrEmpty(listing))
            {
                Log.Error(string.IsNullOrEmpty(output)
                    ? "没收到波次清单回传，请确认已连接游戏且当前在关卡内。"
                    : $"回传里没有 WAVELIST_B64 载荷。输出：{output}");
                return;
            }

            Log.Info("波次出怪（序号）：\n" + ScriptPayload.DecodeUtf8(listing));
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

    /// <summary>开关：翻转状态 → 带着新状态发脚本；发不出去就翻回去，
    /// 否则界面上写着"开"、游戏里根本没装钩子。</summary>
    private ICommand CreateToggleCommand(Func<string> stateGetter, Action<string> stateSetter, string scriptName)
    {
        return new RelayCommand(async _ =>
        {
            var current = stateGetter();
            var newState = ButtonHelper.ToggleCheck(current);
            stateSetter(newState);
            if(!await _scriptExec.ExecuteAsync(Constants.SubFolders.Spawn, scriptName,
                new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(newState) }))
                stateSetter(current);
        });
    }

    private void OnButtonStatusUpdated(Dictionary<string, bool> statusDict)
    {
        UpdatePropertiesFromDict(statusDict, _buttonMapping);
    }

    /// <summary>游戏侧「同步出怪列表」的钩子主动推来的发布，不等宿主提问。
    /// 只认自家那对标记：其它脚本也往 stdout 打 "X =&gt; True"（GetButtonCheck 等），
    /// 不加这道闸就会把别人的输出当出怪列表吃进来。</summary>
    private void OnGameOutput(string msg)
    {
        if(SyncSpawnList != Constants.c_Symbol_On) return;

        int start = msg.IndexOf(Constants.Markers.SpawnListStart, StringComparison.Ordinal);
        int end = msg.IndexOf(Constants.Markers.SpawnListEnd, StringComparison.Ordinal);
        if(start < 0 || end <= start) return;

        UpdateZombieStatesFromOutput(msg.Substring(start, end - start));
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
