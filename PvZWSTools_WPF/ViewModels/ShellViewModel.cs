using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.ViewModels;
using PvZWSTools_Shared.UiModel;

namespace PvZWSTools_WPF.ViewModels;

/// <summary>NewUI 外壳的视图模型：导航、搜索、缩放、常用统计。
/// 功能单元本身来自 <see cref="UnitCatalog"/>，这里只负责把它们呈现出来。
/// 收藏/常用是两端共用的状态，落在 <see cref="FavoriteStore"/> 里，安卓的收藏页读同一份逻辑。</summary>
public sealed class ShellViewModel : INotifyPropertyChanged
{
    private static readonly string LayoutPath =
        Path.Combine(AppContext.BaseDirectory, "ui_layout.cfg");

    private static readonly double[] ZoomSteps = { 0.85, 1.0, 1.15, 1.3, 1.45, 1.6 };

    private readonly Dictionary<string, bool> _layouts = new();
    private IReadOnlyList<UnitDescriptor> _all = Array.Empty<UnitDescriptor>();
    private NavItem? _selected;
    private string _search = string.Empty;
    private double _zoom = 1.0;

    public ObservableCollection<NavItem> Pages { get; } = new();

    /// <summary>收藏页的内容：常用区 + 收藏区（顺序由模板定，常用在前）。</summary>
    public FavoriteStore Favorites { get; } = new(AppContext.BaseDirectory);

    public FavoritesPanel FavoritePanel => Favorites.Panel;

    public NavItem FavoritePage { get; private set; } = null!;

    public NavItem? Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            // 选页面就该退出搜索态，否则 ActiveView 还是结果集，点了页面看起来没反应
            _search = string.Empty;
            Raise(nameof(Search));
            Raise();
            Raise(nameof(ActiveView));
            ScrollTopRequested?.Invoke();
        }
    }

    /// <summary>内容区实际渲染的对象：搜索时是结果集，否则是当前导航页。</summary>
    public object? ActiveView => string.IsNullOrWhiteSpace(_search)
        ? (object?)Selected
        : new SearchResults(Results, Loc.F("命中 {0} 个功能", Results.Count));

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

    public ShellViewModel(MainWindowViewModel root)
    {
        Root = root;

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

        Pages.Add(new NavItem
        {
            Title = "控制台",
            Glyph = "\uE756",
            Kind = NavKind.Console,
            Extra = root.Console,
        });

        // 星标、常用计数、两个集合的填充都在共享层，安卓的收藏页挂的是同一套
        Favorites.Attach(Pages.SelectMany(p => p.Units));
        _all = Favorites.All;

        // 收藏页放在导航最前面，并且是启动默认页：它是聚合视图，
        // 不参与"导航名照抄经典页签"那条约束
        FavoritePage = new NavItem
        {
            Title = "收藏",
            Glyph = "\uE734",
            Kind = NavKind.Favorites,
            Extra = FavoritePanel,
        };
        Pages.Insert(0, FavoritePage);

        ZoomInCommand = new RelayCommand(_ => StepZoom(+1));
        ZoomOutCommand = new RelayCommand(_ => StepZoom(-1));
        ClearSearchCommand = new RelayCommand(_ => Search = string.Empty);

        RestoreLayouts();

        Selected = FavoritePage;
    }

    private static object? Child(object root, string name) =>
        root.GetType().GetProperty(name)?.GetValue(root);

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

    public void SaveUsage() => Favorites.SaveUsage();

    /// <summary>多选块的密排/竖排选择。整块是一个功能，所以按单元 Id 记，不按页记。
    /// 没有记录时默认密排——把这 40 个选项收成一块本来就是为了一屏看完。</summary>
    private void RestoreLayouts()
    {
        LoadLayouts();
        foreach (var u in _all.Where(u => u.Kind == UnitKind.Chips))
        {
            u.Compact = !_layouts.TryGetValue(u.Id, out bool on) || on;
            u.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(UnitDescriptor.Compact))
                    SaveLayouts();
            };
        }
    }

    private void LoadLayouts()
    {
        try
        {
            if (!File.Exists(LayoutPath))
                return;
            foreach (var line in File.ReadAllLines(LayoutPath))
            {
                int i = line.IndexOf('=');
                if (i <= 0 || !bool.TryParse(line[(i + 1)..], out bool on))
                    continue;
                _layouts[line[..i]] = on;
            }
        }
        catch (Exception ex)
        {
            Log.Error("页面布局读取失败: " + ex);
        }
    }

    private void SaveLayouts()
    {
        try
        {
            File.WriteAllLines(LayoutPath, _all.Where(u => u.Kind == UnitKind.Chips)
                                        .Select(u => u.Id + "=" + u.Compact));
        }
        catch (Exception ex)
        {
            Log.Error("页面布局写入失败: " + ex);
        }
    }

    /// <summary>换页或换搜索结果时把内容区滚回顶部，由视图订阅。</summary>
    public event Action? ScrollTopRequested;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Raise([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
