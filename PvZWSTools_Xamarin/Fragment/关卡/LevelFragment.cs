using System.Collections.Generic;
using System.IO;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System.Linq;

namespace PvZWSTools_Xamarin
{
    public class LevelFragment:AndroidX.Fragment.App.Fragment
    {
        private static readonly string mLevelPath = "关卡";
        private Dictionary<string, string> map;
        private Dictionary<string, Dictionary<string, string>> ModeOptions { get; set; }

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
            ModeOptions = new Dictionary<string, Dictionary<string, string>>();
            LoadOptions();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.level_fragment, container, false);
            view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
            {
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.level_strings_1), new Dictionary<string, string>
                {
                }, mLevelPath, GetString(Resource.String.level_strings_1), new Dictionary<string, string>
                {
                });
            };
            view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
            {
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.level_strings_2), new Dictionary<string, string>
                {
                }, mLevelPath, GetString(Resource.String.level_strings_2), new Dictionary<string, string>
                {
                });
            };
            view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
            {
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.level_strings_3), new Dictionary<string, string>
                {
                    [GetString(Resource.String.level_strings_3_1_key)] = GetString(Resource.String.level_strings_3_1_value),
                }, mLevelPath, GetString(Resource.String.level_strings_3), new Dictionary<string, string>
                {
                    ["{FLAG}"] = "0"
                });
            };
            view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
            {
                CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.level_strings_4), new Dictionary<string, string>
                {
                    [GetString(Resource.String.level_strings_4_1_key)] = GetString(Resource.String.level_strings_4_1_value),
                    [GetString(Resource.String.level_strings_4_2_key)] = GetString(Resource.String.level_strings_4_2_value),
                }, mLevelPath, GetString(Resource.String.level_strings_4), new Dictionary<string, string>
                {
                    ["{GAMEMODE}"] = "0",
                    ["{ADVENTURENUM}"] = "1"
                }, map, ModeOptions);
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
                [GetString(Resource.String.level_strings_3_1_key)] = GetString(Resource.String.board_strings_3_1_value),
                [GetString(Resource.String.level_strings_4_1_key)] = GetString(Resource.String.board_strings_4_1_value),
                [GetString(Resource.String.level_strings_4_2_key)] = GetString(Resource.String.board_strings_4_2_value),
            };
        }

        private void LoadOptions()
        {
            try
            {
                string[] option = { "模式" };
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
                            case "模式":
                                ModeOptions[GetString(Resource.String.level_strings_4_1_key)] = Dict;
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
}
