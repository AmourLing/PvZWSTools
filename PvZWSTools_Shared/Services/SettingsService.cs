using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;

namespace PvZWSTools_Shared.Services;

public class SettingsService:ISettingsService
{
    private readonly string _filePath;
    private AppSettings _settings;

    public SettingsService(string baseDirectory)
    {
        _filePath = Path.Combine(baseDirectory, Constants.Folder_Need, "setting.json");
        Load();
    }

    public AppSettings Settings => _settings;

    public void Save()
    {
        try
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
            File.WriteAllText(_filePath, json);

            Log.Info("配置保存成功：");
            LogSettings();
        }
        catch(Exception ex)
        {
            Log.Error($"设置保存失败: {ex}");
        }
    }

    private void Load()
    {
        try
        {
            if(File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _settings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            else
            {
                _settings = new AppSettings();
            }
            Log.Info("配置加载成功：");
            LogSettings();
        }
        catch(Exception ex)
        {
            Log.Error($"设置加载失败: {ex}");
            _settings = new AppSettings();
        }
    }

    /// <summary>
    /// 通过反射自动输出所有带 [Setting] 特性的设置项，新增设置时无需手动修改本方法。
    /// </summary>
    private void LogSettings()
    {
        foreach(var prop in AppSettings.SettingProperties)
        {
            var value = (bool)prop.GetValue(Settings);
            var label = prop.GetCustomAttribute<SettingAttribute>()?.Label ?? prop.Name;
            Log.Raw($"{label}:{value}", value ? ConsoleColor.Green : ConsoleColor.Red);
        }
    }
}
