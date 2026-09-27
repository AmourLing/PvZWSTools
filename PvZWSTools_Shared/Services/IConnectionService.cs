namespace PvZWSTools_Shared.Services;

public interface IConnectionService
{
    bool IsConnected { get; }

    /// <summary>最近一次 ConnectAsync 抛出来的 Winsock 错误码，成功连接后归零。
    /// 只有码没有类型：SocketException 进不了安卓那边的异常形状，而分类只需要这个数。</summary>
    int LastSocketErrorCode { get; }

    event EventHandler<bool> ConnectionStateChanged;

    event EventHandler<string> ConnectionError;

    event EventHandler<string> MessageReceived;

    event EventHandler<string>? MessageSent;

    Task ConnectAsync(string address, CancellationToken cancellationToken = default);

    void Disconnect();

    Task SendAsync(string message);
}
