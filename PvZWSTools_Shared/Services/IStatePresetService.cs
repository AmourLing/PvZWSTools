using System.Collections.Generic;
using PvZWSTools_Shared.Models;

namespace PvZWSTools_Shared.Services;

/// <summary>
/// 状态预设持久化服务接口：管理多组已命名的状态预设。
/// </summary>
public interface IStatePresetService
{
    /// <summary>读取所有已保存的预设（按创建时间倒序）。</summary>
    List<StatePreset> LoadAll();

    /// <summary>保存全部预设列表（覆盖写）。</summary>
    void SaveAll(List<StatePreset> presets);

    /// <summary>新增一个预设（若同名则覆盖）。</summary>
    void Add(StatePreset preset);

    /// <summary>按名称删除预设。</summary>
    bool Delete(string name);
}
