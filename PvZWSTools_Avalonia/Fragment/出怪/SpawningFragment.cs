using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json.Linq;
using PvZWSTools_Avalonia.Helpers;
using Constants = PvZWSTools_Shared.Helpers.Constants;
using ScriptPayload = PvZWSTools_Shared.Helpers.ScriptPayload;

namespace PvZWSTools_Avalonia;

public class SpawningFragment:BaseFragment
{
    private static readonly string mSpawningPath = "出怪";
    private bool _isExportingWaveJson;
    private bool _isReadingWaveList;

    /// <summary>脚本回传等待上限：波次数据要在游戏线程里逐格扫描，留足余量。</summary>
    private const int ScriptOutputTimeoutMs = 30000;

    public override void RefreshAllButtons()
    {
    }

    protected override string FragmentPath => mSpawningPath;

    protected override Dictionary<int, string> OptionFileMappings => new Dictionary<int, string>
    {
        // 开关1
        [Resource.String.spawning_strings_1_1_key] = "开关1",
        [Resource.String.spawning_strings_1_2_key] = "开关1",
        [Resource.String.spawning_strings_4_1_key] = "开关1",
        [Resource.String.spawning_strings_5_1_key] = "开关1",
        [Resource.String.spawning_strings_9_1_key] = "开关1",
        // 开关2（共40个）
        [Resource.String.spawning_strings_2_1_key] = "开关2",
        [Resource.String.spawning_strings_2_2_key] = "开关2",
        [Resource.String.spawning_strings_2_3_key] = "开关2",
        [Resource.String.spawning_strings_2_4_key] = "开关2",
        [Resource.String.spawning_strings_2_5_key] = "开关2",
        [Resource.String.spawning_strings_2_6_key] = "开关2",
        [Resource.String.spawning_strings_2_7_key] = "开关2",
        [Resource.String.spawning_strings_2_8_key] = "开关2",
        [Resource.String.spawning_strings_2_9_key] = "开关2",
        [Resource.String.spawning_strings_2_10_key] = "开关2",
        [Resource.String.spawning_strings_2_11_key] = "开关2",
        [Resource.String.spawning_strings_2_12_key] = "开关2",
        [Resource.String.spawning_strings_2_13_key] = "开关2",
        [Resource.String.spawning_strings_2_14_key] = "开关2",
        [Resource.String.spawning_strings_2_15_key] = "开关2",
        [Resource.String.spawning_strings_2_16_key] = "开关2",
        [Resource.String.spawning_strings_2_17_key] = "开关2",
        [Resource.String.spawning_strings_2_18_key] = "开关2",
        [Resource.String.spawning_strings_2_19_key] = "开关2",
        [Resource.String.spawning_strings_2_20_key] = "开关2",
        [Resource.String.spawning_strings_2_21_key] = "开关2",
        [Resource.String.spawning_strings_2_22_key] = "开关2",
        [Resource.String.spawning_strings_2_23_key] = "开关2",
        [Resource.String.spawning_strings_2_24_key] = "开关2",
        [Resource.String.spawning_strings_2_25_key] = "开关2",
        [Resource.String.spawning_strings_2_26_key] = "开关2",
        [Resource.String.spawning_strings_2_27_key] = "开关2",
        [Resource.String.spawning_strings_2_28_key] = "开关2",
        [Resource.String.spawning_strings_2_29_key] = "开关2",
        [Resource.String.spawning_strings_2_30_key] = "开关2",
        [Resource.String.spawning_strings_2_31_key] = "开关2",
        [Resource.String.spawning_strings_2_32_key] = "开关2",
        [Resource.String.spawning_strings_2_33_key] = "开关2",
        [Resource.String.spawning_strings_2_34_key] = "开关2",
        [Resource.String.spawning_strings_2_35_key] = "开关2",
        [Resource.String.spawning_strings_2_36_key] = "开关2",
        [Resource.String.spawning_strings_2_37_key] = "开关2",
        [Resource.String.spawning_strings_2_38_key] = "开关2",
        [Resource.String.spawning_strings_2_39_key] = "开关2",
        [Resource.String.spawning_strings_2_40_key] = "开关2"
    };

