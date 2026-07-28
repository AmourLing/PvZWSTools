using System;
using WebSocketSharp;
using PvZWSTools_Shared;
using PvZWSTools_Xamarin.Helpers;

namespace PvZWSTools_Xamarin;

public class WebSocketClient
{
    private WebSocket ws;
    private readonly object lockObj = new object();

    private string currentUrl;
    private Action<bool> onConnectionStatusChanged;
    private bool suppressConnectionMessage = false;

    public event EventHandler<string> MessageReceived;

    public WebSocketClient(Action<bool> onConnectionStatusChanged = null)
    {
        this.onConnectionStatusChanged = onConnectionStatusChanged;
    }

    public bool IsConnected
    {
        get
        {
            lock(lockObj)
            {
                return ws?.ReadyState == WebSocketState.Open;
            }
        }
    }

    public void EnableSuppressConnectionMessage(bool enabled)
    {
        suppressConnectionMessage = enabled;
    }

    /// <summary>
    /// 连接到指定地址
    /// </summary>
    public void Connect(string url)
    {
        lock(lockObj)
        {
            currentUrl = url;
        }
        ConnectInternal(url);
    }

    /// <summary>
    /// 内部连接逻辑
    /// </summary>
    private void ConnectInternal(string url)
    {
        WebSocket oldWs = null;

        lock(lockObj)
        {
            if(ws != null)
            {
                if(ws.ReadyState == WebSocketState.Open)
                {
                    return; // 已连接，无需操作
                }

                // 清理旧实例
                try
                {
                    ws.OnMessage -= Ws_OnMessage;
                    ws.OnOpen -= Ws_OnOpen;
                    ws.OnClose -= Ws_OnClose;
                    ws.OnError -= Ws_OnError;

                    if(ws.ReadyState == WebSocketState.Connecting)
                        ws.CloseAsync();
                    else if(ws.ReadyState == WebSocketState.Open)
                        ws.Close();
                }
                catch { }

                oldWs = ws;
                ws = null;
            }

            // 创建新实例
            ws = new WebSocket(url);
            ws.OnMessage += Ws_OnMessage;
            ws.OnOpen += Ws_OnOpen;
            ws.OnClose += Ws_OnClose;
            ws.OnError += Ws_OnError;
        }

        try
        {
            Log.Debug($"[WebSocketClient]正在连接: {url}");
            ws.Connect();
        }
        catch(Exception ex)
        {
            Log.Error($"[WebSocketClient]连接异常: {ex.Message}");
            UpdateConnectionStatus(false);
        }
    }

    private void Ws_OnMessage(object sender, MessageEventArgs e)
    {
        Log.Debug($"[WebSocketClient]收到消息: {e.Data}");
        MessageReceived?.Invoke(this, e.Data);
    }

    private void Ws_OnOpen(object sender, EventArgs e)
    {
        Log.Info($"[WebSocketClient]连接成功: {currentUrl}");
        UpdateConnectionStatus(true);

        string msg = Sharedstring.GetLogoDisplayString(!suppressConnectionMessage);
        Send(msg);

        RunOnMainThread(() =>
        {
            MainActivity.Instance?.UpdateConnectionStatus(true);
        });
    }

    private void Ws_OnClose(object sender, CloseEventArgs e)
    {
        Log.Info($"[WebSocketClient]连接关闭: {e.Reason}");
        UpdateConnectionStatus(false);

        RunOnMainThread(() =>
        {
            MainActivity.Instance?.UpdateConnectionStatus(false);
        });
    }

    private void Ws_OnError(object sender, ErrorEventArgs e)
    {
        Log.Error($"[WebSocketClient]连接错误: {e.Message}");
        UpdateConnectionStatus(false);

        RunOnMainThread(() =>
        {
            MainActivity.Instance?.UpdateConnectionStatus(false);
        });
    }

    private void UpdateConnectionStatus(bool isConnected)
    {
        onConnectionStatusChanged?.Invoke(isConnected);
    }

    public void Disconnect()
    {
        WebSocket wsToClose = null;
        lock(lockObj)
        {
            if(ws?.ReadyState == WebSocketState.Open || ws?.ReadyState == WebSocketState.Connecting)
            {
                wsToClose = ws;
                ws = null;
            }
        }

        if(wsToClose != null)
        {
            try
            {
                if(wsToClose.ReadyState == WebSocketState.Connecting)
                    wsToClose.CloseAsync();
                else
                    wsToClose.Close();
            }
            catch { }
        }

        UpdateConnectionStatus(false);
        Log.Info($"[WebSocketClient]手动断开连接");
    }

    public void Send(string command)
    {
        if(IsConnected)
        {
            try
            {
                ws.SendAsync(command, null);
            }
            catch(Exception ex)
            {
                Log.Error($"[WebSocketClient]发送消息失败: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        Disconnect();
    }

    private void RunOnMainThread(Action action)
    {
        if(action == null) return;
        try
        {
            MainActivity.Instance?.RunOnUiThread(action);
        }
        catch(Exception ex)
        {
            Log.Warning($"[WebSocketClient]无法在主线程执行操作: {ex.Message}");
        }
    }
}
