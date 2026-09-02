using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PvZWSTools_Shared.Models;

/// <summary>
/// 从 GitHub / Gitee Release 解析出的版本信息（平台无关）。
/// </summary>
public class UpdateInfo
{
    /// <summary>发布标签（如 "v2026.09.02" 或 "v2026.09.02-fix1"）。原始字符串，保留前缀。</summary>
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

    /// <summary>百度网盘下载地址（分享链接，自动解析提取码）。</summary>
    [JsonProperty("baidu_url")]
    public string? DownloadUrlBaidu { get; set; }

    /// <summary>百度网盘提取码（若 URL 中未包含）。</summary>
    [JsonIgnore]
    public string? BaiduExtractCode { get; set; }

    /// <summary>期望的 SHA256（若 Release 中提供 sha256 资产，否则为 null）。</summary>
    [JsonProperty("sha256")]
    public string? Sha256 { get; set; }

    /// <summary>包大小（字节）。</summary>
    [JsonProperty("size")]
    public long? Size { get; set; }

    /// <summary>当前来源（"github" 或 "gitee"），便于日志排查。</summary>
    [JsonIgnore]
    public string? Source { get; set; }

    // ---------- 版本解析 ----------

    /// <summary>
    /// 解析后的版本元数据（日期 + fixN 后缀）。
    /// 支持 v2026.09.02 / v2026.09.02-fix1 / v2026.09.02-fix2 等格式。
    /// </summary>
    [JsonIgnore]
    public ParsedVersion? Parsed { get; private set; }

    /// <summary>
    /// 从 TagName 解析出 ParsedVersion。
    /// 解析失败返回 null。
    /// </summary>
    public ParsedVersion? ParseTag()
    {
        if(string.IsNullOrWhiteSpace(TagName)) return null;

        string raw = TagName.Trim();
        // 去掉 v/V 前缀
        if(raw.StartsWith('v') || raw.StartsWith('V'))
            raw = raw[1..];

        // 去掉 build metadata（+ 后面的部分）
        int plus = raw.IndexOf('+');
        if(plus >= 0) raw = raw[..plus];

        // 分割日期部分和 suffix（-fix1 / -fix2 / -beta 等）
        string datePart = raw;
        int dash = raw.IndexOf('-');
        string? suffix = null;
        if(dash >= 0)
        {
            datePart = raw[..dash];
            suffix = raw[(dash + 1)..];
        }

        // 尝试解析日期格式 YYYY.MM.DD
        var dateMatch = Regex.Match(datePart, @"^(\d{4})\.(\d{1,2})\.(\d{1,2})$");
        if(dateMatch.Success &&
           int.TryParse(dateMatch.Groups[1].Value, out int year) &&
           int.TryParse(dateMatch.Groups[2].Value, out int month) &&
           int.TryParse(dateMatch.Groups[3].Value, out int day) &&
           year >= 2020 && year <= 2100 && month >= 1 && month <= 12 && day >= 1 && day <= 31)
        {
            // 提取 fixN 序号
            int? fixNumber = null;
            if(suffix != null)
            {
                var fixMatch = Regex.Match(suffix, @"^fix(\d+)$", RegexOptions.IgnoreCase);
                if(fixMatch.Success && int.TryParse(fixMatch.Groups[1].Value, out int fn))
                    fixNumber = fn;
            }

            Parsed = new ParsedVersion(year, month, day, fixNumber, suffix);
            return Parsed;
        }

        // 回退：标准语义版本 1.2.3 / 1.2.3-beta.1
        if(Version.TryParse(datePart, out var semVer))
        {
            Parsed = new ParsedVersion(semVer.Major, semVer.Minor, semVer.Build, semVer.Revision, suffix);
            return Parsed;
        }

        return null;
    }

    /// <summary>
    /// 兼容旧代码：返回可解析的 System.Version（取数值部分，忽略 suffix）。
    /// 用于 CurrentVersion 比对的 fallback。
    /// </summary>
    [JsonIgnore]
    public Version? Version
    {
        get
        {
            var p = ParseTag();
            if(p is null) return null;
            if(p.Year > 0)
                return new Version(p.Year, p.Month, p.Day);
            if(p.SemVer != null)
                return p.SemVer;
            return null;
        }
    }

