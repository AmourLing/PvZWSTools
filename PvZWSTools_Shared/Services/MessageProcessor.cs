using Newtonsoft.Json.Linq;
using PvZWSTools_Shared;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_Shared.Services;

public class MessageProcessor:IMessageProcessor
{
    // 定义事件，用于通知按钮状态已更新
    public event Action<Dictionary<string, bool>>? ButtonStatusUpdated;

    public void ProcessMessage(string message)
    {
        try
        {
            var jo = JObject.Parse(message);
            string eventType = jo["eventtype"]?.Value<string>();

            if(eventType == "output")
            {
                var output = jo.ToObject<WSEvents.OutputEvent>();
                if(output != null)
                {
                    // 尝试解析按钮状态
                    TryParseButtonStatus(output.msg);
                }
                Log.Info($"[输出] {output?.msg}");
            }
            else if(eventType == "execution")
            {
                var exec = jo.ToObject<WSEvents.ExecutionEvent>();
                if(exec?.statuscode == WSEvents.ExecutionEventResult.error)
                {
                    Log.Error($"[执行错误] {exec.errortype}: {exec.result}");
                }
                else
                {
                    Log.Info($"[执行结果] {exec?.result}");
                }
            }
            else
            {
                Log.Info($"[未知事件] {message}");
            }
        }
        catch(Exception ex)
        {
            Log.Error($"[消息解析失败] {ex.Message}");
        }
    }

    /// <summary>
    /// 解析 GetButtonCheck.py 的完整输出字符串
    /// </summary>
    private void TryParseButtonStatus(string msg)
    {
        // 如果输出包含开始和结束标记，则认为它是按钮状态报告
        if(msg.Contains("开始检查按钮状态") && msg.Contains("检查按钮状态完成"))
        {
            // 按换行符拆分（支持 \r\n 和 \n）
            var lines = msg.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var statusDict = new Dictionary<string, bool>();

            foreach(var line in lines)
            {
                // 查找形如 "VARIABLE_NAME => True/False" 的行
                if(line.Contains("=>"))
                {
                    var parts = line.Split(new[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
                    if(parts.Length == 2)
                    {
                        string varName = parts[0].Trim();
                        if(bool.TryParse(parts[1].Trim(), out bool value))
                        {
                            statusDict[varName] = value;
                        }
                    }
                }
            }

            // 触发事件，将解析结果传递给订阅者
            ButtonStatusUpdated?.Invoke(statusDict);
        }
    }
}
