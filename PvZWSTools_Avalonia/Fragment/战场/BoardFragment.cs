using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Avalonia;

public class BoardFragment:BaseFragment
{
    private static readonly string mBoardPath = "战场";

    protected override string FragmentPath => mBoardPath;

    public override void RefreshAllButtons()
    {
    }

    protected override Dictionary<int, string> OptionFileMappings => new Dictionary<int, string>
    {
        [Resource.String.board_strings_1_1_key] = "行",
        [Resource.String.board_strings_1_2_key] = "列",
        [Resource.String.board_strings_1_3_key] = "植物",
        [Resource.String.board_strings_1_4_key] = "开关1",
        [Resource.String.board_strings_1_5_key] = "开关1",
        [Resource.String.board_strings_1_6_key] = "列偏移量",
        [Resource.String.board_strings_1_7_key] = "行偏移量",
        [Resource.String.board_strings_1_8_key] = "开关1",

        [Resource.String.board_strings_10_1_key] = "开关1",
        [Resource.String.board_strings_12_1_key] = "开关1",
        [Resource.String.board_strings_14_1_key] = "开关1",

        [Resource.String.board_strings_2_1_key] = "行",
        [Resource.String.board_strings_2_2_key] = "僵尸",
        [Resource.String.board_strings_2_3_key] = "开关1",
        [Resource.String.board_strings_2_4_key] = "列",
        [Resource.String.board_strings_2_5_key] = "开关1",
        [Resource.String.board_strings_2_6_key] = "列偏移量",
        [Resource.String.board_strings_2_7_key] = "行偏移量",

        [Resource.String.board_strings_3_1_key] = "行",
        [Resource.String.board_strings_3_2_key] = "列",
        [Resource.String.board_strings_3_3_key] = "物品",
        [Resource.String.board_strings_3_4_key] = "列偏移量",
        [Resource.String.board_strings_3_5_key] = "行偏移量",

        [Resource.String.board_strings_4_1_key] = "行",
        [Resource.String.board_strings_4_2_key] = "列",
        [Resource.String.board_strings_4_3_key] = "道具",
        [Resource.String.board_strings_4_4_key] = "植物",
        [Resource.String.board_strings_4_5_key] = "僵尸",
        [Resource.String.board_strings_4_8_key] = "列偏移量",
        [Resource.String.board_strings_4_9_key] = "行偏移量",

        [Resource.String.board_strings_9_1_key] = "开关1",
        [Resource.String.board_strings_9_2_key] = "开关1",
        [Resource.String.board_strings_9_3_key] = "开关1",
    };

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.board_fragment, container, false);

        view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
        {
            string key1_1 = GetString(Resource.String.board_strings_1_1_key);
            string key1_2 = GetString(Resource.String.board_strings_1_2_key);
            string key1_3 = GetString(Resource.String.board_strings_1_3_key);
            string key1_4 = GetString(Resource.String.board_strings_1_4_key);
            string key1_5 = GetString(Resource.String.board_strings_1_5_key);
            string key1_6 = GetString(Resource.String.board_strings_1_6_key);
            string key1_7 = GetString(Resource.String.board_strings_1_7_key);
            string key1_8 = GetString(Resource.String.board_strings_1_8_key);

            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.board_strings_1),
                BuildInitialData(key1_1, key1_2, key1_3, key1_4, key1_5, key1_6, key1_7, key1_8),
                FragmentPath,
                GetString(Resource.String.board_strings_1),
                new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{COL}"] = "1",
                    ["{SEEDTYPE}"] = "2",
                    ["{IMITATER}"] = "3",
                    ["{LIMITPLANTING}"] = "4",
                    ["{DELTA_MX}"] = "5",
                    ["{DELTA_MY}"] = "6",
                    ["{ISSLEEPING}"] = "7"
                },
                Map,
                BuildDropdownOptions(key1_1, key1_2, key1_3, key1_4, key1_5, key1_6, key1_7, key1_8)
            );
        };

        view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
        {
            string key2_1 = GetString(Resource.String.board_strings_2_1_key);
            string key2_2 = GetString(Resource.String.board_strings_2_2_key);
            string key2_3 = GetString(Resource.String.board_strings_2_3_key);
            string key2_4 = GetString(Resource.String.board_strings_2_4_key);
            string key2_5 = GetString(Resource.String.board_strings_2_5_key);
            string key2_6 = GetString(Resource.String.board_strings_2_6_key);
            string key2_7 = GetString(Resource.String.board_strings_2_7_key);

            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.board_strings_2),
                BuildInitialData(key2_1, key2_2, key2_3, key2_4, key2_5, key2_6, key2_7),
                FragmentPath,
                GetString(Resource.String.board_strings_2),
                new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{ZOMBIETYPE}"] = "1",
                    ["{COLPERMIT}"] = "2",
                    ["{COL}"] = "3",
                    ["{MINDCONTROL}"] = "4",
                    ["{DELTA_MX}"] = "5",
                    ["{DELTA_MY}"] = "6"
                },
                Map,
                BuildDropdownOptions(key2_1, key2_2, key2_3, key2_4, key2_5, key2_6, key2_7)
            );
        };

        view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
        {
            string key3_1 = GetString(Resource.String.board_strings_3_1_key);
            string key3_2 = GetString(Resource.String.board_strings_3_2_key);
            string key3_3 = GetString(Resource.String.board_strings_3_3_key);
            string key3_4 = GetString(Resource.String.board_strings_3_4_key);
            string key3_5 = GetString(Resource.String.board_strings_3_5_key);

            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.board_strings_3),
                BuildInitialData(key3_1, key3_2, key3_3, key3_4, key3_5),
                FragmentPath,
                GetString(Resource.String.board_strings_3),
                new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{COL}"] = "1",
                    ["{COINTYPE}"] = "2",
                    ["{DELTA_MX}"] = "3",
                    ["{DELTA_MY}"] = "4"
                },
                Map,
                BuildDropdownOptions(key3_1, key3_2, key3_3, key3_4, key3_5)
            );
        };

        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
        {
            string key4_1 = GetString(Resource.String.board_strings_4_1_key);
            string key4_2 = GetString(Resource.String.board_strings_4_2_key);
            string key4_3 = GetString(Resource.String.board_strings_4_3_key);
            string key4_4 = GetString(Resource.String.board_strings_4_4_key);
            string key4_5 = GetString(Resource.String.board_strings_4_5_key);
            string key4_6 = GetString(Resource.String.board_strings_4_6_key);
            string key4_7 = GetString(Resource.String.board_strings_4_7_key);
            string key4_8 = GetString(Resource.String.board_strings_4_8_key);
            string key4_9 = GetString(Resource.String.board_strings_4_9_key);

            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.board_strings_4),
                BuildInitialData(key4_1, key4_2, key4_3, key4_4, key4_5, key4_6, key4_7, key4_8, key4_9),
                FragmentPath,
                GetString(Resource.String.board_strings_4),
                new Dictionary<string, string>
                {
                    ["{ROW}"] = "0",
                    ["{COL}"] = "1",
                    ["{ITEM}"] = "2",
                    ["{SCARYPOT_SEEDTYPE}"] = "3",
                    ["{SCARYPOT_ZOMBIETYPE}"] = "4",
                    ["{SCARYPOT_SCARYPOTTYPE}"] = "5",
                    ["{SCARYPOT_STATE}"] = "6",
                    ["{DELTA_MX}"] = "7",
                    ["{DELTA_MY}"] = "8"
                },
                Map,
                BuildDropdownOptions(key4_1, key4_2, key4_3, key4_4, key4_5, key4_8, key4_9)
            );
        };

        view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.board_strings_5),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.board_strings_5),
                new Dictionary<string, string>()
            );

        view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.board_strings_6),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.board_strings_6),
                new Dictionary<string, string>()
            );

        view.FindViewById<Button>(Resource.Id.button7).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.board_strings_7),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.board_strings_7),
                new Dictionary<string, string>()
            );

        view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.board_strings_8),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.board_strings_8),
                new Dictionary<string, string>()
            );

        view.FindViewById<Button>(Resource.Id.button9).Click += (sender, e) =>
        {
            string key9_1 = GetString(Resource.String.board_strings_9_1_key);
            string key9_2 = GetString(Resource.String.board_strings_9_2_key);
            string key9_3 = GetString(Resource.String.board_strings_9_3_key);

            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.board_strings_9),
                BuildInitialData(key9_1, key9_2, key9_3),
                FragmentPath,
                GetString(Resource.String.board_strings_9),
                new Dictionary<string, string>
                {
                    ["{RUN}"] = "0",
                    ["{DE}"] = "1",
                    ["{RE}"] = "2"
                },
                Map, BuildDropdownOptions(key9_1, key9_2, key9_3)
            );
        };

        view.FindViewById<Button>(Resource.Id.button10).Click += (sender, e) =>
        {
            string key10_1 = GetString(Resource.String.board_strings_10_1_key);

            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.board_strings_10),
                BuildInitialData(key10_1),
                FragmentPath,
                GetString(Resource.String.board_strings_10),
                new Dictionary<string, string> { ["{CHECK}"] = "0" },
                Map, BuildDropdownOptions(key10_1)
            );
        };

        view.FindViewById<Button>(Resource.Id.button11).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.board_strings_11),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.board_strings_11),
                new Dictionary<string, string>()
            );

        view.FindViewById<Button>(Resource.Id.button12).Click += (sender, e) =>
        {
            string key12_1 = GetString(Resource.String.board_strings_12_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.board_strings_12),
                BuildInitialData(key12_1),
                FragmentPath,
                GetString(Resource.String.board_strings_12),
                new Dictionary<string, string> { ["{CHECK}"] = "0" }, Map, BuildDropdownOptions(key12_1)
            );
        };

        view.FindViewById<Button>(Resource.Id.button13).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.board_strings_13),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.board_strings_13),
                new Dictionary<string, string>()
            );

        view.FindViewById<Button>(Resource.Id.button14).Click += (sender, e) =>
        {
            string key14_1 = GetString(Resource.String.board_strings_14_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.board_strings_14),
                BuildInitialData(key14_1),
                FragmentPath,
                GetString(Resource.String.board_strings_14),
                new Dictionary<string, string> { ["{CHECK}"] = "0" }, Map, BuildDropdownOptions(key14_1)
            );
        };

        view.FindViewById<Button>(Resource.Id.button15).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(
                Activity,
                GetString(Resource.String.board_strings_15),
                new Dictionary<string, string>(),
                FragmentPath,
                GetString(Resource.String.board_strings_15),
                new Dictionary<string, string>()
            );

        return view;
    }

    protected override void InitializeMap()
    {
        Map[GetString(Resource.String.board_strings_1_1_key)] = GetString(Resource.String.board_strings_1_1_value);
        Map[GetString(Resource.String.board_strings_1_2_key)] = GetString(Resource.String.board_strings_1_2_value);
        Map[GetString(Resource.String.board_strings_1_3_key)] = GetString(Resource.String.board_strings_1_3_value);
        Map[GetString(Resource.String.board_strings_1_4_key)] = GetString(Resource.String.board_strings_1_4_value);
        Map[GetString(Resource.String.board_strings_1_5_key)] = GetString(Resource.String.board_strings_1_5_value);
        Map[GetString(Resource.String.board_strings_1_6_key)] = GetString(Resource.String.board_strings_1_6_value);
        Map[GetString(Resource.String.board_strings_1_7_key)] = GetString(Resource.String.board_strings_1_7_value);
        Map[GetString(Resource.String.board_strings_1_8_key)] = GetString(Resource.String.board_strings_1_8_value);

        Map[GetString(Resource.String.board_strings_2_1_key)] = GetString(Resource.String.board_strings_2_1_value);
        Map[GetString(Resource.String.board_strings_2_2_key)] = GetString(Resource.String.board_strings_2_2_value);
        Map[GetString(Resource.String.board_strings_2_3_key)] = GetString(Resource.String.board_strings_2_3_value);
        Map[GetString(Resource.String.board_strings_2_4_key)] = GetString(Resource.String.board_strings_2_4_value);
        Map[GetString(Resource.String.board_strings_2_5_key)] = GetString(Resource.String.board_strings_2_5_value);
        Map[GetString(Resource.String.board_strings_2_6_key)] = GetString(Resource.String.board_strings_2_6_value);
        Map[GetString(Resource.String.board_strings_2_7_key)] = GetString(Resource.String.board_strings_2_7_value);

        Map[GetString(Resource.String.board_strings_3_1_key)] = GetString(Resource.String.board_strings_3_1_value);
        Map[GetString(Resource.String.board_strings_3_2_key)] = GetString(Resource.String.board_strings_3_2_value);
        Map[GetString(Resource.String.board_strings_3_3_key)] = GetString(Resource.String.board_strings_3_3_value);
        Map[GetString(Resource.String.board_strings_3_4_key)] = GetString(Resource.String.board_strings_3_4_value);
        Map[GetString(Resource.String.board_strings_3_5_key)] = GetString(Resource.String.board_strings_3_5_value);

        Map[GetString(Resource.String.board_strings_4_1_key)] = GetString(Resource.String.board_strings_4_1_value);
        Map[GetString(Resource.String.board_strings_4_2_key)] = GetString(Resource.String.board_strings_4_2_value);
        Map[GetString(Resource.String.board_strings_4_3_key)] = GetString(Resource.String.board_strings_4_3_value);
        Map[GetString(Resource.String.board_strings_4_4_key)] = GetString(Resource.String.board_strings_4_4_value);
        Map[GetString(Resource.String.board_strings_4_5_key)] = GetString(Resource.String.board_strings_4_5_value);
        Map[GetString(Resource.String.board_strings_4_6_key)] = GetString(Resource.String.board_strings_4_6_value);
        Map[GetString(Resource.String.board_strings_4_7_key)] = GetString(Resource.String.board_strings_4_7_value);
        Map[GetString(Resource.String.board_strings_4_8_key)] = GetString(Resource.String.board_strings_4_8_value);
        Map[GetString(Resource.String.board_strings_4_9_key)] = GetString(Resource.String.board_strings_4_9_value);

        Map[GetString(Resource.String.board_strings_9_1_key)] = GetString(Resource.String.board_strings_9_1_value);
        Map[GetString(Resource.String.board_strings_9_2_key)] = GetString(Resource.String.board_strings_9_2_value);
        Map[GetString(Resource.String.board_strings_9_3_key)] = GetString(Resource.String.board_strings_9_3_value);

        Map[GetString(Resource.String.board_strings_10_1_key)] = GetString(Resource.String.board_strings_10_1_value);

        Map[GetString(Resource.String.board_strings_12_1_key)] = GetString(Resource.String.board_strings_12_1_value);

        Map[GetString(Resource.String.board_strings_14_1_key)] = GetString(Resource.String.board_strings_14_1_value);
    }
}
