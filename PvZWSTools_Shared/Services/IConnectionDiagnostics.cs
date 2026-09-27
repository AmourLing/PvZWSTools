#nullable enable

namespace PvZWSTools_Shared.Services;

/// <summary>连接现场的采集入口。只有 Windows 有实现，安卓端拿到的是 null，
/// 于是分类那一层整体跳过——宁可少说，也不能拿一份空快照编出"游戏没开"这种结论。</summary>
public interface IConnectionDiagnostics
{
    ConnectionSnapshot Capture();

    /// <summary>显式点击才做的 bind 探测。端口上已经有人在听就不去 bind，
    /// 免得把游戏正在用的端口抢下来、反过来造出一个新故障。</summary>
    IReadOnlyList<string> ProbeBind(int port);
}
