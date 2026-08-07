namespace PvZWSTools_WPF.Models;

public class AppSettings
{
    /// <summary>
    /// 允许自动连接
    /// </summary>
    public bool AutoConnectEnabled { get; set; }

    /// <summary>
    /// 取消发送连接提醒
    /// </summary>
    public bool SuppressConnectionMessage { get; set; }
}
