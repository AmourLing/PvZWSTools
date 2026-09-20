using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using PvZWSTools_Avalonia.Platform;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.ViewModels;

namespace PvZWSTools_Avalonia;

/// <summary>
/// 快捷脚本页：选脚本 → 按该脚本的 .py.config.json 出参数 → 运行。
///
/// 脚本列表、配置解析、占位符替换、发送全在共享的 QModViewModel 里
/// （LoadQModOptions / LoadScriptParameters / QModCommand），
/// 这里只负责手机上的排布：参数改成选中脚本后即时重建，
/// 因为参数表依赖当前选的是哪个脚本，没法一次画完。
/// </summary>
public class ScriptFragment:AndroidX.Fragment.App.Fragment
{
    private Spinner _scripts;
    private LinearLayout _parameters;
    private TextView _info;
    private TextView _author;
    private QModViewModel Vm => AppServices.Root?.QMod;

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var ctx = Activity ?? Android.App.Application.Context;
        int pad = (int)(14 * ctx.Resources!.DisplayMetrics!.Density + 0.5f);

        var scroll = new ScrollView(ctx);
        var body = new LinearLayout(ctx) { Orientation = Orientation.Vertical };
        body.SetPadding(pad, pad, pad, pad);
        scroll.AddView(body);

        body.AddView(new TextView(ctx) { Text = "选择脚本", TextSize = 15 });

        _scripts = new Spinner(ctx);
        _scripts.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        _scripts.ItemSelected += (_, e) =>
        {
            var options = Vm?.QModOptions;
            if(options == null || e.Position < 0 || e.Position >= options.Count) return;
            if(ReferenceEquals(Vm!.QModSelected, options[e.Position])) return;   // 初始化时会回调一次
            Vm.QModSelected = options[e.Position];
            RefreshFromVm(ctx);
        };
        body.AddView(_scripts);

        _info = new TextView(ctx) { TextSize = 13 };
        _author = new TextView(ctx) { TextSize = 13 };
        body.AddView(_info);
        body.AddView(_author);

        _parameters = new LinearLayout(ctx) { Orientation = Orientation.Vertical };
        body.AddView(_parameters);

        var run = new Button(ctx) { Text = "运行" };
        run.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        run.Click += (_, _) => Vm?.QModCommand.Execute(null);
        body.AddView(run);

        Refresh();
        return scroll;
    }

    public override void OnResume()
    {
        base.OnResume();
        Refresh();
    }

    /// <summary>把 VM 当前的脚本列表和已选中的脚本摆出来。</summary>
    private void Refresh()
    {
        var ctx = Activity ?? Android.App.Application.Context;
        var vm = Vm;
        if(vm == null) return;

        var names = vm.QModOptions.Select(o => o.Name).ToArray();
        var adapter = new ArrayAdapter(ctx, Android.Resource.Layout.SimpleSpinnerItem, names);
        adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
        _scripts.Adapter = adapter;

        int current = vm.QModOptions.ToList().FindIndex(o => o.Name == vm.QMod);
        if(current >= 0) _scripts.SetSelection(current);

        RefreshFromVm(ctx);
    }

    /// <summary>
    /// 脚本参数、说明、作者都是"选中某个脚本之后"才由 VM 加载出来的，
    /// 所以选中回调里要三样一起刷，不能只重建参数行。
    /// </summary>
    private void RefreshFromVm(Context ctx)
    {
        var vm = Vm;
        if(vm == null) return;
        _info.Text = vm.InfoAll ?? string.Empty;
        _author.Text = vm.DisplayAuthor ?? string.Empty;
        RebuildParameters(ctx);
    }

    /// <summary>按当前脚本的参数表逐行重建：有候选项的出下拉，没有的出输入框。</summary>
    private void RebuildParameters(Context ctx)
    {
        _parameters.RemoveAllViews();
        var vm = Vm;
        if(vm == null || vm.Parameters.Count == 0) return;

        _parameters.AddView(new TextView(ctx) { Text = "脚本参数", TextSize = 15 });

        foreach(var param in vm.Parameters)
        {
            var row = new LinearLayout(ctx) { Orientation = Orientation.Horizontal };
            row.SetGravity(GravityFlags.CenterVertical);
            row.AddView(new TextView(ctx)
            {
                Text = param.DisplayDescription,
                TextSize = 13,
                LayoutParameters = new LinearLayout.LayoutParams(0,
                    ViewGroup.LayoutParams.WrapContent, 1f)
            });

            var options = param.Options?.ToArray() ?? Array.Empty<string>();
            if(options.Length > 0 && param.ControlType == "ComboBox")
            {
                var spinner = new Spinner(ctx);
                var adapter = new ArrayAdapter(ctx,
                    Android.Resource.Layout.SimpleSpinnerItem, options);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinner.Adapter = adapter;
                int at = Array.IndexOf(options, param.Value);
                if(at >= 0) spinner.SetSelection(at);
                spinner.ItemSelected += (_, e) =>
                {
                    if(e.Position >= 0) param.Value = options[e.Position];
                };
                row.AddView(spinner);
            }
            else
            {
                var edit = new EditText(ctx)
                {
                    Text = param.Value ?? string.Empty,
                    TextSize = 13,
                    LayoutParameters = new LinearLayout.LayoutParams(
                        (int)(120 * ctx.Resources!.DisplayMetrics!.Density + 0.5f),
                        ViewGroup.LayoutParams.WrapContent)
                };
                edit.AfterTextChanged += (_, _) => param.Value = edit.Text;
                row.AddView(edit);
            }

            _parameters.AddView(row);
        }
    }
}
