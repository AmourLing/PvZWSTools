using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PvZWSTools_Shared.UiModel;
using PvZWSTools_Shared.ViewModels;

namespace PvZWSTools_WPF.Views;

/// <summary>花园编辑页：整张背景图 + 盖在上面的格位热区。
/// 画布尺寸与格位坐标都来自 GardenLayout（照抄游戏源码的格位表），不再按图目测摆位；
/// 页面本身由外壳按调试模式决定是否出现在导航里。</summary>
public partial class GardenPanel:UserControl
{
    /// <summary>卡片自身的下内边距（主题里 PillTabControlStyle 那个 Border 的 Padding=8），
    /// 算可用高度时要把它让出来，否则又会多出纵向滚动条。</summary>
    private const double CardBottomGap = 8;

    private ScrollViewer? _page;
    private ScrollViewer? _pan;
    private GardenTab? _tab;
    private bool _pageHooked;

    public GardenPanel()
    {
        InitializeComponent();

        // 切到这一页就自己读一次：不读的话满屏都是空格号，看不出游戏里现在种着什么
        Loaded += (_, _) => (DataContext as GardenViewModel)?.ReadGardenOnOpen();
    }

    private void Pan_Loaded(object sender, RoutedEventArgs e)
    {
        if(sender is not ScrollViewer pan || pan.DataContext is not GardenTab tab) return;

        _pan = pan;
        _tab = tab;
        _page ??= FindAncestor<ScrollViewer>(this);
        if(_page != null && !_pageHooked)
        {
            _pageHooked = true;
            _page.SizeChanged += (_, _) => FitAndCenter();
        }

        FitAndCenter();
    }

    /// <summary>图片区的高度 = 外壳滚动区的可见高度 - 它上面那些东西（工具条、页签条、卡片内边距）。
    /// 这条链上从外壳到卡片一路都是"纵向可滚"的 ScrollViewer，会给孩子无限高度；
    /// 不显式定高的话整页被撑出纵向滚动条，换页签还得先滚回顶部。</summary>
    private void FitAndCenter()
    {
        if(_pan == null || _page == null || _tab == null) return;

        var above = _pan.TransformToVisual(_page).Transform(new Point(0, 0)).Y;
        var height = _page.ViewportHeight - above - CardBottomGap;
        if(height < 160) return; // 窗口太矮，宁可让它按原来的方式滚

        _pan.Height = height;
        _pan.UpdateLayout();

        // Viewbox 缩放之后 ExtentWidth 才是缩放后的宽度，格子中心要按同一个比例换算
        var scale = _pan.ExtentWidth / _tab.CanvasWidth;
        var offset = _tab.ContentCenterX * scale - _pan.ViewportWidth / 2;
        _pan.ScrollToHorizontalOffset(Math.Clamp(offset, 0, Math.Max(0, _pan.ScrollableWidth)));
    }

    private void Pan_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if(sender is not ScrollViewer pan || pan.ScrollableWidth <= 0) return;

        // 横向有得滚就让滚轮横着走；滚不动了（窗口够宽）就交还给页面做纵向滚动
        pan.ScrollToHorizontalOffset(pan.HorizontalOffset - e.Delta);
        e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        for(var parent = VisualTreeHelper.GetParent(current); parent != null; parent = VisualTreeHelper.GetParent(parent))
        {
            if(parent is T match) return match;
        }

        return null;
    }
}
