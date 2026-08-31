using System.Collections.ObjectModel;
using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

public class BoardViewModel:ViewModelBase
{
    private static readonly IReadOnlyDictionary<string, string> _buttonMapping = new Dictionary<string, string>
    {
        ["FREEPLANT_CHECK"] = nameof(FreePlant),
        ["BAN_SAVEGAME_CHECK"] = nameof(BanSaveGame)
    };

    private readonly IMessageProcessor _messageProcessor;
    private readonly IScriptExecutionService _scriptExec;

    public ICommand UpdateButtonStatusCommand => new RelayCommand(async _ =>
    {
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "GetButtonCheck");
    });

    private string _banSaveGame = Constants.c_Symbol_Off;

    private bool _boardColDropdownToggleIsChecked;

    private bool _boarddeltamXDropdownToggleIsChecked;

    private bool _boarddeltamYDropdownToggleIsChecked;

    private bool _boardRowDropdownToggleIsChecked;

    private bool _clearDropdownToggleIsChecked;

    private string _clearInput = "植物";

    private bool _coinDropdownToggleIsChecked;

    private string _colInput = "第1列";

    private string _ctInput = "银币";

    private string _deltamXInput = "0";

    private string _deltamYInput = "0";

    private string _freePlant = Constants.c_Symbol_Off;

    private string _imitater = Constants.c_Symbol_Off;

    private string _isSleeping = Constants.c_Symbol_Off;
    private bool _itemDropdownToggleIsChecked;

    private string _itemInput = "墓石";

    private string _itInput = Constants.c_Symbol_Off;

    private string _limitPlanting = Constants.c_Symbol_Off;
    private string _limitSeed = Constants.c_Symbol_Off;

    private string _limitSeedInput = Constants.c_Symbol_Off;

    private string _mindCtrl = Constants.c_Symbol_Off;

    private string _mindCtrlInput = Constants.c_Symbol_Off;

    private bool _plantDropdownToggleIsChecked;

    private string _rowInput = "第1行";
    private NameOption _selectedClear;

    private NameOption _selectedCoin;

    private NameOption _selectedCol;

    private NameOption _selecteddeltamX;
    private NameOption _selecteddeltamY;
    private NameOption _selectedItem;

    private NameOption _selectedPlant;

    private NameOption _selectedRow;

    private NameOption _selectedZombie;

    private string _stInput = "豌豆射手";

    private string _vaseState = "3";

    private string _vaseStateInput = "3";

    private string _vaseType = "1";

    private string _vaseTypeInput = "1";

    private bool _zombieDropdownToggleIsChecked;

    private string _ztInput = "普通僵尸";

    private string _zxInput = Constants.c_Symbol_Off;

    private string _zxPermit = Constants.c_Symbol_Off;

    public BoardViewModel(IScriptExecutionService scriptExec, IMessageProcessor messageProcessor)
    {
        _scriptExec = scriptExec;
        _messageProcessor = messageProcessor;
        if(_messageProcessor != null)
            _messageProcessor.ButtonStatusUpdated += OnButtonStatusUpdated;

        PlantOptions = OptionsLoader.Load(Constants.JsonPlantFile);
        ZombieOptions = OptionsLoader.Load(Constants.JsonZombieFile);
        CoinOptions = OptionsLoader.Load(Constants.JsonCoinFile);
        ItemOptions = OptionsLoader.Load(Constants.JsonItemFile);
        ClearOptions = OptionsLoader.Load(Constants.JsonClearFile);
        BoardRowOptions = OptionsLoader.Load(Constants.JsonRowFile);
        BoardColOptions = OptionsLoader.Load(Constants.JsonColFile);
        BoarddeltamYOptions = OptionsLoader.Load(Constants.JsondeltamYFile);
        BoarddeltamXOptions = OptionsLoader.Load(Constants.JsondeltamXFile);
    }

    public ICommand AddCoinCommand => new RelayCommand(async _ =>
    {
        string coinType = NameOption.GetValue(CTInput, CoinOptions);
        string row = NameOption.GetValue(RowInput, BoardRowOptions);
        string col = NameOption.GetValue(ColInput, BoardColOptions);
        string deltaX = NameOption.GetValue(DeltamXInput, BoarddeltamXOptions);
        string deltaY = NameOption.GetValue(DeltamYInput, BoarddeltamYOptions);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "放置物品",
            new Dictionary<string, string>
            {
                [Constants.Placeholders.Row] = row,
                [Constants.Placeholders.Col] = col,
                [Constants.Placeholders.CoinType] = coinType,
                [Constants.Placeholders.GameObjectDeltamX] = deltaX,
                [Constants.Placeholders.GameObjectDeltamY] = deltaY,
            });
    });

    public ICommand AddItemCommand => new RelayCommand(async _ =>
    {
        string item = NameOption.GetValue(ItemInput, ItemOptions);
        string seedType = NameOption.GetValue(STInput, PlantOptions);
        string zombieType = NameOption.GetValue(ZTInput, ZombieOptions);
        string row = NameOption.GetValue(RowInput, BoardRowOptions);
        string col = NameOption.GetValue(ColInput, BoardColOptions);
        string deltaX = NameOption.GetValue(DeltamXInput, BoarddeltamXOptions);
        string deltaY = NameOption.GetValue(DeltamYInput, BoarddeltamYOptions);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "放置道具",
            new Dictionary<string, string>
            {
                [Constants.Placeholders.Row] = row,
                [Constants.Placeholders.Col] = col,
                [Constants.Placeholders.Item] = item,
                ["{SCARYPOT_SEEDTYPE}"] = seedType,
                ["{SCARYPOT_ZOMBIETYPE}"] = zombieType,
                ["{SCARYPOT_SCARYPOTTYPE}"] = VaseType,
                ["{SCARYPOT_STATE}"] = VaseState,
                [Constants.Placeholders.GameObjectDeltamX] = deltaX,
                [Constants.Placeholders.GameObjectDeltamY] = deltaY,
            });
    });

    public ICommand AddPlantCommand => new RelayCommand(async _ =>
    {
        string seedType = NameOption.GetValue(STInput, PlantOptions);
        string row = NameOption.GetValue(RowInput, BoardRowOptions);
        string col = NameOption.GetValue(ColInput, BoardColOptions);
        string deltaX = NameOption.GetValue(DeltamXInput, BoarddeltamXOptions);
        string deltaY = NameOption.GetValue(DeltamYInput, BoarddeltamYOptions);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "放置植物",
            new Dictionary<string, string>
            {
                [Constants.Placeholders.Row] = row,
                [Constants.Placeholders.Col] = col,
                [Constants.Placeholders.SeedType] = seedType,
                [Constants.Placeholders.Imitater] = ButtonHelper.GetCheckValue(Imitater),
                [Constants.Placeholders.LimitPlanting] = ButtonHelper.GetCheckValue(LimitPlantingInput),
                [Constants.Placeholders.GameObjectDeltamX] = deltaX,
                [Constants.Placeholders.GameObjectDeltamY] = deltaY,
                [Constants.Placeholders.IsSleeping] = ButtonHelper.GetCheckValue(IsSleepingInput),
            });
    });

    public ICommand AddZombieCommand => new RelayCommand(async _ =>
    {
        string zombieType = NameOption.GetValue(ZTInput, ZombieOptions);
        string row = NameOption.GetValue(RowInput, BoardRowOptions);
        string col = NameOption.GetValue(ColInput, BoardColOptions);
        string deltaX = NameOption.GetValue(DeltamXInput, BoarddeltamXOptions);
        string deltaY = NameOption.GetValue(DeltamYInput, BoarddeltamYOptions);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "放置僵尸",
            new Dictionary<string, string>
            {
                [Constants.Placeholders.Row] = row,
                [Constants.Placeholders.ZombieType] = zombieType,
                [Constants.Placeholders.ColPermit] = ButtonHelper.GetCheckValue(ZXPermit),
                [Constants.Placeholders.Col] = col,
                [Constants.Placeholders.MindControl] = ButtonHelper.GetCheckValue(MindCtrl),
                [Constants.Placeholders.GameObjectDeltamX] = deltaX,
                [Constants.Placeholders.GameObjectDeltamY] = deltaY,
            });
    });

    public string BanSaveGame
    {
        get => _banSaveGame;
        set { _banSaveGame = value; OnPropertyChanged(); }
    }

    public ICommand BanSaveGameCommand => new RelayCommand(async _ =>
    {
        BanSaveGame = ButtonHelper.ToggleCheck(BanSaveGame);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "禁止存档",
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(BanSaveGame) });
    });

    public bool BoardColDropdownToggleIsChecked
    {
        get => _boardColDropdownToggleIsChecked;
        set { _boardColDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> BoardColOptions { get; }

    public bool BoarddeltamXDropdownToggleIsChecked
    {
        get => _boarddeltamXDropdownToggleIsChecked;
        set { _boarddeltamXDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> BoarddeltamXOptions { get; }

    public bool BoarddeltamYDropdownToggleIsChecked
    {
        get => _boarddeltamYDropdownToggleIsChecked;
        set { _boarddeltamYDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> BoarddeltamYOptions { get; }

    public bool BoardRowDropdownToggleIsChecked
    {
        get => _boardRowDropdownToggleIsChecked;
        set { _boardRowDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> BoardRowOptions { get; }

    public bool ClearDropdownToggleIsChecked
    {
        get => _clearDropdownToggleIsChecked;
        set { _clearDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string ClearInput
    {
        get => _clearInput;
        set { _clearInput = value; OnPropertyChanged(); }
    }

    public ICommand ClearObjectsCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "清除" + ClearInput));

    public ObservableCollection<NameOption> ClearOptions { get; }

    public bool CoinDropdownToggleIsChecked
    {
        get => _coinDropdownToggleIsChecked;
        set { _coinDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> CoinOptions { get; }

    public string ColInput
    {
        get => _colInput;
        set { _colInput = value; OnPropertyChanged(); }
    }

    public string CTInput
    {
        get => _ctInput;
        set { _ctInput = value; OnPropertyChanged(); }
    }

    public ICommand CycleItemStateCommand => new RelayCommand(_ =>
    {
        VaseStateInput = VaseStateInput switch
        {
            "3" => "4",
            "4" => "5",
            "5" => "3",
            _ => "3"
        };
    });

    public ICommand CycleVaseTypeCommand => new RelayCommand(_ =>
    {
        VaseTypeInput = VaseTypeInput switch
        {
            "1" => "2",
            "2" => "3",
            "3" => "1",
            _ => "1"
        };
    });

    public string DeltamXInput
    {
        get => _deltamXInput;
        set { _deltamXInput = value; OnPropertyChanged(); }
    }

    public string DeltamYInput
    {
        get => _deltamYInput;
        set { _deltamYInput = value; OnPropertyChanged(); }
    }

    public ICommand DeMowerCommand => new RelayCommand(async _ =>
                        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "小推车",
            new Dictionary<string, string> { ["{DE}"] = "1" }));

    public ICommand EasyPlantingCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "EasyPlanting"));

    public string FreePlant
    {
        get => _freePlant;
        set { _freePlant = value; OnPropertyChanged(); }
    }

    public ICommand FreePlantCommand => new RelayCommand(async _ =>
    {
        FreePlant = ButtonHelper.ToggleCheck(FreePlant);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "自由种植",
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(FreePlant) });
    });

    public string Imitater
    {
        get => _imitater;
        set { _imitater = value; OnPropertyChanged(); }
    }

    public string IsSleepingInput
    {
        get => _isSleeping;
        set { _isSleeping = value; OnPropertyChanged(); }
    }

    public bool ItemDropdownToggleIsChecked
    {
        get => _itemDropdownToggleIsChecked;
        set { _itemDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string ItemInput
    {
        get => _itemInput;
        set { _itemInput = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> ItemOptions { get; }

    public string ItInput
    {
        get => _itInput;
        set { _itInput = value; OnPropertyChanged(); }
    }

    public string LimitPlantingInput
    {
        get => _limitPlanting;
        set { _limitPlanting = value; OnPropertyChanged(); }
    }

    public string LimitSeed
    {
        get => _limitSeed;
        set { _limitSeed = value; OnPropertyChanged(); }
    }

    public ICommand LimitSeedCommand => new RelayCommand(_ => LimitSeed = ButtonHelper.ToggleCheck(LimitSeed));

    public string LimitSeedInput
    {
        get => _limitSeedInput;
        set { _limitSeedInput = value; OnPropertyChanged(); }
    }

    public ICommand LoadGameCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "立即回档"));

    public string MindCtrl
    {
        get => _mindCtrl;
        set { _mindCtrl = value; OnPropertyChanged(); }
    }

    public string MindCtrlInput
    {
        get => _mindCtrlInput;
        set { _mindCtrlInput = value; OnPropertyChanged(); }
    }

    public bool PlantDropdownToggleIsChecked
    {
        get => _plantDropdownToggleIsChecked;
        set { _plantDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> PlantOptions { get; }

    public ICommand ReMowerCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "小推车",
            new Dictionary<string, string> { ["{RE}"] = "1" }));

    public string RowInput
    {
        get => _rowInput;
        set { _rowInput = value; OnPropertyChanged(); }
    }

    public ICommand RunMowerCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "小推车",
            new Dictionary<string, string> { ["{RUN}"] = "1" }));

    public ICommand SaveGameCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "立即存档"));

    public NameOption SelectedClear
    {
        get => _selectedClear;
        set
        {
            _selectedClear = value;

            if(value != null)
                ClearInput = value.Name;
            ClearDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public NameOption SelectedCoin
    {
        get => _selectedCoin;
        set
        {
            _selectedCoin = value;

            if(value != null)
                CTInput = value.Name;
            CoinDropdownToggleIsChecked = false;
            OnPropertyChanged();
        }
    }

    public NameOption SelectedCol
    {
        get => _selectedCol;
        set
        {
            _selectedCol = value;
            if(value != null)
                ColInput = value.Name;
            BoardColDropdownToggleIsChecked = false;
            OnPropertyChanged();
        }
    }

    public NameOption SelecteddeltamX
    {
        get => _selecteddeltamX;
        set
        {
            _selecteddeltamX = value;

            if(value != null)
                DeltamXInput = value.Name;
            BoarddeltamXDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public NameOption SelecteddeltamY
    {
        get => _selecteddeltamY;
        set
        {
            _selecteddeltamY = value;

            if(value != null)
                DeltamYInput = value.Name;
            BoarddeltamYDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public NameOption SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;

            if(value != null)
                ItemInput = value.Name;
            ItemDropdownToggleIsChecked = false;
            OnPropertyChanged();
        }
    }

    public NameOption SelectedPlant
    {
        get => _selectedPlant;
        set
        {
            _selectedPlant = value;
            OnPropertyChanged();
            if(value != null)
                STInput = value.Name;
            PlantDropdownToggleIsChecked = false;
        }
    }

    public NameOption SelectedRow
    {
        get => _selectedRow;
        set
        {
            _selectedRow = value;
            if(value != null)
                RowInput = value.Name;
            BoardRowDropdownToggleIsChecked = false;
            OnPropertyChanged();
        }
    }

    public NameOption SelectedZombie
    {
        get => _selectedZombie;
        set
        {
            _selectedZombie = value;

            if(value != null)
                ZTInput = value.Name;
            ZombieDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public ICommand SetLadderCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Board, "一键搭梯",
            new Dictionary<string, string> { [Constants.Placeholders.Check] = ButtonHelper.GetCheckValue(LimitSeed) }));

    public string STInput
    {
        get => _stInput;
        set { _stInput = value; OnPropertyChanged(); }
    }

    public ICommand ToggleImitaterCommand => new RelayCommand(_ => Imitater = ButtonHelper.ToggleCheck(Imitater));

    public ICommand ToggleIsSleepingCommand => new RelayCommand(_ => IsSleepingInput = ButtonHelper.ToggleCheck(IsSleepingInput));

    public ICommand ToggleLimitPlantingCommand => new RelayCommand(_ => LimitPlantingInput = ButtonHelper.ToggleCheck(LimitPlantingInput));

    public ICommand ToggleMindCtrlCommand => new RelayCommand(_ => MindCtrl = ButtonHelper.ToggleCheck(MindCtrl));

    public ICommand ToggleZXPermitCommand => new RelayCommand(_ => ZXPermit = ButtonHelper.ToggleCheck(ZXPermit));

    public string VaseState
    {
        get => _vaseState;
        set { _vaseState = value; OnPropertyChanged(); }
    }

    public ICommand VaseStateCycleCommand => new RelayCommand(_ =>
    {
        VaseState = VaseState switch
        {
            "3" => "4",
            "4" => "5",
            "5" => "3",
            _ => "3"
        };
    });

    public string VaseStateInput
    {
        get => _vaseStateInput;
        set { _vaseStateInput = value; OnPropertyChanged(); }
    }

    public string VaseType
    {
        get => _vaseType;
        set { _vaseType = value; OnPropertyChanged(); }
    }

    public ICommand VaseTypeCycleCommand => new RelayCommand(_ =>
    {
        VaseType = VaseType switch
        {
            "1" => "2",
            "2" => "3",
            "3" => "1",
            _ => "1"
        };
    });

    public string VaseTypeInput
    {
        get => _vaseTypeInput;
        set { _vaseTypeInput = value; OnPropertyChanged(); }
    }

    public bool ZombieDropdownToggleIsChecked
    {
        get => _zombieDropdownToggleIsChecked;
        set { _zombieDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> ZombieOptions { get; }

    public string ZTInput
    {
        get => _ztInput;
        set { _ztInput = value; OnPropertyChanged(); }
    }

    public string ZxInput
    {
        get => _zxInput;
        set { _zxInput = value; OnPropertyChanged(); }
    }

    public string ZXPermit
    {
        get => _zxPermit;
        set { _zxPermit = value; OnPropertyChanged(); }
    }

    private void OnButtonStatusUpdated(Dictionary<string, bool> statusDict)
    {
        UpdatePropertiesFromDict(statusDict, _buttonMapping);
    }
}
