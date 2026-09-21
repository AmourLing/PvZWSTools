using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows;
using Newtonsoft.Json.Linq;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_WPF.Services;

/// <summary>
/// WPF 端自动更新实现：
/// 下载 zip → 解压到临时目录 → 生成 PowerShell 脚本等待主进程退出 → 覆盖文件 → 重启。
///
/// Windows 端只取 framework-dependent 小包（几 MB）：用户能启动程序、能进更新界面，
/// 就说明运行时一定可用（要么共享安装，要么 setup.exe 自带），无需探测 dotnet CLI，
/// 也不必下 ~60MB 的大包。覆盖 self-contained 安装时保留其宿主文件，见 <see cref="ApplyUpdateAsync"/>。
/// </summary>
public class WpfUpdateService:UpdateService
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

            // 发布包是「根放入口 exe、其余全在 app\」的两层布局，而 baseDir 本身就是 app\，
            // 所以覆盖源要往下钻一层；平铺的老包（master 那批）走 else，行为不变。
            string nestedDir = Path.Combine(extractDir, "app");
            string payloadDir = Directory.Exists(nestedDir) ? nestedDir : extractDir;

            // 期望包内至少包含 PvZWSTools.exe（或与 AssemblyName 同名 .exe）
            string exeName = (Assembly.GetEntryAssembly()?.GetName().Name ?? "PvZWSTools") + ".exe";
            string extractedExe = Path.Combine(payloadDir, exeName);
            if(!File.Exists(extractedExe))
            {
                // 兜底：扫描包内主程序目录的任意 .exe
                var alt = Directory.GetFiles(payloadDir, "*.exe", SearchOption.TopDirectoryOnly)
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

            // 更新完从外层入口重启。判定"我确实在嵌套布局的内层"要精确：
            // 父目录下的 app\<当前 exe 名> 必须正好就是正在运行的这个文件。
            string restartExe = currentExe;
            var parentDir = Directory.GetParent(
                baseDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if(parentDir != null)
            {
                string innerExe = Path.GetFullPath(
                    Path.Combine(parentDir.FullName, "app", Path.GetFileName(currentExe)));
                string outerEntry = Path.Combine(parentDir.FullName, Path.GetFileName(currentExe));
                if(File.Exists(outerEntry)
                   && string.Equals(innerExe, Path.GetFullPath(currentExe), StringComparison.OrdinalIgnoreCase))
                {
                    restartExe = outerEntry;
                    Log.Info($"重启改走外层入口: {outerEntry}");
                }
            }

            // 安装形态决定能不能覆盖宿主文件：self-contained 安装（setup.exe 装的这种）自带运行时，
            // 换上小包的 runtimeconfig/deps 后宿主会改去找共享运行时，
            // 没装过的机器更新完直接启动不了（实测报 "You must install or update .NET"）。
            var installed = ReadHostInfo(baseDir);
            var package = ReadHostInfo(payloadDir);

            if(installed.RuntimeMajor > 0 && package.RuntimeMajor > 0 && installed.RuntimeMajor != package.RuntimeMajor)
            {
                Log.Error($"更新包要求 .NET {package.RuntimeMajor}，当前安装是 .NET {installed.RuntimeMajor}，请改用安装包更新");
                return false;
            }

            bool keepHostFiles = installed.SelfContained;
            Log.Info($"安装形态: {(keepHostFiles ? "self-contained（保留 runtimeconfig/deps）" : "framework-dependent")}");

            // 用 PowerShell 脚本代替 bat：
            // 1. 完美支持中文路径 + UTF-8
            // 2. Copy-Item -Force 无条件覆盖（robocopy 默认跳过时间戳旧的文件）
            // 3. 自带重试循环应对文件占用
            // 包里的外层入口不覆盖：它只负责拉起 app\ 里的主程序，装一次就够，旧入口带新主程序照样能跑。
            string psPath = Path.Combine(updateRoot, $"pvzwstools_apply_{Guid.NewGuid():N}.ps1");
            File.WriteAllText(psPath, BuildApplyScript(Environment.ProcessId, payloadDir, baseDir, restartExe, keepHostFiles));

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

    /// <summary>
    /// 生成覆盖脚本。keepHostFiles 为 true 时跳过 *.runtimeconfig.json 与 *.deps.json，
    /// 让 self-contained 安装继续用自己目录里的运行时。
    /// </summary>
    private static string BuildApplyScript(int mainPid, string extractDir, string baseDir, string currentExe, bool keepHostFiles)
    {
        // 用纯 verbatim 字符串（不是插值），避免 C# 把 PS 的 $变量 当成插值
        string ps = @"$ErrorActionPreference = 'Continue'
$mainPid = __MAIN_PID__
$src = '__SRC__'
$dst = '__DST__'
$exe = '__EXE__'
$keepHost = __KEEP_HOST_FILES__

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
    if ($keepHost -and ($_.Name -like '*.runtimeconfig.json' -or $_.Name -like '*.deps.json')) {
        Write-Host ""保留宿主文件: $relative""
        return
    }
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

        return ps.Replace("__MAIN_PID__", mainPid.ToString())
                 .Replace("__SRC__", extractDir.Replace("'", "''"))
                 .Replace("__DST__", baseDir.Replace("'", "''"))
                 .Replace("__EXE__", currentExe.Replace("'", "''"))
                 .Replace("__KEEP_HOST_FILES__", keepHostFiles ? "$true" : "$false");
    }

    /// <summary>
    /// 读目录里的 *.runtimeconfig.json，判断安装形态与运行时主版本。
    /// 有 includedFrameworks 即 self-contained（运行时在程序目录里），frameworks 则是共享运行时。
    /// 读不到时返回 (false, 0)，调用方按"可覆盖全部文件、不做版本比对"处理。
    /// </summary>
    private static (bool SelfContained, int RuntimeMajor) ReadHostInfo(string dir)
    {
        try
        {
            string? cfg = Directory.GetFiles(dir, "*.runtimeconfig.json").FirstOrDefault();
            if(cfg == null) return (false, 0);

            var options = JObject.Parse(File.ReadAllText(cfg))["runtimeOptions"] as JObject;
            var included = options?["includedFrameworks"] as JArray;
            var frameworks = included ?? options?["frameworks"] as JArray;
            string? version = frameworks?.OfType<JObject>().FirstOrDefault()?.Value<string>("version");

            if(version == null || !int.TryParse(version.Split('.')[0], out int major)) return (false, 0);
            return (included != null, major);
        }
        catch(Exception ex)
        {
            Log.Warning($"读取 {dir} 的 runtimeconfig 失败: {ex.Message}");
            return (false, 0);
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
