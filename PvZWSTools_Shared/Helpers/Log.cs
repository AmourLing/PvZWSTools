using System.IO;
using System.Text;

namespace PvZWSTools_Shared.Helpers;

/// <summary>一行标准输出连同它当时的控制台颜色。界面内控制台照这个上色，
/// 才能和真控制台看起来是同一份东西。</summary>
public readonly record struct LogLine(string Text, ConsoleColor Color);

public static class Log
{
    private const ConsoleColor defaultConsoleColor = ConsoleColor.Gray;
    private static readonly object LockObj = new object();
    private static readonly string LogDirectory;
    private static readonly string LogFilePath;
    private static bool _initialized = false;
    private static StreamWriter _writer;

    /// <summary>建 ViewModel 之前那几行启动日志也要能在界面上看到，所以留一份最近的。</summary>
    private const int BacklogLimit = 600;

    private static readonly List<LogLine> _backlog = new();
    private static readonly object _sinkLock = new();
    private static string _partial = string.Empty;

    /// <summary>界面内控制台的取数口。收的是标准输出，不是 <see cref="Log"/> 的调用：
    /// websocket-sharp 的连接失败是它自己写控制台的，只钩 Log 就会漏掉，界面上看到的和真控制台不一致。
    /// 颜色是写那一行时控制台的当前色，所以界面里能还原出日志级别的红/黄。</summary>
    public static event Action<LogLine>? LineWritten;

    /// <summary>已经发过的那批行，供后来接上的界面一次性补齐。</summary>
    public static IReadOnlyList<LogLine> RecentLines
    {
        get
        {
            lock(_sinkLock)
                return _backlog.ToArray();
        }
    }

    static Log()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        LogDirectory = Path.Combine(baseDir, Constants.Folder_Need, Constants.Folder_Log);
        if(!Directory.Exists(LogDirectory))
            _ = Directory.CreateDirectory(LogDirectory);

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        LogFilePath = Path.Combine(LogDirectory, $"log_{timestamp}.txt");

        // 静态构造就是最早的时机（第一行日志之前），装在这里能连启动阶段的输出一起收到。
        try
        {
            Console.SetOut(new ConsoleTee(Console.Out));
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"控制台接管失败，界面内控制台只能收到之后的输出：{ex.Message}");
        }

        try
        {
            _writer = new StreamWriter(LogFilePath, append: true) { AutoFlush = true };
            _initialized = true;
        }
        catch(Exception ex)
        {
            Error($"无法初始化日志文件: {ex.Message}");
        }
    }

    /// <summary>写这一行时控制台是什么颜色，界面里就照那个颜色画。
    /// Android 不支持改控制台颜色，所以那边只能靠 <see cref="_pendingColor"/> 显式带过来。</summary>
    private static ConsoleColor CurrentColor()
    {
#if ANDROID
        return defaultConsoleColor;
#else
        try
        {
            return Console.ForegroundColor;
        }
        catch
        {
            return defaultConsoleColor;
        }
#endif
    }

    /// <summary>本行打算用什么颜色。tee 在同一个线程上被回调，所以线程局部就够。</summary>
    [ThreadStatic] private static ConsoleColor? _pendingColor;

    private static void ConsoleLine(string line, ConsoleColor color)
    {
        _pendingColor = color;
        try
        {
            Console.WriteLine(line);
        }
        finally
        {
            _pendingColor = null;
        }
    }

    private static void Publish(string? text, ConsoleColor color)
    {
        if(string.IsNullOrEmpty(text)) return;

        List<LogLine>? complete = null;
        lock(_sinkLock)
        {
            _partial += text;
            int nl;
            while((nl = _partial.IndexOf('\n')) >= 0)
            {
                string line = _partial[..nl].TrimEnd('\r');
                _partial = _partial[(nl + 1)..];
                if(line.Length == 0) continue;

                complete ??= new List<LogLine>();
                complete.Add(new LogLine(line, color));
                _backlog.Add(new LogLine(line, color));
            }

            int overflow = _backlog.Count - BacklogLimit;
            if(overflow > 0)
                _backlog.RemoveRange(0, overflow);
        }

        if(complete == null) return;
        foreach(var line in complete)
            LineWritten?.Invoke(line);
    }

    /// <summary>原样转发到真控制台，同时把内容抄一份给界面内控制台。</summary>
    private sealed class ConsoleTee:TextWriter
    {
        private readonly TextWriter _inner;

        public ConsoleTee(TextWriter inner) => _inner = inner;

        public override Encoding Encoding => _inner.Encoding;

        public override void Write(char value)
        {
            _inner.Write(value);
            Publish(value.ToString(), Color());
        }

        public override void Write(string? value)
        {
            _inner.Write(value ?? string.Empty);
            Publish(value, Color());
        }

        public override void WriteLine(string? value)
        {
            ConsoleColor color = Color();
            _inner.WriteLine(value);
            Publish(value, color);
            Publish(NewLine, color);   // 只用来收口，让没带换行的那次写也能成行
        }

        /// <summary>Log 自己会带颜色（Android 上只能这么带）；别人写 stdout 就照控制台的当前色。</summary>
        private static ConsoleColor Color() => _pendingColor ?? CurrentColor();

        public override void Flush() => _inner.Flush();

        protected override void Dispose(bool disposing)
        {
            if(disposing) _inner.Dispose();
            base.Dispose(disposing);
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
#if ANDROID
            // Android 改不了控制台颜色，所以颜色由 ConsoleLine 直接带给界面内控制台
            ConsoleLine(message, targetColor);
#else
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = targetColor;
                ConsoleLine(message, targetColor);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
#endif

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

        // 级别色打底；调用方显式给了颜色就用它给的。
        ConsoleColor effective = targetColor != defaultConsoleColor
            ? targetColor
            : level switch
            {
                "ERROR" => ConsoleColor.Red,
                "WARN" => ConsoleColor.Yellow,
                _ => defaultConsoleColor,
            };

        lock(LockObj)
        {
#if ANDROID
            // Android 改不了控制台颜色，所以颜色由 ConsoleLine 直接带给界面内控制台
            ConsoleLine(logLine, effective);
#else
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                if(effective != originalColor)
                {
                    Console.ForegroundColor = effective;
                }
                ConsoleLine(logLine, effective);
            }
            catch
            {
                try { ConsoleLine(logLine, effective); } catch { }
            }
            finally
            {
                try { Console.ForegroundColor = originalColor; } catch { }
            }
#endif
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
