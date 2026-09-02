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
    private const string GITHUB_API = "https://api.github.com/repos/{0}/{1}/releases/latest";
    private const string GITEE_API   = "https://gitee.com/api/v5/repos/{0}/{1}/releases/latest";

    private static readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    })
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    static UpdateService()
    {
        // GitHub API 要求 User-Agent，否则 403
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PvZWSTools-Updater/1.0");
    }

    /// <inheritdoc />
    public abstract Version CurrentVersion { get; }

    /// <inheritdoc />
    public abstract Task<bool> ApplyUpdateAsync(string downloadedFilePath);

    /// <inheritdoc />
    public async Task<UpdateInfo?> CheckForUpdatesAsync(string assetName, CancellationToken ct = default)
    {
        // 先 GitHub，失败回退 Gitee
        var info = await TryFetchAsync("github", GITHUB_API, assetName, ct);
        if(info != null) return info;

        info = await TryFetchAsync("gitee", GITEE_API, assetName, ct);
        return info;
    }

    /// <inheritdoc />
    public async Task<string?> DownloadUpdateAsync(UpdateInfo info, IProgress<int>? progress = null, CancellationToken ct = default)
    {
        if(info == null) throw new ArgumentNullException(nameof(info));

        var urls = new[] { info.DownloadUrl, info.DownloadUrlFallback }
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Distinct()
            .ToList();

        if(urls.Count == 0)
        {
            Log.Error("下载失败：UpdateInfo 中未提供任何下载地址");
            return null;
        }

        string tempFile = Path.Combine(Path.GetTempPath(), $"pvzwstools_update_{Guid.NewGuid():N}{Path.GetExtension(assetHint(urls[0]))}");
        Log.Info($"开始下载更新包到临时文件: {tempFile}");

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
            var info = ParseReleaseJson(json, source, assetName);
            if(info == null)
            {
                Log.Warning($"{source}：解析 JSON 失败或未找到匹配资产 {assetName}");
                return null;
            }

            Log.Info($"{source} 最新版本: tag={info.TagName}, url={info.DownloadUrl}");
            return info;
        }
        catch(Exception ex)
        {
            Log.Error($"{source} 检查更新异常: {ex.Message}");
            return null;
        }
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

        // 在 assets 中按文件名匹配（不区分大小写、忽略查询字符串）
        var assets = root["assets"] as JArray;
        if(assets == null) return info;

        string assetNameLower = assetName.ToLowerInvariant();
        foreach(JObject asset in assets.OfType<JObject>())
        {
            var name = asset.Value<string>("name");
            if(string.IsNullOrEmpty(name)) continue;
            if(!name.ToLowerInvariant().Equals(assetNameLower)) continue;

            info.DownloadUrl = asset.Value<string>("browser_download_url");
            info.Size = asset.Value<long?>("size");

            // Gitee 在资产对象上也可能提供 sha256，但字段名不统一，这里不强求
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

    private static async Task<bool> DownloadToFileAsync(string url, string targetFile, IProgress<int>? progress, CancellationToken ct)
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
        while((n = await src.ReadAsync(buffer, ct)) > 0)
        {
            await dst.WriteAsync(buffer.AsMemory(0, n), ct);
            read += n;
            if(total.HasValue && total.Value > 0)
            {
                int pct = (int)(read * 100 / total.Value);
                progress?.Report(pct);
            }
        }
        progress?.Report(100);
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
