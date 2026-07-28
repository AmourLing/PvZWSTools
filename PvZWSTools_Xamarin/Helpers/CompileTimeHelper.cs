using System;

namespace PvZWSTools_Xamarin.Helpers;

public static class CompileTimeHelper
{
    public static DateTime? GetCompileTime()
    {
        try
        {
            string timeStr = BuildInfo.CompileTime;   // 常量字符串
            if(DateTime.TryParse(timeStr, out var buildTime))
            {
                return buildTime.ToLocalTime();       // 转为本地时间
            }
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("CompileTime", $"读取编译时间失败: {ex.Message}");
        }
        return null;
    }
}
