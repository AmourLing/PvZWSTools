using System;
using System.Windows;
using PvZWSTools_WPF.Views;

namespace PvZWSTools_WPF.Helpers
{
    public static class Lock
    {
        private const string EXPIRATION_DATE = "2026-07-05";

        private const string PASSWORD = "LING";

        public static bool VerifyPassword(string input)
        {
            return input == PASSWORD;
        }

        public static bool EnsureAccess()
        {
            if(!IsExpired())
            {
                TimeSpan remaining = GetExpirationDate() - DateTime.Now.Date;
                Log.Info($"程序有效期至 {EXPIRATION_DATE}，剩余 {remaining.Days} 天");
                return true;
            }

            Log.Info($"程序已过期（有效期至 {EXPIRATION_DATE}），需要密码验证");
            return VerifyPasswordWithRetry();
        }

        private static bool IsExpired()
        {
            DateTime today = DateTime.Now.Date;
            DateTime expiration = GetExpirationDate();
            return today >= expiration;
        }

        private static DateTime GetExpirationDate()
        {
            if(DateTime.TryParse(EXPIRATION_DATE, out DateTime expiration))
                return expiration;

            Log.Error("过期日期格式错误，请检查 Lock.EXPIRATION_DATE 常量");
            return DateTime.MaxValue;
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
}
