using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PvZWSTools_Xamarin;

public class ChallengeFragment:AndroidX.Fragment.App.Fragment
{
    private static readonly string mChallengePath = "挑战";

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.challenge_fragment, container, false);
        view.FindViewById<Button>(Resource.Id.button1).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.challenge_strings_1), new Dictionary<string, string>
            {
                [GetString(Resource.String.challenge_strings_1_1_key)] = GetString(Resource.String.challenge_strings_1_1_value),
            }, mChallengePath, GetString(Resource.String.challenge_strings_1), new Dictionary<string, string>
            {
                ["{CHECK}"] = "0",
            });
        view.FindViewById<Button>(Resource.Id.button2).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.challenge_strings_2), new Dictionary<string, string>
            {
                [GetString(Resource.String.challenge_strings_2_1_key)] = GetString(Resource.String.challenge_strings_2_1_value),
            }, mChallengePath, GetString(Resource.String.challenge_strings_2), new Dictionary<string, string>
            {
                ["{CHECK}"] = "0",
            });
        view.FindViewById<Button>(Resource.Id.button3).Click += (sender, e) =>
             CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.challenge_strings_3), new Dictionary<string, string>
             {
                 [GetString(Resource.String.challenge_strings_3_1_key)] = GetString(Resource.String.challenge_strings_3_1_value),
             }, mChallengePath, GetString(Resource.String.challenge_strings_3), new Dictionary<string, string>
             {
                 ["{CHECK}"] = "0",
             });
        view.FindViewById<Button>(Resource.Id.button4).Click += (sender, e) =>
           CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.challenge_strings_4), new Dictionary<string, string>
           {
               [GetString(Resource.String.challenge_strings_4_1_key)] = GetString(Resource.String.challenge_strings_4_1_value),
           }, mChallengePath, GetString(Resource.String.challenge_strings_4), new Dictionary<string, string>
           {
               ["{CHECK}"] = "0",
           });
        view.FindViewById<Button>(Resource.Id.button5).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.challenge_strings_5), new Dictionary<string, string>
            {
                [GetString(Resource.String.challenge_strings_5_1_key)] = GetString(Resource.String.challenge_strings_5_1_value),
            }, mChallengePath, GetString(Resource.String.challenge_strings_5), new Dictionary<string, string>
            {
                ["{CHECK}"] = "0",
            });
        view.FindViewById<Button>(Resource.Id.button6).Click += (sender, e) =>
             CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.challenge_strings_6), new Dictionary<string, string>
             {
                 [GetString(Resource.String.challenge_strings_6_1_key)] = GetString(Resource.String.challenge_strings_6_1_value),
             }, mChallengePath, GetString(Resource.String.challenge_strings_6), new Dictionary<string, string>
             {
                 ["{CHECK}"] = "0",
             });
        view.FindViewById<Button>(Resource.Id.button7).Click += (sender, e) =>
             CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.challenge_strings_7), new Dictionary<string, string>
             {
                 [GetString(Resource.String.challenge_strings_7_1_key)] = GetString(Resource.String.challenge_strings_7_1_value),
             }, mChallengePath, GetString(Resource.String.challenge_strings_7), new Dictionary<string, string>
             {
                 ["{CHECK}"] = "0",
             });
        view.FindViewById<Button>(Resource.Id.button8).Click += (sender, e) =>
           CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.challenge_strings_8), new Dictionary<string, string>
           {
               [GetString(Resource.String.challenge_strings_8_1_key)] = GetString(Resource.String.challenge_strings_8_1_value),
               [GetString(Resource.String.challenge_strings_8_2_key)] = GetString(Resource.String.challenge_strings_8_2_value),
               [GetString(Resource.String.challenge_strings_8_3_key)] = GetString(Resource.String.challenge_strings_8_3_value),
               [GetString(Resource.String.challenge_strings_8_4_key)] = GetString(Resource.String.challenge_strings_8_4_value),
               [GetString(Resource.String.challenge_strings_8_5_key)] = GetString(Resource.String.challenge_strings_8_5_value),
           }, mChallengePath, GetString(Resource.String.challenge_strings_8), new Dictionary<string, string>
           {
               ["{RAIN_CHECK}"] = "0",
               ["{BEGHOULED_CHECK}"] = "1",
               ["{SPEED_CHECK}"] = "2",
               ["{PORTALCOMBAT_CHECK}"] = "3",
               ["{LAST_STAND_CHECK}"] = "4",
           });
        view.FindViewById<Button>(Resource.Id.button9).Click += (sender, e) =>
            CreateInputDialog.OptAndDone(Activity, GetString(Resource.String.challenge_strings_9), new Dictionary<string, string>
            {
                [GetString(Resource.String.challenge_strings_9_1_key)] = GetString(Resource.String.challenge_strings_9_1_value),
            }, mChallengePath, GetString(Resource.String.challenge_strings_9), new Dictionary<string, string>
            {
                ["{CHECK}"] = "0",
            });
        return view;
    }
}
