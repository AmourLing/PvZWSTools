using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Avalonia;

public class LevelFragment:BaseFragment
{
    private static readonly string mLevelPath = "关卡";
    protected override string FragmentPath => mLevelPath;

    public override void RefreshAllButtons()
    {
    }

    protected override Dictionary<int, string> OptionFileMappings => new Dictionary<int, string>
    {
        [Resource.String.level_strings_4_1_key] = "模式"   // 映射到“模式.json”
    };

    protected override void InitializeMap()
    {
        Map[GetString(Resource.String.level_strings_3_1_key)] = GetString(Resource.String.board_strings_3_1_value);
        Map[GetString(Resource.String.level_strings_4_1_key)] = GetString(Resource.String.board_strings_4_1_value);
        Map[GetString(Resource.String.level_strings_4_2_key)] = GetString(Resource.String.board_strings_4_2_value);
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.level_fragment, container, false);

        view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.level_strings_1), new Dictionary<string, string>(), FragmentPath, GetString(Resource.String.level_strings_1), new Dictionary<string, string>());

        view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.level_strings_2), new Dictionary<string, string>(), FragmentPath, GetString(Resource.String.level_strings_2), new Dictionary<string, string>());

        view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.level_strings_3), new Dictionary<string, string> { [GetString(Resource.String.level_strings_3_1_key)] = GetString(Resource.String.level_strings_3_1_value) }, FragmentPath, GetString(Resource.String.level_strings_3), new Dictionary<string, string> { ["{FLAG}"] = "0" });

        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
        {
            string key1 = GetString(Resource.String.level_strings_4_1_key);
            string key2 = GetString(Resource.String.level_strings_4_2_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.level_strings_4),
                BuildInitialData(key1, key2),
                FragmentPath,
                GetString(Resource.String.level_strings_4),
                new Dictionary<string, string> { ["{GAMEMODE}"] = "0", ["{ADVENTURENUM}"] = "1" },
                Map,
                BuildDropdownOptions(key1));   // 只有 key1 有下拉
        };

        return view;
    }
}
