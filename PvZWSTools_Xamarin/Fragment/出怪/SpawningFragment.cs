using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Xamarin;

public class SpawningFragment:BaseFragment
{
    private static readonly string mSpawningPath = "出怪";

    public override void RefreshAllButtons()
    {
    }

    protected override string FragmentPath => mSpawningPath;

    protected override Dictionary<int, string> OptionFileMappings => new Dictionary<int, string>
    {
        // 开关1
        [Resource.String.spawning_strings_1_1_key] = "开关1",
        [Resource.String.spawning_strings_1_2_key] = "开关1",
        [Resource.String.spawning_strings_4_1_key] = "开关1",
        [Resource.String.spawning_strings_5_1_key] = "开关1",
        // 开关2（共40个）
        [Resource.String.spawning_strings_2_1_key] = "开关2",
        [Resource.String.spawning_strings_2_2_key] = "开关2",
        [Resource.String.spawning_strings_2_3_key] = "开关2",
        [Resource.String.spawning_strings_2_4_key] = "开关2",
        [Resource.String.spawning_strings_2_5_key] = "开关2",
        [Resource.String.spawning_strings_2_6_key] = "开关2",
        [Resource.String.spawning_strings_2_7_key] = "开关2",
        [Resource.String.spawning_strings_2_8_key] = "开关2",
        [Resource.String.spawning_strings_2_9_key] = "开关2",
        [Resource.String.spawning_strings_2_10_key] = "开关2",
        [Resource.String.spawning_strings_2_11_key] = "开关2",
        [Resource.String.spawning_strings_2_12_key] = "开关2",
        [Resource.String.spawning_strings_2_13_key] = "开关2",
        [Resource.String.spawning_strings_2_14_key] = "开关2",
        [Resource.String.spawning_strings_2_15_key] = "开关2",
        [Resource.String.spawning_strings_2_16_key] = "开关2",
        [Resource.String.spawning_strings_2_17_key] = "开关2",
        [Resource.String.spawning_strings_2_18_key] = "开关2",
        [Resource.String.spawning_strings_2_19_key] = "开关2",
        [Resource.String.spawning_strings_2_20_key] = "开关2",
        [Resource.String.spawning_strings_2_21_key] = "开关2",
        [Resource.String.spawning_strings_2_22_key] = "开关2",
        [Resource.String.spawning_strings_2_23_key] = "开关2",
        [Resource.String.spawning_strings_2_24_key] = "开关2",
        [Resource.String.spawning_strings_2_25_key] = "开关2",
        [Resource.String.spawning_strings_2_26_key] = "开关2",
        [Resource.String.spawning_strings_2_27_key] = "开关2",
        [Resource.String.spawning_strings_2_28_key] = "开关2",
        [Resource.String.spawning_strings_2_29_key] = "开关2",
        [Resource.String.spawning_strings_2_30_key] = "开关2",
        [Resource.String.spawning_strings_2_31_key] = "开关2",
        [Resource.String.spawning_strings_2_32_key] = "开关2",
        [Resource.String.spawning_strings_2_33_key] = "开关2",
        [Resource.String.spawning_strings_2_34_key] = "开关2",
        [Resource.String.spawning_strings_2_35_key] = "开关2",
        [Resource.String.spawning_strings_2_36_key] = "开关2",
        [Resource.String.spawning_strings_2_37_key] = "开关2",
        [Resource.String.spawning_strings_2_38_key] = "开关2",
        [Resource.String.spawning_strings_2_39_key] = "开关2",
        [Resource.String.spawning_strings_2_40_key] = "开关2"
    };

