using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace PvZWSTools_Xamarin
{
    public class BoardFragment:AndroidX.Fragment.App.Fragment
    {
        private static readonly string mBoardPath = "战场";
        private Dictionary<string, string> map;
        private Dictionary<string, Dictionary<string, string>> BoardColOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> BoarddeltamXOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> BoarddeltamYOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> BoardRowOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> CoinOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> ItemOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> PlantOptions { get; set; }
        private Dictionary<string, Dictionary<string, string>> ZombieOptions { get; set; }

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
            ZombieOptions = new Dictionary<string, Dictionary<string, string>>();
            CoinOptions = new Dictionary<string, Dictionary<string, string>>();
            ItemOptions = new Dictionary<string, Dictionary<string, string>>();
            BoardColOptions = new Dictionary<string, Dictionary<string, string>>();
            BoardRowOptions = new Dictionary<string, Dictionary<string, string>>();
            BoarddeltamXOptions = new Dictionary<string, Dictionary<string, string>>();
            BoarddeltamYOptions = new Dictionary<string, Dictionary<string, string>>();
            LoadOptions();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.board_fragment, container, false);
            view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
            {
                var dropdownOptionsAddPlant = new Dictionary<string, Dictionary<string, string>>();
                dropdownOptionsAddPlant[GetString(Resource.String.board_strings_1_1_key)] = BoardRowOptions[GetString(Resource.String.board_strings_1_1_key)];
                dropdownOptionsAddPlant[GetString(Resource.String.board_strings_1_2_key)] = BoardColOptions[GetString(Resource.String.board_strings_1_2_key)];
                dropdownOptionsAddPlant[GetString(Resource.String.board_strings_1_3_key)] = PlantOptions[GetString(Resource.String.board_strings_1_3_key)];
                dropdownOptionsAddPlant[GetString(Resource.String.board_strings_1_6_key)] = BoarddeltamXOptions[GetString(Resource.String.board_strings_1_6_key)];
                dropdownOptionsAddPlant[GetString(Resource.String.board_strings_1_7_key)] = BoarddeltamYOptions[GetString(Resource.String.board_strings_1_7_key)];

                CreateInputDialog.OptAndDone4(Activity, GetString(Resource.String.board_strings_1), new Dictionary<string, string>
                {
                    [GetString(Resource.String.board_strings_1_1_key)] = map[GetString(Resource.String.board_strings_1_1_key)],
                    [GetString(Resource.String.board_strings_1_2_key)] = map[GetString(Resource.String.board_strings_1_2_key)],
                    [GetString(Resource.String.board_strings_1_3_key)] = map[GetString(Resource.String.board_strings_1_3_key)],
                    [GetString(Resource.String.board_strings_1_4_key)] = map[GetString(Resource.String.board_strings_1_4_key)],
                    [GetString(Resource.String.board_strings_1_5_key)] = map[GetString(Resource.String.board_strings_1_5_key)],
                    [GetString(Resource.String.board_strings_1_6_key)] = map[GetString(Resource.String.board_strings_1_6_key)],
                    [GetString(Resource.String.board_strings_1_7_key)] = map[GetString(Resource.String.board_strings_1_7_key)],
                    [GetString(Resource.String.board_strings_1_8_key)] = map[GetString(Resource.String.board_strings_1_8_key)],
                }, mBoardPath, GetString(Resource.String.board_strings_1), new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{COL}"] = "1",
                    ["{SEEDTYPE}"] = "2",
                    ["{IMITATER}"] = "3",
                    ["{LIMITPLANTING}"] = "4",
                    ["{DELTA_MX}"] = "5",
                    ["{DELTA_MY}"] = "6",
                    ["{ISSLEEPING}"] = "7"
                }, map, dropdownOptionsAddPlant);
            };

            view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
            {
                var dropdownOptionsAddZombie = new Dictionary<string, Dictionary<string, string>>();
                dropdownOptionsAddZombie[GetString(Resource.String.board_strings_2_2_key)] = ZombieOptions[GetString(Resource.String.board_strings_2_2_key)];
                dropdownOptionsAddZombie[GetString(Resource.String.board_strings_2_1_key)] = BoardRowOptions[GetString(Resource.String.board_strings_2_1_key)];
                dropdownOptionsAddZombie[GetString(Resource.String.board_strings_2_4_key)] = BoardColOptions[GetString(Resource.String.board_strings_2_4_key)];
                dropdownOptionsAddZombie[GetString(Resource.String.board_strings_2_6_key)] = BoarddeltamXOptions[GetString(Resource.String.board_strings_2_6_key)];
                dropdownOptionsAddZombie[GetString(Resource.String.board_strings_2_7_key)] = BoarddeltamYOptions[GetString(Resource.String.board_strings_2_7_key)];

                CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.board_strings_2), new Dictionary<string, string>
                {
                    [GetString(Resource.String.board_strings_2_1_key)] = map[GetString(Resource.String.board_strings_2_1_key)],
                    [GetString(Resource.String.board_strings_2_2_key)] = map[GetString(Resource.String.board_strings_2_2_key)],
                    [GetString(Resource.String.board_strings_2_3_key)] = map[GetString(Resource.String.board_strings_2_3_key)],
                    [GetString(Resource.String.board_strings_2_4_key)] = map[GetString(Resource.String.board_strings_2_4_key)],
                    [GetString(Resource.String.board_strings_2_5_key)] = map[GetString(Resource.String.board_strings_2_5_key)],
                    [GetString(Resource.String.board_strings_2_6_key)] = map[GetString(Resource.String.board_strings_2_6_key)],
                    [GetString(Resource.String.board_strings_2_7_key)] = map[GetString(Resource.String.board_strings_2_7_key)],
                }, mBoardPath, GetString(Resource.String.board_strings_2), new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{ZOMBIETYPE}"] = "1",
                    ["{COLPERMIT}"] = "2",
                    ["{COL}"] = "3",
                    ["{MINDCONTROL}"] = "4",
                    ["{DELTA_MX}"] = "5",
                    ["{DELTA_MY}"] = "6"
                }, map, dropdownOptionsAddZombie);
            };
            view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
            {
                var dropdownOptionsForAddCoin = new Dictionary<string, Dictionary<string, string>>();
                dropdownOptionsForAddCoin[GetString(Resource.String.board_strings_3_1_key)] = BoardRowOptions[GetString(Resource.String.board_strings_3_1_key)];
                dropdownOptionsForAddCoin[GetString(Resource.String.board_strings_3_2_key)] = BoardColOptions[GetString(Resource.String.board_strings_3_2_key)];
                dropdownOptionsForAddCoin[GetString(Resource.String.board_strings_3_3_key)] = CoinOptions[GetString(Resource.String.board_strings_3_3_key)];
                dropdownOptionsForAddCoin[GetString(Resource.String.board_strings_3_4_key)] = BoarddeltamXOptions[GetString(Resource.String.board_strings_3_4_key)];
                dropdownOptionsForAddCoin[GetString(Resource.String.board_strings_3_5_key)] = BoarddeltamYOptions[GetString(Resource.String.board_strings_3_5_key)];

                CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.board_strings_3), new Dictionary<string, string>
                {
                    [GetString(Resource.String.board_strings_3_1_key)] = map[GetString(Resource.String.board_strings_3_1_key)],
                    [GetString(Resource.String.board_strings_3_2_key)] = map[GetString(Resource.String.board_strings_3_2_key)],
                    [GetString(Resource.String.board_strings_3_3_key)] = map[GetString(Resource.String.board_strings_3_3_key)],
                    [GetString(Resource.String.board_strings_3_4_key)] = map[GetString(Resource.String.board_strings_3_4_key)],
                    [GetString(Resource.String.board_strings_3_5_key)] = map[GetString(Resource.String.board_strings_3_5_key)],
                }, mBoardPath, GetString(Resource.String.board_strings_3), new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{COL}"] = "1",
                    ["{COINTYPE}"] = "2",
                    ["{DELTA_MX}"] = "3",
                    ["{DELTA_MY}"] = "4"
                }, map, dropdownOptionsForAddCoin);
            };
            view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
            {
                var dropdownOptionsForAddItem = new Dictionary<string, Dictionary<string, string>>();
                dropdownOptionsForAddItem[GetString(Resource.String.board_strings_4_1_key)] = BoardRowOptions[GetString(Resource.String.board_strings_4_1_key)];
                dropdownOptionsForAddItem[GetString(Resource.String.board_strings_4_2_key)] = BoardColOptions[GetString(Resource.String.board_strings_4_2_key)];
                dropdownOptionsForAddItem[GetString(Resource.String.board_strings_4_3_key)] = ItemOptions[GetString(Resource.String.board_strings_4_3_key)];
                dropdownOptionsForAddItem[GetString(Resource.String.board_strings_4_4_key)] = PlantOptions[GetString(Resource.String.board_strings_4_4_key)];
                dropdownOptionsForAddItem[GetString(Resource.String.board_strings_4_5_key)] = ZombieOptions[GetString(Resource.String.board_strings_4_5_key)];
                dropdownOptionsForAddItem[GetString(Resource.String.board_strings_4_8_key)] = BoarddeltamXOptions[GetString(Resource.String.board_strings_4_8_key)];
                dropdownOptionsForAddItem[GetString(Resource.String.board_strings_4_9_key)] = BoarddeltamYOptions[GetString(Resource.String.board_strings_4_9_key)];

                CreateInputDialog.OptAndDone4(Activity, GetString(Resource.String.board_strings_4), new Dictionary<string, string>
                {
                    [GetString(Resource.String.board_strings_4_1_key)] = map[GetString(Resource.String.board_strings_4_1_key)],
                    [GetString(Resource.String.board_strings_4_2_key)] = map[GetString(Resource.String.board_strings_4_2_key)],
                    [GetString(Resource.String.board_strings_4_3_key)] = map[GetString(Resource.String.board_strings_4_3_key)],
                    [GetString(Resource.String.board_strings_4_4_key)] = map[GetString(Resource.String.board_strings_4_4_key)],
                    [GetString(Resource.String.board_strings_4_5_key)] = map[GetString(Resource.String.board_strings_4_5_key)],
                    [GetString(Resource.String.board_strings_4_6_key)] = map[GetString(Resource.String.board_strings_4_6_key)],
                    [GetString(Resource.String.board_strings_4_7_key)] = map[GetString(Resource.String.board_strings_4_7_key)],
                    [GetString(Resource.String.board_strings_4_8_key)] = map[GetString(Resource.String.board_strings_4_8_key)],
                    [GetString(Resource.String.board_strings_4_9_key)] = map[GetString(Resource.String.board_strings_4_9_key)],
                }, mBoardPath, GetString(Resource.String.board_strings_4), new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{COL}"] = "1",
                    ["{ITEM}"] = "2",
                    ["{SCARYPOT_SEEDTYPE}"] = "3",
                    ["{SCARYPOT_ZOMBIETYPE}"] = "4",
                    ["{SCARYPOT_SCARYPOTTYPE}"] = "5",
                    ["{SCARYPOT_STATE}"] = "6",
                    ["{DELTA_MX}"] = "7",
                    ["{DELTA_MY}"] = "8"
                }, map, dropdownOptionsForAddItem);
            };
            view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
               CreateInputDialog.OptAndDone(
                   Activity,
                   GetString(Resource.String.board_strings_5),
                   new Dictionary<string, string>(),
                  mBoardPath,
                   GetString(Resource.String.board_strings_5),
                   new Dictionary<string, string>()
               );
            view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(
                    Activity,
                    GetString(Resource.String.board_strings_6),
                    new Dictionary<string, string>(),
                    mBoardPath,
                   GetString(Resource.String.board_strings_6),
                    new Dictionary<string, string>()
                    );

            view.FindViewById<Button>(Resource.Id.button7).Click += (sender, e) =>
                 CreateInputDialog.OptAndDone(
                     Activity,
                     GetString(Resource.String.board_strings_7),
                     new Dictionary<string, string>(),
                     mBoardPath,
                     GetString(Resource.String.board_strings_7),
                     new Dictionary<string, string>()
                     );

            view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(
                    Activity,
                    GetString(Resource.String.board_strings_8),
                    new Dictionary<string, string>(),
                    mBoardPath,
                    GetString(Resource.String.board_strings_8),
                    new Dictionary<string, string>()
                    );

            view.FindViewById<Button>(Resource.Id.button9).Click += (sender, e) =>
                CreateInputDialog.OptAndDone2(Activity, GetString(Resource.String.board_strings_9), new Dictionary<string, string>
                {
                    [GetString(Resource.String.board_strings_9_1_key)] = map[GetString(Resource.String.board_strings_9_1_key)],
                    [GetString(Resource.String.board_strings_9_2_key)] = map[GetString(Resource.String.board_strings_9_2_key)],
                    [GetString(Resource.String.board_strings_9_3_key)] = map[GetString(Resource.String.board_strings_9_3_key)],
                }, mBoardPath, GetString(Resource.String.board_strings_9), new Dictionary<string, string>
                {
                    ["{RUN}"] = "0",
                    ["{DE}"] = "1",
                    ["{RE}"] = "2"
                }, map);

            view.FindViewById<Button>(Resource.Id.button10).Click += (sender, e) =>
                 CreateInputDialog.OptAndDone2(Activity, GetString(Resource.String.board_strings_10), new Dictionary<string, string>
                 {
                     [GetString(Resource.String.board_strings_10_1_key)] = map[GetString(Resource.String.board_strings_10_1_key)],
                 }, mBoardPath, GetString(Resource.String.board_strings_10), new Dictionary<string, string>
                 {
                     ["{CHECK}"] = "0"
                 }, map);

            view.FindViewById<Button>(Resource.Id.button11).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(
                    Activity,
                    GetString(Resource.String.board_strings_11),
                    new Dictionary<string, string> { },
                   mBoardPath,
                   GetString(Resource.String.board_strings_11),
                    new Dictionary<string, string> { }
                    );

            view.FindViewById<Button>(Resource.Id.button12).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.board_strings_12), new Dictionary<string, string>
                {
                    [GetString(Resource.String.board_strings_12_1_key)] = GetString(Resource.String.board_strings_12_1_value),
                }, mBoardPath, GetString(Resource.String.board_strings_12), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0"
                });
            view.FindViewById<Button>(Resource.Id.button13).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(
                    Activity,
                    GetString(Resource.String.board_strings_13),
                    new Dictionary<string, string> { },
                    mBoardPath,
                    GetString(Resource.String.board_strings_13),
                    new Dictionary<string, string> { }
                );
            view.FindViewById<Button>(Resource.Id.button14).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.board_strings_14), new Dictionary<string, string>
                {
                    [GetString(Resource.String.board_strings_14_1_key)] = GetString(Resource.String.board_strings_14_1_value),
                }, mBoardPath, GetString(Resource.String.board_strings_14), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0"
                });
            view.FindViewById<Button>(Resource.Id.button15).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(
                    Activity,
                    GetString(Resource.String.board_strings_15),
                    new Dictionary<string, string> { },
                    mBoardPath,
                    GetString(Resource.String.board_strings_15),
                    new Dictionary<string, string> { }
                );
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
                [GetString(Resource.String.board_strings_1_1_key)] = GetString(Resource.String.board_strings_1_1_value),
                [GetString(Resource.String.board_strings_1_2_key)] = GetString(Resource.String.board_strings_1_2_value),
                [GetString(Resource.String.board_strings_1_3_key)] = GetString(Resource.String.board_strings_1_3_value),
                [GetString(Resource.String.board_strings_1_4_key)] = GetString(Resource.String.board_strings_1_4_value),
                [GetString(Resource.String.board_strings_1_5_key)] = GetString(Resource.String.board_strings_1_5_value),
                [GetString(Resource.String.board_strings_1_6_key)] = GetString(Resource.String.board_strings_1_6_value),
                [GetString(Resource.String.board_strings_1_7_key)] = GetString(Resource.String.board_strings_1_7_value),
                [GetString(Resource.String.board_strings_1_8_key)] = GetString(Resource.String.board_strings_1_8_value),

                [GetString(Resource.String.board_strings_2_1_key)] = GetString(Resource.String.board_strings_2_1_value),
                [GetString(Resource.String.board_strings_2_2_key)] = GetString(Resource.String.board_strings_2_2_value),
                [GetString(Resource.String.board_strings_2_3_key)] = GetString(Resource.String.board_strings_2_3_value),
                [GetString(Resource.String.board_strings_2_4_key)] = GetString(Resource.String.board_strings_2_4_value),
                [GetString(Resource.String.board_strings_2_5_key)] = GetString(Resource.String.board_strings_2_5_value),
                [GetString(Resource.String.board_strings_2_6_key)] = GetString(Resource.String.board_strings_2_6_value),
                [GetString(Resource.String.board_strings_2_7_key)] = GetString(Resource.String.board_strings_2_7_value),

                [GetString(Resource.String.board_strings_3_1_key)] = GetString(Resource.String.board_strings_3_1_value),
                [GetString(Resource.String.board_strings_3_2_key)] = GetString(Resource.String.board_strings_3_2_value),
                [GetString(Resource.String.board_strings_3_3_key)] = GetString(Resource.String.board_strings_3_3_value),
                [GetString(Resource.String.board_strings_3_4_key)] = GetString(Resource.String.board_strings_3_4_value),
                [GetString(Resource.String.board_strings_3_5_key)] = GetString(Resource.String.board_strings_3_5_value),

                [GetString(Resource.String.board_strings_4_1_key)] = GetString(Resource.String.board_strings_4_1_value),
                [GetString(Resource.String.board_strings_4_2_key)] = GetString(Resource.String.board_strings_4_2_value),
                [GetString(Resource.String.board_strings_4_3_key)] = GetString(Resource.String.board_strings_4_3_value),
                [GetString(Resource.String.board_strings_4_4_key)] = GetString(Resource.String.board_strings_4_4_value),
                [GetString(Resource.String.board_strings_4_5_key)] = GetString(Resource.String.board_strings_4_5_value),
                [GetString(Resource.String.board_strings_4_6_key)] = GetString(Resource.String.board_strings_4_6_value),
                [GetString(Resource.String.board_strings_4_7_key)] = GetString(Resource.String.board_strings_4_7_value),
                [GetString(Resource.String.board_strings_4_8_key)] = GetString(Resource.String.board_strings_4_8_value),
                [GetString(Resource.String.board_strings_4_9_key)] = GetString(Resource.String.board_strings_4_9_value),

                [GetString(Resource.String.board_strings_9_1_key)] = GetString(Resource.String.board_strings_9_1_value),
                [GetString(Resource.String.board_strings_9_2_key)] = GetString(Resource.String.board_strings_9_2_value),
                [GetString(Resource.String.board_strings_9_3_key)] = GetString(Resource.String.board_strings_9_3_value),

                [GetString(Resource.String.board_strings_10_1_key)] = GetString(Resource.String.board_strings_10_1_value),

                [GetString(Resource.String.board_strings_12_1_key)] = GetString(Resource.String.board_strings_12_1_value),

                [GetString(Resource.String.board_strings_14_1_key)] = GetString(Resource.String.board_strings_14_1_value),
            };
        }

        private void LoadOptions()
        {
            try
            {
                string[] option = { "植物", "僵尸", "物品", "道具", "行", "列", "行偏移量", "列偏移量" };
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
                                PlantOptions[GetString(Resource.String.board_strings_1_3_key)] = Dict;
                                PlantOptions[GetString(Resource.String.board_strings_4_4_key)] = Dict;
                                break;

                            case "僵尸":
                                ZombieOptions[GetString(Resource.String.board_strings_2_2_key)] = Dict;
                                ZombieOptions[GetString(Resource.String.board_strings_4_5_key)] = Dict;
                                break;

                            case "物品":
                                CoinOptions[GetString(Resource.String.board_strings_3_3_key)] = Dict;
                                break;

                            case "道具":
                                ItemOptions[GetString(Resource.String.board_strings_4_3_key)] = Dict;
                                break;

                            case "行":
                                BoardRowOptions[GetString(Resource.String.board_strings_1_1_key)] = Dict;
                                BoardRowOptions[GetString(Resource.String.board_strings_2_1_key)] = Dict;
                                BoardRowOptions[GetString(Resource.String.board_strings_3_1_key)] = Dict;
                                BoardRowOptions[GetString(Resource.String.board_strings_4_1_key)] = Dict;
                                break;

                            case "列":
                                BoardColOptions[GetString(Resource.String.board_strings_1_2_key)] = Dict;
                                BoardColOptions[GetString(Resource.String.board_strings_2_4_key)] = Dict;
                                BoardColOptions[GetString(Resource.String.board_strings_3_2_key)] = Dict;
                                BoardColOptions[GetString(Resource.String.board_strings_4_2_key)] = Dict;
                                break;

                            case "行偏移量":
                                BoarddeltamYOptions[GetString(Resource.String.board_strings_1_7_key)] = Dict;
                                BoarddeltamYOptions[GetString(Resource.String.board_strings_2_7_key)] = Dict;
                                BoarddeltamYOptions[GetString(Resource.String.board_strings_3_5_key)] = Dict;
                                BoarddeltamYOptions[GetString(Resource.String.board_strings_4_9_key)] = Dict;

                                break;

                            case "列偏移量":
                                BoarddeltamXOptions[GetString(Resource.String.board_strings_1_6_key)] = Dict;
                                BoarddeltamXOptions[GetString(Resource.String.board_strings_2_6_key)] = Dict;
                                BoarddeltamXOptions[GetString(Resource.String.board_strings_3_4_key)] = Dict;
                                BoarddeltamXOptions[GetString(Resource.String.board_strings_4_8_key)] = Dict;
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
