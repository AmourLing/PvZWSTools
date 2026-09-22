using Android.Content;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;
using PvZWSTools_Shared.UiModel;
using PvZWSTools_Shared.ViewModels;
using PvZWSTools_Android.Services;

namespace PvZWSTools_Android.Platform;

/// <summary>
/// Android 侧的组合根：共享的 ViewModel 图和功能清单各建一次，Fragment 只从这里取，
/// 不再各自靠 strings.xml 的键名拼状态。
///
/// 连接只有这一条：MainWindowViewModel 里的 ConnectionService 是唯一出口，
/// 自动重连、连上后发 logo、把上次开关状态同步给游戏都由它负责。
/// 旧的 WebSocketClient 和 MainActivity 自己那套重连定时器已经删掉，
/// 否则两条 socket 同时连游戏会把脚本执行两遍。
/// </summary>
public static class AppServices
{
    public static MainWindowViewModel Root { get; private set; } = null!;

    public static IReadOnlyList<NavItem> Pages { get; private set; } = Array.Empty<NavItem>();

    public static IUiThreadInvoker UiThread { get; private set; } = null!;

    public static IConnectionService Connection { get; private set; } = null!;

    /// <summary>收藏 + 常用统计，和桌面端同一份共享实现；收藏页读它，功能行的长按写它。</summary>
    public static FavoriteStore Favorites { get; private set; } = null!;

    /// <summary>命名状态预设，供状态管理对话框读写。</summary>
    public static IStatePresetService Presets { get; private set; } = null!;

    public static bool IsReady => Root != null;

    public static bool IsConnected => Connection?.IsConnected ?? false;

    /// <summary>发一段脚本给游戏。原来各处直接 ws.Send，现在统一走这一条连接。</summary>
    public static void Send(string text)
    {
        if(Connection != null) _ = Connection.SendAsync(text);
    }

    /// <summary>要在 配置文件 解压完成之后调用，否则读不到选项 json。</summary>
    public static void Initialize(Context context, string baseDir, AppSettings settings, string settingsPath)
    {
        if(Root != null) return;

        UiThread = new AndroidUiThreadInvoker();
        Connection = new ConnectionService(UiThread);
        var buttonStates = new ButtonStateService(baseDir);
        Presets = new StatePresetService(baseDir);

        // 选项 json 是各 ViewModel 构造时读的，所以根路径必须在那之前定好；
        // 不设置的话 OptionsLoader 会拿进程工作目录，Android 上找不到文件、下拉全空。
        OptionsLoader.BasePath = baseDir;

        Root = new MainWindowViewModel(
            Connection,
            new AndroidSettingsService(settings, settingsPath),
            baseDir,
            new AndroidDialogService(),
            new MessageProcessor(),
            UiThread,
            notifier: new AndroidUserNotifier(),
            updateService: null,
            buttonStateService: buttonStates);

        // 上次连成功的地址只存在 Android 的 AppSettings 里，共享层看不到，得显式喂进去；
        // 不喂的话自动重连会一直去连默认的 localhost，而那是仿真机自己。
        if(!string.IsNullOrWhiteSpace(settings.LastWebSocketAddress))
            Root.WsAddress = settings.LastWebSocketAddress;

        Pages = UnitCatalog.Build(Root);

        // 收藏/常用是共享层的一份状态，配置根跟其它 cfg 同一个目录（baseDir）
        Favorites = new FavoriteStore(baseDir);
        Favorites.Attach(Pages.SelectMany(p => p.Units));
    }
}
