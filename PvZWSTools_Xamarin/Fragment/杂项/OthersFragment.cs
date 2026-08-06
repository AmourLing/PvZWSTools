using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Xamarin;

public class OthersFragment:BaseFragment
{
    private static readonly string mOthersPath = "杂项";
    protected override string FragmentPath => mOthersPath;
    private static readonly string OptionFileNameSwitch1 = "开关1";

    protected override Dictionary<int, string> OptionFileMappings => new Dictionary<int, string>()
    {
        [Resource.String.others_strings_1_1_key] = OptionFileNameSwitch1,
        [Resource.String.others_strings_2_1_key] = OptionFileNameSwitch1,
        [Resource.String.others_strings_3_1_key] = OptionFileNameSwitch1,
        [Resource.String.others_strings_4_1_key] = OptionFileNameSwitch1,
        [Resource.String.others_strings_5_1_key] = OptionFileNameSwitch1,
        [Resource.String.others_strings_6_1_key] = OptionFileNameSwitch1,
        [Resource.String.others_strings_7_1_key] = OptionFileNameSwitch1,
        [Resource.String.others_strings_8_1_key] = OptionFileNameSwitch1,
        [Resource.String.others_strings_10_1_key] = OptionFileNameSwitch1,
        [Resource.String.others_strings_11_1_key] = OptionFileNameSwitch1,
    };

    protected override void InitializeMap()
    {
        Map[GetString(Resource.String.others_strings_1_1_key)] = GetString(Resource.String.others_strings_1_1_value);
        Map[GetString(Resource.String.others_strings_2_1_key)] = GetString(Resource.String.others_strings_2_1_value);
        Map[GetString(Resource.String.others_strings_3_1_key)] = GetString(Resource.String.others_strings_3_1_value);
        Map[GetString(Resource.String.others_strings_4_1_key)] = GetString(Resource.String.others_strings_4_1_value);
        Map[GetString(Resource.String.others_strings_5_1_key)] = GetString(Resource.String.others_strings_5_1_value);
        Map[GetString(Resource.String.others_strings_6_1_key)] = GetString(Resource.String.others_strings_6_1_value);
        Map[GetString(Resource.String.others_strings_7_1_key)] = GetString(Resource.String.others_strings_7_1_value);
        Map[GetString(Resource.String.others_strings_8_1_key)] = GetString(Resource.String.others_strings_8_1_value);
        Map[GetString(Resource.String.others_strings_9_1_key)] = GetString(Resource.String.others_strings_9_1_value);
        Map[GetString(Resource.String.others_strings_10_1_key)] = GetString(Resource.String.others_strings_10_1_value);
        Map[GetString(Resource.String.others_strings_11_1_key)] = GetString(Resource.String.others_strings_11_1_value);
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.others_fragment, container, false);

        void SetupButton(int btnId, int titleResId, int keyResId, string defaultValueKey, string defaultValue)
        {
            string key1 = GetString(keyResId);
            view.FindViewById<Button>(btnId).Click += (sender, e) =>
                CreateInputDialog.OptAndDone3(Activity,
                GetString(titleResId),
                new Dictionary<string, string>
                {
                    [key1] = key1
                },
                FragmentPath,
                GetString(titleResId),
                new Dictionary<string, string>
                {
                    [defaultValueKey] = defaultValue
                }, Map, BuildDropdownOptions(key1));
        }

        SetupButton(Resource.Id.button1, Resource.String.others_strings_1, Resource.String.others_strings_1_1_key, "{CHECK}", "0");
        SetupButton(Resource.Id.button2, Resource.String.others_strings_2, Resource.String.others_strings_2_1_key, "{CHECK}", "0");
        SetupButton(Resource.Id.button3, Resource.String.others_strings_3, Resource.String.others_strings_3_1_key, "{CHECK}", "0");
        SetupButton(Resource.Id.button4, Resource.String.others_strings_4, Resource.String.others_strings_4_1_key, "{CHECK}", "0");
        SetupButton(Resource.Id.button5, Resource.String.others_strings_5, Resource.String.others_strings_5_1_key, "{CHECK}", "0");
        SetupButton(Resource.Id.button6, Resource.String.others_strings_6, Resource.String.others_strings_6_1_key, "{CHECK}", "0");
        SetupButton(Resource.Id.button7, Resource.String.others_strings_7, Resource.String.others_strings_7_1_key, "{CHECK}", "0");
        SetupButton(Resource.Id.button8, Resource.String.others_strings_8, Resource.String.others_strings_8_1_key, "{CHECK}", "0");
        view.FindViewById<Button>(Resource.Id.button9).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.others_strings_9_1_key);
            CreateInputDialog.OptAndDone2(
                Activity,
                GetString(Resource.String.others_strings_9),
                BuildInitialData(key1),
                FragmentPath,
                GetString(Resource.String.others_strings_9_1_key),
                new Dictionary<string, string> { ["{TREEHEIGHT}"] = "0" },
                Map
            );
        };
        SetupButton(Resource.Id.button10, Resource.String.others_strings_10, Resource.String.others_strings_10_1_key, "{CHECK}", "0");
        SetupButton(Resource.Id.button11, Resource.String.others_strings_11, Resource.String.others_strings_11_1_key, "{CHECK}", "0");
        return view;
    }
}
