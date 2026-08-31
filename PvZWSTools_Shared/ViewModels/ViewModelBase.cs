using System.ComponentModel;
using System.Runtime.CompilerServices;
using PvZWSTools_Shared.Helpers;

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
}
