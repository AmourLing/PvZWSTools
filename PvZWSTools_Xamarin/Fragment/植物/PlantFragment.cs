using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace PvZWSTools_Xamarin
{
    public class PlantFragment:AndroidX.Fragment.App.Fragment
    {
        private static readonly string mPlantPath = "植物";
        private Dictionary<string, string> map;
        private Dictionary<string, Dictionary<string, string>> PlantOptions { get; set; }

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
            PlantOptions = new Dictionary<string, Dictionary<string, string>>();
            LoadOptions();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.plant_fragment, container, false);
            view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.plant_strings_1), new Dictionary<string, string>
                {
                    [GetString(Resource.String.plant_strings_1_1_key)] = GetString(Resource.String.plant_strings_1_1_value)
                }, mPlantPath, GetString(Resource.String.plant_strings_1), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.plant_strings_2), new Dictionary<string, string>
                {
                    [GetString(Resource.String.plant_strings_2_1_key)] = GetString(Resource.String.plant_strings_2_1_value)
                }, mPlantPath, GetString(Resource.String.plant_strings_2), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.plant_strings_3), new Dictionary<string, string>
                {
                    [GetString(Resource.String.plant_strings_3_1_key)] = GetString(Resource.String.plant_strings_3_1_value)
                }, mPlantPath, GetString(Resource.String.plant_strings_3), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
                CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.plant_strings_4), new Dictionary<string, string>
                {
                    [GetString(Resource.String.plant_strings_4_1_key)] = GetString(Resource.String.plant_strings_4_1_value)
                }, mPlantPath, GetString(Resource.String.plant_strings_4), new Dictionary<string, string>
                {
                    ["{SEEDTYPE}"] = "0",
                }, map, PlantOptions);
            view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.plant_strings_5), new Dictionary<string, string>
                {
                    [GetString(Resource.String.plant_strings_5_1_key)] = GetString(Resource.String.plant_strings_5_1_value)
                }, mPlantPath, GetString(Resource.String.plant_strings_5), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.plant_strings_6), new Dictionary<string, string>
                {
                    [GetString(Resource.String.plant_strings_6_1_key)] = GetString(Resource.String.plant_strings_6_1_value)
                }, mPlantPath, GetString(Resource.String.plant_strings_6), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button7).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.plant_strings_7), new Dictionary<string, string>
                {
                    [GetString(Resource.String.plant_strings_7_1_key)] = GetString(Resource.String.plant_strings_7_1_value)
                }, mPlantPath, GetString(Resource.String.plant_strings_7), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.plant_strings_8), new Dictionary<string, string>
                {
                    [GetString(Resource.String.plant_strings_8_1_key)] = GetString(Resource.String.plant_strings_8_1_value)
                }, mPlantPath, GetString(Resource.String.plant_strings_8), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button9).Click += (sender, e) =>
               CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.plant_strings_9), new Dictionary<string, string>
               {
                   [GetString(Resource.String.plant_strings_9_1_key)] = GetString(Resource.String.plant_strings_9_1_value)
               }, mPlantPath, GetString(Resource.String.plant_strings_9), new Dictionary<string, string>
               {
                   ["{CHECK}"] = "0",
               });
            view.FindViewById<Button>(Resource.Id.button10).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.plant_strings_10), new Dictionary<string, string>
                {
                    [GetString(Resource.String.plant_strings_10_1_key)] = GetString(Resource.String.plant_strings_10_1_value)
                }, mPlantPath, GetString(Resource.String.plant_strings_10), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button11).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.plant_strings_11), new Dictionary<string, string>
                {
                    [GetString(Resource.String.plant_strings_11_1_key)] = GetString(Resource.String.plant_strings_11_1_value)
                }, mPlantPath, GetString(Resource.String.plant_strings_11), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button12).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.plant_strings_12), new Dictionary<string, string>
                {
                    [GetString(Resource.String.plant_strings_12_1_key)] = GetString(Resource.String.plant_strings_12_1_value)
                }, mPlantPath, GetString(Resource.String.plant_strings_12), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
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
                [GetString(Resource.String.plant_strings_1_1_key)] = GetString(Resource.String.plant_strings_1_1_value),
                [GetString(Resource.String.plant_strings_2_1_key)] = GetString(Resource.String.plant_strings_2_1_value),
                [GetString(Resource.String.plant_strings_3_1_key)] = GetString(Resource.String.plant_strings_3_1_value),
                [GetString(Resource.String.plant_strings_4_1_key)] = GetString(Resource.String.plant_strings_4_1_value),
                [GetString(Resource.String.plant_strings_5_1_key)] = GetString(Resource.String.plant_strings_5_1_value),
                [GetString(Resource.String.plant_strings_6_1_key)] = GetString(Resource.String.plant_strings_6_1_value),
                [GetString(Resource.String.plant_strings_7_1_key)] = GetString(Resource.String.plant_strings_7_1_value),
                [GetString(Resource.String.plant_strings_8_1_key)] = GetString(Resource.String.plant_strings_8_1_value),
                [GetString(Resource.String.plant_strings_9_1_key)] = GetString(Resource.String.plant_strings_9_1_value),
                [GetString(Resource.String.plant_strings_10_1_key)] = GetString(Resource.String.plant_strings_10_1_value),
                [GetString(Resource.String.plant_strings_11_1_key)] = GetString(Resource.String.plant_strings_11_1_value),
                [GetString(Resource.String.plant_strings_12_1_key)] = GetString(Resource.String.plant_strings_12_1_value),
            };
        }

        private void LoadOptions()
        {
            try
            {
                string[] option = { "植物" };
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
                            case "植物":
                                PlantOptions[GetString(Resource.String.plant_strings_4_1_key)] = Dict;
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
