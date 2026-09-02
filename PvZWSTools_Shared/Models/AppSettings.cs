using System.Linq;
using System.Reflection;

namespace PvZWSTools_Shared.Models;

public class AppSettings
{
    /// <summary>
    /// 允许自动连接
    /// </summary>
    [Setting("允许自动连接")]
    public bool AutoConnectEnabled { get; set; }

    /// <summary>
    /// 取消发送连接提醒
    /// </summary>
    [Setting("取消发送连接提醒")]
    public bool SuppressConnectionMessage { get; set; }

    /// <summary>
    /// 允许自动更新按钮状态（切换界面时自动发送脚本刷新按钮开关状态）
    /// </summary>
    [Setting("允许自动更新按钮状态")]
    public bool AllowAutoUpdateButtonStatus { get; set; }

    /// <summary>
    /// 启动时自动检查新版本（仅检查并提示，不自动下载）。
    /// </summary>
    [Setting("启动时自动检查更新")]
    public bool AutoCheckUpdateEnabled { get; set; } = true;

    /// <summary>
    /// 通过反射自动发现所有带 [Setting] 特性的布尔设置项。
    /// 新增设置项时无需手动修改日志与设置对话框代码（自动注册）。
    /// </summary>
    public static IReadOnlyList<PropertyInfo> SettingProperties { get; } =
        typeof(AppSettings)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(bool) && p.GetCustomAttribute<SettingAttribute>() != null)
            .ToArray();
}
