using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Xamarin;

public class ZombieFragment:BaseFragment
{
    private static readonly string mZombiePath = "僵尸";

    protected override string FragmentPath => mZombiePath;
    private static readonly string OptionFileNameSwitch1 = "开关1";

    protected override Dictionary<int, string> OptionFileMappings => new Dictionary<int, string>
    {
        [Resource.String.zombie_strings_1_1_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_2_1_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_3_1_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_4_1_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_5_1_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_6_1_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_7_1_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_7_2_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_8_1_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_8_2_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_9_1_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_9_2_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_10_1_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_10_2_key] = OptionFileNameSwitch1,
        [Resource.String.zombie_strings_11_1_key] = OptionFileNameSwitch1,
    };

    protected override void InitializeMap()
    {
        Map[GetString(Resource.String.zombie_strings_1_1_key)] = GetString(Resource.String.zombie_strings_1_1_value);
        Map[GetString(Resource.String.zombie_strings_2_1_key)] = GetString(Resource.String.zombie_strings_2_1_value);
        Map[GetString(Resource.String.zombie_strings_3_1_key)] = GetString(Resource.String.zombie_strings_3_1_value);
        Map[GetString(Resource.String.zombie_strings_4_1_key)] = GetString(Resource.String.zombie_strings_4_1_value);
        Map[GetString(Resource.String.zombie_strings_5_1_key)] = GetString(Resource.String.zombie_strings_5_1_value);
        Map[GetString(Resource.String.zombie_strings_6_1_key)] = GetString(Resource.String.zombie_strings_6_1_value);
        Map[GetString(Resource.String.zombie_strings_7_1_key)] = GetString(Resource.String.zombie_strings_7_1_value);
        Map[GetString(Resource.String.zombie_strings_7_2_key)] = GetString(Resource.String.zombie_strings_7_2_value);
        Map[GetString(Resource.String.zombie_strings_8_1_key)] = GetString(Resource.String.zombie_strings_8_1_value);
        Map[GetString(Resource.String.zombie_strings_8_2_key)] = GetString(Resource.String.zombie_strings_8_2_value);
        Map[GetString(Resource.String.zombie_strings_9_1_key)] = GetString(Resource.String.zombie_strings_9_1_value);
        Map[GetString(Resource.String.zombie_strings_9_2_key)] = GetString(Resource.String.zombie_strings_9_2_value);
        Map[GetString(Resource.String.zombie_strings_10_1_key)] = GetString(Resource.String.zombie_strings_10_1_value);
        Map[GetString(Resource.String.zombie_strings_10_2_key)] = GetString(Resource.String.zombie_strings_10_2_value);
        Map[GetString(Resource.String.zombie_strings_11_1_key)] = GetString(Resource.String.zombie_strings_11_1_value);
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.zombie_fragment, container, false);

        // Button 1
        view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.zombie_strings_1_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.zombie_strings_1),
                BuildInitialData(key1),
                FragmentPath,
                GetString(Resource.String.zombie_strings_1),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key1)
            );
        };

        // Button 2
        view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.zombie_strings_2_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.zombie_strings_2),
                BuildInitialData(key1),
                FragmentPath,
                GetString(Resource.String.zombie_strings_2),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key1)
            );
        };

        // Button 3
        view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.zombie_strings_3_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.zombie_strings_3),
                BuildInitialData(key1),
                FragmentPath,
                GetString(Resource.String.zombie_strings_3),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key1)
            );
        };

        // Button 4
        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.zombie_strings_4_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.zombie_strings_4),
                BuildInitialData(key1),
                FragmentPath,
                GetString(Resource.String.zombie_strings_4),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key1)
            );
        };

        // Button 5
        view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.zombie_strings_5_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.zombie_strings_5),
                BuildInitialData(key1),
                FragmentPath,
                GetString(Resource.String.zombie_strings_5),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key1)
            );
        };

        // Button 6
        view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.zombie_strings_6_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.zombie_strings_6),
                BuildInitialData(key1),
                FragmentPath,
                GetString(Resource.String.zombie_strings_6),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key1)
            );
        };

        // Button 7
        view.FindViewById<Button>(Resource.Id.button7).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.zombie_strings_7_1_key);
            string key2 = GetString(Resource.String.zombie_strings_7_2_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.zombie_strings_7),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.zombie_strings_7),
                new Dictionary<string, string> { ["{MIND_CHECK}"] = "0", ["{LIMIT_CHECK}"] = "1" },
                Map,
                BuildDropdownOptions(key1, key2)
            );
        };

        // Button 8
        view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.zombie_strings_8_1_key);
            string key2 = GetString(Resource.String.zombie_strings_8_2_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.zombie_strings_8),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.zombie_strings_8),
                new Dictionary<string, string> { ["{MIND_CHECK}"] = "0", ["{LIMIT_CHECK}"] = "1" },
                Map,
                BuildDropdownOptions(key1, key2)
            );
        };

        // Button 9
        view.FindViewById<Button>(Resource.Id.button9).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.zombie_strings_9_1_key);
            string key2 = GetString(Resource.String.zombie_strings_9_2_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.zombie_strings_9),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.zombie_strings_9),
                new Dictionary<string, string> { ["{MIND_CHECK}"] = "0", ["{LIMIT_CHECK}"] = "1" },
                Map,
                BuildDropdownOptions(key1, key2)
            );
        };

        // Button 10
        view.FindViewById<Button>(Resource.Id.button10).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.zombie_strings_10_1_key);
            string key2 = GetString(Resource.String.zombie_strings_10_2_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.zombie_strings_10),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.zombie_strings_10),
                new Dictionary<string, string> { ["{MIND_CHECK}"] = "0", ["{LIMIT_CHECK}"] = "1" },
                Map,
                BuildDropdownOptions(key1, key2)
            );
        };

        // Button 11
        view.FindViewById<Button>(Resource.Id.button11).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.zombie_strings_11_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.zombie_strings_11),
                BuildInitialData(key1),
                FragmentPath,
                GetString(Resource.String.zombie_strings_11),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map,
                BuildDropdownOptions(key1)
            );
        };

        return view;
    }
}
