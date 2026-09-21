using System.Windows;
using System.Windows.Controls;
using PvZWSTools_Shared.UiModel;
using PvZWSTools_WPF.ViewModels;

namespace PvZWSTools_WPF.Controls;

/// <summary>按单元类型挑卡片模板。模板在 XAML 里按属性注入。</summary>
public sealed class UnitKindSelector : DataTemplateSelector
{
    public DataTemplate? Switch { get; set; }
    public DataTemplate? TriState { get; set; }
    public DataTemplate? Field { get; set; }
    public DataTemplate? Picker { get; set; }
    public DataTemplate? Action { get; set; }
    public DataTemplate? Composite { get; set; }
    public DataTemplate? Group { get; set; }
    public DataTemplate? Cycle { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is not UnitDescriptor u)
            return base.SelectTemplate(item, container);
        return u.Kind switch
        {
            UnitKind.Switch => Switch,
            UnitKind.TriState => TriState,
            UnitKind.Field => Field,
            UnitKind.Picker => Picker,
            UnitKind.Composite => Composite,
            UnitKind.Group => Group,
            UnitKind.Cycle => Cycle,
            _ => Action,
        };
    }
}

/// <summary>内容区按当前视图形态挑模板：搜索结果优先，其次导航页的 Kind。</summary>
public sealed class ActiveViewSelector : DataTemplateSelector
{
    public DataTemplate? Page { get; set; }
    public DataTemplate? Script { get; set; }
    public DataTemplate? Garden { get; set; }
    public DataTemplate? Results { get; set; }
    public DataTemplate? Favorites { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            ShellViewModel.SearchResults => Results,
            NavItem { Kind: NavKind.Script } => Script,
            NavItem { Kind: NavKind.Garden } => Garden,
            NavItem { Kind: NavKind.Favorites } => Favorites,
            NavItem => Page,
            _ => base.SelectTemplate(item, container),
        };
    }
}
