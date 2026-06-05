using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Xamarin
{
    public class FunFragment:AndroidX.Fragment.App.Fragment
    {
        private static readonly string mFunPath = "娱乐";
        private Dictionary<string, string> map;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if(savedInstanceState != null)
            {
                var keys = savedInstanceState.GetStringArrayList("savedMapKeys");
                var values = savedInstanceState.GetStringArrayList("savedMapValues");
                map = new Dictionary<string, string>();
                for(int i = 0;i < keys.Count;i++)
                {
                    map[keys[i]] = values[i];
                }
            }
            else
            {
                InitializeMap();
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fun_fragment, container, false);
            view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
                CreateInputDialog.OptAndDone2(Activity, GetString(Resource.String.fun_strings_1), new Dictionary<string, string>
                {
                    [GetString(Resource.String.fun_strings_1_1_key)] = map[GetString(Resource.String.fun_strings_1_1_key)],
                }, mFunPath, GetString(Resource.String.fun_strings_1), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                }, map);

            view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
            {
                CreateInputDialog.OptAndDone2(Activity, GetString(Resource.String.fun_strings_2), new Dictionary<string, string>
                {
                    [GetString(Resource.String.fun_strings_2_1_key)] = map[GetString(Resource.String.fun_strings_2_1_key)],
                }, mFunPath, GetString(Resource.String.fun_strings_2), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                }, map);
            };
            view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
             {
                 CreateInputDialog.OptAndDone2(Activity, GetString(Resource.String.fun_strings_2), new Dictionary<string, string>
                 {
                     [GetString(Resource.String.fun_strings_3_1_key)] = map[GetString(Resource.String.fun_strings_3_1_key)],
                 }, mFunPath, GetString(Resource.String.fun_strings_3), new Dictionary<string, string>
                 {
                     ["{CHECK}"] = "0",
                 }, map);
             };
            return view;
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            var keys = new List<string>(map.Keys);
            var values = new List<string>(map.Values);
            outState.PutStringArrayList("savedMapKeys", keys);
            outState.PutStringArrayList("savedMapValues", values);
        }

        private void InitializeMap()
        {
            map = new Dictionary<string, string>
            {
                [GetString(Resource.String.fun_strings_1_1_key)] = GetString(Resource.String.fun_strings_1_1_value),
                [GetString(Resource.String.fun_strings_2_1_key)] = GetString(Resource.String.fun_strings_2_1_value),
                [GetString(Resource.String.fun_strings_3_1_key)] = GetString(Resource.String.fun_strings_3_1_value),
            };
        }
    }
}
