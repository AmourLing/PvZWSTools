using System.IO;
using System.Reflection;
using System.Text;

namespace PvZWSTools_Shared.Helpers;

/// <summary>读编译进程序集的脚本（<c>PvZWSTools_Shared\Scripts\*.py</c>，见那里的 README 说明什么时候该用这种）。
/// 内嵌资源的全名带宿主根命名空间前缀，WPF 与安卓两边不一样，所以按文件名后缀找而不是写死全名。</summary>
public static class EmbeddedScript
{
    private static readonly object LockObj = new object();
    private static readonly Dictionary<string, string> _cache = new();

    public static string Read(string fileName)
    {
        lock(LockObj)
        {
            if(_cache.TryGetValue(fileName, out string? hit))
                return hit;

            var asm = Assembly.GetExecutingAssembly();
            string? full = Array.Find(asm.GetManifestResourceNames(),
                n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
            if(full == null)
                throw new InvalidOperationException(
                    $"程序集里没有内嵌脚本 {fileName}；现有资源：{string.Join(", ", asm.GetManifestResourceNames())}");

            using var stream = asm.GetManifestResourceStream(full)!;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            string text = reader.ReadToEnd();
            _cache[fileName] = text;
            return text;
        }
    }
}
