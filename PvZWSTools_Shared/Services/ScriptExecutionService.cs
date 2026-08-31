using System.IO;
using System.Text.Json;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_Shared.Services;

public class ScriptExecutionService:IScriptExecutionService
{
    private readonly string _basePath;
    private readonly IConnectionService _connection;
    private readonly IUserNotifier? _notifier;

    public ScriptExecutionService(IConnectionService connection, string basePath, IUserNotifier? notifier = null)
    {
        _connection = connection;
        _basePath = basePath;
        _notifier = notifier;
    }

    public async Task ExecuteAsync(string subFolder, string scriptName, Dictionary<string, string> parameters = null, string outputMessage = null)
    {
        if(!_connection.IsConnected)
        {
            _notifier?.Warn("错误", "WebSocket未连接");
            Log.Error("WebSocket未连接");
            return;
        }

        string targetDir = Path.Combine(_basePath, Constants.Folder_Need, Constants.Folder_Buttons, subFolder);
        if(!Directory.Exists(targetDir))
            _ = Directory.CreateDirectory(targetDir);

        string scriptPath = Path.Combine(targetDir, scriptName + ".py");
        if(!File.Exists(scriptPath))
        {
            _notifier?.Error("错误", $"脚本文件不存在：{scriptPath}");
            Log.Error($"脚本文件不存在：{scriptPath}");
            return;
        }

        try
        {
            string scriptContent = await File.ReadAllTextAsync(scriptPath);
            if(parameters != null)
            {
                foreach(var kv in parameters)
                    scriptContent = scriptContent.Replace(kv.Key, kv.Value);
            }

            await _connection.SendAsync(scriptContent);
            if(!string.IsNullOrEmpty(outputMessage))
                System.Diagnostics.Debug.WriteLine(outputMessage);
        }
        catch(Exception ex)
        {
            _notifier?.Error("错误", $"执行脚本失败：{ex.Message}");
            Log.Error($"执行脚本失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 执行脚本并返回所有输出行的内容（自动处理 JSON 格式和 [输出] 前缀）
    /// </summary>
    public async Task<string> ExecuteWithResultAsync(string subFolder, string scriptName, Dictionary<string, string>? placeholders = null)
    {
        if(!_connection.IsConnected)
        {
            Log.Error("WebSocket未连接");
            return string.Empty;
        }

        string targetDir = Path.Combine(_basePath, Constants.Folder_Need, Constants.Folder_Buttons, subFolder);
        string scriptPath = Path.Combine(targetDir, scriptName + ".py");
        if(!File.Exists(scriptPath))
        {
            Log.Error($"脚本文件不存在：{scriptPath}");
            return string.Empty;
        }

        string scriptContent = await File.ReadAllTextAsync(scriptPath);
        if(placeholders != null)
        {
            foreach(var kv in placeholders)
                scriptContent = scriptContent.Replace(kv.Key, kv.Value);
        }

        var lines = new List<string>();
        var tcs = new TaskCompletionSource<bool>();
        EventHandler<string>? handler = null;

        handler = (sender, msg) =>
        {
            if(string.IsNullOrEmpty(msg)) return;

            string? extractedContent = null;
            try
            {
                using JsonDocument doc = JsonDocument.Parse(msg);
                if(doc.RootElement.TryGetProperty("msg", out JsonElement msgElement))
                {
                    extractedContent = msgElement.GetString();
                }
            }
            catch(JsonException)
            {
            }

            string content = extractedContent ?? msg;
            if(content.StartsWith("[输出]"))
                content = content.Substring(4).TrimStart();

            var messageLines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var line in messageLines)
            {
                string trimmedLine = line.Trim();
                if(string.IsNullOrEmpty(trimmedLine)) continue;

                lines.Add(trimmedLine);
                if(trimmedLine.Contains("===END==="))
                {
                    _ = tcs.TrySetResult(true);
                }
            }
        };

        _connection.MessageReceived += handler;
        await _connection.SendAsync(scriptContent);
        var timeoutTask = Task.Delay(3000);
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);
        if(completedTask == timeoutTask)
            _connection.MessageReceived -= handler;
        string result = string.Join(Environment.NewLine, lines);
        return result;
    }

    public async Task SendRawScriptAsync(string scriptContent)
    {
        if(!_connection.IsConnected)
        {
            Log.Error("WebSocket未连接");
            return;
        }
        await _connection.SendAsync(scriptContent);
    }
}
