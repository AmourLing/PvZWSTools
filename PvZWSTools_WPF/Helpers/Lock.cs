using System.Windows;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_WPF.Views;

namespace PvZWSTools_WPF.Helpers;

public static class Lock
{
    private const int EXTRA_TIME = 14; //day

    private const string PASSWORD = "AMOURLING";

    public static bool EnsureAccess()
    {
        if(!IsExpired())
        {
            TimeSpan remaining = GetExpirationDate() - DateTime.Now.Date;
            Log.Info($"程序有效期至 {EXPIRATION_DATE()}，剩余 {remaining.Days} 天");
            return true;
        }

        Log.Info($"程序已过期（有效期至 {EXPIRATION_DATE()}），需要密码验证");
        Log.Info($"请尝试通过密码验证或通过" +
            $"{PvZWSTools_Shared.Sharedstring.BaseUpdateUrl}" +
            $"或{PvZWSTools_Shared.Sharedstring.BaseUpdateQQ}" +
            $"等途径获取新版本");
        Log.Info("具体途径可以参考文件目录下的readme.md文档");
        return VerifyPasswordWithRetry();
    }

    public static bool VerifyPassword(string input)
    {
        return input == PASSWORD;
    }

    private static string EXPIRATION_DATE()
    {
        DateTime? compileTime = CompileTime.GetCompileTime();
        if(compileTime.HasValue)
        {
            compileTime = compileTime.Value.AddDays(EXTRA_TIME);
            return compileTime.Value.ToString("yyyy-MM-dd");
        }
        return DateTime.MaxValue.ToString("yyyy-MM-dd");
    }

    private static DateTime GetExpirationDate()
    {
        if(DateTime.TryParse(EXPIRATION_DATE(), out DateTime expiration))
            return expiration;

        Log.Error("过期日期格式错误，请检查 Lock.EXPIRATION_DATE() 方法");
        return DateTime.MaxValue;
    }

    private static bool IsExpired()
    {
        DateTime today = DateTime.Now.Date;
        DateTime expiration = GetExpirationDate();
        return today >= expiration;
    }

    private static bool VerifyPasswordWithRetry()
    {
        const int maxRetries = 3;
        for(int attempt = 0;attempt < maxRetries;attempt++)
        {
            var dialog = new PasswordDialog();
            bool? result = dialog.ShowDialog();

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
