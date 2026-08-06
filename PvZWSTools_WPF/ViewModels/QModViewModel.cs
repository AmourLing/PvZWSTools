using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PvZWSTools_WPF.Commands;
using PvZWSTools_WPF.Helpers;
using PvZWSTools_WPF.Models;
using PvZWSTools_WPF.Services;

namespace PvZWSTools_WPF.ViewModels;

public class QModViewModel:ViewModelBase
{
    private readonly string _defaultPath;
    private readonly IScriptExecutionService _scriptExec;
    private string _author;
    private string _infoAll;
    private ObservableCollection<ScriptParameter> _parameters;
    private string _qMod;
    private bool _qModDropdownToggleIsChecked;
    private ObservableCollection<NameOption> _qModOptions;
    private NameOption _qModSelected;

    public QModViewModel(IScriptExecutionService scriptExec, string defaultPath)
    {
        _scriptExec = scriptExec;
        _defaultPath = defaultPath;
        Parameters = new ObservableCollection<ScriptParameter>();
        LoadQModOptions();
    }

    public string Author
    {
        get => _author;
        set
        {
            _author = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayAuthor));
        }
    }

    public string DisplayAuthor => string.IsNullOrEmpty(Author) ? null : $"QMod作者: {Author}";

    public string InfoAll
    {
        get => _infoAll;
        set { _infoAll = value; OnPropertyChanged(); }
    }

    public ObservableCollection<ScriptParameter> Parameters
    {
        get => _parameters;
        set { _parameters = value; OnPropertyChanged(); }
    }

    public string QMod
    {
        get => _qMod;
        set { _qMod = value; OnPropertyChanged(); }
    }

    public ICommand QModCommand => new RelayCommand(async _ =>
    {
        if(QModSelected == null)
        {
            Log.Warning("请先选择一个快捷脚本");
            return;
        }

        string scriptContent = await File.ReadAllTextAsync(QModSelected.Value);

        foreach(var param in Parameters)
        {
            scriptContent = scriptContent.Replace(param.Placeholder, param.Value);
        }

        if(_scriptExec is ScriptExecutionService execService)
        {
            await execService.SendRawScriptAsync(scriptContent);
        }
    });

    public bool QModDropdownToggleIsChecked
    {
        get => _qModDropdownToggleIsChecked;
        set { _qModDropdownToggleIsChecked = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameOption> QModOptions
    {
        get => _qModOptions;
        set { _qModOptions = value; OnPropertyChanged(); }
    }

    public NameOption QModSelected
    {
        get => _qModSelected;
        set
        {
            _qModSelected = value;
            if(value != null)
            {
                QMod = value.Name;
                LoadScriptParameters(value.Value);
            }
            QModDropdownToggleIsChecked = false;
            OnPropertyChanged();
        }
    }

    private void LoadQModOptions()
    {
        QModOptions = new ObservableCollection<NameOption>();

        string scriptsDir = Path.Combine(_defaultPath, Constants.Folder_Need, "快捷脚本");
        if(!Directory.Exists(scriptsDir))
        {
            _ = Directory.CreateDirectory(scriptsDir);
            return;
        }

        var scriptFiles = Directory.GetFiles(scriptsDir, "*.py");
        foreach(var file in scriptFiles)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            QModOptions.Add(new NameOption { Name = name, Value = file });
        }
    }

    private void LoadScriptParameters(string scriptPath)
    {
        Author = null;
        InfoAll = null;
        Parameters.Clear();
        string configPath = scriptPath + ".config.json";
        Log.Info($"尝试加载脚本配置: {configPath}");

        if(!File.Exists(configPath))
        {
            Log.Debug($"配置文件不存在: {configPath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(configPath);
            Log.Debug($"配置文件内容长度: {json.Length} 字符");
            var config = JsonConvert.DeserializeObject<ScriptConfig>(json);

            if(config == null)
            {
                Log.Debug("反序列化失败，config 为 null");
                return;
            }
            if(config.Author == null)
            {
                Log.Debug("配置中 author 字段为空或不存在");
            }
            Author = config.Author;
            if(config.InfoAll == null)
            {
                Log.Debug("配置中 InfoAll 字段为空或不存在");
            }
            InfoAll = config.InfoAll;
            if(config.Replace == null || config.Replace.Count == 0)
            {
                Log.Debug("配置中 replace 字段为空或不存在");
                return;
            }
            Log.Debug($"发现 {config.Replace.Count} 个参数定义");
            foreach(var kv in config.Replace)
            {
                Log.Debug($"处理占位符: {kv.Key}, Info: {kv.Value.Info}");
                string controlType = "TextBox";
                List<string> options = new List<string>();
                string defaultValue = "";
                if(kv.Value.Value != null)
                {
                    Log.Debug($"Value 类型: {kv.Value.Value.Type}");
                    if(kv.Value.Value.Type == JTokenType.Array)
                    {
                        controlType = "ComboBox";
                        var arr = (JArray)kv.Value.Value;
                        options = arr.Select(t => t.ToString()).ToList();
                        Log.Debug($"数组选项数量: {options.Count}, 内容: {string.Join(",", options)}");
                    }
                    else
                    {
                        Log.Debug($"单值内容: {kv.Value.Value}");
                    }
                }
                if(kv.Value.Default != null)
                {
                    defaultValue = kv.Value.Default.ToString();
                    Log.Debug($"Default 字段存在，值为: {defaultValue}");
                }
                else if(controlType == "ComboBox" && options.Any())
                {
                    defaultValue = options.First();
                    Log.Debug($"未指定 Default，使用数组第一项: {defaultValue}");
                }
                var param = new ScriptParameter
                {
                    Placeholder = kv.Key,
                    Description = kv.Value.Info,
                    ControlType = controlType,
                    Options = options,
                    Value = defaultValue
                };
                Log.Debug($"参数构建完成: 占位符={param.Placeholder}, 控件类型={param.ControlType}, 选项数={param.Options.Count}, 默认值={param.Value}");
                Parameters.Add(param);
            }
            Log.Debug($"参数加载完成，共 {Parameters.Count} 个参数");
            Log.Info($"脚本{config.Name}加载成功");
        }
        catch(Exception ex)
        {
            Log.Debug($"加载脚本配置异常: {ex.Message}");
        }
    }
}

public class ScriptConfig
{
    public string Author { get; set; }
    public string InfoAll { get; set; }
    public string Name { get; set; }
    public Dictionary<string, ScriptParameterConfig> Replace { get; set; }
}

public class ScriptParameterConfig
{
    public JToken Default { get; set; }
    public string Info { get; set; }
    public JToken Value { get; set; }
}
