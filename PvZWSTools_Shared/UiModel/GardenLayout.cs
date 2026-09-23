using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.ViewModels;

namespace PvZWSTools_Shared.UiModel;

/// <summary>对应游戏里的 Lawn.GardenType（LawnDLL/Lawn/Lawn/GardenType.cs）。
/// Wheelbarrow 只是手持盆栽的槽位、不是花园，所以这里没有它；
/// 脚本按数字下发，别改顺序值。</summary>
public enum GardenKind
{
    Main = 0,
    Mushroom = 1,
    Aquarium = 3,
    Main2 = 4,
    Mushroom2 = 5,
    Night = 6,
}

/// <summary>花园里的一格热区。X/Y/W/H 是画布像素，画布就是游戏那张背景图本身（1400x600，
/// 换算见 GardenLayout），所以数值 = 游戏算出来的格位像素 + 285。
/// 植物那一半是回读出来的游戏状态，会随"读取花园"变。</summary>
public sealed class GardenCell:ViewModelBase
{
    private string? _plant;
    private string? _seedType;
    private int _facing;
    private int _age;
    private int _need;
    private int _timesFed;

    public GardenCell(int gridX, int gridY, int x, int y)
    {
        GridX = gridX;
        GridY = gridY;
        X = x;
        Y = y;
    }

    /// <summary>(行,列)，行就是 mY、列就是 mX，都从 1 起。</summary>
    public string Label => $"({GridY + 1},{GridX + 1})";

    public int GridX { get; }

    public int GridY { get; }

    public int X { get; }

    public int Y { get; }

    public int W => GardenLayout.CellWidth;

    public int H => GardenLayout.CellHeight;

    /// <summary>回读到的植物枚举名（SeedType 的成员名），没种就是 null。
    /// 下发时要的是它、不是界面上那个名字，别拿 Caption 去拼脚本。</summary>
    public string? SeedType => _seedType;

    public int Facing => _facing;

    public int Age => _age;

    /// <summary>PottedPlantNeed：这一株正在等什么（无/浇水/施肥/杀虫剂/放音乐）。</summary>
    public int Need => _need;

    /// <summary>已经喂过几次肥，和 FeedingsPerGrow 一起决定它长不长个。</summary>
    public int TimesFed => _timesFed;

    public bool IsOccupied => _plant != null;

    /// <summary>格子上显示的字：空着显示格号，种上了显示植物名。</summary>
    public string Caption => _plant ?? Label;

    public string Tip => _plant == null
        ? $"{Loc.T("空格")} {Label}"
        : $"{Label} · {_plant} · {GardenLayout.Name(GardenLayout.FacingNames, _facing)}" +
          $" · {GardenLayout.Name(GardenLayout.AgeNames, _age)}" +
          $" · {GardenLayout.Name(GardenLayout.NeedNames, _need)}" +
          $" · {Loc.T("已喂")} {_timesFed}";

    /// <summary>填这一格的回读结果；displayName 是游戏自己报的名字（见 Scripts\GardenQueryText.py）。</summary>
    public void SetPlant(string? seedType, string displayName, int facing, int age, int need, int timesFed)
    {
        _seedType = seedType;
        _plant = string.IsNullOrEmpty(displayName) ? null : displayName;
        _facing = facing;
        _age = age;
        _need = need;
        _timesFed = timesFed;
        OnPropertyChanged(nameof(IsOccupied));
        OnPropertyChanged(nameof(Caption));
        OnPropertyChanged(nameof(Tip));
        OnPropertyChanged(nameof(SeedType));
        OnPropertyChanged(nameof(Need));
        OnPropertyChanged(nameof(TimesFed));
    }
}

/// <summary>一个花园页签：页签名、背景图、下发给脚本的 GardenType 值、格位。</summary>
public sealed class GardenTab
{
    public required string Header { get; init; }

    /// <summary>背景图的 pack URI。图以 Resource 编进程序集，且只在 Debug 配置下编：
    /// 花园页本身 Release 就不进导航（ShellViewModel 里的 #if !DEBUG），
    /// 没必要让发布包多背这几百 KiB。</summary>
    public required string Backdrop { get; init; }

