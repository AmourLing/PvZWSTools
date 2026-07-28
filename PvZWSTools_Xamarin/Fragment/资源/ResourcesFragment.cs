using System.Collections.Generic;
using System.IO;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System.Linq;

namespace PvZWSTools_Xamarin;

public class ResourcesFragment:AndroidX.Fragment.App.Fragment
{
    private static readonly string mResourcesPath = "资源";
    private Dictionary<string, string> map;
    private Dictionary<string, Dictionary<string, string>> DamageOptions { get; set; }
    private Dictionary<string, Dictionary<string, string>> HealthOptions { get; set; }
    private Dictionary<string, Dictionary<string, string>> TimeOptions { get; set; }
    private Dictionary<string, Dictionary<string, string>> ValueOptions { get; set; }

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
        DamageOptions = new Dictionary<string, Dictionary<string, string>>();
        HealthOptions = new Dictionary<string, Dictionary<string, string>>();
        TimeOptions = new Dictionary<string, Dictionary<string, string>>();
        ValueOptions = new Dictionary<string, Dictionary<string, string>>();
        LoadOptions();
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.resources_fragment, container, false);
        view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
        {
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.resources_strings_1), new Dictionary<string, string>
            {
                [GetString(Resource.String.resources_strings_1_1_key)] = GetString(Resource.String.resources_strings_1_1_value),
                [GetString(Resource.String.resources_strings_1_2_key)] = GetString(Resource.String.resources_strings_1_2_value),
            }, mResourcesPath, GetString(Resource.String.resources_strings_1), new Dictionary<string, string>
            {
                ["{DAMAGE}"] = "0",
                ["{DAMAGE2}"] = "1"
            }, map, DamageOptions);
        };
        view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
        {
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.resources_strings_2), new Dictionary<string, string>
            {
                [GetString(Resource.String.resources_strings_2_1_key)] = GetString(Resource.String.resources_strings_2_1_value),
                [GetString(Resource.String.resources_strings_2_2_key)] = GetString(Resource.String.resources_strings_2_2_value),
            }, mResourcesPath, GetString(Resource.String.resources_strings_2), new Dictionary<string, string>
            {
                ["{HEALTH}"] = "0",
                ["{HEALTH2}"] = "1"
            }, map, HealthOptions);
        };
        view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
        {
            CreateInputDialog.OptAndDone2(Activity, GetString(Resource.String.resources_strings_3), new Dictionary<string, string>
            {
                [GetString(Resource.String.resources_strings_3_1_key)] = GetString(Resource.String.resources_strings_3_1_value),
            }, mResourcesPath, GetString(Resource.String.resources_strings_3), new Dictionary<string, string>
            {
                ["{COIN}"] = "0"
            }, map);
        };
        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
        {
            CreateInputDialog.OptAndDone2(Activity, GetString(Resource.String.resources_strings_4), new Dictionary<string, string>
            {
                [GetString(Resource.String.resources_strings_4_1_key)] = GetString(Resource.String.resources_strings_4_1_value),
            }, mResourcesPath, GetString(Resource.String.resources_strings_4), new Dictionary<string, string>
            {
                ["{COINLIMIT}"] = "0"
            }, map);
        };
        view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
        {
            CreateInputDialog.OptAndDone2(Activity, GetString(Resource.String.resources_strings_5), new Dictionary<string, string>
            {
                [GetString(Resource.String.resources_strings_5_1_key)] = GetString(Resource.String.resources_strings_5_1_value),
            }, mResourcesPath, GetString(Resource.String.resources_strings_5), new Dictionary<string, string>
            {
                ["{SUNMONEY}"] = "0"
            }, map);
        };
        view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
        {
            CreateInputDialog.OptAndDone2(Activity, GetString(Resource.String.resources_strings_6), new Dictionary<string, string>
            {
                [GetString(Resource.String.resources_strings_6_1_key)] = GetString(Resource.String.resources_strings_6_1_value),
            }, mResourcesPath, GetString(Resource.String.resources_strings_6), new Dictionary<string, string>
            {
                ["{SUNMONEYLIMIT}"] = "0"
            }, map);
        };
        view.FindViewById<Button>(Resource.Id.button7).Click += (sender, e) =>
        {
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.resources_strings_7), new Dictionary<string, string>
            {
                [GetString(Resource.String.resources_strings_7_1_key)] = GetString(Resource.String.resources_strings_7_1_value),
                [GetString(Resource.String.resources_strings_7_2_key)] = GetString(Resource.String.resources_strings_7_2_value),
            }, mResourcesPath, GetString(Resource.String.resources_strings_7), new Dictionary<string, string>
            {
                ["{TIME}"] = "0",
                ["{TIME2}"] = "1"
            }, map, TimeOptions);
        };
        view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
        {
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.resources_strings_8), new Dictionary<string, string>
            {
                [GetString(Resource.String.resources_strings_8_1_key)] = GetString(Resource.String.resources_strings_8_1_value),
                [GetString(Resource.String.resources_strings_8_2_key)] = GetString(Resource.String.resources_strings_8_2_value),
            }, mResourcesPath, GetString(Resource.String.resources_strings_8), new Dictionary<string, string>
            {
                ["{VALUE}"] = "0",
                ["{VALUE2}"] = "1"
            }, map, ValueOptions);
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
            [GetString(Resource.String.resources_strings_1_1_key)] = GetString(Resource.String.resources_strings_1_1_value),
            [GetString(Resource.String.resources_strings_1_2_key)] = GetString(Resource.String.resources_strings_1_2_value),

            [GetString(Resource.String.resources_strings_2_1_key)] = GetString(Resource.String.resources_strings_2_1_value),
            [GetString(Resource.String.resources_strings_2_2_key)] = GetString(Resource.String.resources_strings_2_2_value),

            [GetString(Resource.String.resources_strings_3_1_key)] = GetString(Resource.String.resources_strings_3_1_value),

            [GetString(Resource.String.resources_strings_4_1_key)] = GetString(Resource.String.resources_strings_4_1_value),

            [GetString(Resource.String.resources_strings_5_1_key)] = GetString(Resource.String.resources_strings_5_1_value),

            [GetString(Resource.String.resources_strings_6_1_key)] = GetString(Resource.String.resources_strings_6_1_value),

            [GetString(Resource.String.resources_strings_7_1_key)] = GetString(Resource.String.resources_strings_7_1_value),
            [GetString(Resource.String.resources_strings_7_2_key)] = GetString(Resource.String.resources_strings_7_2_value),

            [GetString(Resource.String.resources_strings_8_1_key)] = GetString(Resource.String.resources_strings_8_1_value),
            [GetString(Resource.String.resources_strings_8_2_key)] = GetString(Resource.String.resources_strings_8_2_value),
        };
    }

    private void LoadOptions()
    {
        try
        {
            string[] option = { "时间", "价值", "血量", "伤害" };
            var externalFilesDir = Android.App.Application.Context.GetExternalFilesDir(null);
            if(externalFilesDir == null)
            {
                Toast.MakeText(Activity, "无法访问外部存储", ToastLength.Long).Show();
                return;
            }
            var configPath = Path.Combine(externalFilesDir.AbsolutePath, "配置文件");
            foreach(var opt in option)
            {
                var filePath = Path.Combine(configPath, "选项", opt + ".json");
                if(!File.Exists(filePath))
                {
                    Toast.MakeText(Activity, $"选项文件不存在: {filePath}", ToastLength.Long).Show();
                    continue;
                }
                using(var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using(var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    var Options = JsonConvert.DeserializeObject<List<NameOption>>(json);
                    var Dict = Options.ToDictionary(
                        opt => opt.Name,
                        opt => opt.Value
                    );
                    switch(opt)
                    {
                        case "时间":
                            TimeOptions[GetString(Resource.String.resources_strings_7_1_key)] = Dict;
                            break;

                        case "价值":
                            ValueOptions[GetString(Resource.String.resources_strings_8_1_key)] = Dict;
                            break;

                        case "血量":
                            HealthOptions[GetString(Resource.String.resources_strings_2_1_key)] = Dict;
                            break;

                        case "伤害":
                            DamageOptions[GetString(Resource.String.resources_strings_1_1_key)] = Dict;
                            break;

                        default:
                            break;
                    }
                }
            }
        }
        catch(System.Exception ex)
        {
            Toast.MakeText(Activity, $"加载选项失败: {ex.Message}", ToastLength.Long).Show();
        }
    }
}
