using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Models;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels;

public class FormationViewModel:ViewModelBase
{
    private readonly string _defaultPath;
    private readonly IScriptExecutionService _scriptExec;
    private bool _bgDropdownToggleIsChecked;
    private string _bgInput = "白天";
    private string _formation_Sync_CardInput = Constants.c_Symbol_On;
    private bool _formationColDropdownToggleIsChecked;
    private string _formationColInput = "第1列";
    private bool _formationDropdownToggleIsChecked;
    private string _formationInput = "[PE][终极战术]五角星无炮流";
    private string _formationLadder = Constants.c_Symbol_On;
    private string _formationNameInput = "默认阵型名称";
    private ObservableCollection<NameOption> _formationOptions;
    private string _formationPlant = Constants.c_Symbol_On;
    private bool _formationRowDropdownToggleIsChecked;
    private string _formationRowInput = "第1行";
    private string _formationVase = Constants.c_Symbol_Off;
    private string _gridSquareToget = Constants.c_Symbol_On;
    private string _gridSquareTogetInput = Constants.c_Symbol_On;
    private bool _gridSquareTypeDropdownToggleIsChecked;
    private string _gridSquareTypeInput = "草地";
    private string _imitaterSlot = Constants.c_Symbol_Off;
    private bool _plantRowTypeDropdownToggleIsChecked;
    private string _plantRowTypeInput = "裸地";
    private bool _seedPacketsDropdownToggleIsChecked;
    private string _seedPacketsInput = "默认卡组名称";
    private ObservableCollection<NameOption> _seedPacketsOptions;
    private NameOption _selectedBg;
    private NameOption _selectedFormation;
    private NameOption _selectedFormationCol;
    private NameOption _selectedFormationRow;
    private NameOption _selectedGridSquareType;
    private NameOption _selectedPlantRowType;
    private NameOption _selectedSeedPacket;
    private NameOption _selectedSp1;
    private NameOption _selectedSp2;
    private bool _slotDropdownToggleIsChecked;
    private string _spInput1 = "第1槽";
    private bool _spInput1DropdownToggleIsChecked;
    private string _spInput2 = "豌豆射手";
    private string _spInput3 = Constants.c_Symbol_Off;

    public FormationViewModel(IScriptExecutionService scriptExec, string defaultPath)
    {
        _scriptExec = scriptExec;
        _defaultPath = defaultPath;

        BackgroundOptions = OptionsLoader.Load(Constants.JsonBackgroundFile);
        SlotOptions = OptionsLoader.Load(Constants.JsonSlotFile);
        SpInput1Options = OptionsLoader.Load(Constants.JsonSlotIndexFile);
        PlantRowTypeOptions = OptionsLoader.Load(Constants.JsonPlantRowTypeFile);
        GridSquareTypeOptions = OptionsLoader.Load(Constants.JsonGridSquareTypeFile);
        FormationRowOptions = OptionsLoader.Load(Constants.JsonRowFile);
        FormationColOptions = OptionsLoader.Load(Constants.JsonColFile);

        LoadFormationOptions();
        LoadSeedPacketsOptions();
    }

    // 在类中添加属性（已存在，无需重复）
    public string Formation_Sync_CardInput
    {
        get => _formation_Sync_CardInput;
        set { _formation_Sync_CardInput = value; OnPropertyChanged(); }
    }

    // 添加切换命令
    public ICommand ToggleFormation_Sync_CardCommand => new RelayCommand(_ =>
        Formation_Sync_CardInput = ButtonHelper.ToggleCheck(Formation_Sync_CardInput));

