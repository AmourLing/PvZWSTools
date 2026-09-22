using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_Android.Platform;

/// <summary>
/// Android 端的通知就是 Toast。共享层的每个失败分支本来就写着 <c>_notifier?.Warn/Error</c>
/// （未连接、脚本不存在、执行失败、连接失败），之前这里传的是 null，
/// 所以手机上点了没反应也没有任何说明。
/// </summary>
public sealed class AndroidUserNotifier:IUserNotifier
{
    public void Warn(string title, string message) => Show(title, message);

    public void Error(string title, string message) => Show(title, message);

    /// <summary>Toast 只能从主线程的 Looper 上弹，而这些提示有一半来自脚本回传的延续。</summary>
    private static void Show(string title, string message)
    {
        var context = (Context?)MainActivity.Instance ?? Android.App.Application.Context;
        string text = string.IsNullOrEmpty(title) ? message : $"{title}：{message}";
        new Handler(Looper.MainLooper!).Post(() => Toast.MakeText(context, text, ToastLength.Long)?.Show());
    }

    /// <summary>
    /// 唯一的 Confirm 调用在"发现新版本，是否下载"那一支，而 Android 的更新走
    /// <see cref="MainActivity"/> 自己的浏览器入口，不进共享层的下载流程，所以按"不去下载"答。
    /// </summary>
    public bool Confirm(string title, string message) => false;
}