    public required GardenKind Kind { get; init; }

    public double CanvasWidth => GardenLayout.CanvasWidth;

    public double CanvasHeight => GardenLayout.CanvasHeight;

    public required IReadOnlyList<GardenCell> Cells { get; init; }

    /// <summary>格子铺开部分的水平中心（图上像素）。整张 1400 宽的背景在窄窗口里放不下，
    /// 花园页据此决定初始往右滚多少，让格子正好看在中间。</summary>
    public double ContentCenterX
    {
        get
        {
            if(Cells.Count == 0) return GardenLayout.CanvasWidth / 2;
            double left = double.MaxValue, right = double.MinValue;
            foreach(var cell in Cells)
            {
                if(cell.X < left) left = cell.X;
                if(cell.X + cell.W > right) right = cell.X + cell.W;
            }

            return (left + right) / 2;
        }
    }
}

/// <summary>
/// 花园格位表。数值全部来自反编译源码，改之前先去核对源码，别照着背景图目测调：
///   温室/夜温室 32 格：LawnDLL/Lawn/Lawn/ZenGarden.cs:142-176，
///                      再乘 x*1.125 / y*1.182 并加 (-30, +30)（ZenGarden.cs:188-192，
///                      系数在 Constants.cs:1958-1960）
///   蘑菇园 8 格：      Constants.cs:1998-2008，再加 (+12, +37)（Constants.cs:1961 的 ISUI*10 / ISUI*30）
///   水族馆 8 格：      ZenGarden.cs:177-187，不偏移
///   每格 80x85：       ZenGarden.cs:1069 的命中判定
/// 背景图原生 1680x720、resources.xml 里 invscale=1.2，所以游戏按 1400x600 画，
/// 画在世界 x=-285 处（Board.cs:5111-5170 的 -133*1.25 再减 ZenGarden_Backdrop_X=119），
/// 于是"世界格位像素 + 285"就是图上像素。画布用整张图、不裁剪，窗口放不下时
/// 由花园页左右滚动，初始停在格子的中心上（见 GardenTab.ContentCenterX）。
/// 想核对格位有没有压在架子/土坑上：UI核对\gen_garden_bg.py --preview 会把框画到整图上。
/// </summary>
public static class GardenLayout
{
    /// <summary>画布 = 背景图按 invscale=1.2 画出来的尺寸，三座花园都一样。</summary>
    public const double CanvasWidth = 1400;

    public const double CanvasHeight = 600;

    public const int CellWidth = 80;

    public const int CellHeight = 85;

    /// <summary>背景画在世界坐标 x=-285 处，所以 世界坐标 + 285 = 图上像素。</summary>
    private const int BackdropX = 285;

    /// <summary>朝向与年龄的界面文案，数组下标就是下发给脚本的数字（游戏里的
    /// PottedPlant.FacingDirection{Right=0,Left=1} 与 PottedPlantAge{Sprout,Small,Medium,Full}）。
    /// 花园页的 ToolTip 和编辑对话框的下拉共用这一份，免得两边各写一套对不上。</summary>
    public static readonly string[] FacingNames = ["右", "左"];

    public static readonly string[] AgeNames = ["幼苗", "小", "中", "大"];

    /// <summary>PottedPlantNeed{None,Water,Fertilizer,Bugspray,Phonograph} 的文案。
    /// 后四个直接用游戏给那四件园艺工具的短名（LawnStrings_zh_cn.txt 的
    /// WATERING_CAN_TOOLTIP=浇水、FERTILIZER_TOOLTIP=施肥、BUG_SPRAY_TOOLTIP=杀虫剂、
    /// PHONOGRAPH_TOOLTIP=放音乐），不自己另起一套。
    /// 回读报的是 ZenGarden.GetPlantsNeed 现算出来的那个（玩家看到的就是它），不是存的字段。</summary>
    public static readonly string[] NeedNames = ["无", "浇水", "施肥", "杀虫剂", "放音乐"];

