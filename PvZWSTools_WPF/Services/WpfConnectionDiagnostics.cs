using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;

namespace PvZWSTools_WPF.Services;

/// <summary>本机连接现场的采集：一次 TCP 表读取，认出游戏进程和它实际占用的端口。
/// 用 iphlpapi 而不是 <see cref="IPGlobalProperties"/>，因为后者给不出 PID，
/// 而"这个端口被谁占着"正是本诊断要回答的那一半。</summary>
public sealed class WpfConnectionDiagnostics : IConnectionDiagnostics
{
    /// <summary>游戏进程名（不带 .exe）。名单来自本机那套模组的实际产物
    /// PlantGirlsVsZombies\Lawn.exe 与 Lawn.Console.exe，再加原版那两个叫法。
    /// 名单只用来"认领"进程：就算游戏 exe 被改过名没命中，
    /// 端口那一行照样把占用者的真名和 PID 抖出来，结论不会指到别处去。</summary>
    private static readonly string[] GameProcessNames = { "Lawn", "Lawn.Console", "PlantsVsZombies", "PvZ" };

    private const int AfInet = 2;
    private const int AfInet6 = 23;
    private const int TcpTableOwnerPidAll = 5;
    private const int TcpStateListen = 2;

    // v4 行：state@0 本地地址@4 本地端口@8 远程地址@12 远程端口@16 PID@20
    private const int RowSizeV4 = 24;
    // v6 行按本机 iphlpapi 实测偏移（打原始字节对着 netstat 核出来的，不是照抄文档结构体顺序）：
    // 本地地址[16]@0 本地scope@16 本地端口@20 远程地址[16]@24 远程scope@40 远程端口@44 state@48 PID@52
    private const int RowSizeV6 = 56;

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedTcpTable(IntPtr table, ref int size, bool sorted,
                                                   int addressFamily, int tableClass, uint reserved);

    public ConnectionSnapshot Capture()
    {
        var listeners = new List<ListenerInfo>();
        ReadTable(listeners, AfInet, RowSizeV4);
        ReadTable(listeners, AfInet6, RowSizeV6);

        return new ConnectionSnapshot
        {
            Listeners = listeners,
            GameProcesses = FindGameProcesses(),
            LocalAddresses = FindLocalAddresses(),
        };
    }

