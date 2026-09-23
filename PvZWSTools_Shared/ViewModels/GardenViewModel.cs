using System.Windows.Input;
using PvZWSTools_Shared;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;
using PvZWSTools_Shared.UiModel;

namespace PvZWSTools_Shared.ViewModels;

public class GardenViewModel:ViewModelBase
{
    private readonly IScriptExecutionService _scriptExec;
    private readonly IConnectionService _connection;
    private readonly IDialogService _dialogService;

    private int _selectedTabIndex;
    private static readonly IReadOnlyDictionary<string, string> _buttonMapping = new Dictionary<string, string>();

    private readonly IMessageProcessor _messageProcessor;

    /// <summary>(花园类型, mX, mY) -> 格子。回读的那一行只报这三样，靠它落到具体格子上。</summary>
    private readonly Dictionary<(int GardenType, int X, int Y), GardenCell> _bySpot = new();

    private Dictionary<string, string>? _plantNames;

    /// <summary>游戏自己报的名字表（Plant.GetNameString），显示时优先于 选项/植物.json。</summary>
    private readonly Dictionary<string, string> _gameNames = new();

    /// <summary>六个花园页签，顺序就是页签顺序。背景图与格位都在 GardenLayout 里按源码算好。</summary>
    public IReadOnlyList<GardenTab> Tabs { get; } = GardenLayout.Build();

    private void OnButtonStatusUpdated(Dictionary<string, bool> statusDict)
    {
        UpdatePropertiesFromDict(statusDict, _buttonMapping);
    }

