using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace PvZWSTools_WPF.Themes;

/// <summary>UI 风格与主题的运行时管理：经典 UI = 不加载主题字典（系统默认外观）。</summary>
public static class UiThemeManager
{
    private static readonly string ConfigPath =
        Path.Combine(AppContext.BaseDirectory, "ui_theme.cfg");

    /// <summary>当前是否使用 NewUI（false = 经典 UI）。</summary>
    public static bool UseNewUi { get; private set; } = true;

    /// <summary>NewUI 下的主题：true = 黑夜，false = 白天。</summary>
    public static bool IsDark { get; private set; } = true;

    /// <summary>应用启动时读取上次的选择并应用（须在主窗口创建前调用）。</summary>
    public static void LoadAndApply()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var parts = File.ReadAllText(ConfigPath).Trim().Split('|');
                UseNewUi = parts[0] != "classic";
                IsDark = parts.Length < 2 || parts[1] != "light";
            }
        }
        catch
        {
            // 配置读取失败按默认处理
        }
        Apply();
    }

    /// <summary>保存选择并重启应用使其生效（避免 WPF 运行时换字典的渲染残留问题）。</summary>
    public static void SaveAndRestart(bool useNewUi, bool isDark)
    {
        UseNewUi = useNewUi;
        IsDark = isDark;
        try
        {
            File.WriteAllText(ConfigPath, (useNewUi ? "newui" : "classic") + "|" + (isDark ? "dark" : "light"));
        }
        catch
        {
            // 写入失败也继续重启，按默认处理
        }
        var exe = Environment.ProcessPath;
        Application.Current.Shutdown();
        if (!string.IsNullOrEmpty(exe))
        {
            Process.Start(exe);
        }
    }

    /// <summary>按当前选择应用主题（经典 UI = 清空主题字典，系统默认外观）。</summary>
    public static void Apply()
    {
        var merged = Application.Current.Resources.MergedDictionaries;
        merged.Clear();
        if (!UseNewUi)
        {
            return;
        }
        merged.Add(new ResourceDictionary
        {
            Source = new Uri(IsDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml", UriKind.Relative)
        });
    }
}