    /// <summary>
    /// 判断是否比 <paramref name="current" /> 更新。
    /// 支持日期格式（v2026.09.02 / v2026.09.02-fix1）和标准语义版本。
    /// </summary>
    public bool IsNewerThan(Version? current)
    {
        var remote = ParseTag();
        if(remote is null || current is null)
        {
            // fallback：字符串比较
            return !string.Equals(TagName, current?.ToString(), StringComparison.Ordinal);
        }

        // 判断远端是否为日期格式（Year >= 2020）
        bool remoteIsDate = remote.Year >= 2020 && remote.Month >= 1 && remote.Month <= 12 && remote.Day >= 1 && remote.Day <= 31;
        bool currentIsDate = current.Major >= 2020 && current.Minor >= 1 && current.Minor <= 12 && current.Build >= 1 && current.Build <= 31;

        if(remoteIsDate && currentIsDate)
        {
            // 日期格式 vs 日期格式：直接比较 YYYYMMDD 数值
            long remoteDateNum = remote.Year * 10000L + remote.Month * 100L + remote.Day;
            long currentDateNum = current.Major * 10000L + current.Minor * 100L + Math.Max(0, current.Build);

            if(remoteDateNum != currentDateNum)
                return remoteDateNum > currentDateNum;

            // 日期相同，比 fixN：0 < fix1 < fix2
            int remoteFix = remote.FixNumber ?? 0;
            int currentFix = current.Revision; // WpfUpdateService 把 fixN 塞进 Revision
            return remoteFix > currentFix;
        }

        if(remoteIsDate && !currentIsDate)
        {
            // 远端日期格式，当前语义版本（如 0.9.9 / 1.0.1）→ 远端肯定更新
            return true;
        }

        if(!remoteIsDate && currentIsDate)
        {
            // 远端语义版本（如 v1.0.1），当前日期格式（如 2026.09.02）→ 远端旧
            return false;
        }

        // 都是语义版本
        Version? remoteSem = remote.SemVer;
        if(remoteSem is null) return false;
        if(!remoteSem.Equals(current)) return remoteSem > current;

        // semver 相等时比 suffix
        int remoteFix2 = remote.FixNumber ?? int.MinValue;
        return remoteFix2 > current.Revision;
    }
}

/// <summary>
/// 解析后的版本元数据。
/// 支持日期格式（YYYY.MM.DD + 可选 -fixN）和标准语义版本。
/// </summary>
public class ParsedVersion
{
    /// <summary>年份（日期格式时 > 0；语义版本时 Major）。</summary>
    public int Year { get; }

    /// <summary>月份（日期格式时 1-12；语义版本时 Minor）。</summary>
    public int Month { get; }

    /// <summary>日期号（日期格式时 1-31；语义版本时 Build）。</summary>
    public int Day { get; }

    /// <summary>标准语义版本（如果解析到的是 1.2.3 格式）。</summary>
    public Version? SemVer { get; }

    /// <summary>-fixN 后缀中的 N；没有则为 null。</summary>
    public int? FixNumber { get; }

    /// <summary>原始 suffix 字符串（如 "fix1"、"beta"）。</summary>
    public string? Suffix { get; }

    public ParsedVersion(int year, int month, int day, int? fixNumber, string? suffix)
    {
        Year = year;
        Month = month;
        Day = day;
        FixNumber = fixNumber;
        Suffix = suffix;
    }

    public ParsedVersion(int major, int minor, int build, int revision, string? suffix)
    {
        SemVer = new Version(major, minor, build, Math.Max(0, revision));
        Year = major;
        Month = minor;
        Day = build;
        FixNumber = ParseFixFromSuffix(suffix);
        Suffix = suffix;
    }

    private static int? ParseFixFromSuffix(string? suffix)
    {
        if(suffix is null) return null;
        var m = Regex.Match(suffix, @"fix(\d+)", RegexOptions.IgnoreCase);
        return m.Success && int.TryParse(m.Groups[1].Value, out int n) ? n : null;
    }

    public override string ToString()
    {
        if(Year > 0 && Year >= 2020)
        {
            string s = $"{Year}.{Month:D2}.{Day:D2}";
            if(FixNumber.HasValue) s += $"-fix{FixNumber.Value}";
            return s;
        }
        return SemVer?.ToString() ?? "unknown";
    }
}
