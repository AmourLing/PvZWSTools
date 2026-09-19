using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PvZWSTools_Shared.Helpers;

/// <summary>
/// 脚本与 PvZWSTools 之间经由 WebSocket 传递文件内容的编解码约定。
/// 游戏进程（IronPython 脚本）不直接读写磁盘，跨平台由本类在宿主应用的文件目录内落盘。
/// </summary>
public static class ScriptPayload
{
    /// <summary>取出一段 WebSocket 报文里 msg 字段的内容，不是 JSON 时原样返回。</summary>
    public static string ExtractMessage(string? rawMessage)
    {
        if(string.IsNullOrEmpty(rawMessage)) return string.Empty;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(rawMessage);
            if(doc.RootElement.TryGetProperty("msg", out var msgElement))
                return msgElement.GetString() ?? string.Empty;
        }
        catch(System.Text.Json.JsonException)
        {
        }
        return rawMessage;
    }

    /// <summary>取出成对标记之间的 Base64 载荷，标记缺失或内容为空时返回 null。</summary>
    public static string? ExtractBase64(string? output, string startMarker, string endMarker)
    {
        if(string.IsNullOrEmpty(output)) return null;

        int start = output.IndexOf(startMarker, StringComparison.Ordinal);
        int end = output.IndexOf(endMarker, StringComparison.Ordinal);
        if(start < 0 || end <= start) return null;

        // 载荷可能被 WebSocket 拆成多行，Base64 本身不含空白，直接剔除换行即可
        string raw = Regex.Replace(output[(start + startMarker.Length)..end], @"\s+", "");
        return raw.Length > 0 ? raw : null;
    }

    /// <summary>Base64 解码为 UTF-8 文本；输入不是合法 Base64 时按原文返回。</summary>
    public static string DecodeUtf8(string base64)
    {
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
        catch(FormatException)
        {
            return base64;
        }
    }

    /// <summary>UTF-8 文本编码为 Base64，供内联进脚本占位符。</summary>
    public static string EncodeUtf8(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    /// <summary>读取文本文件并编码为 Base64；文件不存在时返回 null。</summary>
    public static async Task<string?> ReadFileAsBase64Async(string path)
    {
        if(!File.Exists(path)) return null;
        return EncodeUtf8(await File.ReadAllTextAsync(path));
    }

    /// <summary>把 Base64 载荷解码后覆盖写入 directory/fileName，返回落盘的完整路径。</summary>
    public static async Task<string> WriteBase64ToAsync(string directory, string fileName, string base64)
    {
        if(!Directory.Exists(directory))
            _ = Directory.CreateDirectory(directory);

        string path = Path.Combine(directory, fileName);
        await File.WriteAllTextAsync(path, DecodeUtf8(base64));
        return path;
    }
}
