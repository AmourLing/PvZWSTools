using System;
using System.Windows.Markup;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_WPF.Themes;

/// <summary>XAML 里取界面文案：<c>{loc:Loc 常用}</c>。中文原文就是键，没登记译文就退回中文。
/// 切语言走重启（和 UI 模式同一条路），所以加载时定死字面值就够，不需要 DynamicResource。</summary>
[MarkupExtensionReturnType(typeof(string))]
public sealed class LocExtension : MarkupExtension
{
    private readonly string _zh;

    public LocExtension(string zh) => _zh = zh;

    /// <summary>同一个中文在不同词性下有不同译法时用（例：三态的 关闭=Off 与按钮的 关闭=Close）。</summary>
    public string? Ctx { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => Loc.T(_zh, Ctx);
}
