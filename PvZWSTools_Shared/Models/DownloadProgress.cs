namespace PvZWSTools_Shared.Models;

/// <summary>
/// 下载进度信息（给 UI 显示用，线程安全）。
/// </summary>
public readonly record struct DownloadProgress(
    /// <summary>已下载字节数。</summary>
    long BytesDownloaded,
    /// <summary>总字节数；服务器未提供 Content-Length 时为 null。</summary>
    long? TotalBytes,
    /// <summary>进度百分比 0-100；TotalBytes 为 null 时为 null（indeterminate 模式）。</summary>
    int? Percentage,
    /// <summary>当前速度估算（bytes/sec）；首次回调或无法估算时为 null。</summary>
    double? BytesPerSecond
);