    protected override void InitializeMap()
    {
        Map[GetString(Resource.String.spawning_strings_1_1_key)] = GetString(Resource.String.spawning_strings_1_1_value);
        Map[GetString(Resource.String.spawning_strings_1_2_key)] = GetString(Resource.String.spawning_strings_1_2_value);

        Map[GetString(Resource.String.spawning_strings_2_1_key)] = GetString(Resource.String.spawning_strings_2_1_value);
        Map[GetString(Resource.String.spawning_strings_2_2_key)] = GetString(Resource.String.spawning_strings_2_2_value);
        Map[GetString(Resource.String.spawning_strings_2_3_key)] = GetString(Resource.String.spawning_strings_2_3_value);
        Map[GetString(Resource.String.spawning_strings_2_4_key)] = GetString(Resource.String.spawning_strings_2_4_value);
        Map[GetString(Resource.String.spawning_strings_2_5_key)] = GetString(Resource.String.spawning_strings_2_5_value);
        Map[GetString(Resource.String.spawning_strings_2_6_key)] = GetString(Resource.String.spawning_strings_2_6_value);
        Map[GetString(Resource.String.spawning_strings_2_7_key)] = GetString(Resource.String.spawning_strings_2_7_value);
        Map[GetString(Resource.String.spawning_strings_2_8_key)] = GetString(Resource.String.spawning_strings_2_8_value);
        Map[GetString(Resource.String.spawning_strings_2_9_key)] = GetString(Resource.String.spawning_strings_2_9_value);
        Map[GetString(Resource.String.spawning_strings_2_10_key)] = GetString(Resource.String.spawning_strings_2_10_value);
        Map[GetString(Resource.String.spawning_strings_2_11_key)] = GetString(Resource.String.spawning_strings_2_11_value);
        Map[GetString(Resource.String.spawning_strings_2_12_key)] = GetString(Resource.String.spawning_strings_2_12_value);
        Map[GetString(Resource.String.spawning_strings_2_13_key)] = GetString(Resource.String.spawning_strings_2_13_value);
        Map[GetString(Resource.String.spawning_strings_2_14_key)] = GetString(Resource.String.spawning_strings_2_14_value);
        Map[GetString(Resource.String.spawning_strings_2_15_key)] = GetString(Resource.String.spawning_strings_2_15_value);
        Map[GetString(Resource.String.spawning_strings_2_16_key)] = GetString(Resource.String.spawning_strings_2_16_value);
        Map[GetString(Resource.String.spawning_strings_2_17_key)] = GetString(Resource.String.spawning_strings_2_17_value);
        Map[GetString(Resource.String.spawning_strings_2_18_key)] = GetString(Resource.String.spawning_strings_2_18_value);
        Map[GetString(Resource.String.spawning_strings_2_19_key)] = GetString(Resource.String.spawning_strings_2_19_value);
        Map[GetString(Resource.String.spawning_strings_2_20_key)] = GetString(Resource.String.spawning_strings_2_20_value);
        Map[GetString(Resource.String.spawning_strings_2_21_key)] = GetString(Resource.String.spawning_strings_2_21_value);
        Map[GetString(Resource.String.spawning_strings_2_22_key)] = GetString(Resource.String.spawning_strings_2_22_value);
        Map[GetString(Resource.String.spawning_strings_2_23_key)] = GetString(Resource.String.spawning_strings_2_23_value);
        Map[GetString(Resource.String.spawning_strings_2_24_key)] = GetString(Resource.String.spawning_strings_2_24_value);
        Map[GetString(Resource.String.spawning_strings_2_25_key)] = GetString(Resource.String.spawning_strings_2_25_value);
        Map[GetString(Resource.String.spawning_strings_2_26_key)] = GetString(Resource.String.spawning_strings_2_26_value);
        Map[GetString(Resource.String.spawning_strings_2_27_key)] = GetString(Resource.String.spawning_strings_2_27_value);
        Map[GetString(Resource.String.spawning_strings_2_28_key)] = GetString(Resource.String.spawning_strings_2_28_value);
        Map[GetString(Resource.String.spawning_strings_2_29_key)] = GetString(Resource.String.spawning_strings_2_29_value);
        Map[GetString(Resource.String.spawning_strings_2_30_key)] = GetString(Resource.String.spawning_strings_2_30_value);
        Map[GetString(Resource.String.spawning_strings_2_31_key)] = GetString(Resource.String.spawning_strings_2_31_value);
        Map[GetString(Resource.String.spawning_strings_2_32_key)] = GetString(Resource.String.spawning_strings_2_32_value);
        Map[GetString(Resource.String.spawning_strings_2_33_key)] = GetString(Resource.String.spawning_strings_2_33_value);
        Map[GetString(Resource.String.spawning_strings_2_34_key)] = GetString(Resource.String.spawning_strings_2_34_value);
        Map[GetString(Resource.String.spawning_strings_2_35_key)] = GetString(Resource.String.spawning_strings_2_35_value);
        Map[GetString(Resource.String.spawning_strings_2_36_key)] = GetString(Resource.String.spawning_strings_2_36_value);
        Map[GetString(Resource.String.spawning_strings_2_37_key)] = GetString(Resource.String.spawning_strings_2_37_value);
        Map[GetString(Resource.String.spawning_strings_2_38_key)] = GetString(Resource.String.spawning_strings_2_38_value);
        Map[GetString(Resource.String.spawning_strings_2_39_key)] = GetString(Resource.String.spawning_strings_2_39_value);
        Map[GetString(Resource.String.spawning_strings_2_40_key)] = GetString(Resource.String.spawning_strings_2_40_value);

        Map[GetString(Resource.String.spawning_strings_4_1_key)] = GetString(Resource.String.spawning_strings_4_1_value);
        Map[GetString(Resource.String.spawning_strings_5_1_key)] = GetString(Resource.String.spawning_strings_5_1_value);
        Map[GetString(Resource.String.spawning_strings_8_1_key)] = GetString(Resource.String.spawning_strings_8_1_value);
        Map[GetString(Resource.String.spawning_strings_8_2_key)] = GetString(Resource.String.spawning_strings_8_2_value);
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.spawning_fragment, container, false);

