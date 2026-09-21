using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_WPF.ViewModels;

/// <summary>
/// 状态管理窗口的 ViewModel。
/// 预设列表前两项固定：[0]初始状态（应用默认值，不可删除）、[1]上次状态（关闭时自动保存，不可删除），
/// 其后为用户命名保存的多组预设。
/// </summary>
public class StateWindowViewModel:INotifyPropertyChanged
{
    /// <summary>固定的"初始状态"条目显示名。</summary>
    public const string InitialStateName = "初始状态";
    /// <summary>固定的"上次状态"条目显示名。</summary>
    public const string LastStateName = "上次状态";

    private readonly System.Func<Dictionary<string, Dictionary<string, string>>> _getCurrentStates;
    private readonly System.Func<Dictionary<string, Dictionary<string, string>>> _getLastStates;
    private readonly System.Func<Dictionary<string, Dictionary<string, string>>> _getDefaultStates;
    private readonly System.Action<Dictionary<string, Dictionary<string, string>>> _applyStates;
    private readonly IStatePresetService _presetService;
    private string _newPresetName = "";
    private int _selectedPresetIndex = 0;
    private bool _isDetailVisible = false;

    /// <summary>子 ViewModel 属性名 → 中文分类名。</summary>
    private static readonly Dictionary<string, string> CategoryNames = new()
    {
        ["Board"] = "战场", ["Plants"] = "植物", ["Zombies"] = "僵尸",
        ["Others"] = "杂项", ["Spawn"] = "出怪", ["Level"] = "关卡",
        ["Resources"] = "资源", ["Challenge"] = "挑战", ["Formation"] = "阵型",
        ["Fun"] = "娱乐", ["QMod"] = "QMod", ["Garden"] = "花园"
    };

