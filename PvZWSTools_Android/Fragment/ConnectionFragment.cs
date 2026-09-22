using System;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using PvZWSTools_Android.Helpers;

using PvZWSTools_Android.Platform;
using PvZWSTools_Shared.Services;
namespace PvZWSTools_Android;

public class ConnectionFragment:AndroidX.Fragment.App.Fragment
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
            bool isConnected = AppServices.IsConnected;

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

            // 正常状态：文字直接取 VM 的，跟桌面端同一个来源
            buttonConnect.Text = AppServices.Root?.ConnectionButtonText ?? "连接";
            buttonConnect.Enabled = true;
        });
    }

    /// <summary>
    /// 外部可调用此方法来通知 Fragment 连接状态已改变
    /// 例如在 MainActivity.UpdateConnectionStatus 中调用
    /// </summary>
    public void NotifyConnectionStatusChanged(bool isConnected) => OnConnectionSettled();

    /// <summary>
    /// 连接和断开都走 VM 的 ConnectCommand。它内部会置 _stopAutoConnect，
    /// 所以手动断开之后自动重连不会在 1 秒内把连接抢回来；
    /// 自己直接调 Connection.ConnectAsync/Disconnect 就绕过这个保护了。
    /// </summary>
    private void OnConnectButtonClick(object sender, EventArgs e)
    {
        if(isActionInProgress || isCooldown) return;

        string address = editTextAddress.Text?.Trim();
        if(string.IsNullOrEmpty(address))
        {
            Toast.MakeText(Activity, "请输入WebSocket地址", ToastLength.Short).Show();
            return;
        }

        mainActivity?.SaveWebSocketAddress(address);

        var root = AppServices.Root;
        root.WsAddress = address;

        isActionInProgress = true;
        root.ConnectCommand.Execute(null);
    }

    /// <summary>
    /// 连接结果一律由 ConnectionStateChanged 报回来；失败时共享层只发 ConnectionError、
    /// 不发状态变化，所以这里也要接一下，否则按钮会永远停在"连接中"。
    /// </summary>
    private void OnConnectionSettled()
    {
        isActionInProgress = false;
        if(!AppServices.IsConnected) StartCooldown();
        RefreshUi();
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
