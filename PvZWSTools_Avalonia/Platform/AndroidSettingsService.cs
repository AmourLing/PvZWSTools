using PvZWSTools_Shared.Services;
using SharedAppSettings = PvZWSTools_Shared.Models.AppSettings;

namespace PvZWSTools_Avalonia.Platform;

/// <summary>
/// 把 Android 自己的 AppSettings 投影成共享层的 ISettingsService。
///
/// 之所以不直接用共享的 SettingsService：它和 Android 的 AppSettings 是两个类、
/// 读写同一个 setting.json，而 Android 那份多一个 LastWebSocketAddress 字段。
/// 让 VM 用 SettingsService 的话，它一保存就把上次连接地址抹掉，
/// 而且两边各自缓存，改设置后谁的值是真的说不清。
///
/// Settings 每次现取，所以设置界面改完之后 ReloadSettingsFromService() 拿得到新值。
/// </summary>
public sealed class AndroidSettingsService:ISettingsService
{
    private readonly AppSettings _local;
    private readonly string _path;

    public AndroidSettingsService(AppSettings local, string path)
    {
        _local = local;
        _path = path;
    }

    public SharedAppSettings Settings => new()
    {
        AutoConnectEnabled = _local.AutoConnectEnabled,
        SuppressConnectionMessage = _local.SuppressConnectionMessage,
        AllowAutoUpdateButtonStatus = _local.AllowAutoUpdateButtonStatus,
        AutoCheckUpdateEnabled = _local.AutoCheckUpdateEnabled,
        AutoApplyLastState = _local.AutoApplyLastState,
    };

    public void Save() => _local.Save(_path);
}
