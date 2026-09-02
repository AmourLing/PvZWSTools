using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_WPF.Services;

/// <summary>
/// WPF 端自动更新实现：
/// 下载 zip → 解压到临时目录 → 生成 .bat 等待主进程退出 → 替换 exe 与同目录文件 → 重启。
/// </summary>
public class WpfUpdateService : UpdateService
{
    /// <inheritdoc />
    public override Version CurrentVersion
    {
        get
        {
            try
            {
                var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var name = asm.GetName();
                // 优先用 InformationalVersion 的纯版本号部分（去掉 prerelease 标签）
                var infoAttr = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if(infoAttr != null && Version.TryParse(GetNumericPart(infoAttr.InformationalVersion), out var v))
                    return v;
                return name.Version ?? new Version(0, 0, 0);
            }
            catch(Exception ex)
            {
                Log.Error($"读取当前版本失败: {ex.Message}");
                return new Version(0, 0, 0);
            }
        }
    }

    /// <inheritdoc />
    public override async Task<bool> ApplyUpdateAsync(string downloadedFilePath)
    {
        if(!File.Exists(downloadedFilePath))
        {
            Log.Error($"应用更新失败：文件不存在 {downloadedFilePath}");
            return false;
        }

        try
        {
            string baseDir = AppContext.BaseDirectory;
            string extractDir = Path.Combine(Path.GetTempPath(), $"pvzwstools_update_{Guid.NewGuid():N}");
            _ = Directory.CreateDirectory(extractDir);

            Log.Info($"解压更新包到: {extractDir}");
            await Task.Run(() => ZipFile.ExtractToDirectory(downloadedFilePath, extractDir, overwriteFiles: true));

            // 期望 zip 内至少包含 PvZWSTools.exe（或与 AssemblyName 同名 .exe）
            string exeName = (Assembly.GetEntryAssembly()?.GetName().Name ?? "PvZWSTools") + ".exe";
            string extractedExe = Path.Combine(extractDir, exeName);
            if(!File.Exists(extractedExe))
            {
                // 兜底：扫描 zip 根目录的任意 .exe
                var alt = Directory.GetFiles(extractDir, "*.exe", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
                if(alt == null)
                {
                    Log.Error($"更新包中未找到可执行文件 {exeName}");
                    return false;
                }
                extractedExe = alt;
            }

            string currentExe = Process.GetCurrentProcess().MainModule?.FileName
                ?? Path.Combine(baseDir, exeName);

            // 生成 .bat：等待主进程退出 → 复制覆盖 → 重启
            string batPath = Path.Combine(Path.GetTempPath(), $"pvzwstools_apply_{Guid.NewGuid():N}.bat");
            string logFile = Path.Combine(baseDir, "配置文件", "Log", $"update_{DateTime.Now:yyyyMMdd_HHmmss}.log");

            // robocopy 比 xcopy 更可靠（自 Windows Vista 起系统自带），无 DLL 占用问题
            // 用 /E（不使用 /MIR）：仅覆盖源中存在的文件，不删除用户已有的自定义脚本/阵型/setting.json 等
            // 语法: robocopy <源> <目标> /E /R:2 /W:2 /NFL /NDL /NP /LOG+:<log>
            string bat =
$@"@echo off
chcp 65001 >nul
echo 正在等待主进程退出...
:waitloop
tasklist /fi ""PID eq {Environment.ProcessId}"" 2>nul | find ""{Environment.ProcessId}"" >nul
if not errorlevel 1 (
    timeout /t 1 /nobreak >nul
    goto waitloop
)
echo 主进程已退出，开始覆盖文件...
robocopy ""{extractDir}"" ""{baseDir}"" /E /R:2 /W:2 /NFL /NDL /NP /LOG+:""{logFile}"" >nul
echo 覆盖完成，正在重启程序...
start """" ""{currentExe}""
exit
";
            File.WriteAllText(batPath, bat);

            Log.Info($"启动应用脚本: {batPath}");
            var psi = new ProcessStartInfo
            {
                FileName = batPath,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            _ = Process.Start(psi);

            // 给 bat 一点启动时间，然后退出主程序
            await Task.Delay(500);
            Application.Current?.Shutdown();
            return true;
        }
        catch(Exception ex)
        {
            Log.Error($"应用更新失败: {ex}");
            return false;
        }
    }

    private static string GetNumericPart(string? informationalVersion)
    {
        if(string.IsNullOrWhiteSpace(informationalVersion))
            return "0.0.0";

        // 形如 "1.2.3" 或 "1.2.3-beta.1+abc" → 取第一段
        string s = informationalVersion.Trim();
        int plus = s.IndexOf('+');
        if(plus >= 0) s = s[..plus];
        int dash = s.IndexOf('-');
        if(dash >= 0) s = s[..dash];
        return s;
    }
}
