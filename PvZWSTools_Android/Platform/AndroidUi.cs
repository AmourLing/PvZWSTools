using Android.Views;
using Android.Widget;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_Android.Platform;

/// <summary>安卓这边最省事的一层：布局 XML 里的中文由平台按设备语言取值，不跟应用内语种，
/// 所以 inflate 完把视图树走一遍，每个 TextView 就地过文案表。中文原文就是键，漏译退回中文。</summary>
static class AndroidUi
{
    internal static void LocalizeTexts(View? v)
    {
        if(v == null)
            return;
        if(v is TextView tv && !string.IsNullOrEmpty(tv.Text))
            tv.Text = Loc.T(tv.Text);
        if(v is ViewGroup group)
            for(int i = 0; i < group.ChildCount; i++)
                LocalizeTexts(group.GetChildAt(i));
    }
}
