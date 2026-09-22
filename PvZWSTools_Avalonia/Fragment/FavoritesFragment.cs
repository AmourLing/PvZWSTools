using Android.Content;
using PvZWSTools_Avalonia.Platform;
using PvZWSTools_Shared.UiModel;

namespace PvZWSTools_Avalonia;

/// <summary>
/// 收藏页：常用在前（只留几条），收藏在后。行的画法全在 <see cref="CatalogFragment"/>，
/// 这里只回答"这一页要画哪些行"。
///
/// 和桌面端不一样，这一页是切进来时现建的视图，所以从别的页长按收藏之后回到这里能看到新内容，
/// 而在本页长按只影响整块视图（行的 ★ 就地改，集合要下次切进来才重排）。
/// </summary>
public class FavoritesFragment:CatalogFragment
{
    public FavoritesFragment():base("收藏") { }

    protected override IEnumerable<(string? Header, UnitDescriptor Unit)> CollectUnits(Context ctx)
    {
        var panel = AppServices.Favorites.Panel;
        foreach(var unit in panel.Frequent)
            yield return ("常用", unit);
        foreach(var unit in panel.Favorites)
            yield return ("收藏", unit);
    }

    protected override string EmptyHint =>
        "还没有收藏，也还没点过任何功能。在任意一页的功能上长按即可收藏。";
}
