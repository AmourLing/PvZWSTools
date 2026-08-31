using System;

namespace PvZWSTools_Shared.Models;

/// <summary>
/// 标记 AppSettings 中需要在"设置对话框"显示、并在保存/加载时记录日志的布尔设置项。
/// 通过反射自动注册：新增设置项时只需给属性加上此特性即可，无需手动修改日志与对话框代码。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SettingAttribute:Attribute
{
    /// <summary>设置项在界面与日志中显示的名称</summary>
    public string Label { get; }

    public SettingAttribute(string label)
    {
        Label = label;
    }
}
