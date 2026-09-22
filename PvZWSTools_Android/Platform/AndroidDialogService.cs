using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;
using PvZWSTools_Shared.ViewModels;

namespace PvZWSTools_Android.Platform;

/// <summary>
/// 应用内对话框。波次出怪 JSON 走这里 —— Android 没有能打开 .json 的系统关联程序，
/// 导出之后必须由应用内编辑框接手（桌面端那边才是"勾了才开系统编辑器"）。
/// 花园功能在 Android 上没有对应界面，所以其余对话框一律返回"取消"，
/// 这个实现同时满足 MainWindowViewModel 的构造要求。
/// </summary>
public sealed class AndroidDialogService:IDialogService
{
    public Task<bool> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel:class
    {
        var activity = MainActivity.Instance;
        if(viewModel is not WaveJsonEditorViewModel editor || activity == null)
            return Task.FromResult(false);

        var tcs = new TaskCompletionSource<bool>();
        activity.RunOnUiThread(() =>
        {
            try
            {
                ShowWaveJsonEditor(activity, editor, tcs);
            }
            catch(Exception ex)
            {
                // 编辑框没起来也要收口，否则调用方一直等着，那一行功能就永久点不动了。
                Log.Error($"波次出怪编辑框打开失败：{ex.Message}");
                _ = tcs.TrySetResult(false);
            }
        });
        return tcs.Task;
    }

    /// <summary>按钮放在编辑框下方自己做：Android 对话框的按钮点完必然关窗，
    /// 而 JSON 校验不通过时要留住用户已改的内容。</summary>
    private static void ShowWaveJsonEditor(Activity activity, WaveJsonEditorViewModel editor, TaskCompletionSource<bool> tcs)
    {
        int pad = (int)(16 * activity.Resources!.DisplayMetrics!.Density + 0.5f);

        var input = new EditText(activity)
        {
            Text = editor.Json,
            Gravity = GravityFlags.Top | GravityFlags.Start,
            Typeface = Android.Graphics.Typeface.Monospace,
            InputType = Android.Text.InputTypes.ClassText
                | Android.Text.InputTypes.TextFlagMultiLine
                | Android.Text.InputTypes.TextFlagNoSuggestions,
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
        };
        input.SetMinLines(14);
        input.SetMaxLines(24);

        var scroll = new ScrollView(activity)
        {
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, 0, 1f)
        };
        scroll.AddView(input);

        var apply = new Button(activity)
        {
            Text = "载入到游戏",
            LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f)
        };
        var saveOnly = new Button(activity)
        {
            Text = "仅保存",
            LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f)
        };
        var cancel = new Button(activity)
        {
            Text = "取消",
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
        };
        var buttons = new LinearLayout(activity) { Orientation = Orientation.Horizontal };
        buttons.AddView(apply);
        buttons.AddView(saveOnly);
        buttons.AddView(cancel);

        var root = new LinearLayout(activity) { Orientation = Orientation.Vertical };
        root.SetPadding(pad, pad, pad, pad);
        root.AddView(scroll);
        root.AddView(buttons);

        var dialog = new AlertDialog.Builder(activity)
            .SetTitle("编辑波次出怪 JSON")
            .SetView(root)
            .SetCancelable(false)
            .Create();

        apply.Click += (_, _) =>
        {
            editor.Json = input.Text ?? string.Empty;
            if(!editor.TryApply())
            {
                Toast.MakeText(activity, $"JSON 格式错误：{editor.Error}", ToastLength.Long)?.Show();
                return;
            }

            _ = tcs.TrySetResult(true);
            dialog.Dismiss();
        };
        // 与「载入到游戏」走同一份校验，只是收口后不发给游戏
        saveOnly.Click += (_, _) =>
        {
            editor.Json = input.Text ?? string.Empty;
            if(!editor.TryApply(saveOnly: true))
            {
                Toast.MakeText(activity, $"JSON 格式错误：{editor.Error}", ToastLength.Long)?.Show();
                return;
            }

            _ = tcs.TrySetResult(true);
            dialog.Dismiss();
        };
        cancel.Click += (_, _) =>
        {
            _ = tcs.TrySetResult(false);
            dialog.Dismiss();
        };
        dialog.Show();
    }
}
