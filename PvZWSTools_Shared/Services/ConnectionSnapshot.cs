using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace PvZWSTools_Shared.Services;

/// <summary>一条 TCP 监听记录。地址是规范化后的纯文本（127.0.0.1 / ::1 / 0.0.0.0 / 192.168.1.5）。</summary>
public sealed class ListenerInfo
{
    public string Address { get; init; } = string.Empty;
    public int Port { get; init; }
    public int ProcessId { get; init; }
    public string ProcessName { get; init; } = string.Empty;
    public bool IsIPv6 { get; init; }

    /// <summary>0.0.0.0 / :: 这种"所有地址"监听，从回环连过去也连得上。</summary>
    public bool IsWildCard => Address == "0.0.0.0" || Address == "::";

    /// <summary>只绑在某个局域网地址上时，本机拿 127.0.0.1 连不上去。</summary>
    public bool ReachableFromLocalhost => IsWildCard || Address == "127.0.0.1" || Address == "::1";

    /// <summary>IPv6 带方括号，跟 ws:// 地址里的写法一致，免得用户看不出两处为什么不同。</summary>
    public string Display => IsIPv6 ? $"[{Address}]:{Port}" : $"{Address}:{Port}";
}

/// <summary>本机上一个游戏进程。只留名字和 PID：取完整路径要开进程句柄，
/// 游戏以管理员身份跑时会抛，而那一条对本诊断没用。</summary>
public sealed class GameProcessInfo
{
    public int ProcessId { get; init; }
    public string ProcessName { get; init; } = string.Empty;
}

/// <summary>一次采集得到的连接现场。纯数据、不碰系统 API，
/// 所以 ScriptVerifyHost 能手工搓一份出来断言分类结论。</summary>
public sealed class ConnectionSnapshot
{
    private HashSet<int>? _gamePids;

    public IReadOnlyList<ListenerInfo> Listeners { get; init; } = Array.Empty<ListenerInfo>();
    public IReadOnlyList<GameProcessInfo> GameProcesses { get; init; } = Array.Empty<GameProcessInfo>();

    /// <summary>这台机器自己的网卡地址（不含回环）。回环被别的进程精确绑走时，
    /// 全网卡监听仍然可以从这些地址落进去，所以它们也算"本机"。</summary>
    public IReadOnlyList<string> LocalAddresses { get; init; } = Array.Empty<string>();

    public bool HasGameProcess => GameProcesses.Count > 0;

    public string GameProcessSummary => HasGameProcess
        ? string.Join("、", GameProcesses.Select(g => $"{g.ProcessName} (PID {g.ProcessId})"))
        : "未找到";

    public bool IsGameProcess(int processId) =>
        (_gamePids ??= new HashSet<int>(GameProcesses.Select(g => g.ProcessId))).Contains(processId);

    public IEnumerable<ListenerInfo> OnPort(int port) => Listeners.Where(l => l.Port == port);

    /// <summary>游戏名下的全部监听，不限端口。它比"猜端口"权威：
    /// 进程在但这里为空，就是服务没起来（或它的 bind 被挡了），而不是"端口选错了"。</summary>
    public IEnumerable<ListenerInfo> OfGameProcesses() => Listeners.Where(l => IsGameProcess(l.ProcessId));
}