    /// <summary>本机自己的网卡地址。回环被别的进程精确绑走时，游戏那份全网卡监听
    /// 还能从这些地址落进去，所以自动指址要有这份备选。</summary>
    private static string[] FindLocalAddresses()
    {
        var found = new List<string>();
        try
        {
            foreach(var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if(nic.OperationalStatus != OperationalStatus.Up) continue;
                if(nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                foreach(var unicast in nic.GetIPProperties().UnicastAddresses)
                {
                    var ip = unicast.Address;
                    if(IPAddress.IsLoopback(ip)) continue;
                    // 链路本地地址（fe80::）带 zone 才能用，写进 ws:// 地址里没有意义。
                    if(ip.AddressFamily == AddressFamily.InterNetworkV6 && ip.IsIPv6LinkLocal) continue;
                    found.Add(ip.ToString());
                }
            }
        }
        catch(NetworkInformationException ex)
        {
            // 枚举网卡本身失败只是少了备选地址，不该把整份诊断带崩。
            Log.Error("网卡地址枚举失败", ex);
        }
        return found.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>显式点击才做的 bind 探测。表上说这个端口已经有人在听就不去 bind：
    /// 那个端口八成正是游戏的，抢下来只会造出一个新故障。</summary>
    public IReadOnlyList<string> ProbeBind(int port)
    {
        if(port < 1 || port > 65535)
            return new[] { $"端口占用探测：{port} 不是合法端口号" };

        var snapshot = Capture();
        var holders = snapshot.OnPort(port).ToList();
        if(holders.Count > 0)
        {
            return new[]
            {
                $"端口占用探测 {port}：已经有 " +
                string.Join("、", holders.Select(h => $"{h.Display} ← {h.ProcessName} (PID {h.ProcessId})")) +
                "，不做 bind 探测（抢走正在用的端口会造出新故障）。",
            };
        }

        TcpListener? listener = null;
        try
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.ExclusiveAddressUse = true;
            listener.Start();
            return new[]
            {
                $"端口占用探测 {port}：能绑定。端口没落在系统保留段里，" +
                "游戏起不了服务的话，问题在服务侧而不是端口侧。",
            };
        }
        catch(SocketException ex)
        {
            var why = ex.ErrorCode == ConnectFailure.WSAEACCES
                ? "空着却不给绑（10013）：这是系统端口保留段，或被安全软件拦住了。" +
                  "保留段可在命令行用 netsh int ipv4 show excludedportrange protocol=tcp 查。"
                : $"空着却不给绑（{ex.ErrorCode}：{ex.Message}）。";
            return new[] { $"端口占用探测 {port}：" + why };
        }
        finally
        {
            // 成对收回：探测留下的监听必须马上关掉，否则游戏随后绑不上它。
            try { listener?.Stop(); } catch { /* 已经关了或关不掉，探测结论已经出来了 */ }
        }
    }

    private static GameProcessInfo[] FindGameProcesses()
    {
        var found = new List<GameProcessInfo>();
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                if(Array.IndexOf(GameProcessNames, process.ProcessName) < 0) continue;
                found.Add(new GameProcessInfo { ProcessId = process.Id, ProcessName = process.ProcessName });
            }
            catch
            {
                // 进程刚好在这一步退出，或被保护到读不出名字。跳过它，不编造。
            }
            finally
            {
                process.Dispose();
            }
        }
        return found.ToArray();
    }

    private static void ReadTable(List<ListenerInfo> into, int addressFamily, int rowSize)
    {
        int size = 0;
        // 第一次调用必定失败，只为了拿缓冲区大小。
        GetExtendedTcpTable(IntPtr.Zero, ref size, true, addressFamily, TcpTableOwnerPidAll, 0);
        if(size <= 0) return;

        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            if(GetExtendedTcpTable(buffer, ref size, true, addressFamily, TcpTableOwnerPidAll, 0) != 0)
                return;

            int count = Marshal.ReadInt32(buffer);
            bool v6 = addressFamily == AfInet6;
            for(int i = 0; i < count; i++)
            {
                IntPtr row = IntPtr.Add(buffer, 4 + i * rowSize);
                if(Marshal.ReadInt32(row, v6 ? 48 : 0) != TcpStateListen) continue;

                string address;
                int port;
                if(v6)
                {
                    var bytes = new byte[16];
                    Marshal.Copy(IntPtr.Add(row, 0), bytes, 0, 16);
                    address = new IPAddress(bytes).ToString();
                    port = FromNetworkOrder(Marshal.ReadInt32(row, 20));
                }
                else
                {
                    uint raw = unchecked((uint)Marshal.ReadInt32(row, 4));
                    address = $"{raw & 0xFF}.{(raw >> 8) & 0xFF}.{(raw >> 16) & 0xFF}.{raw >> 24}";
                    port = FromNetworkOrder(Marshal.ReadInt32(row, 8));
                }

                int pid = Marshal.ReadInt32(row, v6 ? 52 : 20);
                into.Add(new ListenerInfo
                {
                    Address = address,
                    Port = port,
                    ProcessId = pid,
                    ProcessName = ProcessNameOf(pid),
                    IsIPv6 = v6,
                });
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>表里的端口是网络字节序放在低两字节，读出来的整数得交换一次。</summary>
    private static int FromNetworkOrder(int raw) =>
        ((raw & 0xFF) << 8) | ((raw >> 8) & 0xFF);

    private static string ProcessNameOf(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return process.ProcessName;
        }
        catch
        {
            // PID 在两次调用之间退出了。写"已退出"而不是留空，
            // 免得界面看起来像"有个没名字的程序占着"。
            return "已退出";
        }
    }
}