    /// <summary>游戏回来的数字未必在表里（版本差异、脏存档），越界给个占位符而不是抛。</summary>
    public static string Name(string[] names, int index) =>
        index >= 0 && index < names.Length ? names[index] : "?";

    /// <summary>背景图所在处。写死程序集名，URI 就不依赖谁是入口程序集。</summary>
    private const string BackdropDir = "pack://application:,,,/PvZWSTools;component/Resources/garden/";

    private static readonly (int X, int Y)[] _greenhouseRaw =
    {
        (73, 73), (155, 71), (239, 68), (321, 73), (406, 71), (484, 67), (566, 70), (648, 72),
        (67, 168), (150, 165), (232, 170), (314, 175), (416, 173), (497, 170), (578, 164), (660, 168),
        (41, 268), (130, 266), (219, 260), (310, 266), (416, 267), (504, 261), (594, 265), (684, 269),
        (37, 371), (124, 369), (211, 368), (302, 369), (425, 375), (512, 368), (602, 365), (691, 368),
    };

    private static readonly (int X, int Y)[] _mushroomRaw =
    {
        (80, 435), (220, 360), (290, 458), (355, 296),
        (387, 203), (470, 380), (500, 472), (580, 283),
    };

    private static readonly (int X, int Y)[] _aquariumRaw =
    {
        (113, 185), (306, 120), (356, 270), (622, 120),
        (669, 270), (122, 355), (365, 458), (504, 417),
    };

    public static IReadOnlyList<GardenTab> Build() =>
    [
        Tab("主花园1", "Background_Greenhouse", GardenKind.Main, Greenhouse()),
        // 主花园2 与蘑菇园2 在游戏里就是同一张背景、同一套格位（ZenGarden.cs:820-836）
        Tab("主花园2", "Background_Greenhouse", GardenKind.Main2, Greenhouse()),
        Tab("主花园(夜)", "Background_Greenhouse_Night", GardenKind.Night, Greenhouse()),
        Tab("蘑菇园1", "Background_MushroomGarden", GardenKind.Mushroom, Mushroom()),
        Tab("蘑菇园2", "Background_MushroomGarden", GardenKind.Mushroom2, Mushroom()),
        Tab("水族馆", "aquarium1", GardenKind.Aquarium, Aquarium()),
    ];

    private static GardenTab Tab(string header, string backdrop, GardenKind kind,
        IReadOnlyList<GardenCell> cells) =>
        new GardenTab
        {
            Header = Loc.T(header),
            Backdrop = $"{BackdropDir}{backdrop}.jpg",
            Kind = kind,
            Cells = cells,
        };

    /// <summary>源码表里的像素 -> 世界坐标 -> 图上像素（= 画布坐标）。</summary>
    private static GardenCell Spot(int gridX, int gridY, int worldX, int worldY) =>
        new(gridX, gridY, worldX + BackdropX, worldY);

    private static IReadOnlyList<GardenCell> Greenhouse()
    {
        var cells = new GardenCell[_greenhouseRaw.Length];
        for(var i = 0; i < _greenhouseRaw.Length; i++)
        {
            var (x, y) = _greenhouseRaw[i];
            cells[i] = Spot(i % 8, i / 8, (int)(x * 1.125f) - 30, (int)(y * 1.182f) + 30);
        }

        return cells;
    }

    private static IReadOnlyList<GardenCell> Mushroom()
    {
        var cells = new GardenCell[_mushroomRaw.Length];
        for(var i = 0; i < _mushroomRaw.Length; i++)
        {
            var (x, y) = _mushroomRaw[i];
            cells[i] = Spot(i, 0, x + 12, y + 37);
        }

        return cells;
    }

    private static IReadOnlyList<GardenCell> Aquarium()
    {
        var cells = new GardenCell[_aquariumRaw.Length];
        for(var i = 0; i < _aquariumRaw.Length; i++)
        {
            var (x, y) = _aquariumRaw[i];
            cells[i] = Spot(i, 0, x, y);
        }

        return cells;
    }
}
