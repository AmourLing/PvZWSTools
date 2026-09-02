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

    /// <summary>
    /// 询问用户"是/否"的确认对话框。
    /// 返回 true 表示用户点击"是/确定"。
    /// </summary>
    bool Confirm(string title, string message);
}
