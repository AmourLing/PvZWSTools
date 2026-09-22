using System.Collections.Generic;

namespace PvZWSTools_Shared.Helpers;

/// <summary>English 译文表。键就是界面里的中文原文，所以本文件只放"翻成什么"，
/// 不改变任何调用点：中文模式下这里一条都不会被读到。
///
/// 有意留白的两类：出怪类型那 40 个绰号（普僵/白眼/天尸…是社区叫法，等定译再补）、
/// 以及经典界面（ClassicMainWindow 与 oldui 基准逐字节对齐，动它就破 UI核对 第 4 项）。
/// <c>UI核对/check_i18n.py</c> 会钉住：这里的键必须在源码里真实存在，且 {0} 占位符两侧一致。</summary>
public static partial class Loc
{
    private static readonly Dictionary<string, string> EnTable = new()
    {
        // ---------- 导航（标题同时是查页用的键，只在显示这一层翻译） ----------
        ["杂项"] = "Misc",
        ["关卡"] = "Level",
        ["资源"] = "Resources",
        ["植物"] = "Plants",
        ["僵尸"] = "Zombies",
        ["出怪"] = "Spawning",
        ["战场"] = "Board",
        ["挑战"] = "Challenge",
        ["阵型"] = "Formation",
        ["娱乐"] = "Fun",
        ["快捷脚本"] = "Scripts",
        ["花园"] = "Garden",
        ["收藏"] = "Favorites",
        ["控制台"] = "Console",

        // ---------- 状态词：显示文案，不是协议值（协议走 "0"/"1"/"2"） ----------
        ["开启"] = "On",
        ["关闭"] = "Off",
        ["默认"] = "Default",
        ["未知"] = "Unknown",
        ["开"] = "On",
        ["关"] = "Off",
        ["已选 {0}/{1}"] = "Selected {0}/{1}",

        // ---------- 功能行的通用件 ----------
        ["搜索功能…"] = "Search functions…",
        ["按名称搜索全部功能"] = "Search all functions by name",
        ["状态管理"] = "State manager",
        ["环境"] = "Environment",
        ["获取更新"] = "Check for updates",
        ["打开文件目录"] = "Open data folder",
        ["缩小界面"] = "Zoom out",
        ["放大界面"] = "Zoom in",
        ["点击切换：全部 / 只看日志 / 只看收发"] = "Click to switch: All / Log only / Traffic only",
        ["自动滚动"] = "Auto scroll",
        ["暂停"] = "Pause",
        ["清空"] = "Clear",
        ["发送"] = "Send",
        ["攒住当前画面不再刷新；行照样收着，取消暂停就能看到"] =
            "Freeze the view; lines are still collected and appear when you resume.",
        ["写 Python/IronPython 语句直接发给游戏；Ctrl+Enter 发送"] =
            "Send Python/IronPython statements straight to the game; Ctrl+Enter to send.",
        ["全部"] = "All",
        ["只看日志"] = "Log only",
        ["只看收发"] = "Traffic only",
        ["应用"] = "Apply",
        ["连接"] = "Connect",
        ["断开连接"] = "Disconnect",
        ["允许自动连接"] = "Allow auto connect",
        ["取消发送连接提醒"] = "Suppress connect notices",
        ["允许自动更新按钮状态"] = "Allow auto button-state refresh",
        ["启动时自动检查更新"] = "Check for updates on start",
        ["自动应用上次配置"] = "Reapply last state",
        ["界面语种"] = "Language",
        ["btn:关闭"] = "Close",          // 三态那个"关闭"是 Off，这里同字不同义
        ["取消"] = "Cancel",
        ["确定"] = "OK",
        ["设置"] = "Settings",
        ["设置已保存"] = "Settings saved",
        ["UI 风格"] = "UI style",
        ["主题（仅 NewUI 生效）"] = "Theme (NewUI only)",
        ["界面语言"] = "Language",
        ["黑夜"] = "Dark",
        ["白天"] = "Light",
        ["经典 UI"] = "Classic UI",
        ["UI 选择"] = "UI selection",
        ["密排：一行摆多个；取消：一个选项一行"] = "Compact: many per row; off: one option per row",
        ["清单里没有「{0}」这一页"] = "No \"{0}\" page in the catalog",
        ["还没有收藏，也还没点过任何功能。在任意一页的功能上长按即可收藏。"] =
            "Nothing favorited yet, and nothing tapped. Long-press a function on any page to favorite it.",
        ["收藏 / 取消收藏"] = "Favorite / unfavorite",
        ["密排"] = "Compact",
        ["脚本参数"] = "Script parameters",
        ["命中 {0} 个功能"] = "{0} functions matched",

        // ---------- 收藏页 ----------
        ["常用"] = "Frequent",
        ["点过的功能会自动按次数排到这里（最多 5 条）；一旦收藏就移上去，这里不再重复出现。"] =
            "Used functions are ranked here by how often you tap them (up to 5). " +
            "Once favorited a function moves up and stops repeating here.",
        ["还没有收藏。任意一行左侧点星标即可收藏；组里的子功能（大蒜、黄油这类）可以先搜出来再收藏。"] =
            "Nothing favorited yet. Tap the star on the left of any row; sub-functions inside a group " +
            "(garlic, butter, ...) can be searched for first and then favorited.",

        // ---------- 出怪页（先做这一页当样板；40 个僵尸绰号留给定译） ----------
        ["出怪类型"] = "Spawn types",
        ["暂停出怪"] = "Stop spawning",
        ["最大密度"] = "Max density",
        ["蹦极处理"] = "Bungee handling",
        ["红眼处理"] = "Red-eye handling",
        ["下一波"] = "Next wave",
        ["极限出怪测试"] = "Spawn stress test",
        ["同步出怪列表"] = "Sync spawn list",
        ["打印场上僵尸"] = "Print zombies on lawn",
        ["波次出怪(数量)"] = "Wave spawn (count)",
        ["波次出怪(序号)"] = "Wave spawn (index)",
        ["载入已存波次表"] = "Load saved wave table",
        ["刷新出怪血量"] = "Refresh spawn health",
        ["{0}：点一下切这个类型的出怪开关"] = "{0}: tap to toggle this type's spawning",
    };
}
