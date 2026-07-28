using Newtonsoft.Json;
using System;
using System.IO;

namespace PvZWSTools_Xamarin;

public class AppSettings
{
    public bool AutoConnectEnabled { get; set; }  // 允许自动连接
    public bool SuppressConnectionMessage { get; set; }  // 取消显示连接提醒
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
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("AppSettings", $"保存设置失败: {ex.Message}");
        }
    }

    // 从文件加载设置
    public static AppSettings Load(string settingsPath)
    {
        try
        {
            if(File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                return JsonConvert.DeserializeObject<AppSettings>(json);
            }
        }
        catch(Exception ex)
        {
            _ = Android.Util.Log.Error("AppSettings", $"加载设置失败: {ex.Message}");
        }

        // 如果文件不存在或加载失败，返回默认设置
        return new AppSettings
        {
            AutoConnectEnabled = false,
            SuppressConnectionMessage = false,
            LastWebSocketAddress = "ws://localhost:8080/Py",
        };
    }
}
