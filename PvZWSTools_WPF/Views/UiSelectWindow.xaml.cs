using System.Windows;
using PvZWSTools_WPF.Themes;

namespace PvZWSTools_WPF.Views;

public partial class UiSelectWindow:Window
{
    public UiSelectWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            RbClassic.IsChecked = !UiThemeManager.UseNewUi;
            RbNewUi.IsChecked = UiThemeManager.UseNewUi;
            RbDark.IsChecked = UiThemeManager.IsDark;
            RbLight.IsChecked = !UiThemeManager.IsDark;
        };
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        // 防御式读取：以界面上实际勾选的项为准（避免单选组互斥异常导致误存）
        bool useNewUi = RbNewUi.IsChecked == true || RbClassic.IsChecked != true;
        bool isDark = RbDark.IsChecked == true || RbLight.IsChecked != true;
        UiThemeManager.SaveAndRestart(useNewUi, isDark);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
