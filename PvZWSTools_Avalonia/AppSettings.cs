using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using PvZWSTools_Avalonia.Helpers;
using PvZWSTools_Shared.Models;

namespace PvZWSTools_Avalonia;

public class AppSettings
{
    [Setting("允许自动连接")]
    public bool AutoConnectEnabled { get; set; }  // 允许自动连接

    [Setting("取消发送连接提醒")]
    public bool SuppressConnectionMessage { get; set; }  // 取消显示连接提醒

    [Setting("允许自动更新按钮状态")]
    public bool AllowAutoUpdateButtonStatus { get; set; }  // 允许自动更新按钮状态（切换界面时自动发送脚本刷新按钮开关状态）

    public string LastWebSocketAddress { get; set; }  // 上次连接成功的WebSocket地址

    // 保存设置到文件
    public void Save(string settingsPath)
    {
        try
        {
            string directory = Path.GetDirectoryName(settingsPath);
            if(!Directory.Exists(directory))
            {
                _ = Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(settingsPath, json);

            Log.Info("配置保存成功：");
            LogSettings();
        }
        catch(Exception ex)
        {
            Log.Error("保存设置失败", ex);
            _ = Android.Util.Log.Error("AppSettings", $"保存设置失败: {ex.Message}");
        }
    }

    // 从文件加载设置
    public static AppSettings Load(string settingsPath)
    {
        AppSettings settings = null;
        try
        {
            if(File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                settings = JsonConvert.DeserializeObject<AppSettings>(json);
            }
        }
        catch(Exception ex)
        {
            Log.Error("加载设置失败", ex);
            _ = Android.Util.Log.Error("AppSettings", $"加载设置失败: {ex.Message}");
        }

        // 如果文件不存在或加载失败，返回默认设置
        settings ??= new AppSettings
        {
            AutoConnectEnabled = false,
            SuppressConnectionMessage = false,
            AllowAutoUpdateButtonStatus = false,
            LastWebSocketAddress = "ws://localhost:8080/Py",
        };

        Log.Info("配置加载成功：");
        settings.LogSettings();
        return settings;
    }

    /// <summary>
    /// 通过反射自动输出所有带 [Setting] 特性的设置项，新增设置时无需手动修改本方法。
    /// </summary>
    private void LogSettings()
    {
        foreach(var prop in typeof(AppSettings).GetProperties())
        {
            if(prop.PropertyType != typeof(bool)) continue;
            var attr = prop.GetCustomAttribute<SettingAttribute>();
            if(attr == null) continue;
            Log.Info($"{attr.Label}:{prop.GetValue(this)}");
        }
    }
}