    public StateWindowViewModel(
        System.Func<Dictionary<string, Dictionary<string, string>>> getCurrentStates,
        System.Func<Dictionary<string, Dictionary<string, string>>> getLastStates,
        System.Func<Dictionary<string, Dictionary<string, string>>> getDefaultStates,
        System.Action<Dictionary<string, Dictionary<string, string>>> applyStates)
    {
        _getCurrentStates = getCurrentStates;
        _getLastStates = getLastStates;
        _getDefaultStates = getDefaultStates;
        _applyStates = applyStates;
        _presetService = new StatePresetService(AppContext.BaseDirectory);
        RefreshList();

        SavePresetCommand = new RelayCommand(_ => SaveCurrent());
        LoadPresetCommand = new RelayCommand(_ => LoadSelected(), _ => SelectedPresetIndex >= 0);
        DeletePresetCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedPresetIndex > 1);
        ShowDetailsCommand = new RelayCommand(_ => ToggleDetails(), _ => SelectedPresetIndex >= 0);
    }

    public ObservableCollection<string> PresetNames { get; } = new();

    /// <summary>详细信息面板的各行内容。</summary>
    public ObservableCollection<string> DetailLines { get; } = new();

    /// <summary>详细信息面板是否可见（点击"详细信息"按钮切换）。</summary>
    public bool IsDetailVisible
    {
        get => _isDetailVisible;
        set => SetField(ref _isDetailVisible, value);
    }

    /// <summary>当前选中的条目（0 = 初始状态，1 = 上次状态，>1 = 用户预设）。</summary>
    public int SelectedPresetIndex
    {
        get => _selectedPresetIndex;
        set
        {
            if(SetField(ref _selectedPresetIndex, value))
            {
                // 选中固定项（初始状态/上次状态）时清空名称输入框；选中用户预设时填入名称便于覆盖保存
                if(value <= 1)
                    NewPresetName = "";
                else if(value > 1 && value < PresetNames.Count)
                    NewPresetName = PresetNames[value];

                (DeletePresetCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (LoadPresetCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ShowDetailsCommand as RelayCommand)?.RaiseCanExecuteChanged();

                // 详情面板展开时，切换选中项自动刷新内容
                if(_isDetailVisible)
                    BuildDetailLines();
            }
        }
    }

    public string NewPresetName
    {
        get => _newPresetName;
        set => SetField(ref _newPresetName, value);
    }

    public ICommand SavePresetCommand { get; }
    public ICommand LoadPresetCommand { get; }
    public ICommand DeletePresetCommand { get; }
    public ICommand ShowDetailsCommand { get; }

    public event EventHandler? RequestClose;

    private void RefreshList()
    {
        PresetNames.Clear();
        // 前两项固定：初始状态、上次状态
        PresetNames.Add(InitialStateName);
        PresetNames.Add(LastStateName);
        foreach(var name in _presetService.LoadAll().Select(p => p.Name))
            PresetNames.Add(name);
        SelectedPresetIndex = 0;
    }

    private void SaveCurrent()
    {
        string name = (NewPresetName ?? "").Trim();
        if(string.IsNullOrEmpty(name))
        {
            Log.Warning("状态预设名称不能为空");
            return;
        }
        if(name == InitialStateName || name == LastStateName)
        {
            Log.Warning("不能使用保留名称「初始状态」或「上次状态」");
            return;
        }
        var states = _getCurrentStates();
        var preset = new StatePreset
        {
            Name = name,
            CreatedAt = System.DateTime.Now,
            States = states
        };
        _presetService.Add(preset);
        RefreshList();
        // 选中新保存的项
        int idx = PresetNames.IndexOf(name);
        if(idx >= 0) SelectedPresetIndex = idx;
    }

    private void LoadSelected()
    {
        if(SelectedPresetIndex == 0)
        {
            // 加载"初始状态"（应用默认值）
            var states = _getDefaultStates();
            if(states.Count == 0)
            {
                Log.Warning("没有可加载的初始状态");
                return;
            }
            _applyStates(states);
            return;
        }
        if(SelectedPresetIndex == 1)
        {
            // 加载"上次状态"
            var states = _getLastStates();
            if(states.Count == 0)
            {
                Log.Warning("没有可加载的上次状态");
                return;
            }
            _applyStates(states);
            return;
        }

        // 加载用户命名预设
        var presets = _presetService.LoadAll();
        int presetIdx = SelectedPresetIndex - 2;
        if(presetIdx < 0 || presetIdx >= presets.Count) return;
        var preset = presets[presetIdx];
        if(preset.States.Count == 0) return;
        _applyStates(preset.States);
    }

    private void DeleteSelected()
    {
        // 固定项（初始状态 index 0、上次状态 index 1）不可删除
        if(SelectedPresetIndex <= 1) return;
        string name = PresetNames[SelectedPresetIndex];
        _presetService.Delete(name);
        RefreshList();
        NewPresetName = "";
    }

    /// <summary>切换详细信息面板的显示状态；展开时重新构建当前选中项的内容。</summary>
    private void ToggleDetails()
    {
        IsDetailVisible = !IsDetailVisible;
        if(_isDetailVisible)
            BuildDetailLines();
        else
            DetailLines.Clear();
    }

    /// <summary>
    /// 根据当前选中项构建详细信息内容。
    /// 与默认状态对比，只显示值不同的项；✔️ 的开关用 ★ 标出。
    /// </summary>
    private void BuildDetailLines()
    {
        DetailLines.Clear();
        var states = GetSelectedStates();
        if(states == null || states.Count == 0)
        {
            DetailLines.Add("（无保存内容）");
            return;
        }

        var defaults = _getDefaultStates();

        string presetName = SelectedPresetIndex switch
        {
            0 => InitialStateName,
            1 => LastStateName,
            _ => PresetNames[SelectedPresetIndex]
        };
        DetailLines.Add($"方案：{presetName}（仅显示与默认不同的项）");

        int diffCount = 0;
        foreach(var cat in states)
        {
            if(cat.Value.Count == 0) continue;
            defaults.TryGetValue(cat.Key, out var catDefault);
            var diffItems = new List<string>();
            foreach(var item in cat.Value)
            {
                string? defVal = null;
                catDefault?.TryGetValue(item.Key, out defVal);
                if(item.Value != defVal)
                    diffItems.Add(item.Key);
            }
            if(diffItems.Count == 0) continue;

            string catName = CategoryNames.TryGetValue(cat.Key, out var cn) ? cn : cat.Key;
            DetailLines.Add($"【{catName}】（{diffItems.Count}）");
            foreach(var key in diffItems)
            {
                string val = cat.Value[key];
                bool isOn = val == Constants.c_Symbol_On;
                string mark = isOn ? "★" : "  ";
                DetailLines.Add($"  {mark} {key} = {val}");
            }
            diffCount += diffItems.Count;
        }

        if(diffCount == 0)
            DetailLines.Add("（与默认状态完全一致，无改变）");
        else
            DetailLines.Insert(1, $"共 {diffCount} 项与默认不同");
    }

    /// <summary>读取当前选中项保存的状态（0 = 初始状态；1 = 上次状态；>1 = 用户预设）。</summary>
    private Dictionary<string, Dictionary<string, string>>? GetSelectedStates()
    {
        if(SelectedPresetIndex == 0)
            return _getDefaultStates();
        if(SelectedPresetIndex == 1)
            return _getLastStates();
        var presets = _presetService.LoadAll();
        int idx = SelectedPresetIndex - 2;
        if(idx < 0 || idx >= presets.Count) return null;
        return presets[idx].States;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if(EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
