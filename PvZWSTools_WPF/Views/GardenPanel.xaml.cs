using System.Windows.Controls;

namespace PvZWSTools_WPF.Views;

/// <summary>花园草坪热区。格位按背景图的实际像素摆，只能绝对定位；
/// 页面本身由外壳按调试模式决定是否出现在导航里。</summary>
public partial class GardenPanel:UserControl
{
    public GardenPanel()
    {
        InitializeComponent();
    }
}
