using System;
using System.IO;
using System.Windows;

namespace PvZWSTools_WPF.Helpers
{
    public static class Log
    {
        private const ConsoleColor defaultConsoleColor = ConsoleColor.Gray;
        private static readonly object LockObj = new object();
        private static readonly string LogDirectory;
        private static readonly string LogFilePath;
        private static bool _initialized = false;
        private static StreamWriter _writer;

        static Log()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            LogDirectory = Path.Combine(baseDir, "Log");
            if(!Directory.Exists(LogDirectory))
                _ = Directory.CreateDirectory(LogDirectory);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            LogFilePath = Path.Combine(LogDirectory, $"log_{timestamp}.txt");

            try
            {
                _writer = new StreamWriter(LogFilePath, append: true) { AutoFlush = true };
                _initialized = true;

                if(Application.Current != null)
                {
                    Application.Current.Exit += (s, e) => Shutdown();
                }
            }
            catch(Exception ex)
            {
                Error($"无法初始化日志文件: {ex.Message}");
            }
        }

        public static void Debug(string message, bool IsWriter = true, ConsoleColor targetColor = defaultConsoleColor)
        {
            Write("DEBUG", message, IsWriter, targetColor);
        }

        public static void Error(string message, bool IsWriter = true, ConsoleColor targetColor = defaultConsoleColor)
            => Write("ERROR", message, IsWriter, targetColor);

        public static void Error(string message, Exception ex, bool IsWriter = true, ConsoleColor targetColor = defaultConsoleColor)
            => Write("ERROR", $"{message} - {ex}", IsWriter, targetColor);

        public static void Info(string message, bool IsWriter = true, ConsoleColor targetColor = defaultConsoleColor)
            => Write("INFO", message, IsWriter, targetColor);

        public static void Raw(string message, ConsoleColor targetColor = ConsoleColor.Gray, bool writeToFile = true)
        {
            if(!_initialized)
            {
                return;
            }

            lock(LockObj)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                try
                {
                    Console.ForegroundColor = targetColor;
                    Console.WriteLine(message);
                }
                finally
                {
                    Console.ForegroundColor = originalColor;
                }

                if(writeToFile && _writer != null)
                {
                    try
                    {
                        _writer.WriteLine(message);
                    }
                    catch(Exception ex)
                    {
                        try
                        {
                            Console.WriteLine($"日志文件写入失败: {ex.Message}");
                        }
                        catch { }
                    }
                }
            }
        }

        public static void Shutdown()
        {
            lock(LockObj)
            {
                if(_writer != null)
                {
                    try
                    {
                        _writer.Flush();
                        _writer.Close();
                        _writer.Dispose();
                    }
                    catch(Exception ex)
                    {
                        Error($"关闭日志文件失败: {ex.Message}");
                    }
                    finally
                    {
                        _writer = null;
                        _initialized = false;
                    }
                }
            }
        }

        public static void Warning(string message, bool IsWriter = true, ConsoleColor targetColor = ConsoleColor.Gray)
            => Write("WARN", message, IsWriter, targetColor);

        private static void Write(string level,
            string message,
            bool IsWriter = true,
            ConsoleColor targetColor = ConsoleColor.Gray)
        {
            if(!_initialized) return;

            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logLine = $"[{timeStamp} {level}] {message}";

            lock(LockObj)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                try
                {
                    switch(level)
                    {
                        case "ERROR":
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;

                        case "WARN":
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;

                        case "INFO":
                        case "DEBUG":
                        default:
                            break;
                    }
                    if(targetColor != originalColor)
                    {
                        Console.ForegroundColor = targetColor;
                    }
                    Console.WriteLine(logLine);
                }
                catch
                {
                    try { Console.WriteLine(logLine); } catch { }
                }
                finally
                {
                    try { Console.ForegroundColor = originalColor; } catch { }
                }
                if(IsWriter)
                {
                    try
                    {
                        _writer?.WriteLine(logLine);
                    }
                    catch(Exception ex)
                    {
                        Error($"log日志写入失败{ex}");
                    }
                }
            }
        }
    }
}
