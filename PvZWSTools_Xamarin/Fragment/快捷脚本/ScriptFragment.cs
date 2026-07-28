using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Xamarin;

public class ScriptFragment:AndroidX.Fragment.App.Fragment
{
    private Dictionary<string, string> map;
    private Dictionary<string, Dictionary<string, string>> ModOptions { get; set; }

    public override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        if(savedInstanceState != null)
        {
            var keys = savedInstanceState.GetStringArrayList("savedMapKeys");
            var values = savedInstanceState.GetStringArrayList("savedMapValues");
            map = new Dictionary<string, string>();
            for(int i = 0;i < keys.Count;i++)
            {
                map[keys[i]] = values[i];
            }
        }
        else
        {
            InitializeMap();
        }
        ModOptions = new Dictionary<string, Dictionary<string, string>>();
        LoadOptions();
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.script_fragment, container, false);
        Button btnScript = view.FindViewById<Button>(Resource.Id.button1);
        btnScript.Click += (sender, e) =>
        {
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.script_strings_1), new Dictionary<string, string>
            {
                [GetString(Resource.String.script_strings_1_0_key)] = map[GetString(Resource.String.script_strings_1_0_key)],
                [GetString(Resource.String.script_strings_1_1_key)] = map[GetString(Resource.String.script_strings_1_1_key)],
                [GetString(Resource.String.script_strings_1_2_key)] = map[GetString(Resource.String.script_strings_1_2_key)],
                [GetString(Resource.String.script_strings_1_3_key)] = map[GetString(Resource.String.script_strings_1_3_key)],
                [GetString(Resource.String.script_strings_1_4_key)] = map[GetString(Resource.String.script_strings_1_4_key)],
                [GetString(Resource.String.script_strings_1_5_key)] = map[GetString(Resource.String.script_strings_1_5_key)],
                [GetString(Resource.String.script_strings_1_6_key)] = map[GetString(Resource.String.script_strings_1_6_key)],
                [GetString(Resource.String.script_strings_1_7_key)] = map[GetString(Resource.String.script_strings_1_7_key)],
                [GetString(Resource.String.script_strings_1_8_key)] = map[GetString(Resource.String.script_strings_1_8_key)],
            }, "快捷脚本", string.Empty, new Dictionary<string, string>
            {
                ["{NAME}"] = "0",
                ["{1}"] = "1",
                ["{2}"] = "2",
                ["{3}"] = "3",
                ["{4}"] = "4",
                ["{5}"] = "5",
                ["{6}"] = "6",
                ["{7}"] = "7",
                ["{8}"] = "8",
            }, map, ModOptions);
        };
        return view;
    }

    public override void OnSaveInstanceState(Bundle outState)
    {
        base.OnSaveInstanceState(outState);
        var keys = new List<string>(map.Keys);
        var values = new List<string>(map.Values);
        outState.PutStringArrayList("savedMapKeys", keys);
        outState.PutStringArrayList("savedMapValues", values);
    }

    private void InitializeMap()
    {
        map = new Dictionary<string, string>
        {
            [GetString(Resource.String.script_strings_1_0_key)] = GetString(Resource.String.script_strings_1_0_value),
            [GetString(Resource.String.script_strings_1_1_key)] = GetString(Resource.String.script_strings_1_1_value),
            [GetString(Resource.String.script_strings_1_2_key)] = GetString(Resource.String.script_strings_1_2_value),
            [GetString(Resource.String.script_strings_1_3_key)] = GetString(Resource.String.script_strings_1_3_value),
            [GetString(Resource.String.script_strings_1_4_key)] = GetString(Resource.String.script_strings_1_4_value),
            [GetString(Resource.String.script_strings_1_5_key)] = GetString(Resource.String.script_strings_1_5_value),
            [GetString(Resource.String.script_strings_1_6_key)] = GetString(Resource.String.script_strings_1_6_value),
            [GetString(Resource.String.script_strings_1_7_key)] = GetString(Resource.String.script_strings_1_7_value),
            [GetString(Resource.String.script_strings_1_8_key)] = GetString(Resource.String.script_strings_1_8_value),
        };
    }

    private void LoadOptions()
    {
        try
        {
            var externalFilesDir = Android.App.Application.Context.GetExternalFilesDir(null);
            if(externalFilesDir == null)
            {
                Toast.MakeText(Activity, "无法访问外部存储", ToastLength.Long).Show();
                return;
            }
            var scriptPath = Path.Combine(externalFilesDir.AbsolutePath, "配置文件", "快捷脚本");
            if(!Directory.Exists(scriptPath))
            {
                Toast.MakeText(Activity, "快捷脚本文件夹不存在或为空", ToastLength.Short).Show();
                return;
            }
            var scriptFiles = Directory.GetFiles(scriptPath, "*.py", SearchOption.TopDirectoryOnly)
                .Where(file => Path.GetExtension(file).Equals(".py", StringComparison.OrdinalIgnoreCase))
                .Select(file => Path.GetFileName(file))
                .ToList();
            if(scriptFiles.Count == 0)
            {
                Toast.MakeText(Activity, "快捷脚本文件夹中没有找到.py文件", ToastLength.Short).Show();
                ModOptions[GetString(Resource.String.script_strings_1_0_key)] = new Dictionary<string, string>();
                return;
            }
            var options = new List<NameOption>();
            foreach(var fileName in scriptFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                options.Add(new NameOption
                {
                    Name = fileNameWithoutExtension, // 如果你想要显示名称，可以使用这个
                    Value = fileNameWithoutExtension  // 或者可以使用完整路径：Path.Combine(scriptPath, fileName)
                });
            }

            var dict = options.ToDictionary(
                opt => opt.Value,   // Key
                opt => opt.Name     // Value（显示名称）
            );
            ModOptions[GetString(Resource.String.script_strings_1_0_key)] = dict;
        }
        catch(Exception ex)
        {
            Toast.MakeText(Activity, $"加载选项失败: {ex.Message}", ToastLength.Long).Show();
            Console.WriteLine($"LoadOptions 错误: {ex}");
        }
    }
}
