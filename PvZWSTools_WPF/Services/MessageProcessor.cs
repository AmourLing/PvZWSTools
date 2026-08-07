using Newtonsoft.Json.Linq;
using PvZWSTools_Shared;
using PvZWSTools_WPF.Helpers;

namespace PvZWSTools_WPF.Services;

public class MessageProcessor:IMessageProcessor
{
    public void ProcessMessage(string message)
    {
        try
        {
            var jo = JObject.Parse(message);
            string eventType = jo["eventtype"]?.Value<string>();

            if(eventType == "output")
            {
                var output = jo.ToObject<WSEvents.OutputEvent>();
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
}