        // Button 1
        view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.spawning_strings_1_1_key);
            string key2 = GetString(Resource.String.spawning_strings_1_2_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.spawning_strings_1),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.spawning_strings_1),
                new Dictionary<string, string> { ["{BUNGEE_CHECK}"] = "0", ["{REDEYE_CHECK}"] = "1" },
                Map,
                BuildDropdownOptions(key1, key2)
            );
        };

        // Button 2
        view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
        {
            // 收集所有 40 个键
            var keys = new[]
            {
                Resource.String.spawning_strings_2_1_key,
                Resource.String.spawning_strings_2_2_key,
                Resource.String.spawning_strings_2_3_key,
                Resource.String.spawning_strings_2_4_key,
                Resource.String.spawning_strings_2_5_key,
                Resource.String.spawning_strings_2_6_key,
                Resource.String.spawning_strings_2_7_key,
                Resource.String.spawning_strings_2_8_key,
                Resource.String.spawning_strings_2_9_key,
                Resource.String.spawning_strings_2_10_key,
                Resource.String.spawning_strings_2_11_key,
                Resource.String.spawning_strings_2_12_key,
                Resource.String.spawning_strings_2_13_key,
                Resource.String.spawning_strings_2_14_key,
                Resource.String.spawning_strings_2_15_key,
                Resource.String.spawning_strings_2_16_key,
                Resource.String.spawning_strings_2_17_key,
                Resource.String.spawning_strings_2_18_key,
                Resource.String.spawning_strings_2_19_key,
                Resource.String.spawning_strings_2_20_key,
                Resource.String.spawning_strings_2_21_key,
                Resource.String.spawning_strings_2_22_key,
                Resource.String.spawning_strings_2_23_key,
                Resource.String.spawning_strings_2_24_key,
                Resource.String.spawning_strings_2_25_key,
                Resource.String.spawning_strings_2_26_key,
                Resource.String.spawning_strings_2_27_key,
                Resource.String.spawning_strings_2_28_key,
                Resource.String.spawning_strings_2_29_key,
                Resource.String.spawning_strings_2_30_key,
                Resource.String.spawning_strings_2_31_key,
                Resource.String.spawning_strings_2_32_key,
                Resource.String.spawning_strings_2_33_key,
                Resource.String.spawning_strings_2_34_key,
                Resource.String.spawning_strings_2_35_key,
                Resource.String.spawning_strings_2_36_key,
                Resource.String.spawning_strings_2_37_key,
                Resource.String.spawning_strings_2_38_key,
                Resource.String.spawning_strings_2_39_key,
                Resource.String.spawning_strings_2_40_key
            };
            var keyStrings = new List<string>();
            foreach(var resId in keys)
                keyStrings.Add(GetString(resId));

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

            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.spawning_strings_2),
                BuildInitialData(keyStrings.ToArray()),
                FragmentPath,
                GetString(Resource.String.spawning_strings_2),
                defaultValues,
                Map,
                BuildDropdownOptions(keyStrings.ToArray())
            );
        };

        // Button 3: 极限出怪测试 (无参数)
        view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.spawning_strings_3),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.spawning_strings_3),
                new Dictionary<string, string>()
            );

        // Button 4: 最大密度
        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
        {
            string key = GetString(Resource.String.spawning_strings_4_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.spawning_strings_4),
                BuildInitialData(key),
                FragmentPath,
                GetString(Resource.String.spawning_strings_4),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key)
            );
        };

        // Button 5: 暂停出怪
        view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
        {
            string key = GetString(Resource.String.spawning_strings_5_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.spawning_strings_5),
                BuildInitialData(key),
                FragmentPath,
                GetString(Resource.String.spawning_strings_5),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key)
            );
        };

        // Button 6: 波次出怪_数量 (无参数)
        view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.spawning_strings_6),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.spawning_strings_6),
                new Dictionary<string, string>()
            );

        // Button 7: 载入json (无参数)
        view.FindViewById<Button>(Resource.Id.button7).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.spawning_strings_7),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.spawning_strings_7),
                new Dictionary<string, string>()
            );

        // Button 8: 刷新血量 (浮点数范围，使用 OptAndDone)
        view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.spawning_strings_8_1_key);
            string key2 = GetString(Resource.String.spawning_strings_8_2_key);
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.spawning_strings_8),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.spawning_strings_8),
                new Dictionary<string, string> { ["{MIN}"] = "0", ["{MAX}"] = "1" }
            );
        };

        return view;
    }
}
