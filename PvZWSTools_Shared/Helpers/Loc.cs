using System;
using System.Collections.Generic;

namespace PvZWSTools_Shared.Helpers;

/// <summary>界面文案的语种开关。中文原文本身就是键：没有登记译文就退回中文，
/// 所以漏一句翻译只会露出中文，不会崩、也不会把键名抖给用户看。</summary>
public static partial class Loc
{
    public const string Zh = "zh";
    public const string En = "en";

    /// <summary>只在启动时设一次（切语言走重启，和 UI 模式同一条路）。
    /// 运行期不变，所以清单里那些"启动时拼好的标签"不需要监听语种变化。</summary>
    public static string Language { get; private set; } = Zh;

    public static bool IsEnglish => Language == En;

    public static void SetLanguage(string? language) =>
        Language = string.Equals(language, En, StringComparison.OrdinalIgnoreCase) ? En : Zh;

    /// <summary>取一条文案；中文原文就是查译文的键。
    /// ctx 用来隔开"同一个中文、两种词性"的情况（例：三态的 关闭=Off 与按钮的 关闭=Close），
    /// 查不到 ctx 前缀的条目就退回不带前缀的那条，所以绝大多数文案不用管它。</summary>
    public static string T(string zh, string? ctx = null)
    {
        if(Language == Zh)
            return zh;
        if(ctx != null && EnTable.TryGetValue(ctx + ":" + zh, out string? scoped))
            return scoped;
        return EnTable.TryGetValue(zh, out string? translated) ? translated : zh;
    }

    /// <summary>带占位符的整句：先取译文再格式化，所以两种语言里 {0} 的顺序必须一致
    /// （<c>UI核对/check_i18n.py</c> 就是钉这一条的）。</summary>
    public static string F(string format, params object?[] args) => string.Format(T(format), args);
}
