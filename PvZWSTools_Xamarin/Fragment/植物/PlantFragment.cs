using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Xamarin;

public class PlantFragment:BaseFragment
{
    private static readonly string mPlantPath = "植物";

    protected override string FragmentPath => mPlantPath;

    public override void RefreshAllButtons()
    {
    }

    protected override Dictionary<int, string> OptionFileMappings => new Dictionary<int, string>
    {
        [Resource.String.plant_strings_1_1_key] = "开关1",
        [Resource.String.plant_strings_2_1_key] = "开关1",
        [Resource.String.plant_strings_3_1_key] = "开关1",
        [Resource.String.plant_strings_4_1_key] = "植物",
        [Resource.String.plant_strings_5_1_key] = "开关1",
        [Resource.String.plant_strings_6_1_key] = "开关1",
        [Resource.String.plant_strings_7_1_key] = "开关1",
        [Resource.String.plant_strings_8_1_key] = "开关1",
        [Resource.String.plant_strings_9_1_key] = "开关1",
        [Resource.String.plant_strings_10_1_key] = "开关1",
        [Resource.String.plant_strings_11_1_key] = "开关1",
        [Resource.String.plant_strings_12_1_key] = "开关1",
    };

    protected override void InitializeMap()
    {
        Map[GetString(Resource.String.plant_strings_1_1_key)] = GetString(Resource.String.plant_strings_1_1_value);
        Map[GetString(Resource.String.plant_strings_2_1_key)] = GetString(Resource.String.plant_strings_2_1_value);
        Map[GetString(Resource.String.plant_strings_3_1_key)] = GetString(Resource.String.plant_strings_3_1_value);
        Map[GetString(Resource.String.plant_strings_4_1_key)] = GetString(Resource.String.plant_strings_4_1_value);
        Map[GetString(Resource.String.plant_strings_5_1_key)] = GetString(Resource.String.plant_strings_5_1_value);
        Map[GetString(Resource.String.plant_strings_6_1_key)] = GetString(Resource.String.plant_strings_6_1_value);
        Map[GetString(Resource.String.plant_strings_7_1_key)] = GetString(Resource.String.plant_strings_7_1_value);
        Map[GetString(Resource.String.plant_strings_8_1_key)] = GetString(Resource.String.plant_strings_8_1_value);
        Map[GetString(Resource.String.plant_strings_9_1_key)] = GetString(Resource.String.plant_strings_9_1_value);
        Map[GetString(Resource.String.plant_strings_10_1_key)] = GetString(Resource.String.plant_strings_10_1_value);
        Map[GetString(Resource.String.plant_strings_11_1_key)] = GetString(Resource.String.plant_strings_11_1_value);
        Map[GetString(Resource.String.plant_strings_12_1_key)] = GetString(Resource.String.plant_strings_12_1_value);
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.plant_fragment, container, false);

        void Done(int buttonid, int id1, int id2)
        {
            view.FindViewById<Button>(buttonid).Click += (sender, e) =>
            {
                string key = GetString(id1);
                CreateInputDialog.OptAndDone3(
                    Activity,
                    GetString(id2),
                    BuildInitialData(key),
                    FragmentPath,
                    GetString(id2),
                    new Dictionary<string, string> { ["{CHECK}"] = "0" },
                    Map,
                    BuildDropdownOptions(key)
                );
            };
        }

        Done(Resource.Id.button1, Resource.String.plant_strings_1_1_key, Resource.String.plant_strings_1);
        Done(Resource.Id.button2, Resource.String.plant_strings_2_1_key, Resource.String.plant_strings_2);
        Done(Resource.Id.button3, Resource.String.plant_strings_3_1_key, Resource.String.plant_strings_3);

        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
        {
            string key = GetString(Resource.String.plant_strings_4_1_key);
            CreateInputDialog.OptAndDone3(
                Activity,
                GetString(Resource.String.plant_strings_4),
                BuildInitialData(key),
                FragmentPath,
                GetString(Resource.String.plant_strings_4),
                new Dictionary<string, string> { ["{SEEDTYPE}"] = "0" },
                Map,
                BuildDropdownOptions(key)
            );
        };

        Done(Resource.Id.button5, Resource.String.plant_strings_5_1_key, Resource.String.plant_strings_5);
        Done(Resource.Id.button6, Resource.String.plant_strings_6_1_key, Resource.String.plant_strings_6);
        Done(Resource.Id.button7, Resource.String.plant_strings_7_1_key, Resource.String.plant_strings_7);
        Done(Resource.Id.button8, Resource.String.plant_strings_8_1_key, Resource.String.plant_strings_8);
        Done(Resource.Id.button9, Resource.String.plant_strings_9_1_key, Resource.String.plant_strings_9);
        Done(Resource.Id.button10, Resource.String.plant_strings_10_1_key, Resource.String.plant_strings_10);
        Done(Resource.Id.button11, Resource.String.plant_strings_11_1_key, Resource.String.plant_strings_11);
        Done(Resource.Id.button12, Resource.String.plant_strings_12_1_key, Resource.String.plant_strings_12);

        return view;
    }
}
