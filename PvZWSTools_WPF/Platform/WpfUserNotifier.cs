using System.Windows;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_WPF.Platform;

/// <summary>
/// WPF 实现的用户通知（基于 MessageBox）。
/// </summary>
public class WpfUserNotifier:IUserNotifier
{
    public void Warn(string title, string message)
    {
        _ = MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void Error(string title, string message)
    {
        _ = MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool Confirm(string title, string message)
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
