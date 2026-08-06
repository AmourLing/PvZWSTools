using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Newtonsoft.Json;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Models;

namespace PvZWSTools_WPF.ViewModels;

public class GardenDialogViewModel:ViewModelBase
{
    private readonly int _row;
    private readonly int _col;
    private string _defaultSeedType;

    public GardenDialogViewModel(int row, int col, string defaultSeedType = "豌豆射手")
    {
        _row = row;
        _col = col;
        _defaultSeedType = defaultSeedType;
        LocationText = $"现在正在修改({row},{col})";

        // 加载植物选项
        string configDir = Path.Combine(Directory.GetCurrentDirectory(),
            Constants.Folder_Need, Constants.Folder_Options);
        string plantFilePath = Path.Combine(configDir, Constants.JsonPlantFile);
        if(File.Exists(plantFilePath))
        {
            string json = File.ReadAllText(plantFilePath);
            SeedTypeOptions = JsonConvert.DeserializeObject<ObservableCollection<NameOption>>(json);
        }
        else
        {
            SeedTypeOptions = new ObservableCollection<NameOption>();
        }

        // 朝向选项
        FacingOptions = new ObservableCollection<NameOption>
        {
            new NameOption { Name = "右", Value = "0" },
            new NameOption { Name = "左", Value = "1" }
        };

        // 年龄选项
        AgeOptions = new ObservableCollection<NameOption>
        {
            new NameOption { Name = "幼苗", Value = "0" },
            new NameOption { Name = "小", Value = "1" },
            new NameOption { Name = "中", Value = "2" },
            new NameOption { Name = "大", Value = "3" }
        };

        // 设置默认值
        SelectedSeedTypeName = defaultSeedType;
        SelectedFacingDisplay = "[0]右";
        SelectedAgeDisplay = "[3]大";

        // 命令
        OkCommand = new RelayCommand(_ => Ok());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public event EventHandler RequestClose;

    // 位置文本
    private string _locationText;

    public string LocationText
    {
        get => _locationText;
        set { _locationText = value; OnPropertyChanged(); }
    }

    // 植物类型选项
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

    // 朝向
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

    // 年龄
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

    // 命令
    public ICommand OkCommand { get; }

    public ICommand CancelCommand { get; }

    // 结果属性（供调用者获取）
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

    public int SelectedFacingValue
    {
        get
        {
            var match = System.Text.RegularExpressions.Regex.Match(SelectedFacingDisplay, @"\[(\d+)\].+");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }
    }

    public int SelectedAgeValue
    {
        get
        {
            var match = System.Text.RegularExpressions.Regex.Match(SelectedAgeDisplay, @"\[(\d+)\].+");
            return match.Success ? int.Parse(match.Groups[1].Value) : 3;
        }
    }

    private void Ok()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void Cancel()
    {
        // 不设置结果，直接关闭
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
