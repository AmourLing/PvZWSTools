using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using PvZWSTools_Avalonia.Platform;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.UiModel;
// 本文件在 PvZWSTools_Avalonia 命名空间下，那里还有一份同名旧 NameOption，
// 不显式别名的话 Cast<NameOption>() 会绑到旧的那份并抛 InvalidCastException。
using SharedNameOption = PvZWSTools_Shared.Models.NameOption;

namespace PvZWSTools_Avalonia;

/// <summary>
/// 清单驱动的一页功能列表：从 AppServices 拿到那一页的 UnitDescriptor，按 UnitKind
/// 画成一行 —— 开关点一下就执行，带参数的功能弹一个参数对话框。
/// 取代原先"每页一个 Fragment + 一份 layout + 一叠 strings.xml 键名"的写法。
/// </summary>
public class CatalogFragment:AndroidX.Fragment.App.Fragment
{
    private readonly string _pageTitle;

    public CatalogFragment(string pageTitle) => _pageTitle = pageTitle;

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var ctx = InContext()!;
        float density = ctx.Resources!.DisplayMetrics!.Density;
        int pad = (int)(12 * density + 0.5f);

        var scroll = new ScrollView(ctx);
        var list = new LinearLayout(ctx) { Orientation = Orientation.Vertical };
        list.SetPadding(pad, pad, pad, pad);
        scroll.AddView(list);

        var page = AppServices.Pages.FirstOrDefault(p => p.Title == _pageTitle);
        if(page == null)
        {
            list.AddView(new TextView(ctx) { Text = $"清单里没有「{_pageTitle}」这一页" });
            return scroll;
        }

        foreach(var unit in page.Units)
            list.AddView(BuildRow(ctx, unit));

