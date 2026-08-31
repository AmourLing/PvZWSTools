using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Newtonsoft.Json;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;

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
    }

    public GardenDialogViewModel(int row, int col, string defaultSeedType = "豌豆射手") : this()
    {
        Row = row;
        Col = col;
        // 如有自定义默认种子类型，可在此赋值
        // SelectedSeedTypeName = defaultSeedType;
    }

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
        string configDir = Path.Combine(Directory.GetCurrentDirectory(),
            Constants.Folder_Need, Constants.Folder_Options);
        string plantFilePath = Path.Combine(configDir, Constants.JsonPlantFile);
        if(File.Exists(plantFilePath))
        {
            string json = File.ReadAllText(plantFilePath);
            SeedTypeOptions = JsonConvert.DeserializeObject<ObservableCollection<NameOption>>(json)
                              ?? new ObservableCollection<NameOption>();
        }
        else
        {
            SeedTypeOptions = new ObservableCollection<NameOption>();
        }

        FacingOptions = new ObservableCollection<NameOption>
        {
            new NameOption { Name = "右", Value = "0" },
            new NameOption { Name = "左", Value = "1" }
        };

        AgeOptions = new ObservableCollection<NameOption>
        {
            new NameOption { Name = "幼苗", Value = "0" },
            new NameOption { Name = "小", Value = "1" },
            new NameOption { Name = "中", Value = "2" },
            new NameOption { Name = "大", Value = "3" }
        };
    }

    private void SetDefaultSelections()
    {
        SelectedSeedTypeName = "豌豆射手";
        SelectedFacingDisplay = "[0]右";
        SelectedAgeDisplay = "[3]大";
    }

    // ---------- 命令 ----------
    public ICommand OkCommand { get; private set; }

    public ICommand CancelCommand { get; private set; }

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
}
