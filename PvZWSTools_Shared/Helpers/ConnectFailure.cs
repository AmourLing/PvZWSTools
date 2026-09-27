using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PvZWSTools_Shared.Services;

#nullable enable

namespace PvZWSTools_Shared.Helpers;

/// <summary>同一趟断线里诊断只说一次。自动连接是 1 秒一趟、每 3 趟换一个候选地址，
/// 所以去重必须按"原因"而不是按文案：换端口不算新信息，按文案去重照样三秒刷一次屏。</summary>
public sealed class FailureAnnouncer
{
    private ConnectFailureKind? _last;

    /// <summary>这一趟该不该报。原因变了才报；成功连上后 Reset，下一次断线重新算一趟。</summary>
    public bool ShouldReport(ConnectFailureKind kind)
    {
        if(_last == kind) return false;
        _last = kind;
        return true;
    }

    public void Reset() => _last = null;
}

/// <summary>连不上时能机器判定的那几种原因。种类是从"现场证据"倒推出来的，
/// 不是照着 Windows 错误码表抄的：同一个 10061 背后，游戏没开、端口被抢、
/// 绑错网卡是三件不同的事，给的下一步动作也完全不同。</summary>
public enum ConnectFailureKind
{
    /// <summary>目标端口上有游戏在监听，且从当前地址连得过去。</summary>
    TargetAvailable,
    /// <summary>目标端口上有游戏在监听，但绑的地址从当前地址连不到。</summary>
    BindAddressMismatch,
    /// <summary>目标端口被非游戏进程占着。</summary>
    PortTakenByOther,
    /// <summary>游戏在这个端口上，但别的进程更具体地绑住了本机要连的那个地址，连接会被它截走。</summary>
    InterceptedByOtherBind,
    /// <summary>游戏进程在，但监听的不是这个端口。</summary>
    WrongPort,
    /// <summary>游戏进程在，可它一个端口都没监听。</summary>
    GameNotListening,
    /// <summary>没有游戏进程，这个端口上也没人听。</summary>
    GameNotRunning,
    /// <summary>10013，请求被系统层拒绝。</summary>
    PermissionDenied,
    /// <summary>10060 / 10051 / 10065，地址不可达。</summary>
    Unreachable,
    /// <summary>归不进去的，原样把错误带出来。</summary>
    Other,
}

/// <summary>一份诊断结论：一句给用户看的，多行进控制台留证据。</summary>
public sealed class DiagnosisReport
{
    public ConnectFailureKind Kind { get; init; }
    public string Conclusion { get; init; } = string.Empty;
    public IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();
}

/// <summary>把 socket 错误码 + 本机连接现场 归成一条能行动的结论。
/// 纯函数：不碰系统 API，快照由调用方喂进来，所以能在 ScriptVerifyHost 里离线断言。</summary>
public static class ConnectFailure
{
    // Winsock 错误码，只在"这一码对应哪种现场"时用，不参与别的判断。
    public const int WSAECONNREFUSED = 10061;
    public const int WSAEACCES = 10013;
    public const int WSAETIMEDOUT = 10060;
    public const int WSAENETUNREACH = 10051;
    public const int WSAEHOSTUNREACH = 10065;

