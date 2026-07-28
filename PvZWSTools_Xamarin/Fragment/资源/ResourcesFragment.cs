using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Xamarin;

public class ResourcesFragment:BaseFragment
{
    private static readonly string mResourcesPath = "资源";

    protected override string FragmentPath => mResourcesPath;

    protected override Dictionary<int, string> OptionFileMappings => new Dictionary<int, string>
    {
        // 伤害选项
        [Resource.String.resources_strings_1_1_key] = "伤害",
        // 血量选项
        [Resource.String.resources_strings_2_1_key] = "血量",
        // 时间选项
        [Resource.String.resources_strings_7_1_key] = "时间",
        // 价值选项
        [Resource.String.resources_strings_8_1_key] = "价值"
    };

    protected override void InitializeMap()
    {
        Map[GetString(Resource.String.resources_strings_1_1_key)] = GetString(Resource.String.resources_strings_1_1_value);
        Map[GetString(Resource.String.resources_strings_1_2_key)] = GetString(Resource.String.resources_strings_1_2_value);

        Map[GetString(Resource.String.resources_strings_2_1_key)] = GetString(Resource.String.resources_strings_2_1_value);
        Map[GetString(Resource.String.resources_strings_2_2_key)] = GetString(Resource.String.resources_strings_2_2_value);

        Map[GetString(Resource.String.resources_strings_3_1_key)] = GetString(Resource.String.resources_strings_3_1_value);

        Map[GetString(Resource.String.resources_strings_4_1_key)] = GetString(Resource.String.resources_strings_4_1_value);

        Map[GetString(Resource.String.resources_strings_5_1_key)] = GetString(Resource.String.resources_strings_5_1_value);

        Map[GetString(Resource.String.resources_strings_6_1_key)] = GetString(Resource.String.resources_strings_6_1_value);

        Map[GetString(Resource.String.resources_strings_7_1_key)] = GetString(Resource.String.resources_strings_7_1_value);
        Map[GetString(Resource.String.resources_strings_7_2_key)] = GetString(Resource.String.resources_strings_7_2_value);

        Map[GetString(Resource.String.resources_strings_8_1_key)] = GetString(Resource.String.resources_strings_8_1_value);
        Map[GetString(Resource.String.resources_strings_8_2_key)] = GetString(Resource.String.resources_strings_8_2_value);
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.resources_fragment, container, false);

        // Button 1: 设置伤害
        view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.resources_strings_1_1_key);
            string key2 = GetString(Resource.String.resources_strings_1_2_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.resources_strings_1),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.resources_strings_1),
                new Dictionary<string, string> { ["{DAMAGE}"] = "0", ["{DAMAGE2}"] = "1" },
                Map,
                BuildDropdownOptions(key1) // 仅 key1 有下拉选项
            );
        };

        // Button 2: 设置血量
        view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.resources_strings_2_1_key);
            string key2 = GetString(Resource.String.resources_strings_2_2_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.resources_strings_2),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.resources_strings_2),
                new Dictionary<string, string> { ["{HEALTH}"] = "0", ["{HEALTH2}"] = "1" },
                Map,
                BuildDropdownOptions(key1) // 仅 key1 有下拉选项
            );
        };

        // Button 3: 设置金币
        view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
        {
            string key = GetString(Resource.String.resources_strings_3_1_key);
            CreateInputDialog.OptAndDone2(
                Activity,
                GetString(Resource.String.resources_strings_3),
                BuildInitialData(key),
                FragmentPath,
                GetString(Resource.String.resources_strings_3),
                new Dictionary<string, string> { ["{COIN}"] = "0" },
                Map
            );
        };

        // Button 4: 设置金币上限
        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
        {
            string key = GetString(Resource.String.resources_strings_4_1_key);
            CreateInputDialog.OptAndDone2(
                Activity,
                GetString(Resource.String.resources_strings_4),
                BuildInitialData(key),
                FragmentPath,
                GetString(Resource.String.resources_strings_4),
                new Dictionary<string, string> { ["{COINLIMIT}"] = "0" },
                Map
            );
        };

        // Button 5: 设置阳光
        view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
        {
            string key = GetString(Resource.String.resources_strings_5_1_key);
            CreateInputDialog.OptAndDone2(
                Activity,
                GetString(Resource.String.resources_strings_5),
                BuildInitialData(key),
                FragmentPath,
                GetString(Resource.String.resources_strings_5),
                new Dictionary<string, string> { ["{SUNMONEY}"] = "0" },
                Map
            );
        };

        // Button 6: 设置阳光上限
        view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
        {
            string key = GetString(Resource.String.resources_strings_6_1_key);
            CreateInputDialog.OptAndDone2(
                Activity,
                GetString(Resource.String.resources_strings_6),
                BuildInitialData(key),
                FragmentPath,
                GetString(Resource.String.resources_strings_6),
                new Dictionary<string, string> { ["{SUNMONEYLIMIT}"] = "0" },
                Map
            );
        };

        // Button 7: 设置时间
        view.FindViewById<Button>(Resource.Id.button7).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.resources_strings_7_1_key);
            string key2 = GetString(Resource.String.resources_strings_7_2_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.resources_strings_7),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.resources_strings_7),
                new Dictionary<string, string> { ["{TIME}"] = "0", ["{TIME2}"] = "1" },
                Map,
                BuildDropdownOptions(key1) // 仅 key1 有下拉选项
            );
        };

        // Button 8: 设置价值
        view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.resources_strings_8_1_key);
            string key2 = GetString(Resource.String.resources_strings_8_2_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.resources_strings_8),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.resources_strings_8),
                new Dictionary<string, string> { ["{VALUE}"] = "0", ["{VALUE2}"] = "1" },
                Map,
                BuildDropdownOptions(key1) // 仅 key1 有下拉选项
            );
        };

        return view;
    }
}
