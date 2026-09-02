using System.Text.RegularExpressions;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.Content;
using PvZWSTools_Shared.Services;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_Avalonia.Services;

/// <summary>
/// Android 端自动更新实现：
/// 下载 apk 到 external files dir/updates/ → 通过 FileProvider 暴露 content URI →
/// 触发系统 APK 安装器（由用户在系统安装界面确认）。
/// </summary>
public class AndroidUpdateService : UpdateService
{
    private const string FileProviderAuthority = "net.pvz.pvzwstools.fileprovider";
    private const string UpdatesSubdir = "updates";

    private readonly Context _context;

    public AndroidUpdateService(Context context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public override Version CurrentVersion
    {
        get
        {
            try
            {
                var info = _context.PackageManager?.GetPackageInfo(_context.PackageName ?? "", 0);
                if(info == null) return new Version(0, 0, 0);

                string? vn = info.VersionName;
                if(string.IsNullOrWhiteSpace(vn)) return new Version(0, 0, 0);

                return ParseVersionName(vn);
            }
            catch(Exception ex)
            {
                Log.Error($"Android 读取版本号失败: {ex.Message}");
                return new Version(0, 0, 0);
            }
        }
    }

    /// <inheritdoc />
    public override string CurrentVersionDisplay
    {
        get
        {
            try
            {
                var info = _context.PackageManager?.GetPackageInfo(_context.PackageName ?? "", 0);
                string? vn = info?.VersionName;
                if(!string.IsNullOrWhiteSpace(vn)) return vn;
            }
            catch { }
            return CurrentVersion.ToString();
        }
    }

    /// <summary>
    /// 解析 Android versionName 为 Version 对象。
    /// 支持日期格式（2026.09.02-fix1 → Version(2026, 9, 2, 1)）和标准语义版本。
    /// </summary>
    private static Version ParseVersionName(string versionName)
    {
        string raw = versionName.Trim();
        if(raw.StartsWith('v') || raw.StartsWith('V')) raw = raw[1..];

        // 日期格式：2026.09.02 或 2026.09.02-fix1
        var dateMatch = Regex.Match(raw, @"^(\d{4})\.(\d{1,2})\.(\d{1,2})(?:-fix(\d+))?$", RegexOptions.IgnoreCase);
        if(dateMatch.Success)
        {
            int year = int.Parse(dateMatch.Groups[1].Value);
            int month = int.Parse(dateMatch.Groups[2].Value);
            int day = int.Parse(dateMatch.Groups[3].Value);
            int fix = dateMatch.Groups[4].Success ? int.Parse(dateMatch.Groups[4].Value) : 0;
            return new Version(year, month, day, fix);
        }

        // 标准语义版本回退
        return Version.TryParse(raw, out var v) ? v : new Version(0, 0, 0);
    }

    /// <inheritdoc />
    public override Task<bool> ApplyUpdateAsync(string downloadedFilePath)
    {
        if(!File.Exists(downloadedFilePath))
        {
            Log.Error($"应用更新失败：文件不存在 {downloadedFilePath}");
            return Task.FromResult(false);
        }

        try
        {
            // 触发系统安装器前，先把文件移动到 updates 目录（FileProvider 已声明该路径）
            var updatesDir = new Java.IO.File(_context.GetExternalFilesDir(null), UpdatesSubdir);
            if(!updatesDir.Exists()) _ = updatesDir.Mkdirs();

            var targetFile = new Java.IO.File(updatesDir, $"PvZWSTools-{DateTime.Now:yyyyMMddHHmmss}.apk");
            // 复制（不直接 Move 以保留原临时文件做排查）
            File.Copy(downloadedFilePath, targetFile.AbsolutePath, overwrite: true);

            var uri = FileProvider.GetUriForFile(_context, FileProviderAuthority, targetFile);
            if(uri == null)
            {
                Log.Error("无法为 APK 生成 content URI，FileProvider 配置可能有误");
                return Task.FromResult(false);
            }

            var intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(uri, "application/vnd.android.package-archive");
            intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission);

            _context.StartActivity(intent);
            Log.Info("已调起系统 APK 安装器，请用户确认安装");
            return Task.FromResult(true);
        }
        catch(Exception ex)
        {
            Log.Error($"Android 应用更新失败: {ex.Message}");
            return Task.FromResult(false);
        }
    }
}
