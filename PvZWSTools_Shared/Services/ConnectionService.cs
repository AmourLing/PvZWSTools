using System.Net.Sockets;
using PvZWSTools_Shared.Helpers;
using WebSocketSharp.NetCore;

namespace PvZWSTools_Shared.Services;

public class ConnectionService:IConnectionService, IDisposable
{
    private WebSocket _ws;
    private readonly IUiThreadInvoker _uiThread;
    private CancellationTokenSource _cts;
    private bool _isConnecting;

    public bool IsConnected { get; private set; }

    public int LastSocketErrorCode { get; private set; }

    public event EventHandler<bool> ConnectionStateChanged;

    public event EventHandler<string> ConnectionError;

    public event EventHandler<string> MessageReceived;

    /// <summary>真正写进 socket 的内容，供界面内控制台显示"发出去的是什么"。</summary>
    public event EventHandler<string>? MessageSent;

    public ConnectionService(IUiThreadInvoker uiThread)
    {
        _uiThread = uiThread;
    }

    public async Task ConnectAsync(string address, CancellationToken cancellationToken = default)
    {
        if(_isConnecting || IsConnected) return;

        _isConnecting = true;
        LastSocketErrorCode = 0;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            _ws = new WebSocket(address);
            _ws.OnMessage += (s, e) => MessageReceived?.Invoke(this, e.Data);
            _ws.OnOpen += (s, e) => OnStateChanged(true);
            _ws.OnClose += (s, e) => OnStateChanged(false);
            _ws.OnError += (s, e) => OnError(e.Message);

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, timeoutCts.Token);

            await Task.Run(() => _ws.Connect(), linkedCts.Token);
        }
        catch(OperationCanceledException)
        {
            // 超时是 5 秒预算用完，不是 socket 报错；没有码就别造一个，让上层按现场证据判。
            OnError("连接超时");
        }
        catch(Exception ex)
        {
            LastSocketErrorCode = ExtractSocketCode(ex);
            OnError(ex.Message);
        }
        finally
        {
            _isConnecting = false;
        }
    }

    /// <summary>WebSocketSharp 会把 socket 异常包在自己的 WebSocketException 里，
    /// 有时直接抛原始异常，两种都要走到码。</summary>
    private static int ExtractSocketCode(Exception exception)
    {
        for(var ex = exception; ex != null; ex = ex.InnerException)
            if(ex is SocketException socketException)
                return socketException.ErrorCode;
        return 0;
    }

    public void Disconnect()
    {
        _cts?.Cancel();
        if(_ws?.ReadyState == WebSocketState.Open)
            _ws.Close();
        _ws = null;
        IsConnected = false;
        OnStateChanged(false);
    }

    public async Task SendAsync(string message)
    {
        if(!IsConnected)
        {
            Log.Error("WebSocket未连接");
        }
        else
        {
            // 先记账再交给 socket：等发完再报，慢一点的回包会插到发送行前面，读起来是反的。
            MessageSent?.Invoke(this, message);
            await Task.Run(() => _ws.SendAsync(message, _ => { }));
        }
    }

    private void OnStateChanged(bool connected)
    {
        IsConnected = connected;
        _uiThread.Post(() => ConnectionStateChanged?.Invoke(this, connected));
    }

    private void OnError(string error)
    {
        Log.Error(error);
        _uiThread.Post(() => ConnectionError?.Invoke(this, error));
    }

    public void Dispose()
    {
        _cts?.Dispose();
        _ws?.Close();
    }
}
