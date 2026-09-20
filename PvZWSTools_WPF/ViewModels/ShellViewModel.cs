using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.ViewModels;
using PvZWSTools_WPF.UiModel;

namespace PvZWSTools_WPF.ViewModels;

/// <summary>NewUI 外壳的视图模型：导航、搜索、缩放、常用统计。
/// 功能单元本身来自 <see cref="UnitCatalog"/>，这里只负责把它们呈现出来。</summary>
public sealed class ShellViewModel : INotifyPropertyChanged
{
    private static readonly string UsagePath =
        Path.Combine(AppContext.BaseDirectory, "ui_usage.cfg");

    private static readonly double[] ZoomSteps = { 0.85, 1.0, 1.15, 1.3, 1.45, 1.6 };

    private readonly Dictionary<string, int> _usage = new();
    private readonly List<UnitDescriptor> _all = new();
    private NavItem? _selected;
    private string _search = string.Empty;
    private double _zoom = 1.0;

    public ObservableCollection<NavItem> Pages { get; } = new();

    public NavItem? Selected
    {
        get => _selected;
        set { _selected = value; Raise(); Raise(nameof(ActiveView)); ScrollTopRequested?.Invoke(); }
    }

    /// <summary>内容区实际渲染的对象：搜索时是结果集，否则是当前导航页。</summary>
    public object? ActiveView => string.IsNullOrWhiteSpace(_search)
        ? (object?)Selected
        : new SearchResults(Results, $"命中 {Results.Count} 个功能");

    public string Search
    {
        get => _search;
        set
        {
            _search = value ?? string.Empty;
            Raise();
            Raise(nameof(ActiveView));
            ScrollTopRequested?.Invoke();
        }
    }

    public IReadOnlyList<UnitDescriptor> Results { get; private set; } = Array.Empty<UnitDescriptor>();

    /// <summary>搜索结果连同命中数一起呈现。</summary>
    public sealed record SearchResults(IReadOnlyList<UnitDescriptor> Items, string Count);

    public double Zoom
    {
        get => _zoom;
        private set
        {
            _zoom = value;
            ZoomTransform.ScaleX = ZoomTransform.ScaleY = value;
            Raise();
            Raise(nameof(ZoomText));
        }
    }

    public string ZoomText => (Zoom * 100).ToString("0", CultureInfo.InvariantCulture) + "%";

    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ClearSearchCommand { get; }

    /// <summary>外壳自己挂到 Window.DataContext 上，连接栏等原有绑定走这里。</summary>
    public MainWindowViewModel Root { get; }

    public ScaleTransform ZoomTransform { get; } = new();

    public ShellViewModel(MainWindowViewModel root)
    {
        Root = root;
        LoadUsage();

        var catalog = UnitCatalog.Build(root);
        foreach (var p in catalog)
        {
            // 花园编辑页沿用调试期才开放的做法
#if !DEBUG
            if (p.Kind == NavKind.Garden)
                continue;
#endif
            if (p.Kind == NavKind.Script)
                p.Extra = Child(root, "QMod");
            else if (p.Kind == NavKind.Garden)
                p.Extra = Child(root, "Garden");
            Pages.Add(p);
        }

        _all = Pages.SelectMany(p => Flatten(p.Units)).ToList();

        ZoomInCommand = new RelayCommand(_ => StepZoom(+1));
        ZoomOutCommand = new RelayCommand(_ => StepZoom(-1));
        ClearSearchCommand = new RelayCommand(_ => Search = string.Empty);

        foreach (var u in _all)
            u.RunCommand = new RelayCommand(_ => Run(u));

        Selected = Pages[0];
    }

    private static object? Child(object root, string name) =>
        root.GetType().GetProperty(name)?.GetValue(root);

    /// <summary>组里的成员也要能被搜到、也能记常用，所以摊平成一维。</summary>
    private static IEnumerable<UnitDescriptor> Flatten(IEnumerable<UnitDescriptor> units)
    {
        foreach (var u in units)
        {
            yield return u;
            if (u.Members.Count > 0)
                foreach (var m in Flatten(u.Members))
                    yield return m;
        }
    }

    private void Run(UnitDescriptor unit)
    {
        _usage[unit.Id] = _usage.TryGetValue(unit.Id, out int n) ? n + 1 : 1;
        unit.Command?.Execute(null);
    }

    private void StepZoom(int dir)
    {
        int i = Array.FindIndex(ZoomSteps, z => Math.Abs(z - _zoom) < 0.01);
        i = Math.Clamp((i < 0 ? 1 : i) + dir, 0, ZoomSteps.Length - 1);
        Zoom = ZoomSteps[i];
    }

    /// <summary>按标签、英文属性名、别名关键词过滤全部单元，结果保持页签内的原有顺序。</summary>
    public void ApplySearch()
    {
        string q = _search.Trim().ToLowerInvariant();
        Results = q.Length == 0
            ? Array.Empty<UnitDescriptor>()
            : _all.Where(u => u.SearchBlob.Contains(q, StringComparison.Ordinal)).ToList();
        Raise(nameof(Results));
        Raise(nameof(ActiveView));
    }

    public void SaveUsage()
    {
        try
        {
            File.WriteAllLines(UsagePath, _usage.Select(kv => kv.Key + "=" + kv.Value));
        }
        catch (Exception ex)
        {
            Log.Error("常用统计写入失败: " + ex);
        }
    }

    private void LoadUsage()
    {
        try
        {
            if (!File.Exists(UsagePath))
                return;
            foreach (var line in File.ReadAllLines(UsagePath))
            {
                int i = line.IndexOf('=');
                if (i <= 0 || !int.TryParse(line[(i + 1)..], out int n))
                    continue;
                _usage[line[..i]] = n;
            }
        }
        catch (Exception ex)
        {
            Log.Error("常用统计读取失败: " + ex);
        }
    }

    /// <summary>换页或换搜索结果时把内容区滚回顶部，由视图订阅。</summary>
    public event Action? ScrollTopRequested;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Raise([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