    /// <summary>ws://localhost:8080/Py → host=localhost, port=8080。
    /// 只切字符串、不做 DNS：一做 DNS 这函数就不可离线测了，而且解析失败本身就是下面要报的症状。</summary>
    public static bool TryParseTarget(string? address, out string host, out int port)
    {
        host = string.Empty;
        port = 0;
        if(string.IsNullOrWhiteSpace(address)) return false;

        var s = address.Trim();
        // 没有方案就不是地址，是一句被打断的中文或手抖的输入。放它过去的话，
        // 整句会被当成主机名、端口取默认 80，诊断于是开始回答一个不存在的问题。
        var schemeEnd = s.IndexOf("://", StringComparison.Ordinal);
        if(schemeEnd < 0) return false;
        s = s.Substring(schemeEnd + 3);

        var slash = s.IndexOf('/');
        if(slash >= 0) s = s.Substring(0, slash);
        if(s.Length == 0) return false;

        // [::1]:8080 这种带方括号的先单独走，否则按最后一个冒号切会把 IPv6 切坏。
        if(s.StartsWith("[", StringComparison.Ordinal))
        {
            var close = s.IndexOf(']');
            if(close <= 0) return false;
            host = s.Substring(0, close + 1);
            var rest = s.Substring(close + 1);
            if(rest.StartsWith(":", StringComparison.Ordinal))
            {
                if(!int.TryParse(rest.Substring(1), out port) || port <= 0 || port > 65535) return false;
            }
            else port = 80;
            return true;
        }

        var colon = s.LastIndexOf(':');
        if(colon < 0)
        {
            host = s;
            port = 80;
            return true;
        }

        host = s.Substring(0, colon);
        if(!int.TryParse(s.Substring(colon + 1), out port) || port <= 0 || port > 65535) return false;
        return host.Length > 0;
    }

    /// <summary>只换端口、方案与路径原样留着：ws://localhost:8080/Py → ws://localhost:8081/Py。
    /// 不重建整串，免得把用户手填的路径尾巴弄丢。</summary>
    public static string ReplacePort(string address, int port)
    {
        if(!TryParseTarget(address, out var host, out _)) return address;
        return WithHostAndPort(address, host, port);
    }

    /// <summary>换主机与端口、留着方案和路径。host 里的 IPv6 要自带方括号。</summary>
    public static string WithHostAndPort(string address, string host, int port)
    {
        if(!TryParseTarget(address, out _, out _)) return address;
        var s = address!.Trim();
        var schemeEnd = s.IndexOf("://", StringComparison.Ordinal);
        var prefix = s.Substring(0, schemeEnd + 3);
        var authority = s.Substring(schemeEnd + 3);
        var slash = authority.IndexOf('/');
        var suffix = slash >= 0 ? authority.Substring(slash) : string.Empty;
        return $"{prefix}{host}:{port}{suffix}";
    }

    /// <summary>要让连接落到这条监听上，主机那一栏该写什么；写什么都落不到它上时返回 null。
    /// 全网卡监听在 Windows 上会被同端口的精确回环绑定顶掉（见 <see cref="InterceptsLoopback"/>），
    /// 但同一个端口上那两个服务是靠目标地址分开的：换本机的网卡地址照样能落到游戏那份上。</summary>
    public static string? HostForListener(ListenerInfo listener, ConnectionSnapshot snapshot)
    {
        if(!listener.IsWildCard)
        {
            // 绑在局域网地址上的，本机照那个地址也连得到；回环地址直接照抄。
            return listener.IsIPv6 && listener.ReachableFromLocalhost
                ? $"[{listener.Address}]"
                : listener.Address;
        }

        var takenByOthers = snapshot.OnPort(listener.Port)
            .Where(l => !l.IsWildCard)
            .Select(l => l.Address)
            .ToList();
        var loopback = listener.IsIPv6 ? "::1" : "127.0.0.1";
        if(!takenByOthers.Contains(loopback, StringComparer.OrdinalIgnoreCase))
            return listener.IsIPv6 ? "[::1]" : loopback;

        // 回环被人占了：本机其它网卡地址同样落得到这个全网卡监听，挑一个没被同样占走的、且同族的。
        foreach(var address in snapshot.LocalAddresses)
        {
            if(IsIPv6Text(address) != listener.IsIPv6) continue;
            if(takenByOthers.Contains(address, StringComparer.OrdinalIgnoreCase)) continue;
            return listener.IsIPv6 ? $"[{address}]" : address;
        }
        return null;
    }

    private static bool IsIPv6Text(string address)
        => IPAddress.TryParse(StripBrackets(address), out var ip)
           && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;

