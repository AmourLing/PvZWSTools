using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Xamarin
{
    public class SpawningFragment:AndroidX.Fragment.App.Fragment
    {
        private static readonly string mSpawningPath = "出怪";
        private Dictionary<string, string> map;

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
        }

        //出怪
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.spawning_fragment, container, false);
            view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.spawning_strings_1), new Dictionary<string, string>
                {
                    [GetString(Resource.String.spawning_strings_1_1_key)] = GetString(Resource.String.spawning_strings_1_1_value),
                    [GetString(Resource.String.spawning_strings_1_2_key)] = GetString(Resource.String.spawning_strings_1_2_value),
                }, mSpawningPath, GetString(Resource.String.spawning_strings_1), new Dictionary<string, string>
                {
                    ["{BUNGEE_CHECK}"] = "0",
                    ["{REDEYE_CHECK}"] = "1"
                });
            view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
                CreateInputDialog.OptAndDone2(Activity, GetString(Resource.String.spawning_strings_2), new Dictionary<string, string>
                {
                    [GetString(Resource.String.spawning_strings_2_1_key)] = map[GetString(Resource.String.spawning_strings_2_1_key)],
                    [GetString(Resource.String.spawning_strings_2_2_key)] = map[GetString(Resource.String.spawning_strings_2_2_key)],
                    [GetString(Resource.String.spawning_strings_2_3_key)] = map[GetString(Resource.String.spawning_strings_2_3_key)],
                    [GetString(Resource.String.spawning_strings_2_4_key)] = map[GetString(Resource.String.spawning_strings_2_4_key)],
                    [GetString(Resource.String.spawning_strings_2_5_key)] = map[GetString(Resource.String.spawning_strings_2_5_key)],
                    [GetString(Resource.String.spawning_strings_2_6_key)] = map[GetString(Resource.String.spawning_strings_2_6_key)],
                    [GetString(Resource.String.spawning_strings_2_7_key)] = map[GetString(Resource.String.spawning_strings_2_7_key)],
                    [GetString(Resource.String.spawning_strings_2_8_key)] = map[GetString(Resource.String.spawning_strings_2_8_key)],
                    [GetString(Resource.String.spawning_strings_2_9_key)] = map[GetString(Resource.String.spawning_strings_2_9_key)],
                    [GetString(Resource.String.spawning_strings_2_10_key)] = map[GetString(Resource.String.spawning_strings_2_10_key)],
                    [GetString(Resource.String.spawning_strings_2_11_key)] = map[GetString(Resource.String.spawning_strings_2_11_key)],
                    [GetString(Resource.String.spawning_strings_2_12_key)] = map[GetString(Resource.String.spawning_strings_2_12_key)],
                    [GetString(Resource.String.spawning_strings_2_13_key)] = map[GetString(Resource.String.spawning_strings_2_13_key)],
                    [GetString(Resource.String.spawning_strings_2_14_key)] = map[GetString(Resource.String.spawning_strings_2_14_key)],
                    [GetString(Resource.String.spawning_strings_2_15_key)] = map[GetString(Resource.String.spawning_strings_2_15_key)],
                    [GetString(Resource.String.spawning_strings_2_16_key)] = map[GetString(Resource.String.spawning_strings_2_16_key)],
                    [GetString(Resource.String.spawning_strings_2_17_key)] = map[GetString(Resource.String.spawning_strings_2_17_key)],
                    [GetString(Resource.String.spawning_strings_2_18_key)] = map[GetString(Resource.String.spawning_strings_2_18_key)],
                    [GetString(Resource.String.spawning_strings_2_19_key)] = map[GetString(Resource.String.spawning_strings_2_19_key)],
                    [GetString(Resource.String.spawning_strings_2_20_key)] = map[GetString(Resource.String.spawning_strings_2_20_key)],
                }, mSpawningPath, GetString(Resource.String.spawning_strings_2), new Dictionary<string, string>
                {
                    ["{SPAWN_TRAFFICCONE_CHECK}"] = "0",
                    ["{SPAWN_POLEVAULTER_CHECK}"] = "1",
                    ["{SPAWN_PAIL_CHECK}"] = "2",
                    ["{SPAWN_NEWSPAPER_CHECK}"] = "3",
                    ["{SPAWN_DOOR_CHECK}"] = "4",
                    ["{SPAWN_FOOTBALL_CHECK}"] = "5",
                    ["{SPAWN_DANCE_CHECK}"] = "6",
                    ["{SPAWN_SNORKEL_CHECK}"] = "7",
                    ["{SPAWN_ZAMBONI_CHECK}"] = "8",
                    ["{SPAWN_DOLPHINRIDER_CHECK}"] = "9",
                    ["{SPAWN_JACKINTHEBOX_CHECK}"] = "10",
                    ["{SPAWN_BALLOON_CHECK}"] = "11",
                    ["{SPAWN_DIGGER_CHECK}"] = "12",
                    ["{SPAWN_POGO_CHECK}"] = "13",
                    ["{SPAWN_YETI_CHECK}"] = "14",
                    ["{SPAWN_BUNGEE_CHECK}"] = "15",
                    ["{SPAWN_LADDER_CHECK}"] = "16",
                    ["{SPAWN_CATAPULT_CHECK}"] = "17",
                    ["{SPAWN_GARGANTUAR_CHECK}"] = "18",
                    ["{SPAWN_REDEYEGARGANTUAR_CHECK}"] = "19",
                }, map);
            view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.spawning_strings_3), new Dictionary<string, string>
                {
                }, mSpawningPath, GetString(Resource.String.spawning_strings_3), new Dictionary<string, string>
                {
                });
            view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.spawning_strings_4), new Dictionary<string, string>
                {
                    [GetString(Resource.String.spawning_strings_4_1_key)] = GetString(Resource.String.spawning_strings_4_1_value),
                }, mSpawningPath, GetString(Resource.String.spawning_strings_4), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.spawning_strings_5), new Dictionary<string, string>
                {
                    [GetString(Resource.String.spawning_strings_5_1_key)] = GetString(Resource.String.spawning_strings_5_1_value),
                }, mSpawningPath, GetString(Resource.String.spawning_strings_5), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.spawning_strings_6), new Dictionary<string, string>
                {
                }, mSpawningPath, GetString(Resource.String.spawning_strings_6), new Dictionary<string, string>
                {
                });
            view.FindViewById<Button>(Resource.Id.button7).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.spawning_strings_7), new Dictionary<string, string>
                {
                }, mSpawningPath, GetString(Resource.String.spawning_strings_7), new Dictionary<string, string>
                {
                });
            view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.spawning_strings_8), new Dictionary<string, string>
                {
                    [GetString(Resource.String.spawning_strings_8_1_key)] = GetString(Resource.String.spawning_strings_8_1_value),
                    [GetString(Resource.String.spawning_strings_8_2_key)] = GetString(Resource.String.spawning_strings_8_2_value),
                }, mSpawningPath, GetString(Resource.String.spawning_strings_8), new Dictionary<string, string>
                {
                    ["{MIN}"] = "0",
                    ["{MAX}"] = "1"
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
                [GetString(Resource.String.spawning_strings_1_1_key)] = GetString(Resource.String.spawning_strings_1_1_value),
                [GetString(Resource.String.spawning_strings_1_2_key)] = GetString(Resource.String.spawning_strings_1_2_value),
                [GetString(Resource.String.spawning_strings_2_1_key)] = GetString(Resource.String.spawning_strings_2_1_value),
                [GetString(Resource.String.spawning_strings_2_2_key)] = GetString(Resource.String.spawning_strings_2_2_value),
                [GetString(Resource.String.spawning_strings_2_3_key)] = GetString(Resource.String.spawning_strings_2_3_value),
                [GetString(Resource.String.spawning_strings_2_4_key)] = GetString(Resource.String.spawning_strings_2_4_value),
                [GetString(Resource.String.spawning_strings_2_5_key)] = GetString(Resource.String.spawning_strings_2_5_value),
                [GetString(Resource.String.spawning_strings_2_6_key)] = GetString(Resource.String.spawning_strings_2_6_value),
                [GetString(Resource.String.spawning_strings_2_7_key)] = GetString(Resource.String.spawning_strings_2_7_value),
                [GetString(Resource.String.spawning_strings_2_8_key)] = GetString(Resource.String.spawning_strings_2_8_value),
                [GetString(Resource.String.spawning_strings_2_9_key)] = GetString(Resource.String.spawning_strings_2_9_value),
                [GetString(Resource.String.spawning_strings_2_10_key)] = GetString(Resource.String.spawning_strings_2_10_value),
                [GetString(Resource.String.spawning_strings_2_11_key)] = GetString(Resource.String.spawning_strings_2_11_value),
                [GetString(Resource.String.spawning_strings_2_12_key)] = GetString(Resource.String.spawning_strings_2_12_value),
                [GetString(Resource.String.spawning_strings_2_13_key)] = GetString(Resource.String.spawning_strings_2_13_value),
                [GetString(Resource.String.spawning_strings_2_14_key)] = GetString(Resource.String.spawning_strings_2_14_value),
                [GetString(Resource.String.spawning_strings_2_15_key)] = GetString(Resource.String.spawning_strings_2_15_value),
                [GetString(Resource.String.spawning_strings_2_16_key)] = GetString(Resource.String.spawning_strings_2_16_value),
                [GetString(Resource.String.spawning_strings_2_17_key)] = GetString(Resource.String.spawning_strings_2_17_value),
                [GetString(Resource.String.spawning_strings_2_18_key)] = GetString(Resource.String.spawning_strings_2_18_value),
                [GetString(Resource.String.spawning_strings_2_19_key)] = GetString(Resource.String.spawning_strings_2_19_value),
                [GetString(Resource.String.spawning_strings_2_20_key)] = GetString(Resource.String.spawning_strings_2_20_value),
                [GetString(Resource.String.spawning_strings_4_1_key)] = GetString(Resource.String.spawning_strings_4_1_value),
                [GetString(Resource.String.spawning_strings_5_1_key)] = GetString(Resource.String.spawning_strings_5_1_value),
                [GetString(Resource.String.spawning_strings_8_1_key)] = GetString(Resource.String.spawning_strings_8_1_value),
                [GetString(Resource.String.spawning_strings_8_2_key)] = GetString(Resource.String.spawning_strings_8_2_value),
            };
        }
    }
}

//无用的注释罢了
