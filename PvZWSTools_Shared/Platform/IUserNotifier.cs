namespace PvZWSTools_Shared.Helpers;

/// <summary>
/// 平台无关的用户通知抽象（替代 WPF 的 MessageBox）。
/// WPF 实现基于 MessageBox，Avalonia 实现基于对话框窗口。
/// </summary>
public interface IUserNotifier
{
    /// <summary>警告提示。</summary>
    void Warn(string title, string message);

    /// <summary>错误提示。</summary>
    void Error(string title, string message);
}
