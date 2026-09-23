using System.Windows;
using System.Windows.Input;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_WPF.Views;

public partial class PasswordDialog:Window
{
    public bool IsPasswordCorrect { get; private set; }

    /// <summary>用户点击了"检查更新"按钮——允许启动但仅能更新。</summary>
    public bool IsCheckUpdateRequested { get; private set; }

    private int _attemptCount;

    public PasswordDialog()
    {
        InitializeComponent();
        _ = PasswordBox.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        _attemptCount++;
        string password = PasswordBox.Password;

        if(BetaLock.VerifyPassword(password))
        {
            IsPasswordCorrect = true;
            DialogResult = true;
            Close();
        }
        else
        {
            if(_attemptCount >= BetaLock.MaxAttempts)
            {
                ErrorText.Text = "密码错误已达3次，请点[检查更新]获取新版本";
                DialogResult = false;
                Close();
            }
            else
            {
                ErrorText.Text = $"密码错误，还剩 {BetaLock.MaxAttempts - _attemptCount} 次机会";
                PasswordBox.Clear();
                _ = PasswordBox.Focus();
            }
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsPasswordCorrect = false;
        DialogResult = false;
        Close();
    }

    private void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        IsCheckUpdateRequested = true;
        DialogResult = false;
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if(e.Key == Key.Enter)
        {
            OkButton_Click(this, new RoutedEventArgs());
        }
        base.OnKeyDown(e);
    }
}
