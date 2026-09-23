using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.UiModel;

namespace PvZWSTools_Shared.ViewModels;

public class GardenDialogViewModel:ViewModelBase
{
    private int _row;
    private int _col;
    private string _locationText;

    public GardenDialogViewModel()
    {
        LoadOptions();
        SetDefaultSelections();
        OkCommand = new RelayCommand(_ => Ok());
        CancelCommand = new RelayCommand(_ => Cancel());
        RemoveCommand = new RelayCommand(_ =>
        {
            RemoveRequested = true;
            Ok();
        });
    }

    /// <summary>从花园页的一格热区进来：位置写清楚，格子里本来种着什么就把三项先摆成那样，
    /// 这样"改一点点"不用重新选一遍。displayNames 是 "枚举名 -> 游戏里显示的名字"。</summary>
    public GardenDialogViewModel(GardenCell cell, IReadOnlyDictionary<string, string>? displayNames = null):this()
    {
        ApplyDisplayNames(displayNames);
        SetDefaultSelections(); // 名字可能被换成游戏那份了，默认项要重新按 Value 认一次
        Row = cell.GridY + 1;
        Col = cell.GridX + 1;
        CanRemove = cell.IsOccupied;
        if(cell.SeedType == null) return;

        // 回读到的枚举名在 选项/植物.json 里根本没有这一株时补一项进去，
        // 否则确定按钮会因为认不出名字而什么都发不出去。
        if(!SeedTypeOptions.Any(o => o.Value == cell.SeedType))
            SeedTypeOptions.Add(new NameOption { Name = cell.SeedType, Value = cell.SeedType });

        SelectedSeedTypeName = SeedTypeOptions.First(o => o.Value == cell.SeedType).Name;
        SelectedFacingDisplay = Display(cell.Facing, GardenLayout.FacingNames);
        SelectedAgeDisplay = Display(cell.Age, GardenLayout.AgeNames);
        SelectedNeedDisplay = Display(cell.Need, GardenLayout.NeedNames);
    }

    /// <summary>把下拉里的中文名换成游戏报的那一份（按 Value 匹配）。
    /// 不换的话格子上写"香蒲"、下拉里却是"猫尾草"，同一株两个名字。</summary>
    private void ApplyDisplayNames(IReadOnlyDictionary<string, string>? displayNames)
    {
        if(displayNames == null) return;
        foreach(var opt in SeedTypeOptions)
            if(displayNames.TryGetValue(opt.Value, out string? name) && !string.IsNullOrEmpty(name))
                opt.Name = name;
    }

    /// <summary>点「移除」关的窗。花园页据此决定下发清空还是修改。</summary>
    public bool RemoveRequested { get; private set; }

    /// <summary>空格子没什么可移除，按钮直接灰着。</summary>
    public bool CanRemove { get; }

    public int Row
    {
        get => _row;
        set { _row = value; OnPropertyChanged(); UpdateLocationText(); }
    }

    public int Col
    {
        get => _col;
        set { _col = value; OnPropertyChanged(); UpdateLocationText(); }
    }

    public string LocationText
    {
        get => _locationText;
        private set { _locationText = value; OnPropertyChanged(); }
    }

    private void UpdateLocationText() => LocationText = $"现在正在修改({Row},{Col})";

    private void LoadOptions()
    {
        SeedTypeOptions = Services.OptionsLoader.Load(Constants.JsonPlantFile);
        FacingOptions = BuildOptions(GardenLayout.FacingNames);
        AgeOptions = BuildOptions(GardenLayout.AgeNames);
        NeedOptions = BuildOptions(GardenLayout.NeedNames);
    }

    private static ObservableCollection<NameOption> BuildOptions(string[] names)
    {
        var options = new ObservableCollection<NameOption>();
        for(int i = 0; i < names.Length; i++)
            options.Add(new NameOption { Name = names[i], Value = i.ToString() });
        return options;
    }

    /// <summary>下拉与回读共用的显示形状：[数字]名字，SelectedFacingValue / SelectedAgeValue 按这个解析。</summary>
    private static string Display(int value, string[] names) => $"[{value}]{GardenLayout.Name(names, value)}";

    /// <summary>默认选豌豆射手。按 Value 认、不按中文名认——下拉里的名字可能被游戏那一份覆盖掉，
    /// 认死"豌豆射手"这四个字会让 SelectedSeedTypeValue 变成空串，点确定就什么都发不出去。</summary>
    private void SetDefaultSelections()
    {
        SelectedSeedTypeName = SeedTypeOptions.FirstOrDefault(o => o.Value == DefaultSeedValue)?.Name
                              ?? DefaultSeedValue;
        SelectedFacingDisplay = Display(0, GardenLayout.FacingNames);
        SelectedAgeDisplay = Display(3, GardenLayout.AgeNames);
        SelectedNeedDisplay = Display(0, GardenLayout.NeedNames);
    }

    private const string DefaultSeedValue = "Peashooter";

    // ---------- 命令 ----------
    public ICommand OkCommand { get; private set; }

    public ICommand CancelCommand { get; private set; }

