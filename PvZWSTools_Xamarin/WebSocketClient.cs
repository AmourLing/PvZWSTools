using Android.Widget;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using PvZWSTools_Shared;

namespace PvZWSTools_Xamarin
{
    public class WebSocketClient
    {
        private WebSocket ws;
        private Timer connectionCheckTimer;
        private bool isConnecting = false;
        private bool stopAutoConnect = false;
        private string currentUrl;
        private Action<bool> onConnectionStatusChanged;
        private bool autoConnectEnabled = false;

        public event EventHandler<string> MessageReceived;

        public WebSocketClient(Action<bool> onConnectionStatusChanged = null)
        {
            this.onConnectionStatusChanged = onConnectionStatusChanged;
        }

        public bool IsConnected => ws?.ReadyState == WebSocketState.Open;

        public void EnableAutoConnect(bool enabled)
        {
            autoConnectEnabled = enabled;

            if(enabled)
            {
                StartConnectionCheckTimer();
            }
            else
            {
                StopConnectionCheckTimer();
            }
        }

        private void StartConnectionCheckTimer()
        {
            if(connectionCheckTimer == null)
            {
                connectionCheckTimer = new Timer(ConnectionCheckCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(250));
            }
        }

        private void StopConnectionCheckTimer()
        {
            if(connectionCheckTimer != null)
            {
                connectionCheckTimer.Dispose();
                connectionCheckTimer = null;
            }
        }

        private void ConnectionCheckCallback(object state)
        {
            // 如果设置了停止自动连接，则不检查
            if(stopAutoConnect)
                return;

            // 如果未启用自动连接，则不检查
            if(!autoConnectEnabled)
                return;

            // 如果正在连接中，则跳过
            if(isConnecting)
                return;

            // 如果已经连接，则跳过
            if(IsConnected)
                return;

            // 如果当前URL为空，则跳过
            if(string.IsNullOrWhiteSpace(currentUrl))
                return;

            // 开始连接
            isConnecting = true;

            Task.Run(() =>
            {
                try
                {
                    Android.Util.Log.Info("WebSocketClient", $"自动连接检查：尝试连接到 {currentUrl}");
                    Connect(currentUrl);
                }
                finally
                {
                    isConnecting = false;
                }
            });
        }

        public void Connect(string url)
        {
            if(ws != null && ws.ReadyState == WebSocketState.Open)
            {
                Disconnect();
            }

            currentUrl = url;
            stopAutoConnect = false;

            ws = new WebSocket(url);

            ws.OnMessage += (sender, e) =>
            {
                Android.Util.Log.Debug("WebSocketClient", $"收到消息: {e.Data}");
                MessageReceived?.Invoke(this, e.Data);
            };

            ws.OnOpen += (sender, e) =>
            {
                Android.Util.Log.Info("WebSocketClient", $"连接成功: {url}");
                isConnecting = false;
                stopAutoConnect = false;

                Send(Sharedstring.GetLogoDisplayString(true));

                // 更新连接状态
                onConnectionStatusChanged?.Invoke(true);

                // 发送连接成功的消息到MainActivity
                MainActivity.Instance?.RunOnUiThread(() =>
                {
                    // 更新菜单项的标题
                    MainActivity.Instance.UpdateConnectionStatus(true);
                });
            };

            ws.OnClose += (sender, e) =>
            {
                Android.Util.Log.Info("WebSocketClient", $"连接关闭: {e.Reason}");

                // 更新连接状态
                onConnectionStatusChanged?.Invoke(false);

                // 如果不是手动断开，尝试重连
                if(!stopAutoConnect && autoConnectEnabled)
                {
                    Android.Util.Log.Info("WebSocketClient", "连接断开，将尝试重连");
                }

                MainActivity.Instance?.RunOnUiThread(() =>
                {
                    // 更新菜单项的标题
                    MainActivity.Instance.UpdateConnectionStatus(false);
                });
            };

            ws.OnError += (sender, e) =>
            {
                Android.Util.Log.Error("WebSocketClient", $"连接错误: {e.Message}");

                // 更新连接状态
                onConnectionStatusChanged?.Invoke(false);

                // 连接错误时也尝试重连
                if(!stopAutoConnect && autoConnectEnabled)
                {
                    Android.Util.Log.Info("WebSocketClient", "连接错误，将尝试重连");
                }

                MainActivity.Instance?.RunOnUiThread(() =>
                {
                    // 更新菜单项的标题
                    MainActivity.Instance.UpdateConnectionStatus(false);
                });
            };

            try
            {
                ws.Connect();
            }
            catch(Exception ex)
            {
                Android.Util.Log.Error("WebSocketClient", $"连接异常: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            stopAutoConnect = true;

            if(ws?.ReadyState == WebSocketState.Open)
            {
                ws.Close();
            }

            // 更新连接状态
            onConnectionStatusChanged?.Invoke(false);

            Android.Util.Log.Info("WebSocketClient", "手动断开连接，停止自动连接");
        }

        public void Send(string command)
        {
            if(IsConnected)
            {
                ws.SendAsync(command, null);
            }
        }

        public void Dispose()
        {
            StopConnectionCheckTimer();
            Disconnect();
        }
    }
}
