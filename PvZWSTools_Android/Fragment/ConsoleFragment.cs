using System.ComponentModel;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using PvZWSTools_Android.Platform;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.ViewModels;

namespace PvZWSTools_Android;

/// <summary>
/// 控制台页：日志、WebSocket 收发，以及手动发一条脚本给游戏。
/// 桌面端这些原本靠它的控制台窗口看，Android 上没有那个窗口，所以搬进一个页面；
/// 内容仍是共享的 <see cref="ConsoleViewModel"/>，两端看到的是同一份。
/// </summary>
public class ConsoleFragment:AndroidX.Fragment.App.Fragment
{
    private PropertyChangedEventHandler? _onLineChanged;

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var ctx = Activity ?? Android.App.Application.Context;
        var console = AppServices.Root.Console;
        int pad = (int)(12 * ctx.Resources!.DisplayMetrics!.Density + 0.5f);

        var root = new LinearLayout(ctx) { Orientation = Orientation.Vertical };
        root.SetPadding(pad, pad, pad, pad);

        var filter = new Button(ctx)
        {
            Text = console.FilterText,
            LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f)
        };
        filter.Click += (_, _) =>
        {
            console.CycleFilterCommand.Execute(null);
            filter.Text = console.FilterText;
        };
        var follow = new CheckBox(ctx) { Text = "跟随", Checked = console.AutoScroll };
        follow.CheckedChange += (_, _) => console.AutoScroll = follow.Checked;
        var freeze = new CheckBox(ctx) { Text = "暂停", Checked = console.Frozen };
        freeze.CheckedChange += (_, _) => console.Frozen = freeze.Checked;
        var clear = new Button(ctx)
        {
            Text = "清空",
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
        };
        clear.Click += (_, _) => console.ClearCommand.Execute(null);

        var tools = new LinearLayout(ctx) { Orientation = Orientation.Horizontal };
        tools.AddView(filter);
        tools.AddView(follow);
        tools.AddView(freeze);
        tools.AddView(clear);

        var log = new TextView(ctx)
        {
            TextSize = 12,
            Typeface = Android.Graphics.Typeface.Monospace
        };
        var scroll = new ScrollView(ctx)
        {
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, 0, 1f)
        };
        scroll.AddView(log);

        var input = new EditText(ctx)
        {
            Hint = "写 Python 语句发给游戏",
            Text = console.Input,
            Typeface = Android.Graphics.Typeface.Monospace
        };
        input.SetMinLines(3);
        input.SetMaxLines(6);
        input.AfterTextChanged += (_, _) => console.Input = input.Text ?? string.Empty;

        var send = new Button(ctx)
        {
            Text = "发送",
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
        };
        send.Click += (_, _) => console.SendCommand.Execute(null);

        root.AddView(tools);
        root.AddView(scroll);
        root.AddView(input);
        root.AddView(send);

        _onLineChanged = (_, e) =>
        {
            if(e.PropertyName == nameof(ConsoleViewModel.Shown))
            {
                log.SetText(Render(console.Shown), TextView.BufferType.Normal);
                if(console.AutoScroll)
                    _ = scroll.FullScroll(FocusSearchDirection.Down);
            }
            else if(e.PropertyName == nameof(ConsoleViewModel.Input) && input.Text != console.Input)
            {
                // 发送完 VM 会清空 Input，这边得跟着清空；两边同值时不再回写，免得对弹。
                input.Text = console.Input;
            }
        };
        console.PropertyChanged += _onLineChanged;

        log.SetText(Render(console.Shown), TextView.BufferType.Normal);
        // 进页面时还没布局完，这时候 FullScroll 是空操作；等一帧再贴底。
        if(console.AutoScroll)
            _ = scroll.Post(() => scroll.FullScroll(FocusSearchDirection.Down));

        return root;
    }

    /// <summary>一行一段带颜色的文本。</summary>
    private static SpannableStringBuilder Render(IReadOnlyList<LogLine> lines)
    {
        var text = new SpannableStringBuilder();
        foreach(var line in lines)
        {
            int start = text.Length();
            _ = text.Append(line.Text + "\n");

            var color = ToColor(line.Color);
            if(color.HasValue)
                text.SetSpan(new ForegroundColorSpan(color.Value), start, text.Length(), SpanTypes.ExclusiveExclusive);
        }
        return text;
    }

    /// <summary>挑的是深浅背景都能读的中途色，不是控制台那套原色（原色在浅背景上几乎看不见）。
    /// 灰/白/黑返回 null：不加 span，跟着主题的文字色走。</summary>
    private static Color? ToColor(ConsoleColor color) => color switch
    {
        ConsoleColor.Red or ConsoleColor.DarkRed => Tint("#E5534B"),
        ConsoleColor.Yellow or ConsoleColor.DarkYellow => Tint("#D7A21B"),
        ConsoleColor.Green or ConsoleColor.DarkGreen => Tint("#4C9A2A"),
        ConsoleColor.Cyan or ConsoleColor.DarkCyan or ConsoleColor.Blue or ConsoleColor.DarkBlue => Tint("#2F80ED"),
        ConsoleColor.Magenta or ConsoleColor.DarkMagenta => Tint("#B34FD1"),
        _ => null,
    };

    private static Color Tint(string hex) => new(Color.ParseColor(hex));

    /// <summary>每次切页都是新建一个 Fragment，视图拆掉时必须摘掉订阅，
    /// 否则每来一行日志都要刷一堆已经看不见的旧文本框。</summary>
    public override void OnDestroyView()
    {
        var root = AppServices.Root;
        if(_onLineChanged != null && root != null)
            root.Console.PropertyChanged -= _onLineChanged;
        _onLineChanged = null;

        base.OnDestroyView();
    }
}
