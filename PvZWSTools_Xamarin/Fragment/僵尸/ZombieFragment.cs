using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Xamarin
{
    public class ZombieFragment:AndroidX.Fragment.App.Fragment
    {
        private static readonly string mZombiePath = "僵尸";
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
            View view = inflater.Inflate(Resource.Layout.zombie_fragment, container, false);
            view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.zombie_strings_1), new Dictionary<string, string>
                {
                    [GetString(Resource.String.zombie_strings_1_1_key)] = GetString(Resource.String.zombie_strings_1_1_value)
                }, mZombiePath, GetString(Resource.String.zombie_strings_1), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.zombie_strings_2), new Dictionary<string, string>
                {
                    [GetString(Resource.String.zombie_strings_2_1_key)] = GetString(Resource.String.zombie_strings_2_1_value)
                }, mZombiePath, GetString(Resource.String.zombie_strings_2), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.zombie_strings_3), new Dictionary<string, string>
                {
                    [GetString(Resource.String.zombie_strings_3_1_key)] = GetString(Resource.String.zombie_strings_3_1_value)
                }, mZombiePath, GetString(Resource.String.zombie_strings_3), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.zombie_strings_4), new Dictionary<string, string>
                {
                    [GetString(Resource.String.zombie_strings_4_1_key)] = GetString(Resource.String.zombie_strings_4_1_value)
                }, mZombiePath, GetString(Resource.String.zombie_strings_4), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.zombie_strings_5), new Dictionary<string, string>
                {
                    [GetString(Resource.String.zombie_strings_5_1_key)] = GetString(Resource.String.zombie_strings_5_1_value)
                }, mZombiePath, GetString(Resource.String.zombie_strings_5), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.zombie_strings_6), new Dictionary<string, string>
                {
                    [GetString(Resource.String.zombie_strings_6_1_key)] = GetString(Resource.String.zombie_strings_6_1_value)
                }, mZombiePath, GetString(Resource.String.zombie_strings_6), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
            view.FindViewById<Button>(Resource.Id.button7).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.zombie_strings_7), new Dictionary<string, string>
                {
                    [GetString(Resource.String.zombie_strings_7_1_key)] = GetString(Resource.String.zombie_strings_7_1_value),
                    [GetString(Resource.String.zombie_strings_7_2_key)] = GetString(Resource.String.zombie_strings_7_2_value)
                }, mZombiePath, GetString(Resource.String.zombie_strings_7), new Dictionary<string, string>
                {
                    ["{MIND_CHECK}"] = "0",
                    ["{LIMIT_CHECK}"] = "1"
                });

            view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.zombie_strings_8), new Dictionary<string, string>
                {
                    [GetString(Resource.String.zombie_strings_8_1_key)] = GetString(Resource.String.zombie_strings_8_1_value),
                    [GetString(Resource.String.zombie_strings_8_2_key)] = GetString(Resource.String.zombie_strings_8_2_value)
                }, mZombiePath, GetString(Resource.String.zombie_strings_8), new Dictionary<string, string>
                {
                    ["{MIND_CHECK}"] = "0",
                    ["{LIMIT_CHECK}"] = "1"
                });
            view.FindViewById<Button>(Resource.Id.button9).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.zombie_strings_9), new Dictionary<string, string>
                {
                    [GetString(Resource.String.zombie_strings_9_1_key)] = GetString(Resource.String.zombie_strings_9_1_value),
                    [GetString(Resource.String.zombie_strings_9_2_key)] = GetString(Resource.String.zombie_strings_9_2_value)
                }, mZombiePath, GetString(Resource.String.zombie_strings_9), new Dictionary<string, string>
                {
                    ["{MIND_CHECK}"] = "0",
                    ["{LIMIT_CHECK}"] = "1"
                });
            view.FindViewById<Button>(Resource.Id.button10).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.zombie_strings_10), new Dictionary<string, string>
                {
                    [GetString(Resource.String.zombie_strings_10_1_key)] = GetString(Resource.String.zombie_strings_10_1_value),
                    [GetString(Resource.String.zombie_strings_10_2_key)] = GetString(Resource.String.zombie_strings_10_2_value)
                }, mZombiePath, GetString(Resource.String.zombie_strings_10), new Dictionary<string, string>
                {
                    ["{MIND_CHECK}"] = "0",
                    ["{LIMIT_CHECK}"] = "1"
                });
            view.FindViewById<Button>(Resource.Id.button11).Click += (sender, e) =>
                CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.zombie_strings_11), new Dictionary<string, string>
                {
                    [GetString(Resource.String.zombie_strings_11_1_key)] = GetString(Resource.String.zombie_strings_11_1_value),
                }, mZombiePath, GetString(Resource.String.zombie_strings_11), new Dictionary<string, string>
                {
                    ["{CHECK}"] = "0",
                });
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
                [GetString(Resource.String.zombie_strings_1_1_key)] = GetString(Resource.String.zombie_strings_1_1_value),
                [GetString(Resource.String.zombie_strings_2_1_key)] = GetString(Resource.String.zombie_strings_2_1_value),
                [GetString(Resource.String.zombie_strings_3_1_key)] = GetString(Resource.String.zombie_strings_3_1_value),
                [GetString(Resource.String.zombie_strings_4_1_key)] = GetString(Resource.String.zombie_strings_4_1_value),
                [GetString(Resource.String.zombie_strings_5_1_key)] = GetString(Resource.String.zombie_strings_5_1_value),
                [GetString(Resource.String.zombie_strings_6_1_key)] = GetString(Resource.String.zombie_strings_6_1_value),
                [GetString(Resource.String.zombie_strings_7_1_key)] = GetString(Resource.String.zombie_strings_7_1_value),
                [GetString(Resource.String.zombie_strings_7_2_key)] = GetString(Resource.String.zombie_strings_7_2_value),
                [GetString(Resource.String.zombie_strings_8_1_key)] = GetString(Resource.String.zombie_strings_8_1_value),
                [GetString(Resource.String.zombie_strings_8_2_key)] = GetString(Resource.String.zombie_strings_8_2_value),
                [GetString(Resource.String.zombie_strings_9_1_key)] = GetString(Resource.String.zombie_strings_9_1_value),
                [GetString(Resource.String.zombie_strings_9_2_key)] = GetString(Resource.String.zombie_strings_9_2_value),
                [GetString(Resource.String.zombie_strings_10_1_key)] = GetString(Resource.String.zombie_strings_10_1_value),
                [GetString(Resource.String.zombie_strings_10_2_key)] = GetString(Resource.String.zombie_strings_10_2_value),
                [GetString(Resource.String.zombie_strings_11_1_key)] = GetString(Resource.String.zombie_strings_11_1_value),
            };
        }
    }
}
