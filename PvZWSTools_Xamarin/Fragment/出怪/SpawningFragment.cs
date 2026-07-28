using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Xamarin;

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
        {
            var keysDict = new Dictionary<string, string>
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
                [GetString(Resource.String.spawning_strings_2_21_key)] = map[GetString(Resource.String.spawning_strings_2_21_key)],
                [GetString(Resource.String.spawning_strings_2_22_key)] = map[GetString(Resource.String.spawning_strings_2_22_key)],
                [GetString(Resource.String.spawning_strings_2_23_key)] = map[GetString(Resource.String.spawning_strings_2_23_key)],
                [GetString(Resource.String.spawning_strings_2_24_key)] = map[GetString(Resource.String.spawning_strings_2_24_key)],
                [GetString(Resource.String.spawning_strings_2_25_key)] = map[GetString(Resource.String.spawning_strings_2_25_key)],
                [GetString(Resource.String.spawning_strings_2_26_key)] = map[GetString(Resource.String.spawning_strings_2_26_key)],
                [GetString(Resource.String.spawning_strings_2_27_key)] = map[GetString(Resource.String.spawning_strings_2_27_key)],
                [GetString(Resource.String.spawning_strings_2_28_key)] = map[GetString(Resource.String.spawning_strings_2_28_key)],
                [GetString(Resource.String.spawning_strings_2_29_key)] = map[GetString(Resource.String.spawning_strings_2_29_key)],
                [GetString(Resource.String.spawning_strings_2_30_key)] = map[GetString(Resource.String.spawning_strings_2_30_key)],
                [GetString(Resource.String.spawning_strings_2_31_key)] = map[GetString(Resource.String.spawning_strings_2_31_key)],
                [GetString(Resource.String.spawning_strings_2_32_key)] = map[GetString(Resource.String.spawning_strings_2_32_key)],
                [GetString(Resource.String.spawning_strings_2_33_key)] = map[GetString(Resource.String.spawning_strings_2_33_key)],
                [GetString(Resource.String.spawning_strings_2_34_key)] = map[GetString(Resource.String.spawning_strings_2_34_key)],
                [GetString(Resource.String.spawning_strings_2_35_key)] = map[GetString(Resource.String.spawning_strings_2_35_key)],
                [GetString(Resource.String.spawning_strings_2_36_key)] = map[GetString(Resource.String.spawning_strings_2_36_key)],
                [GetString(Resource.String.spawning_strings_2_37_key)] = map[GetString(Resource.String.spawning_strings_2_37_key)],
                [GetString(Resource.String.spawning_strings_2_38_key)] = map[GetString(Resource.String.spawning_strings_2_38_key)],
                [GetString(Resource.String.spawning_strings_2_39_key)] = map[GetString(Resource.String.spawning_strings_2_39_key)],
                [GetString(Resource.String.spawning_strings_2_40_key)] = map[GetString(Resource.String.spawning_strings_2_40_key)]
            };

            var defaultValues = new Dictionary<string, string>
            {
                ["{SPAWN_ZOMBIENORMAL_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEFLAG_CHECK}"] = "2",
                ["{SPAWN_ZOMBIETRAFFICCONE_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEPOLEVAULTER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEPAIL_CHECK}"] = "2",
                ["{SPAWN_ZOMBIENEWSPAPER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEDOOR_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEFOOTBALL_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEDANCER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEBACKUPDANCER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEDUCKYTUBE_CHECK}"] = "2",
                ["{SPAWN_ZOMBIESNORKEL_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEZAMBONI_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEBOBSLED_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEDOLPHINRIDER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEJACKINTHEBOX_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEBALLOON_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEDIGGER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEPOGO_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEYETI_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEBUNGEE_CHECK}"] = "2",
                ["{SPAWN_ZOMBIELADDER_CHECK}"] = "2",
                ["{SPAWN_ZOMBIECATAPULT_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEGARGANTUAR_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEIMP_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEBOSS_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEPEAHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEWALLNUTHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEJALAPENOHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEGATLINGHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIESQUASHHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIETALLNUTHEAD_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEREDEYEGARGANTUAR_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEROBOTTITAN_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEREDEYEROBOTTITAN_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEMONK_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEFOOTBALLPREMIUM_CHECK}"] = "2",
                ["{SPAWN_ZOMBIENINJA_CHECK}"] = "2",
                ["{SPAWN_ZOMBIETALISMAN_CHECK}"] = "2",
                ["{SPAWN_ZOMBIEPROPELLER_CHECK}"] = "2"
            };

            CreateInputDialog.OptAndDone2(
                Activity,
                GetString(Resource.String.spawning_strings_2),
                keysDict,
                mSpawningPath,
                GetString(Resource.String.spawning_strings_2),
                defaultValues,
                map
            );
        };
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
            [GetString(Resource.String.spawning_strings_2_21_key)] = GetString(Resource.String.spawning_strings_2_21_value),
            [GetString(Resource.String.spawning_strings_2_22_key)] = GetString(Resource.String.spawning_strings_2_22_value),
            [GetString(Resource.String.spawning_strings_2_23_key)] = GetString(Resource.String.spawning_strings_2_23_value),
            [GetString(Resource.String.spawning_strings_2_24_key)] = GetString(Resource.String.spawning_strings_2_24_value),
            [GetString(Resource.String.spawning_strings_2_25_key)] = GetString(Resource.String.spawning_strings_2_25_value),
            [GetString(Resource.String.spawning_strings_2_26_key)] = GetString(Resource.String.spawning_strings_2_26_value),
            [GetString(Resource.String.spawning_strings_2_27_key)] = GetString(Resource.String.spawning_strings_2_27_value),
            [GetString(Resource.String.spawning_strings_2_28_key)] = GetString(Resource.String.spawning_strings_2_28_value),
            [GetString(Resource.String.spawning_strings_2_29_key)] = GetString(Resource.String.spawning_strings_2_29_value),
            [GetString(Resource.String.spawning_strings_2_30_key)] = GetString(Resource.String.spawning_strings_2_30_value),
            [GetString(Resource.String.spawning_strings_2_31_key)] = GetString(Resource.String.spawning_strings_2_31_value),
            [GetString(Resource.String.spawning_strings_2_32_key)] = GetString(Resource.String.spawning_strings_2_32_value),
            [GetString(Resource.String.spawning_strings_2_33_key)] = GetString(Resource.String.spawning_strings_2_33_value),
            [GetString(Resource.String.spawning_strings_2_34_key)] = GetString(Resource.String.spawning_strings_2_34_value),
            [GetString(Resource.String.spawning_strings_2_35_key)] = GetString(Resource.String.spawning_strings_2_35_value),
            [GetString(Resource.String.spawning_strings_2_36_key)] = GetString(Resource.String.spawning_strings_2_36_value),
            [GetString(Resource.String.spawning_strings_2_37_key)] = GetString(Resource.String.spawning_strings_2_37_value),
            [GetString(Resource.String.spawning_strings_2_38_key)] = GetString(Resource.String.spawning_strings_2_38_value),
            [GetString(Resource.String.spawning_strings_2_39_key)] = GetString(Resource.String.spawning_strings_2_39_value),
            [GetString(Resource.String.spawning_strings_2_40_key)] = GetString(Resource.String.spawning_strings_2_40_value),
            [GetString(Resource.String.spawning_strings_4_1_key)] = GetString(Resource.String.spawning_strings_4_1_value),
            [GetString(Resource.String.spawning_strings_5_1_key)] = GetString(Resource.String.spawning_strings_5_1_value),
            [GetString(Resource.String.spawning_strings_8_1_key)] = GetString(Resource.String.spawning_strings_8_1_value),
            [GetString(Resource.String.spawning_strings_8_2_key)] = GetString(Resource.String.spawning_strings_8_2_value),
        };
    }
}
