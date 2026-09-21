using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using PvZWSTools_Shared.Models;

namespace PvZWSTools_Shared.Services;

public static class OptionsLoader
{
    /// <summary>
    /// 配置文件 的上一级目录。桌面端就是主程序 exe 所在目录（不能跟进程工作目录走：
    /// 发布包外层还有一个启动器 exe，工作目录由谁启动都会变），
    /// Android 要在建 ViewModel 图之前显式设成外部存储目录，否则所有下拉都是空的。
    /// </summary>
    public static string BasePath { get; set; } = AppContext.BaseDirectory;

    public static ObservableCollection<NameOption> Load(string fileName)
    {
        string path = Path.Combine(BasePath,
            Helpers.Constants.Folder_Need, Helpers.Constants.Folder_Options, fileName);
        if(!File.Exists(path))
        {
            Helpers.Log.Error($"选项文件不存在：{path}");
            return new ObservableCollection<NameOption>();
        }
        try
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ObservableCollection<NameOption>>(json)
                ?? new ObservableCollection<NameOption>();
        }
        catch(Exception ex)
        {
            // 以前这里静默返回空集合，下拉框少选项时完全查不到原因
            Helpers.Log.Error($"选项文件解析失败 {path}: {ex.Message}");
            return new ObservableCollection<NameOption>();
        }
    }
}
