using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Android.App;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Avalonia.Helpers;

/// <summary>
/// Android 端状态管理服务（与 WPF 端状态管理功能对齐）：
/// - 初始状态：启动时用各 Fragment 的 InitializeMap 默认值采集（不可修改）
/// - 当前状态：内存中的各界面 Map 状态，随 Fragment 创建/暂停实时同步
/// - 上次状态：应用关闭时持久化到 配置文件/button_states.json（不可删除）
/// - 用户预设：多组命名方案，持久化到 配置文件/state_presets.json
/// - 开关同步：连接游戏成功后，把开启的开关脚本批量发送（仅发送含 {"{"}CHECK{"}"} 占位符的脚本，静默跳过缺失项）
/// </summary>
public class AndroidStateService
{
    public static AndroidStateService Instance { get; private set; }

    /// <summary>资源前缀 → 控件子目录（FragmentPath）映射。</summary>
    private static readonly Dictionary<string, string> PrefixToSection = new()
    {
        ["others"] = "杂项",
        ["plant"] = "植物",
        ["zombie"] = "僵尸",
        ["spawning"] = "出怪",
        ["board"] = "战场",
        ["fun"] = "娱乐",
        ["formation"] = "阵型",
        ["challenge"] = "挑战",
        ["resources"] = "资源",
        ["level"] = "关卡",
        ["script"] = "快捷脚本",
    };

    /// <summary>控件子目录 → Fragment 工厂（用于采集默认状态）。</summary>
    private static readonly Dictionary<string, Func<BaseFragment>> SectionFactories = new()
    {
        ["杂项"] = () => new OthersFragment(),
        ["植物"] = () => new PlantFragment(),
        ["僵尸"] = () => new ZombieFragment(),
        ["出怪"] = () => new SpawningFragment(),
        ["战场"] = () => new BoardFragment(),
        ["娱乐"] = () => new FunFragment(),
        ["阵型"] = () => new FormationFragment(),
        ["挑战"] = () => new ChallengeFragment(),
        ["资源"] = () => new ResourcesFragment(),
        ["关卡"] = () => new LevelFragment(),
    };

    /// <summary>固定的"初始状态"条目显示名。</summary>
    public const string InitialStateName = "初始状态";
    /// <summary>固定的"上次状态"条目显示名。</summary>
    public const string LastStateName = "上次状态";

    private readonly Activity _activity;
    private readonly AppSettings _appSettings;
    private readonly IButtonStateService _stateStore;
    private readonly IStatePresetService _presetStore;

    /// <summary>初始状态（各界面默认 Map，启动时采集一次）。</summary>
    public Dictionary<string, Dictionary<string, string>> DefaultStates { get; } = new();

    /// <summary>当前状态（内存，section → key → value）。</summary>
    public Dictionary<string, Dictionary<string, string>> CurrentStates { get; } = new();

    /// <summary>上次状态（关闭时保存的状态；仅在勾选"自动应用上次配置"时于启动时应用）。</summary>
    public Dictionary<string, Dictionary<string, string>> LastStates { get; private set; } = new();

    /// <summary>section → (key字符串 → 脚本名)，从资源命名规则推导，用于开关同步。</summary>
    private readonly Dictionary<string, Dictionary<string, string>> _keyToScript = new();

    /// <summary>本次会话是否已应用过状态（用于决定连接后是否同步开关到游戏）。</summary>
    public bool StatesApplied { get; private set; }

    private AndroidStateService(Activity activity, AppSettings appSettings)
    {
        _activity = activity;
        _appSettings = appSettings;
        string baseDir = MainActivity.AppFilesPath;
        _stateStore = new ButtonStateService(baseDir);
        _presetStore = new StatePresetService(baseDir);
    }

    /// <summary>初始化服务：构建脚本映射 → 采集默认状态 → 加载上次状态 → 按设置应用。</summary>
    public static void Initialize(Activity activity, AppSettings appSettings)
    {
        Instance = new AndroidStateService(activity, appSettings);
        Instance.BuildKeyToScriptMap();
        Instance.CaptureDefaults();

        Instance.LastStates = Instance._stateStore.Load();

        // 预填充当前状态为默认基线：保证 CurrentStates 永远非空，
        // 否则用户未访问过界面时 CurrentStates 为空 → 保存被跳过 → "上次状态"永远为空。
        foreach(var kv in Instance.DefaultStates)
            Instance.CurrentStates[kv.Key] = new Dictionary<string, string>(kv.Value);

        if(appSettings.AutoApplyLastState && Instance.LastStates.Count > 0)
        {
            Instance.ApplyStates(Instance.LastStates, syncIfConnected: false);
            Log.Info($"已自动应用上次状态：{Instance.LastStates.Sum(kv => kv.Value.Count)} 项");
        }
        else
        {
            Log.Info(Instance.LastStates.Count == 0
                ? "没有已保存的上次状态，当前状态使用默认基线"
                : "未开启自动应用上次配置，跳过状态恢复");
        }
    }

