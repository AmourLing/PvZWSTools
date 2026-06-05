// LogHelper.cs
using System;
using System.IO;
using System.Text;

namespace PvZWSTools_Xamarin
{
    public static class LogHelper
    {
        private static string _logFilePath;
        private static readonly object _lock = new object();

        public static void Initialize(string basePath)
        {
            try
            {
                _logFilePath = Path.Combine(basePath, "log.txt");
                Log("===== 应用启动 =====");
                Log($"日志文件: {_logFilePath}");
                Log($"时间: {DateTime.Now}");
            }
            catch(Exception ex)
            {
                // 如果无法初始化日志，至少输出到Android日志
                Android.Util.Log.Error("LogHelper", $"初始化日志失败: {ex.Message}");
            }
        }

        public static void Log(string message)
        {
            try
            {
                // 也输出到Android日志
                Android.Util.Log.Info("PvZWSTools", message);

                // 写入文件
                lock(_lock)
                {
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logFilePath, logEntry, Encoding.UTF8);
                }
            }
            catch(Exception ex)
            {
                Android.Util.Log.Error("LogHelper", $"写入日志失败: {ex.Message}");
            }
        }

        public static void LogError(string message, Exception ex = null)
        {
            var errorMessage = $"错误: {message}";
            if(ex != null)
            {
                errorMessage += $"\n异常: {ex.Message}\n堆栈: {ex.StackTrace}";
            }
            Log(errorMessage);
        }

        public static void ClearLog()
        {
            try
            {
                if(File.Exists(_logFilePath))
                {
                    File.WriteAllText(_logFilePath, string.Empty);
                    Log("===== 日志已清空 =====");
                }
            }
            catch(Exception ex)
            {
                Android.Util.Log.Error("LogHelper", $"清空日志失败: {ex.Message}");
            }
        }
    }
}
