using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_Shared.UiModel;

/// <summary>收藏页的那两块内容：常用区 + 收藏区。都是实时集合，视图不用重建绑定。
///
/// 空状态提示走 <see cref="HasFavorites"/>/<see cref="HasFrequent"/> 而不是直接绑 Count ——
/// ObservableCollection 只在内容变化时发 CollectionChanged，不发 PropertyChanged(Count)，
/// 绑 Count 的触发器不会跟着刷新。</summary>
public sealed class FavoritesPanel : INotifyPropertyChanged
{
    public ObservableCollection<UnitDescriptor> Favorites { get; } = new();
    public ObservableCollection<UnitDescriptor> Frequent { get; } = new();

    public bool HasFavorites => Favorites.Count > 0;
    public bool HasFrequent => Frequent.Count > 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshFlags()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasFavorites)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasFrequent)));
    }
}

/// <summary>收藏 + 常用统计，两端共用的一份状态：WPF 的收藏页和安卓的收藏页读同一个对象，
/// 谁点的都记在同一份 ui_usage.cfg 里。配置根由构造参数传（WPF 是 exe 目录，
/// 安卓是 GetExternalFilesDir），所以这里不碰工作目录。</summary>
public sealed class FavoriteStore
{
    /// <summary>常用区只列这么几条。故意留小：收藏一多就把常用挤出屏幕，
    /// 而常用要看的就是"我最近反复点的那几个"。</summary>
    public const int FrequentLimit = 5;

    private readonly string _favoritesPath;
    private readonly string _usagePath;
    private readonly Dictionary<string, int> _usage = new();
    private readonly HashSet<string> _favorites = new();
    private IReadOnlyList<UnitDescriptor> _all = Array.Empty<UnitDescriptor>();

    public FavoriteStore(string baseDir)
    {
        _favoritesPath = Path.Combine(baseDir, "ui_favorites.cfg");
        _usagePath = Path.Combine(baseDir, "ui_usage.cfg");
        LoadUsage();
    }

    public FavoritesPanel Panel { get; } = new();

    /// <summary>摊平后的全部单元（含组和多选块里的成员）——搜索、收藏、常用都在这份上面算。</summary>
    public IReadOnlyList<UnitDescriptor> All => _all;

    /// <summary>喂各页的顶层单元：组里的成员也要能被搜到、能记常用、能收藏，所以在这里摊成一维，
    /// 再给每个单元挂上星标和"点了就记一次常用"的命令。</summary>
    public void Attach(IEnumerable<UnitDescriptor> pageUnits)
    {
        _all = Flatten(pageUnits).ToList();
        LoadFavorites();
        foreach(var u in _all)
        {
            u.IsFavorite = _favorites.Contains(u.Id);
            u.FavoriteCommand = new RelayCommand(_ => ToggleFavorite(u));
            u.RunCommand = new RelayCommand(_ => Run(u));
        }
        RebuildFavorites();
    }

    private static IEnumerable<UnitDescriptor> Flatten(IEnumerable<UnitDescriptor> units)
    {
        foreach(var u in units)
        {
            yield return u;
            if(u.Members.Count > 0)
                foreach(var m in Flatten(u.Members))
                    yield return m;
        }
    }

    /// <summary>执行 + 记一次常用。两端的功能行都走这里，不然安卓点的在 WPF 的常用区里没影子。</summary>
    public void Run(UnitDescriptor unit)
    {
        _usage[unit.Id] = _usage.TryGetValue(unit.Id, out int n) ? n + 1 : 1;
        RebuildFrequent();
        unit.Command?.Execute(null);
    }

    /// <summary>收藏改动少、又容易丢，所以点一下立刻落盘，不等退出。</summary>
    public void ToggleFavorite(UnitDescriptor unit)
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
        Panel.Favorites.Clear();
        foreach(var u in _all.Where(u => _favorites.Contains(u.Id)))
            Panel.Favorites.Add(u);
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
        Panel.Frequent.Clear();
        foreach(var u in _all
                .Where(u => _usage.ContainsKey(u.Id)
                            && !_favorites.Contains(u.Id)
                            && u.Members.Count == 0)
                .OrderByDescending(u => _usage[u.Id])
                .Take(FrequentLimit))
            Panel.Frequent.Add(u);
        Panel.RefreshFlags();
    }

    public void SaveUsage()
    {
        try
        {
            File.WriteAllLines(_usagePath, _usage.Select(kv => kv.Key + "=" + kv.Value));
        }
        catch(Exception ex)
        {
            Log.Error("常用统计写入失败: " + ex);
        }
    }

    private void LoadUsage()
    {
        try
        {
            if(!File.Exists(_usagePath)) return;
            foreach(var line in File.ReadAllLines(_usagePath))
            {
                int i = line.IndexOf('=');
                if(i <= 0 || !int.TryParse(line[(i + 1)..], out int n)) continue;
                _usage[line[..i]] = n;
            }
        }
        catch(Exception ex)
        {
            Log.Error("常用统计读取失败: " + ex);
        }
    }

    private void LoadFavorites()
    {
        try
        {
            if(!File.Exists(_favoritesPath)) return;
            foreach(var line in File.ReadAllLines(_favoritesPath))
            {
                string id = line.Trim();
                if(id.Length > 0) _favorites.Add(id);
            }
        }
        catch(Exception ex)
        {
            Log.Error("收藏读取失败: " + ex);
        }
    }

    private void SaveFavorites()
    {
        try
        {
            File.WriteAllLines(_favoritesPath, _favorites);
        }
        catch(Exception ex)
        {
            Log.Error("收藏写入失败: " + ex);
        }
    }
}
