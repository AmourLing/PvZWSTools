using System.Windows;

namespace PvZWSTools_WPF.Views;

public partial class ExitConfirmDialog:Window
{
    /// <summary>用户勾选了"不再弹出退出提示"（仅 DialogResult==true 时有意义）。</summary>
    public bool DontAskAgain { get; private set; }

    public ExitConfirmDialog()
    {
        InitializeComponent();
        Themes.UiThemeManager.RefreshWindowChrome(this);
        // 默认焦点在"取消"：回车 / Esc 都留在程序里，误触回车不会直接退出
        _ = CancelButton.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DontAskAgain = DontAskCheckBox.IsChecked == true;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