    // 提取保存卡组的方法
    private async Task SaveSeedPacketsAsync(string name)
    {
        string output = await _scriptExec.ExecuteWithResultAsync(
            Constants.SubFolders.Formation,
            "存储卡组",
            new Dictionary<string, string> { ["{NAME}"] = name }
        );

        string json = ExtractJsonFromOutput(output, "SEEDPACKET_JSON_START", "SEEDPACKET_JSON_END");
        if(string.IsNullOrEmpty(json))
        {
            MessageBox.Show($"卡组“{name}”保存失败，未能提取 JSON 数据", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_SeedPackets);
        if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string uniquePath = GetUniqueFilePath(dir, name);
        await File.WriteAllTextAsync(uniquePath, json);
        LoadSeedPacketsOptions(); // 刷新卡组列表
    }

    // 提取加载卡组的方法
    private async Task LoadSeedPacketsAsync(string name)
    {
        string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_SeedPackets);
        string filePath = Path.Combine(dir, name + ".json");
        if(!File.Exists(filePath))
        {
            MessageBox.Show($"卡组文件不存在：{name}.json", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string jsonContent = await File.ReadAllTextAsync(filePath);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
        string base64 = Convert.ToBase64String(bytes);

        await _scriptExec.ExecuteAsync(
            Constants.SubFolders.Formation,
            "切换卡组",
            new Dictionary<string, string> { ["{JSON_BASE64}"] = base64 }
        );
    }

    // 辅助方法：从脚本输出中提取 JSON
    private string ExtractJsonFromOutput(string output, string startMarker, string endMarker)
    {
        int startIdx = output.IndexOf(startMarker);
        int endIdx = output.IndexOf(endMarker);
        if(startIdx != -1 && endIdx != -1 && endIdx > startIdx)
        {
            return output.Substring(startIdx + startMarker.Length, endIdx - startIdx - startMarker.Length).Trim();
        }

        int braceStart = output.IndexOf('{');
        int braceEnd = output.LastIndexOf('}');
        if(braceStart != -1 && braceEnd != -1 && braceEnd > braceStart)
        {
            return output.Substring(braceStart, braceEnd - braceStart + 1);
        }
        return null;
    }

    // 修改 AddFormationCommand
    public ICommand AddFormationCommand => new RelayCommand(async _ =>
    {
        // 1. 存储阵型（原逻辑）
        string output = await _scriptExec.ExecuteWithResultAsync(
            Constants.SubFolders.Formation,
            "存储阵型",
            new Dictionary<string, string>
            {
                ["{PLANT}"] = ButtonHelper.GetCheckValue(FormationPlant),
                ["{LADDER}"] = ButtonHelper.GetCheckValue(FormationLadder),
                ["{VASE}"] = ButtonHelper.GetCheckValue(FormationVase),
                ["{NAME}"] = FormationNameInput
            }
        );

        string json = ExtractJsonFromOutput(output, "FORMATION_JSON_START", "FORMATION_JSON_END");
        if(string.IsNullOrEmpty(json))
        {
            MessageBox.Show("未能从脚本输出中提取阵型 JSON 数据", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // 保存阵型文件
        string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_Formations);
        if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string uniquePath = GetUniqueFilePath(dir, FormationNameInput);
        await File.WriteAllTextAsync(uniquePath, json);
        LoadFormationOptions();

        // 2. 同步卡组（如果开启）
        if(Formation_Sync_CardInput == Constants.c_Symbol_On)
        {
            await SaveSeedPacketsAsync(FormationNameInput);
        }

        MessageBox.Show($"阵型已保存为 {Path.GetFileName(uniquePath)}", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
    });

    // 修改 SetFormationCommand
    public ICommand SetFormationCommand => new RelayCommand(async _ =>
    {
        string selectedName = FormationInput;
        string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_Formations);
        string filePath = Path.Combine(dir, selectedName + ".json");

        if(!File.Exists(filePath))
        {
            MessageBox.Show($"阵型文件不存在：{selectedName}.json", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string jsonContent = await File.ReadAllTextAsync(filePath);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
        string base64 = Convert.ToBase64String(bytes);

        // 1. 布阵
        await _scriptExec.ExecuteAsync(
            Constants.SubFolders.Formation,
            "一键布阵",
            new Dictionary<string, string> { ["{JSON_BASE64}"] = base64 }
        );

        // 2. 同步卡组（如果开启）
        if(Formation_Sync_CardInput == Constants.c_Symbol_On)
        {
            await LoadSeedPacketsAsync(selectedName);
        }
    });

    public ICommand AddSeedPacketsCommand => new RelayCommand(async _ =>
    {
        string output = await _scriptExec.ExecuteWithResultAsync(
            Constants.SubFolders.Formation,
            "存储卡组",
            new Dictionary<string, string>
            {
                ["{NAME}"] = SeedPacketsInput
            }
        );

        string json = null;
        const string startMarker = "SEEDPACKET_JSON_START";
        const string endMarker = "SEEDPACKET_JSON_END";
        int startIdx = output.IndexOf(startMarker);
        int endIdx = output.IndexOf(endMarker);
        if(startIdx != -1 && endIdx != -1 && endIdx > startIdx)
        {
            json = output.Substring(startIdx + startMarker.Length, endIdx - startIdx - startMarker.Length).Trim();
        }

        // 如果标记提取失败，尝试取第一个 { 到最后一个 }
        if(string.IsNullOrEmpty(json))
        {
            int braceStart = output.IndexOf('{');
            int braceEnd = output.LastIndexOf('}');
            if(braceStart != -1 && braceEnd != -1 && braceEnd > braceStart)
            {
                json = output.Substring(braceStart, braceEnd - braceStart + 1);
            }
        }

        if(string.IsNullOrEmpty(json))
        {
            MessageBox.Show("未能从脚本输出中提取 JSON 数据", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // 存储唯一路径
        string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_SeedPackets);
        if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string uniquePath = GetUniqueFilePath(dir, SeedPacketsInput);
        File.WriteAllText(uniquePath, json);

        LoadSeedPacketsOptions();
        MessageBox.Show($"卡组已保存为 {Path.GetFileName(uniquePath)}", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
    });

    public ObservableCollection<NameOption> BackgroundOptions { get; }

    public bool BgDropdownToggleIsChecked
    {
        get => _bgDropdownToggleIsChecked;
        set { _bgDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string BgInput
    {
        get => _bgInput;
        set { _bgInput = value; OnPropertyChanged(); }
    }

    public bool FormationColDropdownToggleIsChecked
    {
        get => _formationColDropdownToggleIsChecked;
        set { _formationColDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string FormationColInput
    {
        get => _formationColInput;
        set { _formationColInput = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> FormationColOptions { get; }

    public bool FormationDropdownToggleIsChecked
    {
        get => _formationDropdownToggleIsChecked;
        set { _formationDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string FormationInput
    {
        get => _formationInput;
        set { _formationInput = value; OnPropertyChanged(); }
    }

    public string FormationLadder
    {
        get => _formationLadder;
        set { _formationLadder = value; OnPropertyChanged(); }
    }

    public string FormationNameInput
    {
        get => _formationNameInput;
        set { _formationNameInput = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> FormationOptions
    {
        get => _formationOptions;
        set
        {
            _formationOptions = value;
            OnPropertyChanged();
        }
    }

    public string FormationPlant
    {
        get => _formationPlant;
        set { _formationPlant = value; OnPropertyChanged(); }
    }

    public bool FormationRowDropdownToggleIsChecked
    {
        get => _formationRowDropdownToggleIsChecked;
        set { _formationRowDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string FormationRowInput
    {
        get => _formationRowInput;
        set { _formationRowInput = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> FormationRowOptions { get; }

    public string FormationVase
    {
        get => _formationVase;
        set { _formationVase = value; OnPropertyChanged(); }
    }

    public string GridSquareToget
    {
        get => _gridSquareToget;
        set { _gridSquareToget = value; OnPropertyChanged(); }
    }

    public string GridSquareTogetInput
    {
        get => _gridSquareTogetInput;
        set { _gridSquareTogetInput = value; OnPropertyChanged(); }
    }

    public ICommand GridSquareTypeCommand => new RelayCommand(async _ =>
    {
        string row = NameOption.GetValue(FormationRowInput, FormationRowOptions);
        string col = NameOption.GetValue(FormationColInput, FormationColOptions);
        string type = NameOption.GetValue(GridSquareTypeInput, GridSquareTypeOptions);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Formation, "设置格子类型",
            new Dictionary<string, string>
            {
                [Constants.Placeholders.Row] = row,
                [Constants.Placeholders.Col] = col,
                [Constants.Placeholders.Type] = type
            });
    });

    public bool GridSquareTypeDropdownToggleIsChecked
    {
        get => _gridSquareTypeDropdownToggleIsChecked;
        set { _gridSquareTypeDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string GridSquareTypeInput
    {
        get => _gridSquareTypeInput;
        set { _gridSquareTypeInput = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> GridSquareTypeOptions { get; }

    public string ImitaterSlot
    {
        get => _imitaterSlot;
        set { _imitaterSlot = value; OnPropertyChanged(); }
    }

    public ICommand PickRandSeedCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Formation, "随机选卡"));

    public ICommand PlantRowTypeCommand => new RelayCommand(async _ =>
    {
        string row = NameOption.GetValue(FormationRowInput, FormationRowOptions);
        string type = NameOption.GetValue(PlantRowTypeInput, PlantRowTypeOptions);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Formation, "设置道路状况",
            new Dictionary<string, string>
            {
                [Constants.Placeholders.Row] = row,
                [Constants.Placeholders.Type] = type,
                [Constants.Placeholders.GridCheck] = ButtonHelper.GetCheckValue(GridSquareTogetInput)
            });
    });

    public bool PlantRowTypeDropdownToggleIsChecked
    {
        get => _plantRowTypeDropdownToggleIsChecked;
        set { _plantRowTypeDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string PlantRowTypeInput
    {
        get => _plantRowTypeInput;
        set { _plantRowTypeInput = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> PlantRowTypeOptions { get; }

    public bool SeedPacketsDropdownToggleIsChecked
    {
        get => _seedPacketsDropdownToggleIsChecked;
        set { _seedPacketsDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public string SeedPacketsInput
    {
        get => _seedPacketsInput;
        set { _seedPacketsInput = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> SeedPacketsOptions
    {
        get => _seedPacketsOptions;
        set
        {
            _seedPacketsOptions = value;
            OnPropertyChanged();
        }
    }

    public NameOption SelectedBg
    {
        get => _selectedBg;
        set
        {
            _selectedBg = value;
            if(value != null)
                BgInput = value.Name;
            BgDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public NameOption SelectedFormation
    {
        get => _selectedFormation;
        set
        {
            _selectedFormation = value;
            if(value != null)
                FormationInput = value.Name;
            FormationDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public NameOption SelectedFormationCol
    {
        get => _selectedFormationCol;
        set
        {
            _selectedFormationCol = value;
            if(value != null)
                FormationColInput = value.Name;
            FormationColDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public NameOption SelectedFormationRow
    {
        get => _selectedFormationRow;
        set
        {
            _selectedFormationRow = value;
            if(value != null)
                FormationRowInput = value.Name;
            FormationRowDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public NameOption SelectedGridSquareType
    {
        get => _selectedGridSquareType;
        set
        {
            _selectedGridSquareType = value;
            if(value != null)
                GridSquareTypeInput = value.Name;
            GridSquareTypeDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public NameOption SelectedPlantRowType
    {
        get => _selectedPlantRowType;
        set
        {
            _selectedPlantRowType = value;
            if(value != null)
                PlantRowTypeInput = value.Name;
            PlantRowTypeDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public NameOption SelectedSeedPacket
    {
        get => _selectedSeedPacket;
        set
        {
            _selectedSeedPacket = value;

            if(value != null)
                SeedPacketsInput = value.Name;
            SeedPacketsDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public NameOption SelectedSp1
    {
        get => _selectedSp1;
        set
        {
            _selectedSp1 = value;

            if(value != null)
                SpInput1 = value.Name;
            SpInput1DropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public NameOption SelectedSp2
    {
        get => _selectedSp2;
        set
        {
            _selectedSp2 = value;

            if(value != null)
                SpInput2 = value.Name;
            SlotDropdownToggleIsChecked = false; OnPropertyChanged();
        }
    }

    public ICommand SetBgCommand => new RelayCommand(async _ =>
    {
        string bgValue = NameOption.GetValue(BgInput, BackgroundOptions);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Formation, "设置场景",
            new Dictionary<string, string> { ["{BACKGROUNDTYPE}"] = bgValue });
    });

    public ICommand SetSeedPacketsCommand => new RelayCommand(async _ =>
    {
        string selectedName = SeedPacketsInput;
        string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_SeedPackets);
        string filePath = Path.Combine(dir, selectedName + ".json");

        if(!File.Exists(filePath))
        {
            MessageBox.Show($"卡组文件不存在：{selectedName}.json", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string jsonContent = await File.ReadAllTextAsync(filePath);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
        string base64 = Convert.ToBase64String(bytes);

        await _scriptExec.ExecuteAsync(
            Constants.SubFolders.Formation,
            "切换卡组",
            new Dictionary<string, string>
            {
                ["{JSON_BASE64}"] = base64
            }
        );
    });

    public ICommand SetSpCommand => new RelayCommand(async _ =>
    {
        string spNum = NameOption.GetValue(SpInput1, SpInput1Options);
        string st = NameOption.GetValue(SpInput2, SlotOptions);
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Formation, "设置卡槽",
            new Dictionary<string, string>
            {
                [Constants.Placeholders.SPNum] = spNum,
                [Constants.Placeholders.ST] = st,
                [Constants.Placeholders.ItCheck] = ButtonHelper.GetCheckValue(SpInput3)
            });
    });

    public bool SlotDropdownToggleIsChecked
    {
        get => _slotDropdownToggleIsChecked;
        set { _slotDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> SlotOptions { get; }

    public string SpInput1
    {
        get => _spInput1;
        set { _spInput1 = value; OnPropertyChanged(); }
    }

    public bool SpInput1DropdownToggleIsChecked
    {
        get => _spInput1DropdownToggleIsChecked;
        set { _spInput1DropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> SpInput1Options { get; }

    public string SpInput2
    {
        get => _spInput2;
        set { _spInput2 = value; OnPropertyChanged(); }
    }

    public string SpInput3
    {
        get => _spInput3;
        set { _spInput3 = value; OnPropertyChanged(); }
    }

    public ICommand ToggleFormationLadderCommand => new RelayCommand(_ => FormationLadder = ButtonHelper.ToggleCheck(FormationLadder));

    public ICommand ToggleFormationPlantCommand => new RelayCommand(_ => FormationPlant = ButtonHelper.ToggleCheck(FormationPlant));

    public ICommand ToggleFormationVaseCommand => new RelayCommand(_ => FormationVase = ButtonHelper.ToggleCheck(FormationVase));

    public ICommand ToggleGridSquareTogetCommand => new RelayCommand(_ => GridSquareTogetInput = ButtonHelper.ToggleCheck(GridSquareTogetInput));

    public ICommand ToggleImitaterSlotCommand => new RelayCommand(_ => ImitaterSlot = ButtonHelper.ToggleCheck(ImitaterSlot));

    public ICommand ToggleSpImitaterCommand => new RelayCommand(_ => SpInput3 = ButtonHelper.ToggleCheck(SpInput3));

    // 查看草坪
    public ICommand ViewLawnCommand => new RelayCommand(async _ =>
        await _scriptExec.ExecuteAsync(Constants.SubFolders.Formation, "查看草坪"));

    private string GetUniqueFilePath(string dir, string baseName, string extension = ".json")
    {
        string basePath = Path.Combine(dir, baseName + extension);
        if(!File.Exists(basePath))
            return basePath;

        int index = 1;
        while(true)
        {
            string candidate = Path.Combine(dir, $"{baseName}_{index}{extension}");
            if(!File.Exists(candidate))
                return candidate;
            index++;
        }
    }

    private void LoadFormationOptions()
    {
        FormationOptions = new ObservableCollection<NameOption>();
        string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_Formations);
        if(Directory.Exists(dir))
        {
            foreach(var file in Directory.GetFiles(dir, "*.json"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                FormationOptions.Add(new NameOption { Name = name, Value = file });
            }
        }
    }

    private void LoadSeedPacketsOptions()
    {
        SeedPacketsOptions = new ObservableCollection<NameOption>();
        string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_SeedPackets);
        if(Directory.Exists(dir))
        {
            foreach(var file in Directory.GetFiles(dir, "*.json"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                SeedPacketsOptions.Add(new NameOption { Name = name, Value = file });
            }
        }
    }
}
