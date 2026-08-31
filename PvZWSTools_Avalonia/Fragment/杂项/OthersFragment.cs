using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Avalonia;

public class OthersFragment:BaseFragment
{
    private static readonly string mOthersPath = "杂项";
    private static readonly string OptionFileNameSwitch1 = "开关1";

    private readonly Dictionary<int, int> _buttonKeyMap = new Dictionary<int, int>
    {
        [Resource.Id.button1] = Resource.String.others_strings_1_1_key,
        [Resource.Id.button2] = Resource.String.others_strings_2_1_key,
        [Resource.Id.button3] = Resource.String.others_strings_3_1_key,
        [Resource.Id.button4] = Resource.String.others_strings_4_1_key,
        [Resource.Id.button5] = Resource.String.others_strings_5_1_key,
        [Resource.Id.button6] = Resource.String.others_strings_6_1_key,
        [Resource.Id.button7] = Resource.String.others_strings_7_1_key,
        [Resource.Id.button8] = Resource.String.others_strings_8_1_key,
        [Resource.Id.button10] = Resource.String.others_strings_10_1_key,
        [Resource.Id.button11] = Resource.String.others_strings_11_1_key,
    };

    protected override string FragmentPath => mOthersPath;

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
                }, Map, BuildDropdownOptions(key1), onAfterConfirm: _ => RefreshAllButtons());
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

        RefreshAllButtons();

        return view;
    }

    public override void RefreshAllButtons()
    {
        foreach(var kv in _buttonKeyMap)
        {
            var btn = View?.FindViewById<Button>(kv.Key);
            if(btn == null) continue;
            string key = GetString(kv.Value);
            string value = Map.ContainsKey(key) ? Map[key] : "0";
            string status = value == "1" ? "(开)" : "(关)";
            string title = GetString(GetButtonTitleResId(kv.Key));
            btn.Text = $"{title}{status}";
        }
    }

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

    private int GetButtonTitleResId(int buttonId)
    {
        switch(buttonId)
        {
            case Resource.Id.button1: return Resource.String.others_strings_1;
            case Resource.Id.button2: return Resource.String.others_strings_2;
            case Resource.Id.button3: return Resource.String.others_strings_3;
            case Resource.Id.button4: return Resource.String.others_strings_4;
            case Resource.Id.button5: return Resource.String.others_strings_5;
            case Resource.Id.button6: return Resource.String.others_strings_6;
            case Resource.Id.button7: return Resource.String.others_strings_7;
            case Resource.Id.button8: return Resource.String.others_strings_8;
            case Resource.Id.button10: return Resource.String.others_strings_10;
            case Resource.Id.button11: return Resource.String.others_strings_11;
            default: return 0;
        }
    }
}