    public GardenViewModel(IScriptExecutionService scriptExec,
        IConnectionService connection,
        IDialogService dialogService,
        IMessageProcessor messageProcessor)
    {
        _scriptExec = scriptExec;
        _connection = connection;
        _dialogService = dialogService;
        _messageProcessor = messageProcessor;

        foreach(var tab in Tabs)
            foreach(var cell in tab.Cells)
                _bySpot[((int)tab.Kind, cell.GridX, cell.GridY)] = cell;

        if(_messageProcessor != null)
        {
            _messageProcessor.ButtonStatusUpdated += OnButtonStatusUpdated;
            _messageProcessor.OutputReceived += OnGameOutput;
        }

        _selectedTabIndex = 0;
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if(_selectedTabIndex != value)
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }
    }

    private GardenTab Current => Tabs[_selectedTabIndex];

    /// <summary>点一格：弹编辑框，确认后按框里的选择下发。</summary>
    public ICommand GardenButtonCommand => new RelayCommand(param =>
    {
        if(param is GardenCell cell)
            _ = EditCellAsync(cell);
    });

    /// <summary>一次性把游戏里的盆栽读回来。改动之后也会自动走一次，见 EditCellAsync。</summary>
    public ICommand ReadGardenCommand => new RelayCommand(_ => _ = ReadGardenAsync());

    /// <summary>切到花园页时由页面自己调一次。没连上就安静地跳过——
    /// 每进一次页签就往日志里塞一条"WebSocket未连接"，会把真正的报错淹掉。</summary>
    public void ReadGardenOnOpen()
    {
        if(_connection == null || !_connection.IsConnected) return;
        _ = ReadGardenAsync();
    }

    private async Task ReadGardenAsync() =>
        await _connection.SendAsync(Sharedstring.GardenQueryText);

    private async Task EditCellAsync(GardenCell cell)
    {
        var vm = new GardenDialogViewModel(cell, DisplayNames());
        bool confirmed = await _dialogService.ShowDialogAsync(vm);
        if(!confirmed) return;

        if(vm.RemoveRequested)
        {
            await _connection.SendAsync(SpotText(Sharedstring.GardenClearText, cell));
        }
        else
        {
            // 认不出的植物名会拼出 "SeedType."，那是 Python 语法错误：宁可不发。
            if(string.IsNullOrEmpty(vm.SelectedSeedTypeValue))
            {
                Log.Error($"花园编辑：植物「{vm.SelectedSeedTypeName}」在 选项/{Constants.JsonPlantFile} 里没有对应项，未下发");
                return;
            }

            await _connection.SendAsync(SpotText(Sharedstring.GardenChangeText, cell)
                .Replace("{mSeedType}", vm.SelectedSeedTypeValue)
                .Replace("{mFacing}", vm.SelectedFacingValue.ToString())
                .Replace("{mPlantAge}", vm.SelectedAgeValue.ToString())
                .Replace("{mNeed}", vm.SelectedNeedValue.ToString()));
        }

        // 下发完回读一遍：界面上摆的永远是游戏里的真值。槽位满、那一格本来就没有盆栽
        // 这类"脚本自己放弃了"的情况，才不会在界面上显示成改成功了。
        await ReadGardenAsync();
    }

    private string SpotText(string script, GardenCell cell) =>
        script.Replace("{mGardenType}", ((int)Current.Kind).ToString())
            .Replace("{mX}", cell.GridX.ToString())
            .Replace("{mY}", cell.GridY.ToString());

    /// <summary>只认自家那对标记：别的脚本也往 stdout 打东西，不加这道闸就会把别人的输出吃进来。</summary>
    private void OnGameOutput(string msg)
    {
        string? names = Between(msg, Constants.Markers.GardenNamesStart, Constants.Markers.GardenNamesEnd);
        if(names != null) ApplyNameTable(names);

        string? list = Between(msg, Constants.Markers.GardenListStart, Constants.Markers.GardenListEnd);
        if(list != null) ApplyGardenList(list);
    }

    private static string? Between(string msg, string start, string end)
    {
        int i = msg.IndexOf(start, StringComparison.Ordinal);
        int j = msg.IndexOf(end, StringComparison.Ordinal);
        return i < 0 || j <= i ? null : msg.Substring(i + start.Length, j - i - start.Length);
    }

    /// <summary>一行 "枚举名,游戏里的显示名"。名字以游戏为准：工具那份 选项/植物.json
    /// 有 24 条和游戏的字符串表对不上，界面上摆的必须是玩家在游戏里看到的那三个字。</summary>
    private void ApplyNameTable(string block)
    {
        foreach(var line in block.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var f = line.Trim().Split(',', 2);
            if(f.Length < 2 || f[0].Length == 0 || f[1].Length == 0) continue;
            // 当前语言的 LawnStrings 里没有这个键时，游戏回的是 "<Missing KEY>"
            // （TodStringFile.cs:276），模组自加的融合植物最容易踩到。这种值不上界面，
            // 让它落到 选项/植物.json 那份名字上。
            if(f[1].StartsWith("<Missing ", StringComparison.Ordinal)) continue;
            _gameNames[f[0]] = f[1];
        }
    }

    /// <summary>一行八个逗号分隔字段：花园类型,mX,mY,植物枚举名,朝向,年龄,需求,已喂食次数。
    /// 见 Scripts\GardenQueryText.py。</summary>
    private void ApplyGardenList(string block)
    {
        _plantNames ??= LoadPlantNames();
        var hit = new HashSet<GardenCell>();

        foreach(var line in block.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var f = line.Trim().Split(',');
            if(f.Length < 8) continue;
            if(!int.TryParse(f[0], out int gardenType) || !int.TryParse(f[1], out int x) || !int.TryParse(f[2], out int y))
                continue;
            if(!int.TryParse(f[4], out int facing) || !int.TryParse(f[5], out int age)) continue;
            if(!int.TryParse(f[6], out int need) || !int.TryParse(f[7], out int timesFed)) continue;
            if(!_bySpot.TryGetValue((gardenType, x, y), out GardenCell? cell)) continue;

            cell.SetPlant(f[3], PlantDisplayName(f[3]), facing, age, need, timesFed);
            hit.Add(cell);
        }

        // 这份列表里没出现的格子就是空的：整张图要刷成游戏现在的样子，
        // 只更新有盆栽的格子会让删掉的那格一直挂着旧名字。
        foreach(var tab in Tabs)
            foreach(var cell in tab.Cells)
                if(!hit.Contains(cell))
                    cell.SetPlant(null, "", 0, 0, 0, 0);
    }

    private string PlantDisplayName(string seedType) => DisplayName(seedType, _plantNames);

    /// <summary>游戏报过就用游戏的；没报过（还没读、或那一株游戏侧没有定义）退回 选项/植物.json；
    /// 两边都没有就直接摆枚举名，至少看得出"这格有东西"，别显示成空格。</summary>
    private string DisplayName(string seedType, Dictionary<string, string>? fromOptions)
    {
        if(_gameNames.TryGetValue(seedType, out string? game) && !string.IsNullOrEmpty(game)) return game;
        return fromOptions != null && fromOptions.TryGetValue(seedType, out string? name) ? name : seedType;
    }

    /// <summary>给编辑对话框用的 "枚举名 -> 该显示什么"，游戏名字优先。</summary>
    private Dictionary<string, string> DisplayNames()
    {
        _plantNames ??= LoadPlantNames();
        var merged = new Dictionary<string, string>(_plantNames);
        foreach(var kvp in _gameNames)
            merged[kvp.Key] = kvp.Value;
        return merged;
    }

    private static Dictionary<string, string> LoadPlantNames()
    {
        var map = new Dictionary<string, string>();
        foreach(var opt in OptionsLoader.Load(Constants.JsonPlantFile))
            if(!string.IsNullOrEmpty(opt.Value) && !string.IsNullOrEmpty(opt.Name))
                map[opt.Value] = opt.Name;
        return map;
    }
}
