using System;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using PvZWSTools_Xamarin.Helpers;

namespace PvZWSTools_Xamarin;

public class ConnectionFragment:Fragment
{
    private EditText editTextAddress;
    private Button buttonConnect;
    private MainActivity mainActivity;

    // 标记是否正在执行连接/断开操作
    private bool isActionInProgress = false;

    // 标记是否处于冷却期
    private bool isCooldown = false;

    // 用于取消冷却计时的令牌源（可选，用于更精确的控制）
    private CancellationTokenSource cooldownCts;

    public override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        mainActivity = Activity as MainActivity;
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var view = inflater.Inflate(Resource.Layout.connection_fragment, container, false);

        editTextAddress = view.FindViewById<EditText>(Resource.Id.editText);
        buttonConnect = view.FindViewById<Button>(Resource.Id.button1);

        // 初始化界面状态
        RefreshUi();

        var lastAddress = mainActivity?.GetLastWebSocketAddress();
        if(!string.IsNullOrEmpty(lastAddress))
        {
            editTextAddress.Text = lastAddress;
        }

        buttonConnect.Click += OnConnectButtonClick;

        return view;
    }

    public override void OnResume()
    {
        base.OnResume();
        // 每次回到页面时，重新同步 UI 状态，以防后台状态已改变
        RefreshUi();
    }

    /// <summary>
    /// 统一刷新 UI，根据当前连接状态、操作状态和冷却状态决定按钮表现
    /// </summary>
    private void RefreshUi()
    {
        if(Activity == null || buttonConnect == null) return;

        Activity.RunOnUiThread(() =>
        {
            bool isConnected = MainActivity.ws?.IsConnected ?? false;

            // 优先级：冷却期 > 操作中 > 正常状态

            if(isCooldown)
            {
                buttonConnect.Enabled = false;
                buttonConnect.Text = "请稍后...";
                return;
            }

            if(isActionInProgress)
            {
                buttonConnect.Enabled = false;
                // 根据当前连接状态判断是正在连接还是正在断开
                if(isConnected)
                {
                    // 理论上断开操作中 ws 可能还显示 connected 直到 onClose 触发，
                    // 但为了用户体验，点击断开后立即显示断开中
                    buttonConnect.Text = "断开中...";
                }
                else
                {
                    buttonConnect.Text = "连接中...";
                }
                return;
            }

            // 正常状态
            if(isConnected)
            {
                buttonConnect.Text = "断开连接";
                buttonConnect.Enabled = true;
            }
            else
            {
                buttonConnect.Text = "连接";
                buttonConnect.Enabled = true;
            }
        });
    }

    /// <summary>
    /// 外部可调用此方法来通知 Fragment 连接状态已改变
    /// 例如在 MainActivity.UpdateConnectionStatus 中调用
    /// </summary>
    public void NotifyConnectionStatusChanged(bool isConnected)
    {
        // 如果不在冷却期且不在操作中，则立即刷新 UI
        if(!isCooldown && !isActionInProgress)
        {
            RefreshUi();
        }
        else if(isActionInProgress)
        {
            // 如果操作进行中，状态改变意味着操作结束
            // 例如：点击断开 -> ws 断开 -> onClose 触发
            isActionInProgress = false;

            // 如果是断开操作完成，进入冷却
            if(!isConnected)
            {
                StartCooldown();
            }
            else
            {
                // 如果是连接操作成功
                RefreshUi();
            }
        }
    }

    private async void OnConnectButtonClick(object sender, EventArgs e)
    {
        // 如果正在操作中或处于冷却期，忽略点击
        if(isActionInProgress || isCooldown) return;

        string address = editTextAddress.Text?.Trim();

        if(string.IsNullOrEmpty(address))
        {
            Toast.MakeText(Activity, "请输入WebSocket地址", ToastLength.Short).Show();
            return;
        }

        mainActivity?.SaveWebSocketAddress(address);

        if(MainActivity.ws == null) return;

        bool wasConnected = MainActivity.ws.IsConnected;

        if(wasConnected)
        {
            // --- 断开连接逻辑 ---
            isActionInProgress = true;
            RefreshUi(); // 立即更新 UI 为 "断开中..."

            try
            {
                await Task.Run(() =>
                {
                    MainActivity.ws.Disconnect();
                });

                // Disconnect() 是同步阻塞直到关闭吗？WebSocketSharp 的 Close 可能是异步的。
                // 但我们的 ws.OnClose 会触发 NotifyConnectionStatusChanged(false)
                // 所以这里不需要做太多，等待事件触发即可。
                // 为了防止事件未及时触发导致 UI 卡住，我们可以加一个超时或强制刷新
            }
            catch(Exception ex)
            {
                Log.Error($"[ConnectionFragment]断开连接异常: {ex.Message}");
                isActionInProgress = false;
                Toast.MakeText(Activity, "断开连接失败", ToastLength.Short).Show();
                RefreshUi();
            }
        }
        else
        {
            // --- 连接逻辑 ---
            isActionInProgress = true;
            RefreshUi(); // 立即更新 UI 为 "连接中..."

            _ = Task.Run(() =>
            {
                try
                {
                    Log.Info($"[ConnectionFragment]开始连接: {address}");
                    MainActivity.ws.Connect(address);

                    // Connect() 是阻塞的，直到连接成功或失败
                    // 连接成功后，ws.OnOpen 会触发，进而调用 MainActivity.UpdateConnectionStatus(true)
                    // 进而调用 NotifyConnectionStatusChanged(true)
                }
                catch(Exception ex)
                {
                    Log.Error($"[ConnectionFragment]连接异常: {ex.Message}");
                    Activity?.RunOnUiThread(() =>
                    {
                        isActionInProgress = false;
                        StartCooldown(); // 连接异常也进入冷却
                        Toast.MakeText(Activity, $"连接错误: {ex.Message}", ToastLength.Long).Show();
                    });
                }
            });
        }
    }

    /// <summary>
    /// 启动3秒冷却计时器
    /// </summary>
    private void StartCooldown()
    {
        // 取消之前的冷却计时（如果有）
        cooldownCts?.Cancel();
        cooldownCts = new CancellationTokenSource();

        isActionInProgress = false;
        isCooldown = true;

        RefreshUi(); // 更新为 "请稍后..."

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(3000, cooldownCts.Token);

                if(Activity != null && !cooldownCts.IsCancellationRequested)
                {
                    Activity.RunOnUiThread(() =>
                    {
                        isCooldown = false;
                        RefreshUi(); // 冷却结束，恢复正常状态
                    });
                }
            }
            catch(TaskCanceledException)
            {
                // 任务被取消，忽略
            }
        }, cooldownCts.Token);
    }

    public override void OnDestroyView()
    {
        base.OnDestroyView();
        buttonConnect.Click -= OnConnectButtonClick;
        cooldownCts?.Cancel(); // 清理资源
    }
}
