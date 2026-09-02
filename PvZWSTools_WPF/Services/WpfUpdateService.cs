using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Windows;
using PvZWSTools_Shared;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_WPF.Services;

/// <summary>
/// WPF 端自动更新实现：
/// 下载 zip → 解压到临时目录 → 生成 .bat 等待主进程退出 → 替换 exe 与同目录文件 → 重启。
///
/// 资产选择策略：
/// - 先检查本机是否有 .NET 10 Desktop Runtime
/// - 如果有，优先查 framework-dependent 小包（PvZWSTools-win-fwdep.zip，几MB）
/// - 如果小包不存在或无 runtime，查 self-contained 大包（PvZWSTools-win.zip，~60MB）
/// </summary>
public class WpfUpdateService : UpdateService
{
    /// <summary>
    /// 检查本机是否已安装指定主版本的 .NET Desktop Runtime。
    /// 用 dotnet CLI 检测，失败时兜底返回 false（保守策略让用户下大包）。
    /// </summary>
    public static bool HasDesktopRuntime(string majorVersion)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--list-runtimes",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            if(proc == null) return false;
            proc.WaitForExit(5000);
            if(proc.ExitCode != 0) return false;

            string output = proc.StandardOutput.ReadToEnd();
            // 匹配 "Microsoft.WindowsDesktop.App 10.x.x" 或 "Microsoft.WindowsDesktop.App runtime 10.x.x"
            return System.Text.RegularExpressions.Regex.IsMatch(output,
                @"Microsoft\.WindowsDesktop\.App(?:\.Framework)?[^\d]*" + System.Text.RegularExpressions.Regex.Escape(majorVersion) + @"\.",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        catch(Exception ex)
        {
            Log.Warning($"检测 .NET Runtime 失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 重写检查更新：优先查 framework-dependent 小包（如果本机有运行时）。
    /// 没找到小包再回退到大包。
    /// </summary>
    public override async Task<UpdateInfo?> CheckForUpdatesAsync(string assetName, CancellationToken ct = default)
    {
        bool hasRuntime = HasDesktopRuntime(Sharedstring.TargetNetRuntimeMajor);
        Log.Info($"本机 .NET {Sharedstring.TargetNetRuntimeMajor} Desktop Runtime: {(hasRuntime ? "已安装" : "未安装")}");

        if(hasRuntime)
        {
            // 先查 framework-dependent 小包
            var fwdep = await base.CheckForUpdatesAsync(Sharedstring.AssetNameWindowsFwDepend, ct);
            if(fwdep != null)
            {
                Log.Info("使用 framework-dependent 小包（几MB）");
                return fwdep;
            }
            Log.Info("未找到 framework-dependent 资产，回退到 self-contained 大包");
        }

        return await base.CheckForUpdatesAsync(Sharedstring.AssetNameWindows, ct);
    }
    /// <inheritdoc />
    public override Version CurrentVersion
    {
        get
        {
            try
            {
                var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var name = asm.GetName();

                // 优先用 InformationalVersion：日期格式 + 可能带 -fixN 后缀
                var infoAttr = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if(infoAttr != null)
                {
                    var parsed = ParseInformationalVersion(infoAttr.InformationalVersion);
                    if(parsed != null) return parsed;
                }

                return name.Version ?? new Version(0, 0, 0);
            }
            catch(Exception ex)
            {
                Log.Error($"读取当前版本失败: {ex.Message}");
                return new Version(0, 0, 0);
            }
        }
    }

    /// <summary>
    /// 友好显示版本号：优先用 InformationalVersion（日期格式 YYYY.MM.dd-fixN），
    /// 否则回退到 Version.ToString()。
    /// </summary>
    public override string CurrentVersionDisplay
    {
        get
        {
            try
            {
                var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var infoAttr = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if(infoAttr != null && !string.IsNullOrWhiteSpace(infoAttr.InformationalVersion))
                {
                    string s = infoAttr.InformationalVersion.Trim();
                    int plus = s.IndexOf('+');
                    if(plus >= 0) s = s[..plus];
                    return s;
                }
            }
            catch { }
            return CurrentVersion.ToString();
        }
    }

    /// <summary>
    /// 从 InformationalVersion 字符串解析出 Version 对象。
    /// 日期格式 YYYY.M.D 直接解析；带 -fixN 后缀时把 N 塞进 Revision 字段。
    /// </summary>
    private static Version? ParseInformationalVersion(string? info)
    {
        if(string.IsNullOrWhiteSpace(info)) return null;

        // 去掉 build metadata（+ 后面的部分）
        string s = info.Trim();
        int plus = s.IndexOf('+');
        if(plus >= 0) s = s[..plus];

        // 分割日期部分和 suffix
        string datePart = s;
        int dash = s.IndexOf('-');
        string? suffix = null;
        if(dash >= 0) { datePart = s[..dash]; suffix = s[(dash + 1)..]; }

        // 解析 fixN 后缀
        int? fixNumber = null;
        if(suffix != null)
        {
            var m = System.Text.RegularExpressions.Regex.Match(suffix, @"^fix(\d+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if(m.Success && int.TryParse(m.Groups[1].Value, out int fn)) fixNumber = fn;
        }

        // 日期格式 YYYY.M.D
        var dateMatch = System.Text.RegularExpressions.Regex.Match(datePart, @"^(\d{4})\.(\d{1,2})\.(\d{1,2})$");
        if(dateMatch.Success &&
           int.TryParse(dateMatch.Groups[1].Value, out int y) &&
           int.TryParse(dateMatch.Groups[2].Value, out int mth) &&
           int.TryParse(dateMatch.Groups[3].Value, out int d) &&
           y >= 2020 && y <= 2100 && mth >= 1 && mth <= 12 && d >= 1 && d <= 31)
        {
            return new Version(y, mth, d, fixNumber ?? 0);
        }

        // 标准语义版本
        return Version.TryParse(datePart, out var v) ? v : null;
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
            // 解压到 exe 同级的 update 临时子目录
            string updateRoot = Path.Combine(baseDir, "update");
            _ = Directory.CreateDirectory(updateRoot);
            string extractDir = Path.Combine(updateRoot, $"pvzwstools_extract");
            if(Directory.Exists(extractDir)) Directory.Delete(extractDir, true);
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

            // 用 PowerShell 脚本代替 bat：
            // 1. 完美支持中文路径 + UTF-8
            // 2. Copy-Item -Force 无条件覆盖（robocopy 默认跳过时间戳旧的文件）
            // 3. 自带重试循环应对文件占用
            string psPath = Path.Combine(updateRoot, $"pvzwstools_apply_{Guid.NewGuid():N}.ps1");
            string escapedExtractDir = extractDir.Replace("'", "''");
            string escapedBaseDir = baseDir.Replace("'", "''");
            string escapedCurrentExe = currentExe.Replace("'", "''");

            // 用纯 verbatim 字符串（不是插值），避免 C# 把 PS 的 $变量 当成插值
            string ps = @"$ErrorActionPreference = 'Continue'
$mainPid = __MAIN_PID__
$src = '__SRC__'
$dst = '__DST__'
$exe = '__EXE__'

# 统一尾部反斜杠，避免 Substring/Join-Path 拼错
$src = $src.TrimEnd('\')
$dst = $dst.TrimEnd('\') + '\'

Write-Host ""等待主进程退出 PID=$mainPid...""
while (Get-Process -Id $mainPid -ErrorAction SilentlyContinue) {
    Start-Sleep -Milliseconds 300
}
Write-Host ""主进程已退出，开始覆盖文件...""

$maxRetries = 5
$retryDelay = 200
Get-ChildItem -Path $src -Recurse -File | ForEach-Object {
    $relative = $_.FullName.Substring($src.Length + 1)  # 跳过 src 后面的反斜杠
    $destPath = Join-Path $dst $relative
    $destDir = Split-Path $destPath -Parent
    if (-not (Test-Path $destDir)) {
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    }
    $copied = $false
    for ($r = 0; $r -lt $maxRetries; $r++) {
        try {
            Copy-Item -Path $_.FullName -Destination $destPath -Force -ErrorAction Stop
            $copied = $true
            break
        } catch {
            Start-Sleep -Milliseconds $retryDelay
        }
    }
    if (-not $copied) {
        Write-Host ""覆盖失败: $relative""
    }
}

Write-Host ""覆盖完成，3秒后重启程序...""
Start-Sleep -Seconds 3
Start-Process -FilePath $exe

Start-Sleep -Seconds 2
Remove-Item -Path (Join-Path $dst 'update') -Recurse -Force -ErrorAction SilentlyContinue
";
            ps = ps.Replace("__MAIN_PID__", Environment.ProcessId.ToString())
                   .Replace("__SRC__", escapedExtractDir)
                   .Replace("__DST__", escapedBaseDir)
                   .Replace("__EXE__", escapedCurrentExe);

            File.WriteAllText(psPath, ps);

            Log.Info($"启动应用脚本: {psPath}");
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{psPath}\"",
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
