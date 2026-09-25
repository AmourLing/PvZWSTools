using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using PvZWSTools_Shared.Helpers;
using PvZWSTools_Shared.Models;

namespace PvZWSTools_Android.Platform;

public class AndroidAppSettings
{
    [Setting("允许自动连接")]
    public bool AutoConnectEnabled { get; set; }  // 允许自动连接

    [Setting("取消发送连接提醒")]
    public bool SuppressConnectionMessage { get; set; }  // 取消显示连接提醒

    [Setting("允许自动更新按钮状态")]
    public bool AllowAutoUpdateButtonStatus { get; set; }  // 允许自动更新按钮状态（切换界面时自动发送脚本刷新按钮开关状态）

    [Setting("启动时自动检查更新")]
    public bool AutoCheckUpdateEnabled { get; set; }  // 启动时自动检查新版本（仅检查并提示，不自动下载）

    [Setting("自动应用上次配置")]
    public bool AutoApplyLastState { get; set; }  // 启动时自动应用上次关闭前保存的状态（关闭时始终保存"上次状态"）

    [Setting("退出时不弹出确认提示")]
    public bool SuppressExitPrompt { get; set; }  // 退出时不再弹确认框（在确认框里勾选"不再提示"后记住；退出仍走 SafeExit）

    public string LastWebSocketAddress { get; set; }  // 上次连接成功的WebSocket地址

    /// <summary>界面语种："zh"（默认）或 "en"。中文原文就是文案的键，所以这里只决定要不要查英文表。
    /// 改它要重启生效：清单标签和导航文案都是启动时一次性拼好的。</summary>
    [Setting("界面语种")]
    public string Language { get; set; } = Loc.Zh;

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
            _ = Android.Util.Log.Error("AndroidAppSettings", $"保存设置失败: {ex.Message}");
        }
    }

    // 从文件加载设置
    public static AndroidAppSettings Load(string settingsPath)
    {
        AndroidAppSettings settings = null;
        try
        {
            if(File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                settings = JsonConvert.DeserializeObject<AndroidAppSettings>(json);
            }
        }
        catch(Exception ex)
        {
            Log.Error("加载设置失败", ex);
            _ = Android.Util.Log.Error("AndroidAppSettings", $"加载设置失败: {ex.Message}");
        }

        // 如果文件不存在或加载失败，返回默认设置
        settings ??= new AndroidAppSettings
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
        foreach(var prop in typeof(AndroidAppSettings).GetProperties())
        {
            if(prop.PropertyType != typeof(bool)) continue;
            var attr = prop.GetCustomAttribute<SettingAttribute>();
            if(attr == null) continue;
            Log.Info($"{attr.Label}:{prop.GetValue(this)}");
        }
    }
}