    protected override void InitializeMap()
    {
        Map[GetString(Resource.String.spawning_strings_1_1_key)] = GetString(Resource.String.spawning_strings_1_1_value);
        Map[GetString(Resource.String.spawning_strings_1_2_key)] = GetString(Resource.String.spawning_strings_1_2_value);

        Map[GetString(Resource.String.spawning_strings_2_1_key)] = GetString(Resource.String.spawning_strings_2_1_value);
        Map[GetString(Resource.String.spawning_strings_2_2_key)] = GetString(Resource.String.spawning_strings_2_2_value);
        Map[GetString(Resource.String.spawning_strings_2_3_key)] = GetString(Resource.String.spawning_strings_2_3_value);
        Map[GetString(Resource.String.spawning_strings_2_4_key)] = GetString(Resource.String.spawning_strings_2_4_value);
        Map[GetString(Resource.String.spawning_strings_2_5_key)] = GetString(Resource.String.spawning_strings_2_5_value);
        Map[GetString(Resource.String.spawning_strings_2_6_key)] = GetString(Resource.String.spawning_strings_2_6_value);
        Map[GetString(Resource.String.spawning_strings_2_7_key)] = GetString(Resource.String.spawning_strings_2_7_value);
        Map[GetString(Resource.String.spawning_strings_2_8_key)] = GetString(Resource.String.spawning_strings_2_8_value);
        Map[GetString(Resource.String.spawning_strings_2_9_key)] = GetString(Resource.String.spawning_strings_2_9_value);
        Map[GetString(Resource.String.spawning_strings_2_10_key)] = GetString(Resource.String.spawning_strings_2_10_value);
        Map[GetString(Resource.String.spawning_strings_2_11_key)] = GetString(Resource.String.spawning_strings_2_11_value);
        Map[GetString(Resource.String.spawning_strings_2_12_key)] = GetString(Resource.String.spawning_strings_2_12_value);
        Map[GetString(Resource.String.spawning_strings_2_13_key)] = GetString(Resource.String.spawning_strings_2_13_value);
        Map[GetString(Resource.String.spawning_strings_2_14_key)] = GetString(Resource.String.spawning_strings_2_14_value);
        Map[GetString(Resource.String.spawning_strings_2_15_key)] = GetString(Resource.String.spawning_strings_2_15_value);
        Map[GetString(Resource.String.spawning_strings_2_16_key)] = GetString(Resource.String.spawning_strings_2_16_value);
        Map[GetString(Resource.String.spawning_strings_2_17_key)] = GetString(Resource.String.spawning_strings_2_17_value);
        Map[GetString(Resource.String.spawning_strings_2_18_key)] = GetString(Resource.String.spawning_strings_2_18_value);
        Map[GetString(Resource.String.spawning_strings_2_19_key)] = GetString(Resource.String.spawning_strings_2_19_value);
        Map[GetString(Resource.String.spawning_strings_2_20_key)] = GetString(Resource.String.spawning_strings_2_20_value);
        Map[GetString(Resource.String.spawning_strings_2_21_key)] = GetString(Resource.String.spawning_strings_2_21_value);
        Map[GetString(Resource.String.spawning_strings_2_22_key)] = GetString(Resource.String.spawning_strings_2_22_value);
        Map[GetString(Resource.String.spawning_strings_2_23_key)] = GetString(Resource.String.spawning_strings_2_23_value);
        Map[GetString(Resource.String.spawning_strings_2_24_key)] = GetString(Resource.String.spawning_strings_2_24_value);
        Map[GetString(Resource.String.spawning_strings_2_25_key)] = GetString(Resource.String.spawning_strings_2_25_value);
        Map[GetString(Resource.String.spawning_strings_2_26_key)] = GetString(Resource.String.spawning_strings_2_26_value);
        Map[GetString(Resource.String.spawning_strings_2_27_key)] = GetString(Resource.String.spawning_strings_2_27_value);
        Map[GetString(Resource.String.spawning_strings_2_28_key)] = GetString(Resource.String.spawning_strings_2_28_value);
        Map[GetString(Resource.String.spawning_strings_2_29_key)] = GetString(Resource.String.spawning_strings_2_29_value);
        Map[GetString(Resource.String.spawning_strings_2_30_key)] = GetString(Resource.String.spawning_strings_2_30_value);
        Map[GetString(Resource.String.spawning_strings_2_31_key)] = GetString(Resource.String.spawning_strings_2_31_value);
        Map[GetString(Resource.String.spawning_strings_2_32_key)] = GetString(Resource.String.spawning_strings_2_32_value);
        Map[GetString(Resource.String.spawning_strings_2_33_key)] = GetString(Resource.String.spawning_strings_2_33_value);
        Map[GetString(Resource.String.spawning_strings_2_34_key)] = GetString(Resource.String.spawning_strings_2_34_value);
        Map[GetString(Resource.String.spawning_strings_2_35_key)] = GetString(Resource.String.spawning_strings_2_35_value);
        Map[GetString(Resource.String.spawning_strings_2_36_key)] = GetString(Resource.String.spawning_strings_2_36_value);
        Map[GetString(Resource.String.spawning_strings_2_37_key)] = GetString(Resource.String.spawning_strings_2_37_value);
        Map[GetString(Resource.String.spawning_strings_2_38_key)] = GetString(Resource.String.spawning_strings_2_38_value);
        Map[GetString(Resource.String.spawning_strings_2_39_key)] = GetString(Resource.String.spawning_strings_2_39_value);
        Map[GetString(Resource.String.spawning_strings_2_40_key)] = GetString(Resource.String.spawning_strings_2_40_value);

        Map[GetString(Resource.String.spawning_strings_4_1_key)] = GetString(Resource.String.spawning_strings_4_1_value);
        Map[GetString(Resource.String.spawning_strings_5_1_key)] = GetString(Resource.String.spawning_strings_5_1_value);
        Map[GetString(Resource.String.spawning_strings_8_1_key)] = GetString(Resource.String.spawning_strings_8_1_value);
        Map[GetString(Resource.String.spawning_strings_8_2_key)] = GetString(Resource.String.spawning_strings_8_2_value);
        Map[GetString(Resource.String.spawning_strings_9_1_key)] = GetString(Resource.String.spawning_strings_9_1_value);

    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.spawning_fragment, container, false);

