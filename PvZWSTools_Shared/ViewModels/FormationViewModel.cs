using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using PvZWSTools_Shared.Commands;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_Shared.ViewModels;

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
    private static readonly IReadOnlyDictionary<string, string> _buttonMapping = new Dictionary<string, string>();

    private readonly IMessageProcessor _messageProcessor;

    private void OnButtonStatusUpdated(Dictionary<string, bool> statusDict)
    {
        UpdatePropertiesFromDict(statusDict, _buttonMapping);
    }

    public FormationViewModel(IScriptExecutionService scriptExec, string defaultPath, IMessageProcessor messageProcessor)
    {
        _scriptExec = scriptExec;
        _defaultPath = defaultPath;
        _messageProcessor = messageProcessor;
        if(_messageProcessor != null)
            _messageProcessor.ButtonStatusUpdated += OnButtonStatusUpdated;
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

    public string Formation_Sync_CardInput { get => _formation_Sync_CardInput; set { _formation_Sync_CardInput = value; OnPropertyChanged(); } }

    public ICommand ToggleFormation_Sync_CardCommand => new RelayCommand(_ => Formation_Sync_CardInput = ButtonHelper.ToggleCheck(Formation_Sync_CardInput));

    public ObservableCollection<NameOption> BackgroundOptions { get; }
    public bool BgDropdownToggleIsChecked { get => _bgDropdownToggleIsChecked; set { _bgDropdownToggleIsChecked = value; OnPropertyChanged(); } }
    public string BgInput { get => _bgInput; set { _bgInput = value; OnPropertyChanged(); } }

    public bool FormationColDropdownToggleIsChecked { get => _formationColDropdownToggleIsChecked; set { _formationColDropdownToggleIsChecked = value; OnPropertyChanged(); } }
    public string FormationColInput { get => _formationColInput; set { _formationColInput = value; OnPropertyChanged(); } }
    public ObservableCollection<NameOption> FormationColOptions { get; }

    public bool FormationDropdownToggleIsChecked { get => _formationDropdownToggleIsChecked; set { _formationDropdownToggleIsChecked = value; OnPropertyChanged(); } }
    public string FormationInput { get => _formationInput; set { _formationInput = value; OnPropertyChanged(); } }

    public string FormationLadder { get => _formationLadder; set { _formationLadder = value; OnPropertyChanged(); } }
    public string FormationNameInput { get => _formationNameInput; set { _formationNameInput = value; OnPropertyChanged(); } }

    public ObservableCollection<NameOption> FormationOptions { get => _formationOptions; set { _formationOptions = value; OnPropertyChanged(); } }
    public string FormationPlant { get => _formationPlant; set { _formationPlant = value; OnPropertyChanged(); } }

    public bool FormationRowDropdownToggleIsChecked { get => _formationRowDropdownToggleIsChecked; set { _formationRowDropdownToggleIsChecked = value; OnPropertyChanged(); } }
    public string FormationRowInput { get => _formationRowInput; set { _formationRowInput = value; OnPropertyChanged(); } }
    public ObservableCollection<NameOption> FormationRowOptions { get; }

    public string FormationVase { get => _formationVase; set { _formationVase = value; OnPropertyChanged(); } }

    public string GridSquareToget { get => _gridSquareToget; set { _gridSquareToget = value; OnPropertyChanged(); } }
    public string GridSquareTogetInput { get => _gridSquareTogetInput; set { _gridSquareTogetInput = value; OnPropertyChanged(); } }

    public bool GridSquareTypeDropdownToggleIsChecked { get => _gridSquareTypeDropdownToggleIsChecked; set { _gridSquareTypeDropdownToggleIsChecked = value; OnPropertyChanged(); } }
    public string GridSquareTypeInput { get => _gridSquareTypeInput; set { _gridSquareTypeInput = value; OnPropertyChanged(); } }
    public ObservableCollection<NameOption> GridSquareTypeOptions { get; }

    public string ImitaterSlot { get => _imitaterSlot; set { _imitaterSlot = value; OnPropertyChanged(); } }

    public bool PlantRowTypeDropdownToggleIsChecked { get => _plantRowTypeDropdownToggleIsChecked; set { _plantRowTypeDropdownToggleIsChecked = value; OnPropertyChanged(); } }
    public string PlantRowTypeInput { get => _plantRowTypeInput; set { _plantRowTypeInput = value; OnPropertyChanged(); } }
    public ObservableCollection<NameOption> PlantRowTypeOptions { get; }

    public bool SeedPacketsDropdownToggleIsChecked { get => _seedPacketsDropdownToggleIsChecked; set { _seedPacketsDropdownToggleIsChecked = value; OnPropertyChanged(); } }
    public string SeedPacketsInput { get => _seedPacketsInput; set { _seedPacketsInput = value; OnPropertyChanged(); } }

    public ObservableCollection<NameOption> SeedPacketsOptions { get => _seedPacketsOptions; set { _seedPacketsOptions = value; OnPropertyChanged(); } }

    public NameOption SelectedBg { get => _selectedBg; set { _selectedBg = value; if(value != null) BgInput = value.Name; BgDropdownToggleIsChecked = false; OnPropertyChanged(); } }
    public NameOption SelectedFormation { get => _selectedFormation; set { _selectedFormation = value; if(value != null) FormationInput = value.Name; FormationDropdownToggleIsChecked = false; OnPropertyChanged(); } }
    public NameOption SelectedFormationCol { get => _selectedFormationCol; set { _selectedFormationCol = value; if(value != null) FormationColInput = value.Name; FormationColDropdownToggleIsChecked = false; OnPropertyChanged(); } }
    public NameOption SelectedFormationRow { get => _selectedFormationRow; set { _selectedFormationRow = value; if(value != null) FormationRowInput = value.Name; FormationRowDropdownToggleIsChecked = false; OnPropertyChanged(); } }
    public NameOption SelectedGridSquareType { get => _selectedGridSquareType; set { _selectedGridSquareType = value; if(value != null) GridSquareTypeInput = value.Name; GridSquareTypeDropdownToggleIsChecked = false; OnPropertyChanged(); } }
    public NameOption SelectedPlantRowType { get => _selectedPlantRowType; set { _selectedPlantRowType = value; if(value != null) PlantRowTypeInput = value.Name; PlantRowTypeDropdownToggleIsChecked = false; OnPropertyChanged(); } }
    public NameOption SelectedSeedPacket { get => _selectedSeedPacket; set { _selectedSeedPacket = value; if(value != null) SeedPacketsInput = value.Name; SeedPacketsDropdownToggleIsChecked = false; OnPropertyChanged(); } }
    public NameOption SelectedSp1 { get => _selectedSp1; set { _selectedSp1 = value; if(value != null) SpInput1 = value.Name; SpInput1DropdownToggleIsChecked = false; OnPropertyChanged(); } }
    public NameOption SelectedSp2 { get => _selectedSp2; set { _selectedSp2 = value; if(value != null) SpInput2 = value.Name; SlotDropdownToggleIsChecked = false; OnPropertyChanged(); } }

    public bool SlotDropdownToggleIsChecked { get => _slotDropdownToggleIsChecked; set { _slotDropdownToggleIsChecked = value; OnPropertyChanged(); } }
    public ObservableCollection<NameOption> SlotOptions { get; }

    public string SpInput1 { get => _spInput1; set { _spInput1 = value; OnPropertyChanged(); } }
    public bool SpInput1DropdownToggleIsChecked { get => _spInput1DropdownToggleIsChecked; set { _spInput1DropdownToggleIsChecked = value; OnPropertyChanged(); } }
    public ObservableCollection<NameOption> SpInput1Options { get; }

    public string SpInput2 { get => _spInput2; set { _spInput2 = value; OnPropertyChanged(); } }
    public string SpInput3 { get => _spInput3; set { _spInput3 = value; OnPropertyChanged(); } }

    public ICommand AddFormationCommand => new RelayCommand(async _ =>
    {
        try
        {
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

            string jsonBase64 = ExtractJsonFromOutput(output, "FORMATION_JSON_START", "FORMATION_JSON_END");

            if(string.IsNullOrEmpty(jsonBase64))
            {
                ShowError("未能从脚本输出中提取阵型数据。请检查脚本是否正常运行。");
                return;
            }

            string jsonContent;
            try
            {
                byte[] bytes = Convert.FromBase64String(jsonBase64);
                jsonContent = System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch(FormatException)
            {
                ShowError("阵型数据损坏：Base64 解码失败。可能是数据传输不完整。");
                return;
            }

            string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_Formations);
            if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string uniquePath = GetUniqueFilePath(dir, FormationNameInput);
            await File.WriteAllTextAsync(uniquePath, jsonContent);

            LoadFormationOptions();

            if(Formation_Sync_CardInput == Constants.c_Symbol_On)
            {
                await SaveSeedPacketsAsync(FormationNameInput);
            }

            Log.Info($"阵型已保存为 {Path.GetFileName(uniquePath)}");
        }
        catch(Exception ex)
        {
            ShowError($"保存阵型失败: {ex.Message}");
        }
    });

    public ICommand SetFormationCommand => new RelayCommand(async _ =>
    {
        try
        {
            string selectedName = FormationInput;
            string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_Formations);
            string filePath = Path.Combine(dir, selectedName + ".json");

            if(!File.Exists(filePath))
            {
                ShowWarning($"阵型文件不存在：{selectedName}.json");
                return;
            }

            string jsonContent = await File.ReadAllTextAsync(filePath);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
            string base64 = Convert.ToBase64String(bytes);

            await _scriptExec.ExecuteAsync(
                Constants.SubFolders.Formation,
                "一键布阵",
                new Dictionary<string, string> { ["{JSON_BASE64}"] = base64 }
            );

            if(Formation_Sync_CardInput == Constants.c_Symbol_On)
            {
                await LoadSeedPacketsAsync(selectedName);
            }
        }
        catch(Exception ex)
        {
            ShowError($"应用阵型失败: {ex.Message}");
        }
    });

    public ICommand AddSeedPacketsCommand => new RelayCommand(async _ =>
    {
        try
        {
            string output = await _scriptExec.ExecuteWithResultAsync(
                Constants.SubFolders.Formation,
                "存储卡组",
                new Dictionary<string, string> { ["{NAME}"] = SeedPacketsInput }
            );

            string jsonBase64 = ExtractJsonFromOutput(output, "SEEDPACKET_JSON_START", "SEEDPACKET_JSON_END");

            if(string.IsNullOrEmpty(jsonBase64))
            {
                ShowError("未能从脚本输出中提取卡组数据。");
                return;
            }

            string jsonContent;
            try
            {
                byte[] bytes = Convert.FromBase64String(jsonBase64);
                jsonContent = System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch(FormatException)
            {
                ShowError("卡组数据损坏：Base64 解码失败。");
                return;
            }

            string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_SeedPackets);
            if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string uniquePath = GetUniqueFilePath(dir, SeedPacketsInput);
            await File.WriteAllTextAsync(uniquePath, jsonContent);

            LoadSeedPacketsOptions();
            Log.Info($"卡组已保存为 {Path.GetFileName(uniquePath)}");
        }
        catch(Exception ex)
        {
            ShowError($"保存卡组失败: {ex.Message}");
        }
    });

    public ICommand SetSeedPacketsCommand => new RelayCommand(async _ =>
    {
        try
        {
            string selectedName = SeedPacketsInput;
            string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_SeedPackets);
            string filePath = Path.Combine(dir, selectedName + ".json");

            if(!File.Exists(filePath))
            {
                ShowWarning($"卡组文件不存在：{selectedName}.json");
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
        catch(Exception ex)
        {
            ShowError($"加载卡组失败: {ex.Message}");
        }
    });

    public ICommand SetBgCommand => new RelayCommand(async _ =>
    {
        try
        {
            string bgValue = NameOption.GetValue(BgInput, BackgroundOptions);
            await _scriptExec.ExecuteAsync(Constants.SubFolders.Formation, "设置场景",
                new Dictionary<string, string> { ["{BACKGROUNDTYPE}"] = bgValue });
        }
        catch(Exception ex) { ShowError(ex.Message); }
    });

    public ICommand GridSquareTypeCommand => new RelayCommand(async _ =>
    {
        try
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
        }
        catch(Exception ex) { ShowError(ex.Message); }
    });

    public ICommand PlantRowTypeCommand => new RelayCommand(async _ =>
    {
        try
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
        }
        catch(Exception ex) { ShowError(ex.Message); }
    });

    public ICommand SetSpCommand => new RelayCommand(async _ =>
    {
        try
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
        }
        catch(Exception ex) { ShowError(ex.Message); }
    });

    public ICommand PickRandSeedCommand => new RelayCommand(async _ =>
    {
        try { await _scriptExec.ExecuteAsync(Constants.SubFolders.Formation, "随机选卡"); }
        catch(Exception ex) { ShowError(ex.Message); }
    });

    public ICommand ViewLawnCommand => new RelayCommand(async _ =>
    {
        try { await _scriptExec.ExecuteAsync(Constants.SubFolders.Formation, "查看草坪"); }
        catch(Exception ex) { ShowError(ex.Message); }
    });

    public ICommand ToggleFormationLadderCommand => new RelayCommand(_ => FormationLadder = ButtonHelper.ToggleCheck(FormationLadder));

    public ICommand ToggleFormationPlantCommand => new RelayCommand(_ => FormationPlant = ButtonHelper.ToggleCheck(FormationPlant));
    public ICommand ToggleFormationVaseCommand => new RelayCommand(_ => FormationVase = ButtonHelper.ToggleCheck(FormationVase));
    public ICommand ToggleGridSquareTogetCommand => new RelayCommand(_ => GridSquareTogetInput = ButtonHelper.ToggleCheck(GridSquareTogetInput));
    public ICommand ToggleImitaterSlotCommand => new RelayCommand(_ => ImitaterSlot = ButtonHelper.ToggleCheck(ImitaterSlot));
    public ICommand ToggleSpImitaterCommand => new RelayCommand(_ => SpInput3 = ButtonHelper.ToggleCheck(SpInput3));

    /// <summary>
    /// 从脚本输出中提取 JSON 数据（支持 Base64 或纯 JSON）
    /// </summary>
    private string ExtractJsonFromOutput(string output, string startMarker, string endMarker)
    {
        if(string.IsNullOrEmpty(output)) return null;

        if(!string.IsNullOrEmpty(startMarker) && !string.IsNullOrEmpty(endMarker))
        {
            int startIdx = output.IndexOf(startMarker);
            int endIdx = output.IndexOf(endMarker);

            if(startIdx != -1 && endIdx != -1 && endIdx > startIdx)
            {
                string content = output.Substring(startIdx + startMarker.Length, endIdx - startIdx - startMarker.Length).Trim();
                if(!string.IsNullOrEmpty(content))
                {
                    return Regex.Replace(content, @"\s+", "");
                }
            }
        }

        int braceStart = output.LastIndexOf('{');
        int braceEnd = output.LastIndexOf('}');

        if(braceStart != -1 && braceEnd != -1 && braceEnd > braceStart)
        {
            string potentialJson = output.Substring(braceStart, braceEnd - braceStart + 1);
            if(potentialJson.StartsWith("{") && potentialJson.EndsWith("}"))
            {
                return potentialJson;
            }
        }

        if(!output.Contains("{") && Regex.IsMatch(output.Trim(), @"^[A-Za-z0-9+/=]+$"))
        {
            return output.Trim();
        }

        return null;
    }

    private async Task SaveSeedPacketsAsync(string name)
    {
        string output = await _scriptExec.ExecuteWithResultAsync(
            Constants.SubFolders.Formation,
            "存储卡组",
            new Dictionary<string, string> { ["{NAME}"] = name }
        );

        string jsonBase64 = ExtractJsonFromOutput(output, "SEEDPACKET_JSON_START", "SEEDPACKET_JSON_END");

        if(string.IsNullOrEmpty(jsonBase64))
        {
            throw new Exception("未能提取卡组 JSON 数据");
        }

        string jsonContent;
        try
        {
            byte[] bytes = Convert.FromBase64String(jsonBase64);
            jsonContent = System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            jsonContent = jsonBase64;
        }

        string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_SeedPackets);
        if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string uniquePath = GetUniqueFilePath(dir, name);
        await File.WriteAllTextAsync(uniquePath, jsonContent);
        LoadSeedPacketsOptions();
    }

    private async Task LoadSeedPacketsAsync(string name)
    {
        string dir = Path.Combine(_defaultPath, Constants.Folder_Need, Constants.Folder_SeedPackets);
        string filePath = Path.Combine(dir, name + ".json");

        if(!File.Exists(filePath))
        {
            throw new FileNotFoundException($"卡组文件不存在：{name}.json");
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

    private void ShowError(string message)
    {
        Log.Error(message);
    }

    private void ShowWarning(string message)
    {
        Log.Warning(message);
    }
}
