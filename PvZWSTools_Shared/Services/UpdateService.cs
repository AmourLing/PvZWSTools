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
public abstract class UpdateService:IUpdateService
{
    private const string GITHUB_API = "https://api.github.com/repos/{0}/{1}/releases?per_page=50";
    private const string GITEE_API = "https://gitee.com/api/v5/repos/{0}/{1}/releases?per_page=50";

    private const string SourceGithub = "github";
    private const string SourceGitee = "gitee";

    /// <summary>
    /// 单个源的检查超时（秒）。GitHub 不通时往往是静默丢包而不是立刻 reset，
    /// 不能让它拖住整个检查——下载仍走 <see cref="_httpClient"/> 的 5 分钟超时。
    /// </summary>
    private const int SourceTimeoutSeconds = 10;

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
        // 两源并行查、各自带短超时：访问不了 GitHub 的用户不必等 GitHub 超时才轮到 Gitee
        var githubTask = TryFetchAsync(SourceGithub, GITHUB_API, assetName, ct);
        var giteeTask = TryFetchAsync(SourceGitee, GITEE_API, assetName, ct);
        _ = await Task.WhenAll(githubTask, giteeTask);

        var github = githubTask.Result;
        var gitee = giteeTask.Result;

        // 版本信息（tag / 更新说明）取更新的源；同版本时保留 GitHub，Gitee 只是镜像
        var primary = NewerOf(github, gitee);
        if(primary == null) return null;

        // 直链按源分开存，UI 才能把渠道标签对上。镜像同步有延迟时两源版本可能不同，
        // 不同版本的直链不能混用，所以只在同版本时借用另一源的直链。
        var other = ReferenceEquals(primary, github) ? gitee : github;
        if(other != null && string.Equals(other.TagName, primary.TagName, StringComparison.Ordinal))
        {
            primary.GithubUrl ??= other.GithubUrl;
            primary.GiteeUrl ??= other.GiteeUrl;
        }

        return primary;
    }

    /// <summary>取版本更新的 <see cref="UpdateInfo" />；同版本或无法解析时保留 <paramref name="a" />。</summary>
    private static UpdateInfo? NewerOf(UpdateInfo? a, UpdateInfo? b)
    {
        if(a == null) return b;
        if(b == null) return a;

        var pa = a.Parsed ?? a.ParseTag();
        var pb = b.Parsed ?? b.ParseTag();
        if(pa == null) return b;
        if(pb == null) return a;

        return CompareParsed(pb, pa) > 0 ? b : a;
    }

    /// <inheritdoc />
    public async Task<string?> DownloadUpdateAsync(UpdateInfo info, IProgress<DownloadProgress>? progress = null, CancellationToken ct = default)
    {
        if(info == null) throw new ArgumentNullException(nameof(info));

        // 用户选的渠道优先（由 info.Source 表达）；没指定时 Gitee 优先（国内快）
        bool giteeFirst = info.Source != SourceGithub;
        string? first = giteeFirst ? info.GiteeUrl : info.GithubUrl;
        string? second = giteeFirst ? info.GithubUrl : info.GiteeUrl;

        var urls = new List<string>();
        if(!string.IsNullOrWhiteSpace(first))
            urls.Add(first);
        if(!string.IsNullOrWhiteSpace(second) && second != first)
            urls.Add(second);

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
        var (owner, repo) = source == SourceGithub
            ? (Sharedstring.GitHubOwner, Sharedstring.GitHubRepo)
            : (Sharedstring.GiteeOwner, Sharedstring.GiteeRepo);

        string url = string.Format(apiTemplate, owner, repo);
        Log.Info($"检查更新（{source}）: {url}");

        // 单源短超时：外部 ct 取消时一起取消，超时只放弃这一个源
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(SourceTimeoutSeconds));

        try
        {
            using var resp = await _httpClient.GetAsync(url, timeoutCts.Token);
            if(!resp.IsSuccessStatusCode)
            {
                Log.Warning($"{source} API 返回 {(int)resp.StatusCode} {resp.ReasonPhrase}");
                return null;
            }

            string json = await resp.Content.ReadAsStringAsync(timeoutCts.Token);

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
        catch(OperationCanceledException) when(!ct.IsCancellationRequested)
        {
            Log.Warning($"{source}：{SourceTimeoutSeconds}s 内无响应，跳过该源");
            return null;
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

        // 在 assets 中按文件名匹配（忽略大小写与 -/_ 差异、忽略查询字符串）
        // GitHub: assets[].browser_download_url
        // Gitee:  attach_files[].download_url
        var assets = root["assets"] as JArray ?? root["attach_files"] as JArray;
        if(assets == null) return info;

        string assetKey = NormalizeAssetName(assetName);
        foreach(JObject asset in assets.OfType<JObject>())
        {
            var name = asset.Value<string>("name");
            if(string.IsNullOrEmpty(name)) continue;
            if(NormalizeAssetName(name) != assetKey) continue;

            // GitHub: browser_download_url；Gitee: download_url
            string? url = asset.Value<string>("browser_download_url")
                       ?? asset.Value<string>("download_url");
            if(source == SourceGitee)
                info.GiteeUrl = url;
            else
                info.GithubUrl = url;

            info.Size = asset.Value<long?>("size");

            var digest = asset.Value<string>("digest");
            if(!string.IsNullOrWhiteSpace(digest))
                info.Sha256 = ParseSha256(digest);
            break;
        }

        // GitHub 没有匹配的 asset，但更新包可能存在同名 .sha256 文件，留作扩展点
        return info;
    }

    /// <summary>
    /// 资产名在不同 release 之间漂移过（PvZWSTools-win.zip / PvZWSTools_windows_setup.exe、
    /// .apk / .APK），比较时统一大小写和分隔符。
    /// </summary>
    private static string NormalizeAssetName(string name) =>
        name.Trim().ToLowerInvariant().Replace('_', '-');

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
