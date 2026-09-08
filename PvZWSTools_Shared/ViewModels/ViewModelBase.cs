using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;

namespace PvZWSTools_Shared.ViewModels;

public abstract class ViewModelBase:INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 设置字段值，如果值发生变化则触发 PropertyChanged 事件。
    /// </summary>
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if(EqualityComparer<T>.Default.Equals(storage, value))
            return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// 根据字典更新属性（使用反射），并触发 PropertyChanged。
    /// </summary>
    /// <param name="statusDict">变量名->布尔值 字典</param>
    /// <param name="mapping">变量名->属性名 映射</param>
    protected void UpdatePropertiesFromDict(Dictionary<string, bool> statusDict, IReadOnlyDictionary<string, string> mapping)
    {
        var properties = this.GetType().GetProperties();
        foreach(var kvp in statusDict)
        {
            if(mapping.TryGetValue(kvp.Key, out string? propName))
            {
                string symbol = kvp.Value ? Constants.c_Symbol_On : Constants.c_Symbol_Off;
                var prop = Array.Find(properties, p => p.Name == propName);
                if(prop != null && prop.CanWrite && prop.PropertyType == typeof(string))
                {
                    string? currentValue = prop.GetValue(this) as string;
                    prop.SetValue(this, symbol);
                }
            }
        }
    }

    /// <summary>
    /// 导出当前 ViewModel 中需要持久化的全部状态。
    /// 默认通过反射捕获所有 public string 属性（输入框、开关符号、下拉显示值）
    /// 和 NameOption 类型的选中项（存储其 Name）。
    /// 跳过 ICommand、集合、只读属性。
    /// 子类可 override 以定制行为。
    /// </summary>
    public virtual Dictionary<string, string> ExportButtonStates()
    {
        var states = new Dictionary<string, string>();
        foreach(var prop in GetType().GetProperties())
        {
            if(!prop.CanRead) continue;
            // 跳过命令、集合
            if(typeof(System.Windows.Input.ICommand).IsAssignableFrom(prop.PropertyType)) continue;
            if(typeof(System.Collections.IList).IsAssignableFrom(prop.PropertyType)) continue;

            if(prop.PropertyType == typeof(string))
                states[prop.Name] = (string)prop.GetValue(this)! ?? "";
            else if(prop.PropertyType == typeof(NameOption))
            {
                var opt = prop.GetValue(this) as NameOption;
                states[prop.Name] = opt?.Name ?? "";
            }
        }
        return states;
    }

    /// <summary>
    /// 从持久化数据恢复状态。
    /// 先恢复 string 属性，再恢复 NameOption（从对应 Options 集合中按 Name 匹配）。
    /// </summary>
    public virtual void ImportButtonStates(Dictionary<string, string> states)
    {
        // 收集所有 ObservableCollection<NameOption> 以供 NameOption 匹配
        var optionCollections = new List<ObservableCollection<NameOption>>();
        foreach(var prop in GetType().GetProperties())
        {
            if(prop.PropertyType == typeof(ObservableCollection<NameOption>))
            {
                if(prop.GetValue(this) is ObservableCollection<NameOption> coll)
                    optionCollections.Add(coll);
            }
        }

        // 第一遍：恢复 string 属性
        foreach(var kvp in states)
        {
            var prop = GetType().GetProperty(kvp.Key);
            if(prop == null || !prop.CanWrite) continue;
            if(prop.PropertyType == typeof(string))
                prop.SetValue(this, kvp.Value);
        }

        // 第二遍：恢复 NameOption 选中项（按 Name 在集合中匹配）
        foreach(var kvp in states)
        {
            var prop = GetType().GetProperty(kvp.Key);
            if(prop == null || !prop.CanWrite) continue;
            if(prop.PropertyType == typeof(NameOption) && !string.IsNullOrEmpty(kvp.Value))
            {
                var match = optionCollections
                    .SelectMany(c => c)
                    .FirstOrDefault(o => o.Name == kvp.Value);
                if(match != null)
                    prop.SetValue(this, match);
            }
        }
    }

    /// <summary>
    /// 把状态中"开启"的开关（Checkbox）同步到游戏。
    /// 仅处理值为 ✔️ 的 string 开关属性；复用各开关既有的命令（命令内部封装了脚本名与参数），
    /// 通过"先重置为 ❌ 再执行命令"让命令把开关 Toggle 到 ✔️ 并发送 check=1 脚本。
    /// 纯本地参数开关（Toggle 前缀命令、无发送命令）和输入框/下拉框不受影响。
    /// 必须在已连接游戏后调用；未连接时命令发送无效。
    /// </summary>
    /// <returns>实际发送脚本到游戏的开关数量。</returns>
    public virtual int SyncToggleStatesToGame(Dictionary<string, string> states)
    {
        int sent = 0;
        foreach(var kvp in states)
        {
            // 只同步需要"开启"的开关；❌ 的状态游戏侧默认即为关闭，无需发送
            if(kvp.Value != Constants.c_Symbol_On) continue;

            var prop = GetType().GetProperty(kvp.Key);
            if(prop == null || !prop.CanWrite || prop.PropertyType != typeof(string)) continue;

            var cmd = FindToggleCommand(kvp.Key);
            if(cmd == null) continue; // 没有发送命令（纯本地参数开关/输入框）→ 跳过

            // ImportButtonStates 已把 UI 置为 ✔️；先重置为 ❌，命令 Toggle 后回到 ✔️ 并发送脚本
            prop.SetValue(this, Constants.c_Symbol_Off);
            if(cmd.CanExecute(null))
            {
                cmd.Execute(null);
                sent++;
            }
        }
        return sent;
    }

    /// <summary>
    /// 查找开关属性对应的"发送脚本"命令。
    /// 规则（按优先级）：
    /// 1. 直接命名 {propName}Command（如 AutoCollect→AutoCollectCommand、FreePlant→FreePlantCommand）
    /// 2. Check 后缀→Handle 前缀（如 BungeeCheck→BungeeHandleCommand）
    /// 不匹配 Toggle 前缀命令：这些是纯本地切换（IsSendSync=false），不会发送脚本到游戏。
    /// </summary>
    private ICommand? FindToggleCommand(string propName)
    {
        var direct = GetType().GetProperty(propName + "Command");
        if(direct?.GetValue(this) is ICommand directCmd)
            return directCmd;

        if(propName.EndsWith("Check"))
        {
            string handleName = propName.Substring(0, propName.Length - "Check".Length) + "HandleCommand";
            var handle = GetType().GetProperty(handleName);
            if(handle?.GetValue(this) is ICommand handleCmd)
                return handleCmd;
        }
        return null;
    }
}
