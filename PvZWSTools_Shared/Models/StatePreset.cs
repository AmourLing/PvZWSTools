using System;
using Newtonsoft.Json;

namespace PvZWSTools_Shared.Models;

/// <summary>
/// 一组已命名的状态预设（如"无尽模式"、"休闲模式"）。
/// 外层字典 Key 为子 ViewModel 属性名（如 "Others"、"Plants"），
/// 内层字典 Key 为属性名，Value 为序列化值。
/// </summary>
public class StatePreset
{
    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonProperty("states")]
    public Dictionary<string, Dictionary<string, string>> States { get; set; } = new();
}
