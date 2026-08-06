namespace PvZWSTools_WPF.Services;

public interface IScriptExecutionService
{
    Task ExecuteAsync(string subFolder, string scriptName, Dictionary<string, string> parameters = null, string outputMessage = null);

    Task SendRawScriptAsync(string scriptContent);

    /// <summary>
    /// 执行脚本并返回标准输出内容（从 WebSocket 返回的 [输出] 行）
    /// </summary>
    Task<string> ExecuteWithResultAsync(string subFolder, string scriptName, Dictionary<string, string>? placeholders = null);
}
