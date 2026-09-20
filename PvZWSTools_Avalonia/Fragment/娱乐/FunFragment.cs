using Android.Views;

namespace PvZWSTools_Avalonia;

public class FunFragment:BaseFragment
{
    private static readonly string mFunPath = "娱乐";

    protected override string FragmentPath => mFunPath;

    protected override Dictionary<int, string> OptionFileMappings => new Dictionary<int, string>()
    {
        [Resource.String.fun_strings_1_1_key] = "开关1",
        [Resource.String.fun_strings_2_1_key] = "开关1",
        [Resource.String.fun_strings_3_1_key] = "开关1",
        [Resource.String.fun_strings_4_1_key] = "开关1",
        [Resource.String.fun_strings_5_1_key] = "开关1",
        [Resource.String.fun_strings_6_1_key] = "开关1",
    };

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.fun_fragment, container, false);
        view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.fun_strings_1), BuildInitialData(GetString(Resource.String.fun_strings_1_1_key)), FragmentPath, GetString(Resource.String.fun_strings_1), new Dictionary<string, string> { ["{CHECK}"] = "0" }, Map, BuildDropdownOptions(GetString(Resource.String.fun_strings_1_1_key)));

        view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.fun_strings_2), BuildInitialData(GetString(Resource.String.fun_strings_2_1_key)), FragmentPath, GetString(Resource.String.fun_strings_2), new Dictionary<string, string> { ["{CHECK}"] = "0" }, Map, BuildDropdownOptions(GetString(Resource.String.fun_strings_2_1_key)));

        view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.fun_strings_3), BuildInitialData(GetString(Resource.String.fun_strings_3_1_key)), FragmentPath, GetString(Resource.String.fun_strings_3), new Dictionary<string, string> { ["{CHECK}"] = "0" }, Map, BuildDropdownOptions(GetString(Resource.String.fun_strings_3_1_key)));

        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.fun_strings_4), BuildInitialData(GetString(Resource.String.fun_strings_4_1_key)), FragmentPath, GetString(Resource.String.fun_strings_4), new Dictionary<string, string> { ["{CHECK}"] = "0" }, Map, BuildDropdownOptions(GetString(Resource.String.fun_strings_4_1_key)));

        view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.fun_strings_5), BuildInitialData(GetString(Resource.String.fun_strings_5_1_key)), FragmentPath, GetString(Resource.String.fun_strings_5), new Dictionary<string, string> { ["{CHECK}"] = "0" }, Map, BuildDropdownOptions(GetString(Resource.String.fun_strings_5_1_key)));

        view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
            CreateInputDialog.OptAndDone3(Activity, GetString(Resource.String.fun_strings_6), BuildInitialData(GetString(Resource.String.fun_strings_6_1_key)), FragmentPath, GetString(Resource.String.fun_strings_6), new Dictionary<string, string> { ["{CHECK}"] = "0" }, Map, BuildDropdownOptions(GetString(Resource.String.fun_strings_6_1_key)));

        return view;
    }

    public override void RefreshAllButtons()
    {
    }

    protected override void InitializeMap()
    {
        Map[GetString(Resource.String.fun_strings_1_1_key)] = GetString(Resource.String.fun_strings_1_1_value);
        Map[GetString(Resource.String.fun_strings_2_1_key)] = GetString(Resource.String.fun_strings_2_1_value);
        Map[GetString(Resource.String.fun_strings_3_1_key)] = GetString(Resource.String.fun_strings_3_1_value);
        Map[GetString(Resource.String.fun_strings_4_1_key)] = GetString(Resource.String.fun_strings_4_1_value);
        Map[GetString(Resource.String.fun_strings_5_1_key)] = GetString(Resource.String.fun_strings_5_1_value);
        Map[GetString(Resource.String.fun_strings_6_1_key)] = GetString(Resource.String.fun_strings_6_1_value);
    }
}
