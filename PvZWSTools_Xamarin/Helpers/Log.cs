using System;
using System.IO;
using System.Text;
using Android.Util;
using Android.App;

namespace PvZWSTools_Xamarin.Helpers;

public static class Log
{
    private static readonly object _lock = new object();
    private static StreamWriter _writer;
    private static bool _initialized = false;

    private static string _logDirectory;
    private static string _currentLogFilePath;

    private static int _writeCount = 0;
    private const int FLUSH_INTERVAL = 10;

    /// <summary>
    /// 初始化日志系统，使用默认的应用内部存储路径
    /// </summary>
    public static void Initialize()
    {
        try
        {
            string basePath = Application.Context.FilesDir.AbsolutePath;
            Initialize(basePath);
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("LogHelper", $"获取默认路径失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 初始化日志系统
    /// </summary>
    /// <param name="basePath">基础路径</param>
    public static void Initialize(string basePath)
    {
        if(_initialized) return;

        lock(_lock)
        {
            if(_initialized) return;

            try
            {
                _logDirectory = Path.Combine(basePath, "Logs");

                if(!Directory.Exists(_logDirectory))
                {
                    _ = Directory.CreateDirectory(_logDirectory);
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                _currentLogFilePath = Path.Combine(_logDirectory, $"log_{timestamp}.txt");
                var fileStream = new FileStream(_currentLogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(fileStream, Encoding.UTF8)
                {
                    AutoFlush = false
                };

                _initialized = true;
                _writeCount = 0;

                Info("===== 应用启动 =====");
                Info($"日志文件: {_currentLogFilePath}");
            }
            catch(Exception ex)
            {
                _ = Android.Util.Log.Error("LogHelper", $"初始化日志失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 关闭日志流，释放资源
    /// </summary>
    public static void Shutdown()
    {
        lock(_lock)
        {
            if(!_initialized || _writer == null) return;

            try
            {
                _writer.Flush();
                _writer.Close();
                _writer.Dispose();
                _writer = null;
                _initialized = false;
            }
            catch(Exception ex)
            {
                _ = Android.Util.Log.Error("LogHelper", $"关闭日志文件失败: {ex.Message}");
            }
        }
    }

    public static void Debug(string message)
    {
        WriteLog("DEBUG", message, LogPriority.Debug);
    }

    public static void Info(string message)
    {
        WriteLog("INFO", message, LogPriority.Info);
    }

    public static void Warning(string message)
    {
        WriteLog("WARN", message, LogPriority.Warn);
    }

    public static void Error(string message, Exception ex = null)
    {
        string errorMessage = message;
        if(ex != null)
        {
            errorMessage += $"\n异常: {ex.Message}\n堆栈: {ex.StackTrace}";
        }
        WriteLog("ERROR", errorMessage, LogPriority.Error);
    }

    public static void Raw(string message)
    {
        if(!_initialized) return;

        lock(_lock)
        {
            if(_writer == null) return;

            try
            {
                // 输出到 Android Logcat
                _ = Android.Util.Log.Info("PvZWSTools_Raw", message);

                // 写入文件
                _writer.WriteLine(message);
                CheckAndFlush();
            }
            catch(Exception ex)
            {
                // 静默失败，避免递归
                _ = Android.Util.Log.Error("LogHelper", $"写入原始日志失败: {ex.Message}");
            }
        }
    }

    public static void ClearLog()
    {
        lock(_lock)
        {
            if(!_initialized || _writer == null) return;

            try
            {
                // 先关闭当前流
                _writer.Flush();
                _writer.Close();
                _writer.Dispose();
                _writer = null;

                // 清空文件内容
                if(File.Exists(_currentLogFilePath))
                {
                    File.WriteAllText(_currentLogFilePath, string.Empty);
                }

                // 重新打开流
                var fileStream = new FileStream(_currentLogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(fileStream, Encoding.UTF8)
                {
                    AutoFlush = false
                };

                _writeCount = 0;
                Info("===== 日志已清空 =====");
            }
            catch(Exception ex)
            {
                _ = Android.Util.Log.Error("LogHelper", $"清空日志失败: {ex.Message}");
            }
        }
    }


    private enum LogPriority
    {
        Debug,
        Info,
        Warn,
        Error
    }

    private static void WriteLog(string level, string message, LogPriority priority)
    {
        if(!_initialized) return;

        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logLine = $"[{timeStamp} {level}] {message}";

        lock(_lock)
        {
            if(_writer == null) return;

            try
            {
                // 输出到 Android Logcat
                switch(priority)
                {
                    case LogPriority.Debug:
                        _ = Android.Util.Log.Debug("PvZWSTools", message);
                        break;
                    case LogPriority.Info:
                        _ = Android.Util.Log.Info("PvZWSTools", message);
                        break;
                    case LogPriority.Warn:
                        _ = Android.Util.Log.Warn("PvZWSTools", message);
                        break;
                    case LogPriority.Error:
                        _ = Android.Util.Log.Error("PvZWSTools", message);
                        break;
                }
                _writer.WriteLine(logLine);
                CheckAndFlush();
            }
            catch(Exception ex)
            {
                try
                {
                    _ = Android.Util.Log.Error("LogHelper", $"写入日志失败: {ex.Message}");
                }
                catch { }
            }
        }
    }

    private static void CheckAndFlush()
    {
        _writeCount++;
        if(_writeCount >= FLUSH_INTERVAL)
        {
            try
            {
                _writer.Flush();
                _writeCount = 0;
            }
            catch { }
        }
    }
}