    public ICommand RemoveCommand { get; private set; }

    public event EventHandler RequestClose;

    public bool? DialogResult { get; private set; }

    private void Ok()
    {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private ObservableCollection<NameOption> _seedTypeOptions;

    public ObservableCollection<NameOption> SeedTypeOptions
    {
        get => _seedTypeOptions;
        set { _seedTypeOptions = value; OnPropertyChanged(); }
    }

    private bool _seedTypeDropdownOpen;

    public bool SeedTypeDropdownOpen
    {
        get => _seedTypeDropdownOpen;
        set { _seedTypeDropdownOpen = value; OnPropertyChanged(); }
    }

    private NameOption _selectedSeedTypeOption;

    public NameOption SelectedSeedTypeOption
    {
        get => _selectedSeedTypeOption;
        set
        {
            _selectedSeedTypeOption = value;
            if(value != null)
            {
                SelectedSeedTypeName = value.Name;
            }
            SeedTypeDropdownOpen = false;
            OnPropertyChanged();
        }
    }

    private string _selectedSeedTypeName;

    public string SelectedSeedTypeName
    {
        get => _selectedSeedTypeName;
        set { _selectedSeedTypeName = value; OnPropertyChanged(); }
    }

    private ObservableCollection<NameOption> _facingOptions;

    public ObservableCollection<NameOption> FacingOptions
    {
        get => _facingOptions;
        set { _facingOptions = value; OnPropertyChanged(); }
    }

    private bool _facingDropdownOpen;

    public bool FacingDropdownOpen
    {
        get => _facingDropdownOpen;
        set { _facingDropdownOpen = value; OnPropertyChanged(); }
    }

    private NameOption _selectedFacingOption;

    public NameOption SelectedFacingOption
    {
        get => _selectedFacingOption;
        set
        {
            _selectedFacingOption = value;
            if(value != null)
            {
                SelectedFacingDisplay = $"[{value.Value}]{value.Name}";
            }
            FacingDropdownOpen = false;
            OnPropertyChanged();
        }
    }

    private string _selectedFacingDisplay;

    public string SelectedFacingDisplay
    {
        get => _selectedFacingDisplay;
        set { _selectedFacingDisplay = value; OnPropertyChanged(); }
    }

    private ObservableCollection<NameOption> _ageOptions;

    public ObservableCollection<NameOption> AgeOptions
    {
        get => _ageOptions;
        set { _ageOptions = value; OnPropertyChanged(); }
    }

    private bool _ageDropdownOpen;

    public bool AgeDropdownOpen
    {
        get => _ageDropdownOpen;
        set { _ageDropdownOpen = value; OnPropertyChanged(); }
    }

    private NameOption _selectedAgeOption;

    public NameOption SelectedAgeOption
    {
        get => _selectedAgeOption;
        set
        {
            _selectedAgeOption = value;
            if(value != null)
            {
                SelectedAgeDisplay = $"[{value.Value}]{value.Name}";
            }
            AgeDropdownOpen = false;
            OnPropertyChanged();
        }
    }

    private string _selectedAgeDisplay;

    public string SelectedAgeDisplay
    {
        get => _selectedAgeDisplay;
        set { _selectedAgeDisplay = value; OnPropertyChanged(); }
    }

    private ObservableCollection<NameOption> _needOptions;

    public ObservableCollection<NameOption> NeedOptions
    {
        get => _needOptions;
        set { _needOptions = value; OnPropertyChanged(); }
    }

    private bool _needDropdownOpen;

    public bool NeedDropdownOpen
    {
        get => _needDropdownOpen;
        set { _needDropdownOpen = value; OnPropertyChanged(); }
    }

    private NameOption _selectedNeedOption;

    public NameOption SelectedNeedOption
    {
        get => _selectedNeedOption;
        set
        {
            _selectedNeedOption = value;
            if(value != null)
            {
                SelectedNeedDisplay = $"[{value.Value}]{value.Name}";
            }
            NeedDropdownOpen = false;
            OnPropertyChanged();
        }
    }

    private string _selectedNeedDisplay;

    public string SelectedNeedDisplay
    {
        get => _selectedNeedDisplay;
        set { _selectedNeedDisplay = value; OnPropertyChanged(); }
    }

    public int SelectedNeedValue => DisplayValue(SelectedNeedDisplay);

    public string SelectedSeedTypeValue
    {
        get
        {
            foreach(var opt in SeedTypeOptions)
                if(opt.Name == SelectedSeedTypeName)
                    return opt.Value;
            return string.Empty;
        }
    }

    public int SelectedFacingValue => DisplayValue(SelectedFacingDisplay);

    public int SelectedAgeValue => DisplayValue(SelectedAgeDisplay, 3);

    /// <summary>从 "[数字]名字" 里取那个数字；取不到就用给定的默认值。</summary>
    private static int DisplayValue(string display, int fallback = 0)
    {
        var match = Regex.Match(display ?? "", @"\[(\d+)\]");
        return match.Success ? int.Parse(match.Groups[1].Value) : fallback;
    }
}
