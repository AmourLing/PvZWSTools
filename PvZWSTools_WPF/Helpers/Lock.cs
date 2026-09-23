using System.Windows;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_WPF.Views;

namespace PvZWSTools_WPF.Helpers;

/// <summary>beta 包的准入：规则和安卓共用共享层的 <see cref="BetaLock"/>（有效期、密码），
/// 这个类只管 WPF 这边的弹框。</summary>
public static class Lock
{
    /// <summary>
    /// 返回值：
    ///   true  — 有效期内，或密码验证成功 → 完整功能
    ///   false — 密码失败 + 取消 → 退出
    ///   null  — 密码失败 + 用户选了"检查更新" → 仅更新模式
    /// </summary>
    public static bool? EnsureAccess()
    {
        if(!BetaLock.IsExpired())
        {
            Log.Info($"程序有效期至 {BetaLock.ExpirationDateText()}，剩余 {BetaLock.RemainingDays()} 天");
            return true;
        }

        Log.Info($"程序已过期（有效期至 {BetaLock.ExpirationDateText()}），需要密码验证");
        Log.Info($"请尝试通过密码验证或通过" +
            $"{PvZWSTools_Shared.Helpers.Sharedstring.BaseUpdateUrl}" +
            $"或{PvZWSTools_Shared.Helpers.Sharedstring.BaseUpdateQQ}" +
            $"等途径获取新版本");
        Log.Info("具体途径可以参考文件目录下的readme.md文档");

        return VerifyPasswordWithRetry();
    }

    private static bool? VerifyPasswordWithRetry()
    {
        const int maxRetries = BetaLock.MaxAttempts;
        for(int attempt = 0;attempt < maxRetries;attempt++)
        {
            var dialog = new PasswordDialog();
            bool? result = dialog.ShowDialog();

            if(dialog.IsCheckUpdateRequested)
            {
                // 用户选了"检查更新"——允许启动但仅更新模式
                Log.Info("用户选择检查更新，进入仅更新模式");
                return null;
            }

            if(result == true && dialog.IsPasswordCorrect)
            {
                Log.Info("密码验证成功，继续启动程序");
                return true;
            }

            int remainingAttempts = maxRetries - (attempt + 1);
            if(remainingAttempts > 0)
            {
                _ = MessageBox.Show($"密码错误，还剩 {remainingAttempts} 次尝试", "密码错误",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        Log.Info("密码验证失败次数过多，程序退出");
        _ = MessageBox.Show("密码验证失败，程序将退出。", "验证失败",
            MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }
}
