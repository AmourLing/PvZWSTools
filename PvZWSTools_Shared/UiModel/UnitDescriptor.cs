using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PvZWSTools_Shared.UiModel;

public enum UnitKind
{
    /// <summary>开/关：状态符 + 切换按钮。</summary>
    Switch,
    /// <summary>三态：0 关闭 / 1 开启 / 2 默认。</summary>
    TriState,
    /// <summary>数值：标签 + 输入框 + 动作按钮。</summary>
    Field,
    /// <summary>选择：可编辑下拉 + 动作按钮。</summary>
    Picker,
    /// <summary>纯动作按钮。</summary>
    Action,
    /// <summary>复合：多个输入共用一个动作（按钮与它的参数同卡）。</summary>
    Composite,
    /// <summary>循环取值：点一下换下一个值，本身不是独立功能（如罐子类型）。</summary>
    Cycle,
    /// <summary>成组：旧界面里同处一个容器的几个功能，视觉上放一起但不改名字。</summary>
    Group,
    /// <summary>多选块：成员是同一个多选控件的一批选项，整块算一个功能（出怪类型）。</summary>
    Chips,
}

/// <summary>反射读写根 DataContext 上的一条属性（形如 "Others.ClearFog"），
/// 并把源对象的 PropertyChanged 转发给自己，使模板能实时刷新。
/// 开 AOT 或 trimming 前必须先换掉这里的反射，否则属性路径会静默解析不到。</summary>
internal sealed class PropRef : INotifyPropertyChanged
{
    private static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance;

    private object? _target;
    private PropertyInfo? _prop;
    private INotifyPropertyChanged? _watched;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>路径为空时什么都不做，模板据此隐藏该部分。</summary>
    public string Path { get; private set; } = string.Empty;
    public bool HasTarget => _prop != null;

    public void Resolve(object root, string? path)
    {
        Path = path ?? string.Empty;
        if (root == null || string.IsNullOrEmpty(path))
            return;

        var parts = path.Split('.');
        object? cur = root;
        for (int i = 0; i < parts.Length - 1 && cur != null; i++)
            cur = cur.GetType().GetProperty(parts[i], Flags)?.GetValue(cur);

        _prop = cur?.GetType().GetProperty(parts[^1], Flags);
        if (_prop == null)
        {
            Unwatch();
            return;
        }
        _target = cur;
        Watch();
    }

    public object? Get() => _prop?.GetValue(_target);

    public void Set(object? value)
    {
        if (_prop?.CanWrite != true)
            return;
        _prop.SetValue(_target, value);
        Raise(nameof(Text));
    }

    public string? Text
    {
        get => Get() as string;
        set => Set(value);
    }

    private void Watch()
    {
        if (_target is INotifyPropertyChanged n)
        {
            _watched = n;
            n.PropertyChanged += OnSourceChanged;
        }
    }

    private void Unwatch()
    {
        if (_watched != null)
            _watched.PropertyChanged -= OnSourceChanged;
        _watched = null;
    }

