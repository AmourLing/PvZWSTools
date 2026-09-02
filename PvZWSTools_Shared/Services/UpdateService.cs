using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;

namespace PvZWSTools_Shared.Services;

/// <summary>
/// 自动更新服务基类：实现 HTTP 检查 + 下载的跨平台共用逻辑。
/// <see cref="ApplyUpdateAsync"/> 由各平台子类实现。
/// </summary>
public abstract class UpdateService : IUpdateService
{
    private const string GITHUB_API = "https://api.github.com/repos/{0}/{1}/releases?per_page=50";
    private const string GITEE_API   = "https://gitee.com/api/v5/repos/{0}/{1}/releases?per_page=50";

    private static readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    })
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    static UpdateService()
    {
        // GitHub API 要求 User-Agent，否则 403
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PvZWSTools-Updater/1.0");
    }

    /// <inheritdoc />
    public abstract Version CurrentVersion { get; }

    /// <summary>
    /// 当前版本的友好显示字符串（如 "2026.09.02" 或 "2026.09.02-fix1"）。
    /// 默认返回 CurrentVersion.ToString()，子类可重写。
    /// </summary>
    public virtual string CurrentVersionDisplay => CurrentVersion.ToString();

    /// <inheritdoc />
    public abstract Task<bool> ApplyUpdateAsync(string downloadedFilePath);

    /// <inheritdoc />
    public virtual async Task<UpdateInfo?> CheckForUpdatesAsync(string assetName, CancellationToken ct = default)
    {
        // 同时查 GitHub 和 Gitee，两个源的 URL 都存好，下载时 Gitee 优先（国内快）
        var gh = await TryFetchAsync("github", GITHUB_API, assetName, ct);
        var gitee = await TryFetchAsync("gitee", GITEE_API, assetName, ct);

        // 合并：选有新版本的那个作为主，另一个的 URL 作为 fallback
        UpdateInfo? primary;
        UpdateInfo? secondary;

        // 优先返回有下载 URL 的（检查更充分）
        if(gh != null && !string.IsNullOrEmpty(gh.DownloadUrl))
        {
            primary = gh;
            secondary = gitee;
        }
        else if(gitee != null && !string.IsNullOrEmpty(gitee.DownloadUrl))
        {
            primary = gitee;
            secondary = gh;
        }
        else if(gh != null) { primary = gh; secondary = gitee; }
        else if(gitee != null) { primary = gitee; secondary = gh; }
        else return null;

        // 把另一个源的 URL 合并到 primary
        if(secondary != null && !string.IsNullOrEmpty(secondary.DownloadUrl))
        {
            // 如果 primary 没有 fallback，把 secondary 的 URL 作为 fallback
            if(string.IsNullOrEmpty(primary.DownloadUrlFallback) && secondary.DownloadUrl != primary.DownloadUrl)
                primary.DownloadUrlFallback = secondary.DownloadUrl;
        }

        return primary;
    }

    /// <inheritdoc />
    public async Task<string?> DownloadUpdateAsync(UpdateInfo info, IProgress<DownloadProgress>? progress = null, CancellationToken ct = default)
    {
        if(info == null) throw new ArgumentNullException(nameof(info));

        // 收集所有 URL，Gitee 优先（国内 CDN 速度快），GitHub 作 fallback
        var urls = new List<string>();
        if(!string.IsNullOrWhiteSpace(info.DownloadUrl))
            urls.Add(info.DownloadUrl);
        if(!string.IsNullOrWhiteSpace(info.DownloadUrlFallback) && info.DownloadUrlFallback != info.DownloadUrl)
            urls.Add(info.DownloadUrlFallback);

        // Gitee URL 排到前面
        urls.Sort((a, b) =>
        {
            bool aIsGitee = a.Contains("gitee.com", StringComparison.OrdinalIgnoreCase);
            bool bIsGitee = b.Contains("gitee.com", StringComparison.OrdinalIgnoreCase);
            return aIsGitee == bIsGitee ? 0 : (aIsGitee ? -1 : 1);
        });

        if(urls.Count == 0)
        {
            Log.Error("下载失败：UpdateInfo 中未提供任何下载地址");
            return null;
        }

        // 下载到 exe 同级的 update 子目录（便于 bat 脚本后续处理）
        string updateDir = Path.Combine(AppContext.BaseDirectory, "update");
        _ = Directory.CreateDirectory(updateDir);
        string tempFile = Path.Combine(updateDir, $"pvzwstools_update_{Guid.NewGuid():N}{Path.GetExtension(assetHint(urls[0]))}");
        Log.Info($"开始下载更新包到: {tempFile}");

        foreach(var url in urls)
        {
            try
            {
                Log.Info($"尝试下载: {url}（来源 {info.Source}）");
                bool ok = await DownloadToFileAsync(url, tempFile, progress, ct);
                if(!ok) continue;

                // SHA256 校验（若提供）
                if(!string.IsNullOrWhiteSpace(info.Sha256))
                {
                    string actual = await ComputeSha256Async(tempFile, ct);
                    if(!string.Equals(actual, info.Sha256, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Error($"SHA256 校验失败：期望 {info.Sha256}，实际 {actual}");
                        TryDelete(tempFile);
                        continue;
                    }
                    Log.Info("SHA256 校验通过");
                }

                Log.Info("下载完成");
                return tempFile;
            }
            catch(Exception ex)
            {
                Log.Error($"下载失败（{url}）: {ex.Message}");
                TryDelete(tempFile);
            }
        }

        return null;
    }

    // ---------- 共用辅助 ----------

    private async Task<UpdateInfo?> TryFetchAsync(string source, string apiTemplate, string assetName, CancellationToken ct)
    {
        var (owner, repo) = source == "github"
            ? (Sharedstring.GitHubOwner, Sharedstring.GitHubRepo)
            : (Sharedstring.GiteeOwner, Sharedstring.GiteeRepo);

        string url = string.Format(apiTemplate, owner, repo);
        Log.Info($"检查更新（{source}）: {url}");

        try
        {
            using var resp = await _httpClient.GetAsync(url, ct);
            if(!resp.IsSuccessStatusCode)
            {
                Log.Warning($"{source} API 返回 {(int)resp.StatusCode} {resp.ReasonPhrase}");
                return null;
            }

            string json = await resp.Content.ReadAsStringAsync(ct);

            // releases?per_page=50 返回数组；releases/latest 返回单对象
            var releaseJsons = TryParseReleaseArray(json);
            if(releaseJsons == null || releaseJsons.Count == 0)
            {
                // 空数组或非数组：尝试单对象（跳过空数组）
                if(json.TrimStart().StartsWith('['))
                {
                    Log.Warning($"{source}：API 返回空数组，无 release");
                    return null;
                }
                var single = ParseReleaseJson(json, source, assetName);
                if(single != null)
                {
                    single.ParseTag();
                    Log.Info($"{source} 最新版本: tag={single.TagName}");
                    return single;
                }
                Log.Warning($"{source}：未找到任何 release 或匹配资产 {assetName}");
                return null;
            }

            // 遍历所有 release，按 ParsedVersion 找最新且包含目标资产的
            UpdateInfo? best = null;
            ParsedVersion? bestParsed = null;
            foreach(var rawJson in releaseJsons)
            {
                var info = ParseReleaseJson(rawJson, source, assetName);
                if(info == null) continue;

                var parsed = info.ParseTag();
                if(parsed == null) continue;

                if(bestParsed == null || CompareParsed(parsed, bestParsed) > 0)
                {
                    best = info;
                    bestParsed = parsed;
                }
            }

            if(best == null)
            {
                Log.Warning($"{source}：所有 release 中均未找到匹配资产 {assetName}");
                return null;
            }

            Log.Info($"{source} 最新版本: tag={best.TagName}（已过滤 draft/prerelease）");
            return best;
        }
        catch(Exception ex)
        {
            Log.Error($"{source} 检查更新异常: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 把 API 返回的 JSON 解析为 release 对象 JSON 字符串列表。
    /// 支持 GitHub releases 数组（过滤 draft/prerelease）和单对象 fallback。
    /// </summary>
    private static List<string>? TryParseReleaseArray(string json)
    {
        if(string.IsNullOrWhiteSpace(json)) return null;

        if(json.TrimStart().StartsWith('['))
        {
            var arr = JArray.Parse(json);
            var list = new List<string>();
            foreach(JObject obj in arr.OfType<JObject>())
            {
                bool isDraft = obj.Value<bool?>("draft") ?? false;
                bool isPrerelease = obj.Value<bool?>("prerelease") ?? false;
                if(isDraft || isPrerelease) continue;

                var tag = obj.Value<string>("tag_name");
                if(string.IsNullOrWhiteSpace(tag)) continue;

                list.Add(obj.ToString());
            }
            return list;
        }

        return null;
    }

    /// <summary>
    /// 比较两个 ParsedVersion。正数表示 a 更新。
    /// </summary>
    private static int CompareParsed(ParsedVersion a, ParsedVersion b)
    {
        bool aIsDate = a.Year >= 2020 && a.Month >= 1 && a.Month <= 12 && a.Day >= 1 && a.Day <= 31;
        bool bIsDate = b.Year >= 2020 && b.Month >= 1 && b.Month <= 12 && b.Day >= 1 && b.Day <= 31;

        if(aIsDate && bIsDate)
        {
            long aNum = a.Year * 10000L + a.Month * 100L + a.Day;
            long bNum = b.Year * 10000L + b.Month * 100L + b.Day;
            int cmp = aNum.CompareTo(bNum);
            if(cmp != 0) return cmp;
            return (a.FixNumber ?? 0).CompareTo(b.FixNumber ?? 0);
        }

        if(!aIsDate && !bIsDate)
        {
            var aSem = a.SemVer;
            var bSem = b.SemVer;
            if(aSem != null && bSem != null)
            {
                int cmp = aSem.CompareTo(bSem);
                if(cmp != 0) return cmp;
            }
            return (a.FixNumber ?? int.MinValue).CompareTo(b.FixNumber ?? int.MinValue);
        }

        return aIsDate ? 1 : -1;
    }

    /// <summary>
    /// 兼容 GitHub / Gitee 的 JSON 字段差异：
    /// GitHub: tag_name / published_at / assets[].browser_download_url / assets[].size
    /// Gitee:  tag_name / created_at        / assets[].browser_download_url / assets[].size
    /// </summary>
    private static UpdateInfo? ParseReleaseJson(string json, string source, string assetName)
    {
        if(string.IsNullOrWhiteSpace(json)) return null;
        var root = JObject.Parse(json);

        var tag = root.Value<string>("tag_name");
        if(string.IsNullOrWhiteSpace(tag)) return null;

        var info = new UpdateInfo
        {
            TagName = tag,
            Name = root.Value<string>("name"),
            ReleaseNotes = root.Value<string>("body"),
            PublishedAt = root.Value<DateTime?>("published_at") ?? root.Value<DateTime?>("created_at"),
            Source = source
        };

        // 百度网盘链接：优先使用硬编码常量，没配置时从 Release Notes 正则提取
        var baiduFromConst = Sharedstring.BaiduNetdiskUrl;
        if(!string.IsNullOrWhiteSpace(baiduFromConst))
        {
            info.DownloadUrlBaidu = baiduFromConst;
            info.BaiduExtractCode = Sharedstring.BaiduNetdiskCode;
            Log.Info($"使用硬编码百度网盘链接: {baiduFromConst}");
        }
        else
        {
            // 从 Release Notes 中提取百度网盘链接（格式：https://pan.baidu.com/s/1xxxxx 或 https://pan.baidu.com/share/link?shareid=xxx&uk=xxx）
            // 同时支持提取码：提取码：abcd / code: abcd / pwd=abcd
            var notes = info.ReleaseNotes ?? "";
            var baiduMatch = Regex.Match(notes, @"https?://pan\.baidu\.com/(?:s/[\w-]+|share/link\?[^\s)]+)", RegexOptions.IgnoreCase);
            if(baiduMatch.Success)
            {
                info.DownloadUrlBaidu = baiduMatch.Value;
                var codeMatch = Regex.Match(notes, @"提取码[：:]\s*([a-zA-Z0-9]{4})|code[：:]\s*([a-zA-Z0-9]{4})|pwd[=:]\s*([a-zA-Z0-9]{4})", RegexOptions.IgnoreCase);
                if(codeMatch.Success)
                {
                    info.BaiduExtractCode = codeMatch.Groups[1].Value
                        ?? codeMatch.Groups[2].Value
                        ?? codeMatch.Groups[3].Value;
                }
                Log.Info($"从 Release Notes 提取到百度网盘链接: {info.DownloadUrlBaidu}");
            }
        }

        // 在 assets 中按文件名匹配（不区分大小写、忽略查询字符串）
        // GitHub: assets[].browser_download_url
        // Gitee:  assets[].download_url
        var assets = root["assets"] as JArray ?? root["attach_files"] as JArray;
        if(assets == null) return info;

        string assetNameLower = assetName.ToLowerInvariant();
        foreach(JObject asset in assets.OfType<JObject>())
        {
            var name = asset.Value<string>("name");
            if(string.IsNullOrEmpty(name)) continue;
            if(!name.ToLowerInvariant().Equals(assetNameLower)) continue;

            // GitHub: browser_download_url；Gitee: download_url
            info.DownloadUrl = asset.Value<string>("browser_download_url")
                            ?? asset.Value<string>("download_url");
            info.Size = asset.Value<long?>("size");

            var digest = asset.Value<string>("digest");
            if(!string.IsNullOrWhiteSpace(digest))
                info.Sha256 = ParseSha256(digest);
            break;
        }

        // GitHub 没有匹配的 asset，但更新包可能存在同名 .sha256 文件，留作扩展点
        return info;
    }

    private static string? ParseSha256(string raw)
    {
        if(string.IsNullOrWhiteSpace(raw)) return null;
        // 形如 "sha256:abc..." 或单独 64 位 hex
        var m = Regex.Match(raw, "([0-9a-fA-F]{64})");
        return m.Success ? m.Groups[1].Value : raw;
    }

    private static async Task<bool> DownloadToFileAsync(string url, string targetFile, IProgress<DownloadProgress>? progress, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.UserAgent.ParseAdd("PvZWSTools-Updater/1.0");

        using var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        if(!resp.IsSuccessStatusCode)
        {
            Log.Warning($"下载响应 {(int)resp.StatusCode} {resp.ReasonPhrase}");
            return false;
        }

        long? total = resp.Content.Headers.ContentLength;
        await using var src = await resp.Content.ReadAsStreamAsync(ct);
        await using var dst = File.Create(targetFile);

        var buffer = new byte[81920];
        long read = 0;
        int n;

        // 速度估算：每秒采样一次
        long lastReportedBytes = 0;
        var lastReportTime = DateTime.UtcNow;

        // 先发一次初始进度
        progress?.Report(new DownloadProgress(0, total, total.HasValue ? 0 : null, null));

        while((n = await src.ReadAsync(buffer, ct)) > 0)
        {
            await dst.WriteAsync(buffer.AsMemory(0, n), ct);
            read += n;

            // 限流：每秒最多上报一次完整进度，避免 UI 抖动
            var now = DateTime.UtcNow;
            double elapsed = (now - lastReportTime).TotalSeconds;
            if(elapsed >= 0.5)
            {
                double speed = (read - lastReportedBytes) / elapsed;
                int? pct = total.HasValue && total.Value > 0
                    ? (int)(read * 100 / total.Value)
                    : null;
                progress?.Report(new DownloadProgress(read, total, pct, speed));
                lastReportedBytes = read;
                lastReportTime = now;
            }
        }

        // 最终进度
        int? finalPct = total.HasValue && total.Value > 0 ? 100 : null;
        progress?.Report(new DownloadProgress(read, total, finalPct, null));
        return true;
    }

    private static async Task<string> ComputeSha256Async(string file, CancellationToken ct)
    {
        using var sha = SHA256.Create();
        await using var fs = File.OpenRead(file);
        var hash = await sha.ComputeHashAsync(fs, ct);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private static string assetHint(string url)
    {
        try
        {
            var uri = new Uri(url);
            return Path.GetExtension(uri.LocalPath);
        }
        catch { return ".bin"; }
    }

    private static void TryDelete(string file)
    {
        try { if(File.Exists(file)) File.Delete(file); }
        catch { }
    }
}
