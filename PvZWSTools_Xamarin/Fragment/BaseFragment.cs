using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;

namespace PvZWSTools_Xamarin;

public abstract class BaseFragment:AndroidX.Fragment.App.Fragment
{
    protected Dictionary<string, string> Map { get; set; } = new Dictionary<string, string>();

    protected Dictionary<string, Dictionary<string, string>> AllOptions { get; set; } = new Dictionary<string, Dictionary<string, string>>();

    protected abstract string FragmentPath { get; }

    protected abstract Dictionary<int, string> OptionFileMappings { get; }

    public override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if(savedInstanceState != null)
        {
            var keys = savedInstanceState.GetStringArrayList("savedMapKeys");
            var values = savedInstanceState.GetStringArrayList("savedMapValues");
            if(keys != null && values != null && keys.Count == values.Count)
            {
                for(int i = 0;i < keys.Count;i++)
                {
                    Map[keys[i]] = values[i];
                }
            }
            else
            {
                InitializeMapInternal();
            }
        }
        else
        {
            InitializeMapInternal();
        }

        LoadOptionsInternal();
    }

    public override void OnSaveInstanceState(Bundle outState)
    {
        base.OnSaveInstanceState(outState);
        var keys = new List<string>(Map.Keys);
        var values = new List<string>(Map.Values);
        outState.PutStringArrayList("savedMapKeys", keys);
        outState.PutStringArrayList("savedMapValues", values);
    }

    /// <summary>
    /// 子类必须实现此方法来初始化默认值
    /// </summary>
    protected abstract void InitializeMap();

    private void InitializeMapInternal()
    {
        Map.Clear();
        InitializeMap();
    }

    private void LoadOptionsInternal()
    {
        try
        {
            var externalFilesDir = Android.App.Application.Context.GetExternalFilesDir(null);
            if(externalFilesDir == null) return;

            var configPath = Path.Combine(externalFilesDir.AbsolutePath, "配置文件", "选项");

            var uniqueFiles = OptionFileMappings.Values.Distinct().ToList();

            foreach(var fileName in uniqueFiles)
            {
                var filePath = Path.Combine(configPath, fileName + ".json");
                if(!File.Exists(filePath)) continue;

                using(var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using(var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    var optionsList = JsonConvert.DeserializeObject<List<NameOption>>(json);
                    if(optionsList == null || !optionsList.Any()) continue;

                    var dict = optionsList.ToDictionary(opt => opt.Name, opt => opt.Value);
                    foreach(var mapping in OptionFileMappings)
                    {
                        if(mapping.Value == fileName)
                        {
                            string key = GetString(mapping.Key);
                            if(!string.IsNullOrEmpty(key))
                            {
                                AllOptions[key] = dict;
                            }
                        }
                    }
                }
            }
        }
        catch(System.Exception ex)
        {
            Toast.MakeText(Activity, $"加载选项失败: {ex.Message}", ToastLength.Long).Show();
        }
    }

    /// <summary>
    /// 获取指定Key的下拉选项，如果不存在则返回空字典
    /// </summary>
    protected Dictionary<string, string> GetOptionsForKey(string key)
    {
        return AllOptions.ContainsKey(key) ? AllOptions[key] : new Dictionary<string, string>();
    }

    /// <summary>
    /// 构建下拉选项字典用于 OptAndDone3
    /// </summary>
    protected Dictionary<string, Dictionary<string, string>> BuildDropdownOptions(params string[] keys)
    {
        var dropdown = new Dictionary<string, Dictionary<string, string>>();
        foreach(var key in keys)
        {
            dropdown[key] = GetOptionsForKey(key);
        }
        return dropdown;
    }

    /// <summary>
    /// 构建初始数据字典用于 OptAndDone3
    /// </summary>
    protected Dictionary<string, string> BuildInitialData(params string[] keys)
    {
        var data = new Dictionary<string, string>();
        foreach(var key in keys)
        {
            data[key] = Map.ContainsKey(key) ? Map[key] : "";
        }
        return data;
    }
}
