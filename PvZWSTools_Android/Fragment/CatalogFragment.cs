using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using PvZWSTools_Android.Platform;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.UiModel;
// 本文件在 PvZWSTools_Android 命名空间下，那里还有一份同名旧 NameOption，
// 不显式别名的话 Cast<NameOption>() 会绑到旧的那份并抛 InvalidCastException。
using SharedNameOption = PvZWSTools_Shared.Models.NameOption;

using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_Android;

/// <summary>
/// 清单驱动的一页功能列表：从 AppServices 拿到那一页的 UnitDescriptor，按 UnitKind
/// 画成一行 —— 开关点一下就执行，带参数的功能弹一个参数对话框。
/// 取代原先"每页一个 Fragment + 一份 layout + 一叠 strings.xml 键名"的写法。
/// 收藏页（<see cref="FavoritesFragment"/>）复用同一套画行的代码，只换"这一页要画哪些行"。
/// </summary>
public class CatalogFragment:AndroidX.Fragment.App.Fragment
{
    private readonly string _pageTitle;

    public CatalogFragment(string pageTitle) => _pageTitle = pageTitle;

    /// <summary>这一页在共享清单里的页签名，切页时用它去要一次真实开关状态。</summary>
    public string PageTitle => _pageTitle;

    /// <summary>这一页要画的行，连同每行前面的分区标题（普通页没有分区）。
    /// 返回空则显示 <see cref="EmptyHint"/>。</summary>
    protected virtual IEnumerable<(string? Header, UnitDescriptor Unit)> CollectUnits(Context ctx)
    {
        var page = AppServices.Pages.FirstOrDefault(p => p.Title == _pageTitle);
        if(page == null) yield break;
        foreach(var unit in page.Units)
            yield return (null, unit);
    }

    protected virtual string EmptyHint => Loc.F("清单里没有「{0}」这一页", Loc.T(_pageTitle));

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var ctx = InContext()!;
        float density = ctx.Resources!.DisplayMetrics!.Density;
        int pad = (int)(12 * density + 0.5f);

        var scroll = new ScrollView(ctx);
        var list = new LinearLayout(ctx) { Orientation = Orientation.Vertical };
        list.SetPadding(pad, pad, pad, pad);
        scroll.AddView(list);

        string? lastHeader = null;
        int rows = 0;
        foreach(var (header, unit) in CollectUnits(ctx))
        {
            if(header != null && header != lastHeader)
            {
                list.AddView(BuildSectionHeader(ctx, header));
                lastHeader = header;
            }
            list.AddView(BuildRow(ctx, unit));
            rows++;
        }

        if(rows == 0)
            list.AddView(new TextView(ctx) { Text = EmptyHint });

