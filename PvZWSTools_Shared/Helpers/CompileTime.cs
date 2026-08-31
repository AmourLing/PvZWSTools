using System.Reflection;

namespace PvZWSTools_Shared.Helpers;

public static class CompileTime
{
    /// <summary>
    /// 获取程序集编译时间戳（须在项目文件中设置 BuildTimestamp 元数据）。
    /// </summary>
    /// <returns>成功则返回 DateTime，否则返回 null。</returns>
    public static DateTime? GetCompileTime()
    {
        try
        {
            var asm = Assembly.GetExecutingAssembly();
            var attr = asm.GetCustomAttribute<AssemblyMetadataAttribute>();

            if(attr?.Key == "BuildTimestamp" &&
                !string.IsNullOrWhiteSpace(attr.Value) &&
                DateTime.TryParse(attr.Value, out var buildTime))
            {
                return buildTime;
            }

            Log.Warning("BuildTimestamp metadata missing or invalid.");
            return null;
        }
        catch(Exception ex)
        {
            Log.Error($"Failed to read compile time: {ex.Message}");
            return null;
        }
    }
}
