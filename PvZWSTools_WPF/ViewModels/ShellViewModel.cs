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
/// 功能单元本身来自 <see cref="UnitCatalog"/>，这里只负责把它们呈现出来。</summary>
public sealed class ShellViewModel : INotifyPropertyChanged
{
    private static readonly string UsagePath =
        Path.Combine(AppContext.BaseDirectory, "ui_usage.cfg");

    private static readonly string FavoritesPath =
        Path.Combine(AppContext.BaseDirectory, "ui_favorites.cfg");

    private static readonly string LayoutPath =
        Path.Combine(AppContext.BaseDirectory, "ui_layout.cfg");

    private static readonly double[] ZoomSteps = { 0.85, 1.0, 1.15, 1.3, 1.45, 1.6 };

    private readonly Dictionary<string, int> _usage = new();
    private readonly HashSet<string> _favorites = new();
    private readonly Dictionary<string, bool> _layouts = new();
    private readonly List<UnitDescriptor> _all = new();
    private NavItem? _selected;
    private string _search = string.Empty;
    private double _zoom = 1.0;

    public ObservableCollection<NavItem> Pages { get; } = new();

    /// <summary>收藏页的内容：收藏区 + 常用区，两个都是实时集合，界面不用重建绑定。</summary>
    public sealed class FavoritesPanel : INotifyPropertyChanged
    {
        public ObservableCollection<UnitDescriptor> Favorites { get; } = new();
        public ObservableCollection<UnitDescriptor> Frequent { get; } = new();

        /// <summary>
        /// 空状态提示用这两个开关，而不是直接绑集合的 Count ——
        /// ObservableCollection 只在内容变化时发 CollectionChanged，不发 PropertyChanged(Count)，
        /// 绑 Count 的触发器不会跟着刷新。
        /// </summary>
        public bool HasFavorites => Favorites.Count > 0;
        public bool HasFrequent => Frequent.Count > 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RefreshFlags()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasFavorites)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasFrequent)));
        }
    }

    public FavoritesPanel FavoritePanel { get; } = new();

    /// <summary>常用区最多列这么多条，且不含已收藏的（否则同一页出现两遍）。</summary>
    private const int FrequentLimit = 12;

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

        LoadFavorites();
        foreach (var u in _all)
        {
            u.IsFavorite = _favorites.Contains(u.Id);
            u.FavoriteCommand = new RelayCommand(_ => ToggleFavorite(u));
        }

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
        RebuildFavorites();

        ZoomInCommand = new RelayCommand(_ => StepZoom(+1));
        ZoomOutCommand = new RelayCommand(_ => StepZoom(-1));
        ClearSearchCommand = new RelayCommand(_ => Search = string.Empty);

        foreach (var u in _all)
            u.RunCommand = new RelayCommand(_ => Run(u));

        RestoreLayouts();

        Selected = FavoritePage;
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
        RebuildFrequent();
        unit.Command?.Execute(null);
    }

    /// <summary>收藏改动少、又容易丢，所以点一下立刻落盘，不等退出。</summary>
    private void ToggleFavorite(UnitDescriptor unit)
    {
        if(!_favorites.Remove(unit.Id))
            _favorites.Add(unit.Id);
        unit.IsFavorite = _favorites.Contains(unit.Id);
        RebuildFavorites();
        SaveFavorites();
    }

    /// <summary>收藏区按清单原序排，不按收藏先后；常用区跟着一起重算。</summary>
    private void RebuildFavorites()
    {
        FavoritePanel.Favorites.Clear();
        foreach (var u in _all.Where(u => _favorites.Contains(u.Id)))
            FavoritePanel.Favorites.Add(u);
        RebuildFrequent();
    }

    /// <summary>
    /// 常用区按点击次数降序（次数相同的保持清单原序），并排掉已收藏的，
    /// 否则同一个功能会在收藏页出现两遍。
    /// 带成员的也排掉（组、多选块）：它们渲染出来是整块面板，一行顶五格，常用区就没法一眼扫完；
    /// 而且成员本身会被单独计数，等于同一件事列两遍。
    /// </summary>
    private void RebuildFrequent()
    {
        FavoritePanel.Frequent.Clear();
        foreach (var u in _all
                 .Where(u => _usage.ContainsKey(u.Id)
                             && !_favorites.Contains(u.Id)
                             && u.Members.Count == 0)
                 .OrderByDescending(u => _usage[u.Id])
                 .Take(FrequentLimit))
            FavoritePanel.Frequent.Add(u);
        FavoritePanel.RefreshFlags();
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

    private void LoadFavorites()
    {
        try
        {
            if (!File.Exists(FavoritesPath))
                return;
            foreach (var line in File.ReadAllLines(FavoritesPath))
            {
                string id = line.Trim();
                if (id.Length > 0)
                    _favorites.Add(id);
            }
        }
        catch (Exception ex)
        {
            Log.Error("收藏读取失败: " + ex);
        }
    }

    private void SaveFavorites()
    {
        try
        {
            File.WriteAllLines(FavoritesPath, _favorites);
        }
        catch (Exception ex)
        {
            Log.Error("收藏写入失败: " + ex);
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