        return scroll;
    }

    private static View BuildSectionHeader(Context ctx, string text)
    {
        int top = (int)(10 * ctx.Resources!.DisplayMetrics!.Density + 0.5f);
        return new TextView(ctx)
        {
            Text = Loc.T(text),
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {
                TopMargin = top
            }
        };
    }

    private Context? InContext() => Activity ?? Android.App.Application.Context;

    private View BuildRow(Context ctx, UnitDescriptor unit)
    {
        // 纯成员组没有主动作，它只是"这些是一回事"的容器；
        // 当成一个按钮画的话，点上去 Command 是 null，等于一个按不动的按钮。
        if(!unit.HasAction && unit.Kind is UnitKind.Group or UnitKind.Chips)
            return unit.Kind == UnitKind.Chips ? BuildChipsEntry(ctx, unit) : BuildMemberGroup(ctx, unit);

        var btn = new Button(ctx)
        {
            Text = Caption(unit),
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
        };

        btn.Click += (_, _) =>
        {
            // 走 RunCommand 而不是 Command：那一层顺带记一次常用，
            // 桌面端绑的也是它，两端点的都算进同一份统计。
            if(NeedsDialog(unit)) ShowParameterDialog(ctx, unit);
            else unit.RunCommand?.Execute(null);
        };

        // 手机上没有桌面端那颗常驻的星标，长按同一行来收藏/取消收藏。
        btn.LongClick += (_, _) =>
        {
            AppServices.Favorites.ToggleFavorite(unit);
        };

        // 状态被别处改动（状态管理、脚本回报）时，按钮文字要跟着刷新
        unit.PropertyChanged += (_, _) => btn.Post(() => btn.Text = Caption(unit));

        return btn;
    }

    /// <summary>成员组展开成各自的行；成员自己带名字，所以不加组标题。
    /// 多选块例外：普僵、旗子单看不知道是什么，块名得留着。</summary>
    private View BuildMemberGroup(Context ctx, UnitDescriptor unit)
    {
        var box = new LinearLayout(ctx) { Orientation = Orientation.Vertical };
        if(unit.Kind == UnitKind.Chips)
            box.AddView(new TextView(ctx) { Text = unit.Label });
        foreach(var member in unit.Members)
            box.AddView(BuildRow(ctx, member));
        return box;
    }

    /// <summary>多选块（40 个出怪类型）在手机上收成一行入口，点开才是网格。
    /// 直接铺开就是 40 个整宽按钮，一屏放不下，也看不出这块整体是什么状态。</summary>
    private View BuildChipsEntry(Context ctx, UnitDescriptor unit)
    {
        var btn = new Button(ctx)
        {
            Text = ChipsCaption(unit),
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
        };
        btn.Click += (_, _) => ShowChipsDialog(ctx, unit);
        unit.PropertyChanged += (_, _) => btn.Post(() => btn.Text = ChipsCaption(unit));
        return btn;
    }

    private static string ChipsCaption(UnitDescriptor unit) => Loc.F("{0}（{1}）", unit.Label, unit.SelectedSummary);

    /// <summary>三个一行的勾选网格。每个勾就是它自己那个开关，点完立刻生效，所以不需要"应用"收尾。
    /// 打开期间列表可能被别处改动（同步出怪列表的钩子随时会推一份过来），
    /// 所以订一块的 PropertyChanged 整体刷一遍；关窗必摘，不然每次打开都漏一个持着 View 的订阅。</summary>
    private void ShowChipsDialog(Context ctx, UnitDescriptor unit)
    {
        const int perRow = 3;
        var body = new LinearLayout(ctx) { Orientation = Orientation.Vertical };
        var boxes = new List<(UnitDescriptor Member, CheckBox Box)>();
        AlertDialog? dialog = null;
        // 回填勾的时候先立个标志：否则 CheckedChange 会把"程序刷状态"当成"用户点的"，脚本又发一遍
        bool refreshing = false;

        void Refresh()
        {
            refreshing = true;
            foreach(var (member, box) in boxes)
                box.Checked = member.IsOn;
            refreshing = false;
            if(dialog != null) dialog.SetTitle(ChipsCaption(unit));
        }

        PropertyChangedEventHandler onUnitChanged = (_, _) => Refresh();
        unit.PropertyChanged += onUnitChanged;

        for(int i = 0; i < unit.Members.Count; i += perRow)
        {
            var row = new LinearLayout(ctx) { Orientation = Orientation.Horizontal };
            foreach(var member in unit.Members.Skip(i).Take(perRow))
            {
                var box = new CheckBox(ctx)
                {
                    Text = member.Label,
                    Checked = member.IsOn,
                    LayoutParameters = new LinearLayout.LayoutParams(
                        0, ViewGroup.LayoutParams.WrapContent, 1f)
                };
                box.CheckedChange += (_, _) =>
                {
                    if(refreshing) return;
                    member.Command?.Execute(null);
                };
                boxes.Add((member, box));
                row.AddView(box);
            }

            // 最后一行不满三个时补占位，免得那几个勾被拉成别的宽度
            for(int pad = row.ChildCount; pad < perRow; pad++)
                row.AddView(new View(ctx)
                {
                    LayoutParameters = new LinearLayout.LayoutParams(
                        0, ViewGroup.LayoutParams.WrapContent, 1f)
                });

            body.AddView(row);
        }

        var scroll = new ScrollView(ctx);
        scroll.AddView(body);

        // SetCancelable(false) 不是省事：返回键/点外面也能关掉的话，就没有可靠的地方摘 PropertyChanged，
        // 每开一次漏一个持着 View 的订阅。应用内其它对话框也是这个做法。
        dialog = new AlertDialog.Builder(ctx)
            .SetTitle(ChipsCaption(unit))
            .SetView(scroll)
            .SetCancelable(false)
            .SetNegativeButton(Loc.T("关闭", "btn"), (_, _) => unit.PropertyChanged -= onUnitChanged)
            .Show();
    }

    /// <summary>开关把当前状态直接写进按钮文字，省掉一列没有意义的状态符。
    /// 已收藏的前面加一颗星——手机上星标只能靠长按切，不加标记就看不出哪些已经收过。</summary>
    private static string Caption(UnitDescriptor unit) => (unit.IsFavorite ? "★ " : "") + unit.Kind switch
    {
        UnitKind.Switch or UnitKind.TriState => Loc.F("{0}（{1}）", unit.Label, unit.DisplayState),
        UnitKind.Cycle => Loc.F("{0}：{1}", unit.Label, unit.DisplayValue),
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
            .SetPositiveButton(Loc.T("应用"), (_, _) => unit.RunCommand?.Execute(null))
            .SetNegativeButton(Loc.T("取消"), (_, _) => { })
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

    /// <summary>输入框永远在，有选项时旁边再挂一个下拉。
    /// 之前是"有选项就只给下拉"，于是列表里没有的值（新起的卡组名、自定义植物名）在手机上填不进去，
    /// 而桌面端一直是可编辑下拉。下拉选中后由 VM 把值写回输入属性，输入框跟着刷一次。</summary>
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

        var edit = new EditText(ctx)
        {
            Text = slot.Text ?? string.Empty,
            LayoutParameters = new LinearLayout.LayoutParams(
                0, ViewGroup.LayoutParams.WrapContent, 1f)
        };
        edit.AfterTextChanged += (_, _) => slot.Text = edit.Text;
        slot.PropertyChanged += (_, e) =>
        {
            string want = slot.Text ?? string.Empty;
            if(e.PropertyName == nameof(UnitField.Text) && edit.Text != want)
                edit.Text = want;
        };
        row.AddView(edit);

        var options = slot.OptionList?.Cast<SharedNameOption>().ToArray() ?? Array.Empty<SharedNameOption>();
        if(options.Length == 0) return row;

        var adapter = new ArrayAdapter(ctx,
            Android.Resource.Layout.SimpleSpinnerItem,
            options.Select(o => o.DisplayName).ToArray());
        adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

        var spinner = new Spinner(ctx) { Adapter = adapter };
        int current = Array.FindIndex(options, o => ReferenceEquals(o, slot.SelectedItem));
        if(current < 0)
            current = Array.FindIndex(options, o => o.Name == slot.Text);
        if(current >= 0) spinner.SetSelection(current);
        spinner.ItemSelected += (_, e) =>
        {
            if(e.Position >= 0) slot.SelectedItem = options[e.Position];
        };
        row.AddView(spinner);

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