    private void OnSourceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _prop?.Name)
        {
            Raise(nameof(Text));
            Raise(nameof(Get));
        }
    }

    private void Raise([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>复合单元里的一个输入位。</summary>
public sealed class UnitField : INotifyPropertyChanged
{
    internal readonly PropRef Input = new();
    internal readonly PropRef Options = new();
    internal readonly PropRef Selected = new();

    public string Label { get; init; } = string.Empty;
    public string InputPath { get; init; } = string.Empty;
    public string? OptionsPath { get; init; }
    public string? SelectedPath { get; init; }
    public double Width { get; init; } = double.NaN;
    public bool HasOptions => Options.HasTarget;
    public double FieldWidth => double.IsNaN(Width) ? 90 : Width;

    public string? Text
    {
        get => Input.Text;
        set => Input.Text = value;
    }

    public IEnumerable? OptionList => Options.Get() as IEnumerable;

    public object? SelectedItem
    {
        get => Selected.Get();
        set => Selected.Set(value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    internal void Resolve(object root)
    {
        Input.Resolve(root, InputPath);
        Options.Resolve(root, OptionsPath);
        Selected.Resolve(root, SelectedPath);
        Input.PropertyChanged += (_, _) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
    }
}

/// <summary>一个功能单元：按钮 + 它真正消费的那些控件。</summary>
public sealed class UnitDescriptor : INotifyPropertyChanged
{
    internal readonly PropRef State = new();
    internal readonly PropRef Input = new();
    internal readonly PropRef Input2 = new();
    internal readonly PropRef Options = new();
    internal readonly PropRef Selected = new();

    public required string Id { get; init; }
    public required string Label { get; init; }
    public string? Hint { get; init; }
    public UnitKind Kind { get; init; } = UnitKind.Action;

    public string? StatePath { get; init; }
    public string? CommandPath { get; init; }
    public string? InputPath { get; init; }
    public string? Input2Path { get; init; }
    public string? OptionsPath { get; init; }
    public string? SelectedPath { get; init; }
    public IReadOnlyList<UnitField> Fields { get; init; } = Array.Empty<UnitField>();

    /// <summary>Group 的成员；成员在组内改用无边框的紧凑画法。</summary>
    public IReadOnlyList<UnitDescriptor> Members { get; init; } = Array.Empty<UnitDescriptor>();

    /// <summary>由 Grp(...) 在装配时置位，模板据此去掉外框与整行高度。</summary>
    public bool InGroup { get; internal set; }

    /// <summary>搜索别名：中文同义词、英文属性名都塞这里。</summary>
    public string[] Keywords { get; init; } = Array.Empty<string>();

    /// <summary>所属页签，名称与经典 UI 一致；搜索结果用它标出处。</summary>
    public string Group { get; internal set; } = string.Empty;

    /// <summary>
    /// 多选块当前是密排芯片还是竖排一行一个。只是视图偏好，外壳负责落盘；
    /// 别的单元类型读不到它（模板里只有 <see cref="UnitKind.Chips"/> 用它做触发）。
    /// </summary>
    private bool _compact;

    public bool Compact
    {
        get => _compact;
        set
        {
            if (_compact == value)
                return;
            _compact = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Compact)));
        }
    }

    /// <summary>40 个选项铺开以后，"选了几个"是这一块唯一还能一眼看出来的信息。</summary>
    public string SelectedSummary => Kind == UnitKind.Chips
        ? $"已选 {Members.Count(m => m.IsOn)}/{Members.Count}"
        : string.Empty;

    public ICommand? Command { get; private set; }

    /// <summary>界面绑这个而不是 Command：外壳借这一跳记录常用次数。</summary>
    public ICommand? RunCommand { get; internal set; }

    /// <summary>收藏开关。真正的集合和落盘在外壳里，这里只存当前这一条的状态供界面绑定。</summary>
    public ICommand? FavoriteCommand { get; internal set; }

    private bool _isFavorite;
    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            if (_isFavorite == value)
                return;
            _isFavorite = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFavorite)));
        }
    }

    public string? StateText => State.Text;
    public string? Text
    {
        get => Input.Text;
        set => Input.Text = value;
    }
    public string? Text2
    {
        get => Input2.Text;
        set => Input2.Text = value;
    }
    public IEnumerable? OptionList => Options.Get() as IEnumerable;
    public object? SelectedItem
    {
        get => Selected.Get();
        set => Selected.Set(value);
    }

    public bool HasOptions => Options.HasTarget;
    public bool HasInput2 => Input2.HasTarget;

    /// <summary>组里有没有应用这个收尾动作；有则组首显示功能名。</summary>
    public bool HasAction => Command != null;

    /// <summary>循环取值单元直接把自己的值当文字显示。</summary>
    public string DisplayValue => State.Text ?? string.Empty;
    public bool IsOn => State.Text == On;
    public bool IsOff => State.Text == Off;

    /// <summary>把 VM 里的协议符号（✔️/❌/""，或挑战的 0/1/2）翻译成界面文案。
    /// 只影响显示，不回写，脚本协议保持不变。</summary>
    public string DisplayState => Kind == UnitKind.TriState
        ? State.Text switch { "1" => "开启", "0" => "关闭", "2" => "默认", _ => "未知" }
        : State.Text switch { On => "开", Off => "关", "" or null => "未知", _ => "未知" };

    /// <summary>VM 里存的是发给脚本的协议符号，显示层只读不写。</summary>
    public const string On = "✔️";
    public const string Off = "❌";

    public string SearchBlob { get; private set; } = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    internal UnitDescriptor Bind(object root)
    {
        State.Resolve(root, StatePath);
        Input.Resolve(root, InputPath);
        Input2.Resolve(root, Input2Path);
        Options.Resolve(root, OptionsPath);
        Selected.Resolve(root, SelectedPath);
        State.PropertyChanged += (_, _) => Forward();
        foreach (var f in Fields)
            f.Resolve(root);
        foreach (var m in Members)
        {
            m.InGroup = true;
            m.Bind(root);
            // 成员开关联动到块头的"已选 N/M"；多选块整块算一个功能，计数是它唯一的汇总信息
            m.PropertyChanged += (_, _) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSummary)));
        }
        if (CommandPath != null)
            Command = ResolveCommand(root, CommandPath);
        SearchBlob = string.Join(' ', Label, Id, string.Join(' ', Keywords)).ToLowerInvariant();
        return this;
    }

    internal void Mark(string group)
    {
        Group = group;
        foreach (var m in Members)
            m.Mark(group);
    }

    private static ICommand? ResolveCommand(object root, string path)
    {
        var parts = path.Split('.');
        object? cur = root;
        for (int i = 0; i < parts.Length && cur != null; i++)
            cur = cur.GetType()
                     .GetProperty(parts[i], BindingFlags.Public | BindingFlags.Instance)
                     ?.GetValue(cur);
        return cur as ICommand;
    }

    private void Forward()
    {
        foreach (var n in new[] { nameof(StateText), nameof(IsOn), nameof(IsOff),
                                  nameof(DisplayState), nameof(DisplayValue) })
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}

/// <summary>内容区的渲染形态：功能行列表页，或两块特殊面板。</summary>
public enum NavKind { Units, Script, Garden, Favorites, Console }

/// <summary>左侧导航的一项，名称与顺序跟经典 UI 的页签一致。</summary>
public sealed class NavItem
{
    public required string Title { get; init; }
    public string Glyph { get; init; } = string.Empty;
    public NavKind Kind { get; init; } = NavKind.Units;
    public IReadOnlyList<UnitDescriptor> Units { get; init; } = Array.Empty<UnitDescriptor>();

    /// <summary>特殊页的数据源：脚本=QMod，花园=Garden。</summary>
    public object? Extra { get; internal set; }
}
