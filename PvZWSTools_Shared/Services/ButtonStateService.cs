using System.IO;
using Newtonsoft.Json;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_Shared.Services;

/// <summary>
/// 基于本地 JSON 文件的按钮状态持久化实现。
/// 文件路径：{baseDirectory}/配置文件/button_states.json
/// </summary>
public class ButtonStateService:IButtonStateService
{
    private readonly string _filePath;

    public ButtonStateService(string baseDirectory)
    {
        _filePath = Path.Combine(baseDirectory, Constants.Folder_Need, "button_states.json");
    }

    public Dictionary<string, Dictionary<string, string>> Load()
    {
        try
        {
            if(File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json)
                    ?? new Dictionary<string, Dictionary<string, string>>();
            }
        }
        catch(Exception ex)
        {
            Log.Error($"按钮状态加载失败: {ex}");
        }
        return new Dictionary<string, Dictionary<string, string>>();
    }

    public void Save(Dictionary<string, Dictionary<string, string>> states)
    {
        try
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var json = JsonConvert.SerializeObject(states, Formatting.Indented);
            File.WriteAllText(_filePath, json);
            Log.Info($"按钮状态保存成功：{states.Sum(kv => kv.Value.Count)} 项");
        }
        catch(Exception ex)
        {
            Log.Error($"按钮状态保存失败: {ex}");
        }
    }
}
