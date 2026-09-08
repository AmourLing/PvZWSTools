namespace PvZWSTools_Shared.Services;

public interface IScriptExecutionService
{
    /// <summary>
    /// 批量静默模式：为 true 时，脚本缺失/执行失败等错误只写入日志，不弹出模态对话框。
    /// 用于"恢复状态后批量同步开关到游戏"等后台批量场景，避免连环弹窗阻塞流程。
    /// </summary>
    bool SilentMode { get; set; }

    /// <summary>
    /// 执行脚本。返回 true 表示已发送到游戏；false 表示未发送（未连接）或脚本缺失。
    /// 调用方据此决定是否回滚 UI 状态。
    /// </summary>
    Task<bool> ExecuteAsync(string subFolder, string scriptName, Dictionary<string, string>? parameters = null, string? outputMessage = null);

    Task SendRawScriptAsync(string scriptContent);

    /// <summary>
    /// 执行脚本并返回标准输出内容（从 WebSocket 返回的 [输出] 行）
    /// </summary>
    Task<string> ExecuteWithResultAsync(string subFolder, string scriptName, Dictionary<string, string>? placeholders = null);
}
