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

    public event EventHandler<bool> ConnectionStateChanged;

    public event EventHandler<string> ConnectionError;

    public event EventHandler<string> MessageReceived;

    public ConnectionService(IUiThreadInvoker uiThread)
    {
        _uiThread = uiThread;
    }

    public async Task ConnectAsync(string address, CancellationToken cancellationToken = default)
    {
        if(_isConnecting || IsConnected) return;

        _isConnecting = true;
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
            OnError("连接超时");
        }
        catch(Exception ex)
        {
            OnError(ex.Message);
        }
        finally
        {
            _isConnecting = false;
        }
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