        return scroll;
    }

    private Context? InContext() => Activity ?? Android.App.Application.Context;

    private View BuildRow(Context ctx, UnitDescriptor unit)
    {
        // 纯成员组没有主动作，它只是"这些是一回事"的容器；
        // 当成一个按钮画的话，点上去 Command 是 null，等于一个按不动的按钮。
        if(unit.Kind == UnitKind.Group && !unit.HasAction)
            return BuildMemberGroup(ctx, unit);

        var btn = new Button(ctx)
        {
            Text = Caption(unit),
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
        };

        btn.Click += (_, _) =>
        {
            if(NeedsDialog(unit)) ShowParameterDialog(ctx, unit);
            else unit.Command?.Execute(null);
        };

        // 状态被别处改动（状态管理、脚本回报）时，按钮文字要跟着刷新
        unit.PropertyChanged += (_, _) => btn.Post(() => btn.Text = Caption(unit));

        return btn;
    }

    /// <summary>成员组展开成各自的行；成员自己带名字，所以不加组标题。</summary>
    private View BuildMemberGroup(Context ctx, UnitDescriptor unit)
    {
        var box = new LinearLayout(ctx) { Orientation = Orientation.Vertical };
        foreach(var member in unit.Members)
            box.AddView(BuildRow(ctx, member));
        return box;
    }

    /// <summary>开关把当前状态直接写进按钮文字，省掉一列没有意义的状态符。</summary>
    private static string Caption(UnitDescriptor unit) => unit.Kind switch
    {
        UnitKind.Switch or UnitKind.TriState => $"{unit.Label}（{unit.DisplayState}）",
        UnitKind.Cycle => $"{unit.Label}：{unit.DisplayValue}",
        _ => unit.Label
    };

    private static bool NeedsDialog(UnitDescriptor unit) =>
        unit.Fields.Count > 0 || unit.Kind is UnitKind.Field or UnitKind.Picker or UnitKind.Composite;

    /// <summary>
    /// 参数对话框：每个输入一行（有选项的下拉、没选项的文本框），组的成员开关列在下面，
    /// 确定时先回写属性再执行动作。桌面端是"按钮和它的参数同处一行"，
    /// 手机宽度放不下，就改成弹层 —— 消费的输入还是同一批。
    /// </summary>
    private void ShowParameterDialog(Context ctx, UnitDescriptor unit)
    {
        int pad = (int)(16 * ctx.Resources!.DisplayMetrics!.Density + 0.5f);
        var body = new LinearLayout(ctx) { Orientation = Orientation.Vertical };
        body.SetPadding(pad, pad, pad, 0);

        foreach(var slot in InputSlots(unit))
            body.AddView(BuildInput(ctx, slot));

        if(unit.Members.Count > 0)
        {
            // 成员各自带名字，不再加"开关"这类小标题——桌面端也没给组起标题
            foreach(var member in unit.Members)
                body.AddView(BuildMemberToggle(ctx, member));
        }

        _ = new AlertDialog.Builder(ctx)
            .SetTitle(unit.Label)
            .SetView(body)
            .SetPositiveButton("应用", (_, _) => unit.Command?.Execute(null))
            .SetNegativeButton("取消", (_, _) => { })
            .Show();
    }

    /// <summary>
    /// Field/Picker 把参数放在单元自己身上，Composite/Group 放在 Fields 里。
    /// 前者这里现造一个 UnitField 顶上，路径和已绑定的单元一致。
    /// </summary>
    private static IEnumerable<UnitField> InputSlots(UnitDescriptor unit)
    {
        if(unit.Fields.Count > 0)
            return unit.Fields;

        if(unit.Kind is UnitKind.Field or UnitKind.Picker)
        {
            var solo = new UnitField
            {
                Label = unit.Label,
                InputPath = unit.InputPath,
                OptionsPath = unit.OptionsPath,
                SelectedPath = unit.SelectedPath
            };
            solo.Resolve(AppServices.Root);
            return new[] { solo };
        }

        return Array.Empty<UnitField>();
    }

    private static View BuildInput(Context ctx, UnitField slot)
    {
        int labelWidth = (int)(88 * ctx.Resources!.DisplayMetrics!.Density + 0.5f);
        var row = new LinearLayout(ctx) { Orientation = Orientation.Horizontal };
        row.SetGravity(GravityFlags.CenterVertical);
        row.AddView(new TextView(ctx)
        {
            Text = slot.Label,
            LayoutParameters = new LinearLayout.LayoutParams(
                labelWidth, ViewGroup.LayoutParams.WrapContent)
        });

        var options = slot.OptionList?.Cast<SharedNameOption>().ToArray() ?? Array.Empty<SharedNameOption>();
        if(options.Length > 0)
        {
            var selected = slot.SelectedItem;
            var adapter = new ArrayAdapter(ctx,
                Android.Resource.Layout.SimpleSpinnerItem,
                options.Select(o => o.DisplayName).ToArray());
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            var spinner = new Spinner(ctx) { Adapter = adapter };
            int current = Array.FindIndex(options, o => ReferenceEquals(o, selected));
            if(current >= 0) spinner.SetSelection(current);
            spinner.ItemSelected += (_, e) =>
            {
                if(e.Position >= 0) slot.SelectedItem = options[e.Position];
            };
            row.AddView(spinner);
        }
        else
        {
            var edit = new EditText(ctx)
            {
                Text = slot.Text ?? string.Empty,
                LayoutParameters = new LinearLayout.LayoutParams(
                    0, ViewGroup.LayoutParams.WrapContent, 1f)
            };
            edit.AfterTextChanged += (_, _) => slot.Text = edit.Text;
            row.AddView(edit);
        }

        return row;
    }

    /// <summary>
    /// 组内成员：开关画成勾选框；Cycle（罐子类型、状态这种）不是开/关，
    /// 画成勾选框会永远显示未勾选，所以按按钮画、点一下换下一个取值。
    /// </summary>
    private static View BuildMemberToggle(Context ctx, UnitDescriptor member)
    {
        if(member.Kind == UnitKind.Cycle)
        {
            var btn = new Button(ctx) { Text = Caption(member) };
            btn.Click += (_, _) =>
            {
                member.Command?.Execute(null);
                btn.Text = Caption(member);
            };
            return btn;
        }

        var box = new CheckBox(ctx)
        {
            Text = member.Label,
            Checked = member.IsOn
        };
        box.CheckedChange += (_, _) => member.Command?.Execute(null);
        member.PropertyChanged += (_, _) => box.Post(() => box.Checked = member.IsOn);
        return box;
    }
}
