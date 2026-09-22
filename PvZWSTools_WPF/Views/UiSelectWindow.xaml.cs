using System.Windows;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_WPF.Themes;

namespace PvZWSTools_WPF.Views;

public partial class UiSelectWindow:Window
{
    public UiSelectWindow()
    {
        InitializeComponent();
        // 经典 UI 下主题资源不存在，显式用系统标准色，避免黑底黑字
        UiThemeManager.RefreshWindowChrome(this);
        Loaded += (_, _) =>
        {
            RbClassic.IsChecked = !UiThemeManager.UseNewUi;
            RbNewUi.IsChecked = UiThemeManager.UseNewUi;
            RbDark.IsChecked = UiThemeManager.IsDark;
            RbLight.IsChecked = !UiThemeManager.IsDark;
            RbEn.IsChecked = UiThemeManager.Language == Loc.En;
            RbZh.IsChecked = UiThemeManager.Language != Loc.En;
        };
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        // 防御式读取：以界面上实际勾选的项为准（避免单选组互斥异常导致误存）
        bool useNewUi = RbNewUi.IsChecked == true || RbClassic.IsChecked != true;
        bool isDark = RbDark.IsChecked == true || RbLight.IsChecked != true;
        string language = RbEn.IsChecked == true ? Loc.En : Loc.Zh;
        UiThemeManager.SaveAndRestart(useNewUi, isDark, language);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