    /// <summary>
    /// 从 Resource.String 资源命名规则推导 key→脚本名 映射：
    /// key 资源名形如 {prefix}_strings_N_M_key，去掉尾部 _M_key 得到标题资源名 {prefix}_strings_N（即脚本名）。
    /// </summary>
    private void BuildKeyToScriptMap()
    {
        try
        {
            var res = _activity.Resources;
            string packageName = _activity.PackageName;
            var fields = typeof(Resource.String).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.Name.EndsWith("_key") && f.FieldType == typeof(int));

            foreach(var field in fields)
            {
                string entryName = field.Name;                       // 如 spawning_strings_2_21_key
                string stem = entryName.Substring(0, entryName.Length - 4); // 去 "_key" → spawning_strings_2_21
                int lastUnderscore = stem.LastIndexOf('_');
                if(lastUnderscore <= 0) continue;
                string titleName = stem.Substring(0, lastUnderscore);       // spawning_strings_2
                string prefix = titleName.Split('_')[0];
                if(!PrefixToSection.TryGetValue(prefix, out var section)) continue;

                int keyResId = (int)field.GetValue(null)!;
                int titleResId = res.GetIdentifier(titleName, "string", packageName);
                if(keyResId == 0 || titleResId == 0) continue;

                string keyText = res.GetString(keyResId);
                string titleText = res.GetString(titleResId);
                if(string.IsNullOrEmpty(keyText) || string.IsNullOrEmpty(titleText)) continue;

                if(!_keyToScript.TryGetValue(section, out var dict))
                    _keyToScript[section] = dict = new Dictionary<string, string>();
                dict[keyText] = titleText;
            }
            Log.Info($"脚本映射构建完成：{_keyToScript.Sum(kv => kv.Value.Count)} 项");
        }
        catch(Exception ex)
        {
            Log.Error($"脚本映射构建失败: {ex.Message}");
        }
    }

    /// <summary>用各 Fragment 的 InitializeMap 采集默认状态（未附加 Activity，GetString 回退到应用级 Context）。</summary>
    private void CaptureDefaults()
    {
        foreach(var kv in SectionFactories)
        {
            try
            {
                var fragment = kv.Value();
                fragment.ResetToDefaults();
                DefaultStates[kv.Key] = fragment.GetStateSnapshot();
            }
            catch(Exception ex)
            {
                Log.Error($"默认状态采集失败({kv.Key}): {ex.Message}");
            }
        }
        Log.Info($"初始状态采集完成：{DefaultStates.Count} 个界面，{DefaultStates.Sum(kv => kv.Value.Count)} 项");
    }

    // --- Fragment 生命周期钩子（由 BaseFragment 调用） ---

    /// <summary>Fragment 创建时：把存储的当前状态覆盖到其 Map（只覆盖 Map 已有的键）。</summary>
    public void OnFragmentCreated(BaseFragment fragment)
    {
        string section = fragment.StateSection;
        if(CurrentStates.TryGetValue(section, out var stored))
        {
            fragment.ApplyStateSnapshot(stored);
        }
        else
        {
            CurrentStates[section] = fragment.GetStateSnapshot();
        }
    }

    /// <summary>Fragment 暂停/销毁时：把其 Map 快照写回当前状态。</summary>
    public void OnFragmentStateDirty(BaseFragment fragment)
    {
        CurrentStates[fragment.StateSection] = fragment.GetStateSnapshot();
    }

    /// <summary>
    /// 把当前正在显示的 Fragment 的 Map 写回当前状态。
    /// 打开状态管理对话框、持久化之前必须调用——用户改动按钮时 Fragment 未必发生 Pause，
    /// 否则读到的是 Fragment 创建时的旧快照（会显示"与默认一致"）。
    /// </summary>
    public void CaptureActiveFragment()
    {
        if(MainActivity.Instance?.CurrentFragment is BaseFragment fragment)
            OnFragmentStateDirty(fragment);
    }

    // --- 状态应用 ---

    /// <summary>
    /// 应用一组状态：每个 section 用"默认值 + 预设覆盖"合成完整状态写入 CurrentStates，
    /// 并同步到当前显示的对应 Fragment。states 为空字典时表示完全恢复默认。
    /// </summary>
    public void ApplyStates(Dictionary<string, Dictionary<string, string>> states, bool syncIfConnected)
    {
        foreach(var section in SectionFactories.Keys)
        {
            var merged = new Dictionary<string, string>();
            if(DefaultStates.TryGetValue(section, out var defaults))
            {
                foreach(var kv in defaults)
                    merged[kv.Key] = kv.Value;
            }
            if(states.TryGetValue(section, out var sectionStates))
            {
                foreach(var kv in sectionStates)
                    merged[kv.Key] = kv.Value;
            }
            CurrentStates[section] = merged;
        }

        // 同步到当前显示的 Fragment
        var current = MainActivity.Instance?.CurrentFragment as BaseFragment;
        if(current != null && CurrentStates.TryGetValue(current.StateSection, out var map))
        {
            current.ApplyStateSnapshot(map);
            current.RefreshAllButtons();
        }

        StatesApplied = true;

        if(syncIfConnected)
            SyncTogglesToGame();
    }

    // --- 持久化 ---

    /// <summary>把当前状态保存为"上次状态"（应用关闭时调用）。当前状态为空时跳过，避免覆盖旧记录。</summary>
    public void PersistLastStates()
    {
        if(CurrentStates.Count == 0)
        {
            Log.Info("当前状态为空，跳过保存上次状态");
            return;
        }
        LastStates = new Dictionary<string, Dictionary<string, string>>(CurrentStates);
        _stateStore.Save(LastStates);
    }

    /// <summary>保存命名预设（同名覆盖；禁止使用保留名）。</summary>
    public bool SavePreset(string name)
    {
        if(string.IsNullOrWhiteSpace(name)) return false;
        if(name == InitialStateName || name == LastStateName) return false;
        _presetStore.Add(new StatePreset
        {
            Name = name.Trim(),
            CreatedAt = DateTime.Now,
            States = CurrentStates.ToDictionary(kv => kv.Key, kv => new Dictionary<string, string>(kv.Value)),
        });
        return true;
    }

    /// <summary>删除命名预设（保留名不可删除）。</summary>
    public bool DeletePreset(string name)
    {
        if(name == InitialStateName || name == LastStateName) return false;
        return _presetStore.Delete(name);
    }

    /// <summary>加载全部用户预设。</summary>
    public List<StatePreset> LoadPresets() => _presetStore.LoadAll();

    // --- 开关同步到游戏 ---

    /// <summary>
    /// 把开启的开关脚本批量发送到游戏。只发送含 {"{"}CHECK{"}"} 占位符的脚本，
    /// 脚本缺失/不匹配时静默跳过（仅写日志），避免连环弹窗。
    /// 返回实际发送数量。
    /// </summary>
    public int SyncTogglesToGame()
    {
        var ws = MainActivity.ws;
        if(ws == null || !ws.IsConnected) return 0;

        int sent = 0;
        try
        {
            foreach(var (section, states) in CurrentStates)
            {
                if(!_keyToScript.TryGetValue(section, out var scriptMap)) continue;
                foreach(var (key, value) in states)
                {
                    if(value != "1") continue;
                    if(!scriptMap.TryGetValue(key, out var scriptName)) continue;

                    string scriptPath = Path.Combine(MainActivity.AppFilesPath, "配置文件", "控件", section, scriptName + ".py");
                    if(!File.Exists(scriptPath)) continue;

                    string content = File.ReadAllText(scriptPath);
                    if(!content.Contains(Constants.Placeholders.Check)) continue;

                    ws.Send(content.Replace(Constants.Placeholders.Check, "1"));
                    sent++;
                }
            }
            if(sent > 0)
                Log.Info($"已将 {sent} 个开启状态同步到游戏");
            else
                Log.Info("没有需要同步到游戏的开启状态");
        }
        catch(Exception ex)
        {
            Log.Error($"同步开关状态失败: {ex.Message}");
        }
        return sent;
    }
}