        // Button 1
        view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.spawning_strings_1_1_key);
            string key2 = GetString(Resource.String.spawning_strings_1_2_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.spawning_strings_1),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.spawning_strings_1),
                new Dictionary<string, string> { ["{BUNGEE_CHECK}"] = "0", ["{REDEYE_CHECK}"] = "1" },
                Map,
                BuildDropdownOptions(key1, key2)
            );
        };

        // Button 2
        view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
        {
            // 收集所有 40 个键
            var keys = new[]
            {
                Resource.String.spawning_strings_2_1_key,
                Resource.String.spawning_strings_2_2_key,
                Resource.String.spawning_strings_2_3_key,
                Resource.String.spawning_strings_2_4_key,
                Resource.String.spawning_strings_2_5_key,
                Resource.String.spawning_strings_2_6_key,
                Resource.String.spawning_strings_2_7_key,
                Resource.String.spawning_strings_2_8_key,
                Resource.String.spawning_strings_2_9_key,
                Resource.String.spawning_strings_2_10_key,
                Resource.String.spawning_strings_2_11_key,
                Resource.String.spawning_strings_2_12_key,
                Resource.String.spawning_strings_2_13_key,
                Resource.String.spawning_strings_2_14_key,
                Resource.String.spawning_strings_2_15_key,
                Resource.String.spawning_strings_2_16_key,
                Resource.String.spawning_strings_2_17_key,
                Resource.String.spawning_strings_2_18_key,
                Resource.String.spawning_strings_2_19_key,
                Resource.String.spawning_strings_2_20_key,
                Resource.String.spawning_strings_2_21_key,
                Resource.String.spawning_strings_2_22_key,
                Resource.String.spawning_strings_2_23_key,
                Resource.String.spawning_strings_2_24_key,
                Resource.String.spawning_strings_2_25_key,
                Resource.String.spawning_strings_2_26_key,
                Resource.String.spawning_strings_2_27_key,
                Resource.String.spawning_strings_2_28_key,
                Resource.String.spawning_strings_2_29_key,
                Resource.String.spawning_strings_2_30_key,
                Resource.String.spawning_strings_2_31_key,
                Resource.String.spawning_strings_2_32_key,
                Resource.String.spawning_strings_2_33_key,
                Resource.String.spawning_strings_2_34_key,
                Resource.String.spawning_strings_2_35_key,
                Resource.String.spawning_strings_2_36_key,
                Resource.String.spawning_strings_2_37_key,
                Resource.String.spawning_strings_2_38_key,
                Resource.String.spawning_strings_2_39_key,
                Resource.String.spawning_strings_2_40_key
            };
            var keyStrings = new List<string>();
            foreach(var resId in keys)
                keyStrings.Add(GetString(resId));

            var defaultValues = new Dictionary<string, string>
            {
                ["{SPAWN_ZOMBIENORMAL_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEFLAG_CHECK}"] = "2",
                ["{SPAWN_ZOMBIETRAFFICCONE_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEPOLEVAULTER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEPAIL_CHECK}"] = "2",
                ["{SPAWN_ZOMBIENEWSPAPER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEDOOR_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEFOOTBALL_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEDANCER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEBACKUPDANCER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEDUCKYTUBE_CHECK}"] = "2",
                ["{SPAWN_ZOMBIESNORKEL_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEZAMBONI_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEBOBSLED_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEDOLPHINRIDER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEJACKINTHEBOX_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEBALLOON_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEDIGGER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEPOGO_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEYETI_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEBUNGEE_CHECK}"] = "2",
                ["{SPAWN_ZOMBIELADDER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIECATAPULT_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEGARGANTUAR_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEIMP_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEBOSS_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEPEAHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEWALLNUTHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEJALAPENOHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEGATLINGHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIESQUASHHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIETALLNUTHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEREDEYEGARGANTUAR_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEROBOTTITAN_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEREDEYEROBOTTITAN_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEMONK_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEFOOTBALLPREMIUM_CHECK}"] = "2",
                ["{SPAWN_ZOMBIENINJA_CHECK}"] = "2",
                ["{SPAWN_ZOMBIETALISMAN_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEPROPELLER_CHECK}"] = "2"
            };

            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.spawning_strings_2),
                BuildInitialData(keyStrings.ToArray()),
                FragmentPath,
                GetString(Resource.String.spawning_strings_2),
                defaultValues,
                Map,
                BuildDropdownOptions(keyStrings.ToArray())
            );
        };

        // Button 3: 极限出怪测试 (无参数)
        view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.spawning_strings_3),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.spawning_strings_3),
                new Dictionary<string, string>()
            );

        // Button 4: 最大密度
        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
        {
            string key = GetString(Resource.String.spawning_strings_4_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.spawning_strings_4),
                BuildInitialData(key),
                FragmentPath,
                GetString(Resource.String.spawning_strings_4),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key)
            );
        };

        // Button 5: 暂停出怪
        view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
        {
            string key = GetString(Resource.String.spawning_strings_5_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.spawning_strings_5),
                BuildInitialData(key),
                FragmentPath,
                GetString(Resource.String.spawning_strings_5),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key)
            );
        };

        // Button 6: 波次出怪_数量 —— 导出后在应用内编辑（安卓没有系统关联程序可打开 JSON）
        view.FindViewById<Button>(Resource.Id.button6).Click += async (sender, e) =>
            await ExportWaveJsonAsync();

        // Button 7: 载入json
        view.FindViewById<Button>(Resource.Id.button7).Click += async (sender, e) =>
            await LoadWaveJsonAsync();

        // Button 10（显示在 button6 之后）: 波次出怪_序号
        view.FindViewById<Button>(Resource.Id.button10).Click += async (sender, e) =>
            await ShowWaveListingAsync();

        // Button 8: 刷新血量 (浮点数范围，使用 OptAndDone)
        view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.spawning_strings_8_1_key);
            string key2 = GetString(Resource.String.spawning_strings_8_2_key);
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.spawning_strings_8),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.spawning_strings_8),
                new Dictionary<string, string> { ["{MIN}"] = "0", ["{MAX}"] = "1" }
            );
        };
        // Button9: 
        view.FindViewById<Button>(Resource.Id.button9).Click += (sender, e) =>
        {
            string key = GetString(Resource.String.spawning_strings_9_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.spawning_strings_9),
                BuildInitialData(key),
                FragmentPath,
                GetString(Resource.String.spawning_strings_9),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key)
            );
        };

        return view;
    }

    // ====================================================================
    // 波次出怪 JSON
    // 游戏内脚本只负责回传数据，落盘与读取全由本应用完成（Android 无目录写权限）
    // ====================================================================
    private string ConfigPath => Path.Combine(MainActivity.AppFilesPath, Constants.Folder_Need);

    private string SpawnWaveDir => Path.Combine(ConfigPath, Constants.Folder_SpawnWave);

    private string SpawnWaveFilePath => Path.Combine(SpawnWaveDir, Constants.JsonWaveFile);

    private string GetScriptPath(string scriptName) =>
        Path.Combine(ConfigPath, Constants.Folder_Buttons, mSpawningPath, scriptName + ".py");

    private async Task<string> ReadZombieNameMapBase64Async() =>
        await ScriptPayload.ReadFileAsBase64Async(
            Path.Combine(ConfigPath, Constants.Folder_Options, Constants.JsonZombieFile)) ?? string.Empty;

    private async Task ExportWaveJsonAsync()
    {
        if(_isExportingWaveJson) return;

        var ws = MainActivity.ws;
        if(ws == null || !ws.IsConnected)
        {
            Toast.MakeText(Activity, "ws未连接", ToastLength.Long).Show();
            return;
        }

        string scriptPath = GetScriptPath(GetString(Resource.String.spawning_strings_6));
        if(!File.Exists(scriptPath))
        {
            Toast.MakeText(Activity, "波次出怪_数量脚本不存在", ToastLength.Long).Show();
            return;
        }

        _isExportingWaveJson = true;
        try
        {
            // 安卓没有输出面板，文本列举看不到，所以固定走 JSON 分支
            string script = (await File.ReadAllTextAsync(scriptPath))
                .Replace(Constants.Placeholders.ZombieJsonBase64, await ReadZombieNameMapBase64Async())
                .Replace(Constants.Placeholders.Check, Constants.c_Value_Checked);

            string output = await CollectOutputAsync(ws, script, Constants.Markers.WaveJsonEnd);
            string base64 = ScriptPayload.ExtractBase64(output,
                Constants.Markers.WaveJsonStart, Constants.Markers.WaveJsonEnd);
            if(string.IsNullOrEmpty(base64))
            {
                Log.Error($"未取到波次出怪数据，脚本输出：{output}");
                Toast.MakeText(Activity, "未取到波次出怪数据，请确认当前在关卡内", ToastLength.Long).Show();
                return;
            }

            string json = ScriptPayload.DecodeUtf8(base64);
            await SaveWaveJsonAsync(json);
            ShowWaveJsonEditor(json);
        }
        catch(Exception ex)
        {
            Log.Error("导出波次出怪失败", ex);
            Toast.MakeText(Activity, $"导出失败: {ex.Message}", ToastLength.Long).Show();
        }
        finally
        {
            _isExportingWaveJson = false;
        }
    }

    private async Task LoadWaveJsonAsync()
    {
        var ws = MainActivity.ws;
        if(ws == null || !ws.IsConnected)
        {
            Toast.MakeText(Activity, "ws未连接", ToastLength.Long).Show();
            return;
        }

        string base64 = await ScriptPayload.ReadFileAsBase64Async(SpawnWaveFilePath);
        if(string.IsNullOrEmpty(base64))
        {
            Toast.MakeText(Activity, "还没有出怪数据，请先执行波次出怪_数量", ToastLength.Long).Show();
            return;
        }

        string scriptPath = GetScriptPath(GetString(Resource.String.spawning_strings_7));
        if(!File.Exists(scriptPath))
        {
            Toast.MakeText(Activity, "载入json脚本不存在", ToastLength.Long).Show();
            return;
        }

        try
        {
            string script = (await File.ReadAllTextAsync(scriptPath))
                .Replace(Constants.Placeholders.WaveJsonBase64, base64);
            ws.Send(script);
        }
        catch(Exception ex)
        {
            Log.Error("载入波次出怪失败", ex);
            Toast.MakeText(Activity, $"载入失败: {ex.Message}", ToastLength.Long).Show();
        }
    }

    private async Task ShowWaveListingAsync()
    {
        if(_isReadingWaveList) return;

        var ws = MainActivity.ws;
        if(ws == null || !ws.IsConnected)
        {
            Toast.MakeText(Activity, "ws未连接", ToastLength.Long).Show();
            return;
        }

        string scriptName = GetString(Resource.String.spawning_strings_10);
        string scriptPath = GetScriptPath(scriptName);
        if(!File.Exists(scriptPath))
        {
            Toast.MakeText(Activity, scriptName + "脚本不存在", ToastLength.Long).Show();
            return;
        }

        _isReadingWaveList = true;
        try
        {
            // CHECK=2：要 Base64 包裹的文本载荷。print 直接带中文会被换成 U+FFFD，弹窗才需要这一层
            string script = (await File.ReadAllTextAsync(scriptPath))
                .Replace(Constants.Placeholders.ZombieJsonBase64, await ReadZombieNameMapBase64Async())
                .Replace(Constants.Placeholders.Check, Constants.c_Value_Base64Text);

            string output = await CollectOutputAsync(ws, script, Constants.Markers.WaveListEnd);
            string listingB64 = ScriptPayload.ExtractBase64(output,
                Constants.Markers.WaveListStart, Constants.Markers.WaveListEnd);
            if(string.IsNullOrEmpty(listingB64))
            {
                Log.Error($"未取到{scriptName}数据，脚本输出：{output}");
                Toast.MakeText(Activity, "未取到波次数据，请确认当前在关卡内", ToastLength.Long).Show();
                return;
            }

            ShowTextDialog(scriptName, ScriptPayload.DecodeUtf8(listingB64));
        }
        catch(Exception ex)
        {
            Log.Error($"读取{scriptName}失败", ex);
            Toast.MakeText(Activity, $"读取失败: {ex.Message}", ToastLength.Long).Show();
        }
        finally
        {
            _isReadingWaveList = false;
        }
    }

    /// <summary>只读文本弹窗：安卓没有 WPF 那样的输出面板，脚本 print 的内容靠它露出来。</summary>
    private void ShowTextDialog(string title, string text)
    {
        int pad = (int)(16 * Resources.DisplayMetrics.Density);
        var content = new TextView(Activity)
        {
            Text = string.IsNullOrEmpty(text) ? "（无输出）" : text,
            Typeface = Android.Graphics.Typeface.Monospace
        };
        content.SetPadding(pad, pad, pad, pad);

        var scroll = new ScrollView(Activity)
        {
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };
        scroll.AddView(content);

        _ = new Android.App.AlertDialog.Builder(Activity)
            .SetTitle(title)
            .SetView(scroll)
            .SetPositiveButton("关闭", (Android.Content.IDialogInterfaceOnClickListener)null)
            .Show();
    }

    /// <summary>发送脚本并收集输出，直到出现 endMarker 或超时。</summary>
    private static async Task<string> CollectOutputAsync(WebSocketClient ws, string script, string endMarker)
    {
        var buffer = new StringBuilder();
        var tcs = new TaskCompletionSource<string>();
        EventHandler<string> handler = null;
        handler = (s, msg) =>
        {
            string content = ScriptPayload.ExtractMessage(msg);
            string snapshot;
            lock(buffer)
                snapshot = buffer.Append(content).Append('\n').ToString();

            if(content.Contains(endMarker))
            {
                ws.MessageReceived -= handler;
                _ = tcs.TrySetResult(snapshot);
            }
        };

        ws.MessageReceived += handler;
        ws.Send(script);
        bool finished = await Task.WhenAny(tcs.Task, Task.Delay(ScriptOutputTimeoutMs)) == tcs.Task;
        ws.MessageReceived -= handler;

        if(!finished)
            throw new TimeoutException($"等待脚本回传超时（{ScriptOutputTimeoutMs / 1000}秒）");

        return await tcs.Task;
    }

    private async Task SaveWaveJsonAsync(string json)
    {
        if(!Directory.Exists(SpawnWaveDir))
            _ = Directory.CreateDirectory(SpawnWaveDir);
        await File.WriteAllTextAsync(SpawnWaveFilePath, json);
    }

    private void ShowWaveJsonEditor(string json)
    {
        var editor = new EditText(Activity)
        {
            Text = json,
            Gravity = GravityFlags.Top | GravityFlags.Start,
            Typeface = Android.Graphics.Typeface.Monospace,
            InputType = Android.Text.InputTypes.ClassText
                | Android.Text.InputTypes.TextFlagMultiLine
                | Android.Text.InputTypes.TextFlagNoSuggestions
        };
        editor.SetMinLines(14);
        editor.SetMaxLines(24);

        var scroll = new ScrollView(Activity)
        {
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
        };
        scroll.AddView(editor);

        _ = new Android.App.AlertDialog.Builder(Activity)
            .SetTitle("编辑波次出怪 JSON")
            .SetView(scroll)
            .SetPositiveButton("载入到游戏", async (d, w) =>
            {
                if(!IsValidWaveJson(editor.Text))
                    return;
                await SaveWaveJsonAsync(editor.Text);
                await LoadWaveJsonAsync();
            })
            .SetNeutralButton("仅保存", async (d, w) =>
            {
                if(!IsValidWaveJson(editor.Text))
                    return;
                await SaveWaveJsonAsync(editor.Text);
                Toast.MakeText(Activity, "出怪数据已保存", ToastLength.Long).Show();
            })
            .SetNegativeButton("取消", (Android.Content.IDialogInterfaceOnClickListener)null)
            .Show();
    }

    /// <summary>校验失败时带着原文重开编辑器，避免对话框关闭后丢掉未保存的修改。</summary>
    private bool IsValidWaveJson(string json)
    {
        try
        {
            _ = JObject.Parse(json);
            return true;
        }
        catch(Exception ex)
        {
            Log.Error("波次出怪 JSON 校验失败", ex);
            ShowWaveJsonEditor(json);
            Toast.MakeText(Activity, $"JSON 格式错误：{ex.Message}", ToastLength.Long).Show();
            return false;
        }
    }
}
