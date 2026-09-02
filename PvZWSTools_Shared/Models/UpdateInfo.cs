using Newtonsoft.Json;

namespace PvZWSTools_Shared.Models;

/// <summary>
/// 从 GitHub / Gitee Release 解析出的版本信息（平台无关）。
/// </summary>
public class UpdateInfo
{
    /// <summary>发布标签（如 "v1.2.3"）。原始字符串，保留前缀。</summary>
    [JsonProperty("tag_name")]
    public string? TagName { get; set; }

    /// <summary>发布名称（Release Title）。</summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>发布说明（Markdown 文本）。</summary>
    [JsonProperty("body")]
    public string? ReleaseNotes { get; set; }

    /// <summary>发布时间（UTC）。</summary>
    [JsonProperty("published_at")]
    public DateTime? PublishedAt { get; set; }

    /// <summary>主下载地址（GitHub Release asset 直链）。</summary>
    [JsonProperty("url")]
    public string? DownloadUrl { get; set; }

    /// <summary>Fallback 下载地址（Gitee Release asset 直链）。</summary>
    [JsonProperty("fallback_url")]
    public string? DownloadUrlFallback { get; set; }

    /// <summary>期望的 SHA256（若 Release 中提供 sha256 资产，否则为 null）。</summary>
    [JsonProperty("sha256")]
    public string? Sha256 { get; set; }

    /// <summary>包大小（字节）。</summary>
    [JsonProperty("size")]
    public long? Size { get; set; }

    /// <summary>当前来源（"github" 或 "gitee"），便于日志排查。</summary>
    [JsonIgnore]
    public string? Source { get; set; }

    /// <summary>
    /// 从 TagName 解析语义版本号（去掉 v/V 前缀）。
    /// 解析失败返回 null。
    /// </summary>
    [JsonIgnore]
    public Version? Version
    {
        get
        {
            if(string.IsNullOrWhiteSpace(TagName))
                return null;

            string raw = TagName.Trim();
            if(raw.StartsWith("v", StringComparison.OrdinalIgnoreCase) ||
               raw.StartsWith("V", StringComparison.OrdinalIgnoreCase))
            {
                raw = raw[1..];
            }

            return Version.TryParse(raw, out var v) ? v : null;
        }
    }

    /// <summary>
    /// 判断是否比 <paramref name="current" /> 更新。
    /// 任一侧版本号无法解析时，按 TagName 字符串不等作 fallback 判断。
    /// </summary>
    public bool IsNewerThan(Version? current)
    {
        var remote = Version;
        if(remote is null || current is null)
            return !string.Equals(TagName, current?.ToString(), StringComparison.Ordinal);

        return remote > current;
    }
}
