using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Avalonia;

public class ChallengeFragment:BaseFragment
{
    private static readonly string mChallengePath = "挑战";

    public override void RefreshAllButtons()
    {
    }

    protected override string FragmentPath => mChallengePath;
    private static readonly string OptionFileNameSwitch2 = "开关2";

    protected override Dictionary<int, string> OptionFileMappings => new Dictionary<int, string>
    {
        // 假设所有挑战开关都使用 "开关.json" (对应之前的SwitchOptions逻辑，这里统一为一个文件)
        [Resource.String.challenge_strings_1_1_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_2_1_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_3_1_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_4_1_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_5_1_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_6_1_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_7_1_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_8_1_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_8_2_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_8_3_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_8_4_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_8_5_key] = OptionFileNameSwitch2,
        [Resource.String.challenge_strings_9_1_key] = OptionFileNameSwitch2,
    };

    protected override void InitializeMap()
    {
        Map[GetString(Resource.String.challenge_strings_1_1_key)] = GetString(Resource.String.challenge_strings_1_1_value);
        Map[GetString(Resource.String.challenge_strings_2_1_key)] = GetString(Resource.String.challenge_strings_2_1_value);
        Map[GetString(Resource.String.challenge_strings_3_1_key)] = GetString(Resource.String.challenge_strings_3_1_value);
        Map[GetString(Resource.String.challenge_strings_4_1_key)] = GetString(Resource.String.challenge_strings_4_1_value);
        Map[GetString(Resource.String.challenge_strings_5_1_key)] = GetString(Resource.String.challenge_strings_5_1_value);
        Map[GetString(Resource.String.challenge_strings_6_1_key)] = GetString(Resource.String.challenge_strings_6_1_value);
        Map[GetString(Resource.String.challenge_strings_7_1_key)] = GetString(Resource.String.challenge_strings_7_1_value);
        Map[GetString(Resource.String.challenge_strings_8_1_key)] = GetString(Resource.String.challenge_strings_8_1_value);
        Map[GetString(Resource.String.challenge_strings_8_2_key)] = GetString(Resource.String.challenge_strings_8_2_value);
        Map[GetString(Resource.String.challenge_strings_8_3_key)] = GetString(Resource.String.challenge_strings_8_3_value);
        Map[GetString(Resource.String.challenge_strings_8_4_key)] = GetString(Resource.String.challenge_strings_8_4_value);
        Map[GetString(Resource.String.challenge_strings_8_5_key)] = GetString(Resource.String.challenge_strings_8_5_value);
        Map[GetString(Resource.String.challenge_strings_9_1_key)] = GetString(Resource.String.challenge_strings_9_1_value);
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.challenge_fragment, container, false);

        // Helper for single key buttons
        void HandleSingleButton(int btnId, int titleResId, int keyResId)
        {
            view.FindViewById<Button>(btnId).Click += (sender, e) =>
            {
                string key = GetString(keyResId);
                CreateInputDialog.OptAndDone3(
                    Activity,
                    GetString(titleResId),
                    BuildInitialData(key),
                    FragmentPath,
                    GetString(titleResId),
                    new Dictionary<string, string> { ["{CHECK}"] = "0" },
                    Map,
                    BuildDropdownOptions(key)
                );
            };
        }

        HandleSingleButton(Resource.Id.button1, Resource.String.challenge_strings_1, Resource.String.challenge_strings_1_1_key);
        HandleSingleButton(Resource.Id.button2, Resource.String.challenge_strings_2, Resource.String.challenge_strings_2_1_key);
        HandleSingleButton(Resource.Id.button3, Resource.String.challenge_strings_3, Resource.String.challenge_strings_3_1_key);
        HandleSingleButton(Resource.Id.button4, Resource.String.challenge_strings_4, Resource.String.challenge_strings_4_1_key);
        HandleSingleButton(Resource.Id.button5, Resource.String.challenge_strings_5, Resource.String.challenge_strings_5_1_key);
        HandleSingleButton(Resource.Id.button6, Resource.String.challenge_strings_6, Resource.String.challenge_strings_6_1_key);
        HandleSingleButton(Resource.Id.button7, Resource.String.challenge_strings_7, Resource.String.challenge_strings_7_1_key);
        HandleSingleButton(Resource.Id.button9, Resource.String.challenge_strings_9, Resource.String.challenge_strings_9_1_key);

        // Button 8: Multiple keys
        view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
        {
            string k1 = GetString(Resource.String.challenge_strings_8_1_key);
            string k2 = GetString(Resource.String.challenge_strings_8_2_key);
            string k3 = GetString(Resource.String.challenge_strings_8_3_key);
            string k4 = GetString(Resource.String.challenge_strings_8_4_key);
            string k5 = GetString(Resource.String.challenge_strings_8_5_key);

            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.challenge_strings_8),
                BuildInitialData(k1, k2, k3, k4, k5),
                FragmentPath,
                GetString(Resource.String.challenge_strings_8),
                new Dictionary<string, string>
                {
                    ["{RAIN_CHECK}"] = "0",
                    ["{BEGHOULED_CHECK}"] = "1",
                    ["{SPEED_CHECK}"] = "2",
                    ["{PORTALCOMBAT_CHECK}"] = "3",
                    ["{LAST_STAND_CHECK}"] = "4",
                },
                Map,
                BuildDropdownOptions(k1, k2, k3, k4, k5)
            );
        };

        return view;
    }
}
