using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;

namespace PvZWSTools_Shared.Services;

/// <summary>
/// 基于本地 JSON 文件的状态预设持久化实现。
/// 文件路径：{baseDirectory}/配置文件/state_presets.json
/// </summary>
public class StatePresetService:IStatePresetService
{
    private readonly string _filePath;

    public StatePresetService(string baseDirectory)
    {
        _filePath = Path.Combine(baseDirectory, Constants.Folder_Need, "state_presets.json");
    }

    public List<StatePreset> LoadAll()
    {
        try
        {
            if(File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var list = JsonConvert.DeserializeObject<List<StatePreset>>(json) ?? new List<StatePreset>();
                return list.OrderByDescending(p => p.CreatedAt).ToList();
            }
        }
        catch(System.Exception ex)
        {
            Log.Error($"状态预设加载失败: {ex}");
        }
        return new List<StatePreset>();
    }

    public void SaveAll(List<StatePreset> presets)
    {
        try
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var json = JsonConvert.SerializeObject(presets, Formatting.Indented);
            File.WriteAllText(_filePath, json);
            Log.Info($"状态预设保存成功：{presets.Count} 组");
        }
        catch(System.Exception ex)
        {
            Log.Error($"状态预设保存失败: {ex}");
        }
    }

    public void Add(StatePreset preset)
    {
        var all = LoadAll();
        // 同名覆盖
        all.RemoveAll(p => p.Name == preset.Name);
        all.Add(preset);
        // 保存时按时间倒序
        SaveAll(all.OrderByDescending(p => p.CreatedAt).ToList());
    }

    public bool Delete(string name)
    {
        var all = LoadAll();
        int removed = all.RemoveAll(p => p.Name == name);
        if(removed > 0)
            SaveAll(all);
        return removed > 0;
    }
}