    /// <summary>游戏此刻真正连得到的地址；不需要改（或本机根本没有游戏在听）就返回 null。
    /// 端口从小到大试，但**当前端口优先**，免得把一个已经指着游戏的好地址改走。
    /// 写死的跨机地址一律不改：本机的表说什么都管不着别的机器。</summary>
    public static string? BestAddressFor(ConnectionSnapshot snapshot, string currentAddress,
                                         ICollection<int> skippedPorts)
    {
        if(snapshot == null || !TryParseTarget(currentAddress, out var host, out var currentPort)) return null;
        if(!IsLocalTarget(host, snapshot)) return null;

        var candidates = snapshot.OfGameProcesses()
            .Where(l => !skippedPorts.Contains(l.Port))
            .OrderBy(l => l.Port == currentPort ? 0 : 1)
            .ThenBy(l => l.Port)
            .ToList();

        foreach(var listener in candidates)
        {
            var hostText = HostForListener(listener, snapshot);
            if(hostText == null) continue;
            var best = WithHostAndPort(currentAddress, hostText, listener.Port);
            return best == currentAddress ? null : best;
        }
        return null;
    }

    /// <summary>这台机器上"本机自己"的几种写法。防火墙只在不是本机时才有资格被提出来。</summary>
    public static bool IsLocalTarget(string host)
    {
        if(string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)) return true;
        return IPAddress.TryParse(StripBrackets(host), out var ip) && IPAddress.IsLoopback(ip);
    }

    /// <summary>带上现场快照的版本：这张机器自己的网卡地址同样是本机。
    /// 自动指址在回环被占走时会写到那类地址上，不带快照就会被当成"别的机器"。</summary>
    public static bool IsLocalTarget(string host, ConnectionSnapshot? snapshot)
    {
        if(IsLocalTarget(host)) return true;
        var bare = StripBrackets(host);
        return snapshot != null &&
               snapshot.LocalAddresses.Any(a => string.Equals(a, bare, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsIPv6Literal(string host)
    {
        var bare = StripBrackets(host);
        return IPAddress.TryParse(bare, out var ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
    }

    /// <summary>这条监听记录，从 host 这个地址出发连不连得上。
    /// localhost 交给系统逐个试解析出来的地址，所以两族都算；写成字面 IP 的必须同族。</summary>
    public static bool ReachableFrom(string host, ListenerInfo listener)
    {
        if(!listener.ReachableFromLocalhost) return false;
        if(string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)) return true;
        if(!IPAddress.TryParse(StripBrackets(host), out var ip)) return false;

        var v6 = ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
        if(v6 != listener.IsIPv6) return false;
        return listener.IsWildCard || string.Equals(StripBrackets(host), listener.Address,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>本机 connect 到某个回环地址时，Windows 把它交给"绑得最具体"的那个 socket。
    /// 0.0.0.0:8080 与 127.0.0.1:8080 能同时绑住（两边都成功），而连 127.0.0.1 的那一趟由后者接走，
    /// 前者一无所获——所以一个第三方进程精确绑住回环，就能把工具的连接整个截走。
    /// 这条不是推理，是在本机 18080 上实测出来的（<c>.workbuddy\tmp\bindpref\run.py</c>：
    /// 两边都 BOUND，specific 那份 ACCEPTED、wildcard 那份 NOACCEPT）。</summary>
    public static bool InterceptsLoopback(ListenerInfo listener, string host)
    {
        if(listener.IsWildCard) return false;
        if(string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
            return listener.Address == "127.0.0.1" || listener.Address == "::1";
        return IPAddress.TryParse(StripBrackets(host), out var ip) && IPAddress.IsLoopback(ip)
               && string.Equals(listener.Address, ip.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public static ConnectFailureKind Classify(int socketErrorCode, ConnectionSnapshot snapshot,
                                              string host, int port)
    {
        if(snapshot == null) throw new ArgumentNullException(nameof(snapshot));

        var onPort = snapshot.OnPort(port).ToList();
        var gameOnPort = onPort.Where(l => snapshot.IsGameProcess(l.ProcessId)).ToList();

        // 有比游戏更具体的回环绑定时，本机这一趟根本到不了游戏。
        // 只剩截胡者自己（端口上没有别的服务）就不算截胡，那是明明白白的占用。
        var interceptors = onPort.Where(l => !snapshot.IsGameProcess(l.ProcessId)
                                             && InterceptsLoopback(l, host)).ToList();
        var intercepted = interceptors.Count > 0 && onPort.Count > interceptors.Count;

        if(socketErrorCode == WSAEACCES) return ConnectFailureKind.PermissionDenied;
        if(socketErrorCode is WSAETIMEDOUT or WSAENETUNREACH or WSAEHOSTUNREACH)
            return ConnectFailureKind.Unreachable;
        if(intercepted) return ConnectFailureKind.InterceptedByOtherBind;

        // 其余非零码（10054 连接被重置这类）恰恰说明当时对面有人，拿现场证据去判成"游戏没开"是反的，
        // 所以只有"明确没人应答"(10061) 和"没有错误码、纯粹看一眼现场"继续往下走证据链。
        if(socketErrorCode != 0 && socketErrorCode != WSAECONNREFUSED)
            return ConnectFailureKind.Other;

        if(gameOnPort.Count > 0)
            return gameOnPort.Any(l => ReachableFrom(host, l))
                ? ConnectFailureKind.TargetAvailable
                : ConnectFailureKind.BindAddressMismatch;

        if(onPort.Count > 0) return ConnectFailureKind.PortTakenByOther;
        if(!snapshot.HasGameProcess) return ConnectFailureKind.GameNotRunning;
        return snapshot.OfGameProcesses().Any()
            ? ConnectFailureKind.WrongPort
            : ConnectFailureKind.GameNotListening;
    }

    /// <summary>出报告。rawError 是上层拿到的原始异常文字，只在归不进任何一类时才露出来。</summary>
    public static DiagnosisReport Diagnose(int socketErrorCode, ConnectionSnapshot snapshot,
                                           string address, string rawError = "")
    {
        if(!TryParseTarget(address, out var host, out var port))
            return new DiagnosisReport
            {
                Kind = ConnectFailureKind.Other,
                Conclusion = $"地址写法看不懂：{address}",
                Lines = new[] { $"连接诊断 {address}", "结论：地址写法看不懂" },
            };

        var kind = Classify(socketErrorCode, snapshot, host, port);
        // 拿到了错误码才叫"这次连接失败"；没有码是用户主动点的体检，措辞得跟着变。
        var hasError = socketErrorCode != 0 || !string.IsNullOrEmpty(rawError);
        var conclusion = Describe(kind, snapshot, host, port, socketErrorCode, hasError, rawError);
        var lines = BuildLines(kind, snapshot, address, host, port, socketErrorCode, conclusion, rawError);
        return new DiagnosisReport { Kind = kind, Conclusion = conclusion, Lines = lines };
    }

    private static string Describe(ConnectFailureKind kind, ConnectionSnapshot snapshot,
                                   string host, int port, int code, bool hasError, string rawError)
    {
        var gameOnPort = snapshot.OnPort(port).Where(l => snapshot.IsGameProcess(l.ProcessId)).ToList();

        switch(kind)
        {
            case ConnectFailureKind.TargetAvailable:
                return hasError
                    ? $"{gameOnPort.First().Display} 确实是游戏在听，失败不在端口这一层：多半是服务端刚起来就断了，或者握手被掐。"
                    : $"游戏正在 {gameOnPort.First().Display} 上监听，这个地址对得上。";

            case ConnectFailureKind.InterceptedByOtherBind:
            {
                var thief = snapshot.OnPort(port).First(l =>
                    !snapshot.IsGameProcess(l.ProcessId) && InterceptsLoopback(l, host));
                var game = snapshot.OnPort(port).FirstOrDefault(l => snapshot.IsGameProcess(l.ProcessId));
                return game != null
                    ? $"{port} 上游戏绑的是 {game.Display}，但 {thief.ProcessName} (PID {thief.ProcessId}) " +
                      $"更具体地绑在 {thief.Display}：本机连过去落到的是它，不是游戏。关掉那个程序，或让游戏换一个没被占的端口。"
                    : $"{port} 上 {thief.ProcessName} (PID {thief.ProcessId}) 精确绑在 {thief.Display}，" +
                      "游戏绑的是别的网卡地址，本机这个地址连不到它。";
            }

            case ConnectFailureKind.BindAddressMismatch:
                return $"游戏在听 {gameOnPort.First().Display}，绑的不是本机地址，从 {host} 连不过去。改用它绑的那个地址。";

            case ConnectFailureKind.PortTakenByOther:
            {
                var t = snapshot.OnPort(port).First();
                return $"{port} 已经被 {t.ProcessName} (PID {t.ProcessId}) 占着，游戏绑不上它。先关掉那个程序，或让游戏换个端口。";
            }

            case ConnectFailureKind.WrongPort:
            {
                var list = string.Join("、", snapshot.OfGameProcesses().Select(l => l.Display).Distinct());
                return $"游戏在监听 {list}，不是 {port}。把地址改过去。";
            }

            case ConnectFailureKind.GameNotListening:
                return "游戏进程在，但没有监听任何端口：内置服务没起来，或者它的 bind 被挡住了。用端口占用探测确认这个端口能不能绑。";

            case ConnectFailureKind.GameNotRunning:
                return "没找到游戏进程，这个端口上也没有监听。请先打开游戏。";

            case ConnectFailureKind.PermissionDenied:
                return IsLocalTarget(host, snapshot)
                    ? $"连本机都被拒（{code}）。本机回环不走 Windows 防火墙，这一类通常是端口落在系统保留段，或被安全软件插手了。"
                    : $"连 {host} 被拒（{code}）。这多半不是端口问题，是本机出站被拦。";

            case ConnectFailureKind.Unreachable:
                return IsLocalTarget(host, snapshot)
                    ? $"连本机都超时（{code}）。这通常不是防火墙，而是安全软件拦了本机连接。"
                    : $"{host} 不可达（{code}）。跨机连接才需要看对端防火墙有没有放行入站，以及游戏绑的是不是 0.0.0.0。";

            default:
                return code == 0
                    ? $"没归类出来：{rawError}"
                    : $"没归类出来（错误 {code}）：{RawOr(rawError)}";
        }
    }

    private static string RawOr(string rawError) => string.IsNullOrEmpty(rawError) ? "无原始错误文字" : rawError;

    private static string[] BuildLines(ConnectFailureKind kind, ConnectionSnapshot snapshot, string address,
                                       string host, int port, int code, string conclusion, string rawError)
    {
        var gameListeners = snapshot.OfGameProcesses().Select(l => l.Display).Distinct().ToList();
        var onPort = snapshot.OnPort(port).Select(l => $"{l.Display} ← {l.ProcessName} (PID {l.ProcessId})").ToList();

        var lines = new List<string>
        {
            $"连接诊断 {address}" + (code != 0 ? $" 错误码 {code}" : ""),
            "结论：" + conclusion,
            "游戏进程：" + snapshot.GameProcessSummary,
            "游戏监听：" + (gameListeners.Count > 0 ? string.Join("、", gameListeners) : "无"),
            $"端口 {port} 上的监听：" + (onPort.Count > 0 ? string.Join("；", onPort) : "无"),
        };
        if(!string.IsNullOrEmpty(rawError) && kind == ConnectFailureKind.Other)
            lines.Add("原始错误：" + rawError);
        return lines.ToArray();
    }

    private static string StripBrackets(string host) =>
        host.Length > 1 && host[0] == '[' && host[^1] == ']' ? host[1..^1] : host;
}
