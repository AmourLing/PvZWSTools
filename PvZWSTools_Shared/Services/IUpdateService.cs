using PvZWSTools_Shared.Models;

namespace PvZWSTools_Shared.Services;

/// <summary>
/// 自动更新服务抽象（检查 + 下载 + 应用）。
/// 平台差异在 <see cref="ApplyUpdateAsync"/> 中由各端实现。
/// </summary>
public interface IUpdateService
{
    /// <summary>当前本地版本号（程序集 / 包信息）。</summary>
    Version CurrentVersion { get; }

    /// <summary>
    /// 后台检查最新版本，失败返回 null（不抛异常，由调用方决定如何提示）。
    /// 优先 GitHub Releases，失败回退 Gitee。
    /// </summary>
    /// <param name="assetName">平台对应的资产文件名（如 PvZWSTools-win.zip / PvZWSTools-android.apk）。</param>
    Task<UpdateInfo?> CheckForUpdatesAsync(string assetName, CancellationToken ct = default);

    /// <summary>
    /// 下载更新包到临时文件，返回本地路径；失败返回 null。
    /// 内部会先尝试主链接，失败再尝试 fallback。
    /// </summary>
    /// <param name="info">由 <see cref="CheckForUpdatesAsync"/> 返回的版本信息。</param>
    /// <param name="progress">可选进度回调（0-100）。</param>
    Task<string?> DownloadUpdateAsync(UpdateInfo info, IProgress<int>? progress = null, CancellationToken ct = default);

    /// <summary>
    /// 应用更新（替换 exe/apk 并重启）。
    /// 不同平台实现：
    /// <list type="bullet">
    ///   <item>WPF：生成 .bat 等待主进程退出 → 替换 exe → 重启。</item>
    ///   <item>Android：触发系统 APK 安装 Intent（由用户在系统安装器中确认）。</item>
    /// </list>
    /// </summary>
    /// <returns>是否已成功触发应用流程（不代表替换完成）。</returns>
    Task<bool> ApplyUpdateAsync(string downloadedFilePath);
}
