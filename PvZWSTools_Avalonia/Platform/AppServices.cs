using Android.Content;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Services;
using PvZWSTools_Shared.UiModel;
using PvZWSTools_Shared.ViewModels;
using PvZWSTools_Avalonia.Services;

namespace PvZWSTools_Avalonia.Platform;

/// <summary>
/// Android 侧的组合根：共享的 ViewModel 图和功能清单各建一次，Fragment 只从这里取，
/// 不再各自靠 strings.xml 的键名拼状态。
///
/// 过渡期原则：凡是 Android 上还有别的主人在管的东西，这里一律不接，免得两份真相
/// 互相覆盖 —— 连接仍由 MainActivity.ws 管（所以建好就把 VM 的自动连接关掉，
/// 否则两条 socket 同时连游戏会把脚本执行两遍），开关状态持久化仍由
/// AndroidStateService 管（所以不传 ButtonStateService，两者写的是同一个
/// button_states.json 但键不一样），更新流程仍走 AndroidUpdateService 自己那套。
/// 等 Fragment 全部改成吃清单之后，这三样再一并收回来。
/// </summary>
public static class AppServices
{
    public static MainWindowViewModel Root { get; private set; } = null!;

    public static IReadOnlyList<NavItem> Pages { get; private set; } = Array.Empty<NavItem>();

    public static IUiThreadInvoker UiThread { get; private set; } = null!;

    public static bool IsReady => Root != null;

    /// <summary>要在 配置文件 解压完成之后调用，否则读不到 setting.json。</summary>
    public static void Initialize(Context context, string baseDir)
    {
        if(Root != null) return;

        UiThread = new AndroidUiThreadInvoker();
        var connection = new ConnectionService(UiThread);

        // 选项 json 是各 ViewModel 构造时读的，所以根路径必须在那之前定好；
        // 不设置的话 OptionsLoader 会拿进程工作目录，Android 上找不到文件、下拉全空。
        OptionsLoader.BasePath = baseDir;

        Root = new MainWindowViewModel(
            connection,
            new SettingsService(baseDir),
            baseDir,
            new AndroidDialogService(),
            new MessageProcessor(),
            UiThread,
            notifier: null,
            updateService: null,
            buttonStateService: null);

        Root.AutoConnectEnabled = false;

        Pages = UnitCatalog.Build(Root);
    }
}
