namespace PvZWSTools_Shared.Services;

/// <summary>
/// 按钮状态持久化服务接口。
/// 用于保存/恢复用户在各页签中切换的按钮开关状态。
/// 外层字典 Key 为子 ViewModel 属性名（如 "Others"、"Plants"），
/// 内层字典 Key 为按钮属性名，Value 为按钮符号（✔️/❌）。
/// </summary>
public interface IButtonStateService
{
    Dictionary<string, Dictionary<string, string>> Load();
    void Save(Dictionary<string, Dictionary<string, string>> states);
}
