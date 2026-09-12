using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace PvZWSTools_WPF.Themes;

/// <summary>UI 风格与主题的运行时管理：经典 UI = master 分支的原生外观（仅覆写标签样式）。</summary>
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

    /// <summary>保存选择并重启应用使其生效（主题字典整体重载，渲染零残留）。</summary>
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

    /// <summary>按当前选择应用主题资源：NewUI 合并黑夜/白天地图，经典合并原生覆写。</summary>
    public static void Apply()
    {
        var merged = Application.Current.Resources.MergedDictionaries;
        merged.Clear();
        merged.Add(new ResourceDictionary
        {
            Source = new Uri(UseNewUi
                ? (IsDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml")
                : "Themes/ClassicOverrides.xaml", UriKind.Relative)
        });
    }

    /// <summary>窗口字体/颜色随模式刷新：NewUI 用主题画刷与雅黑；经典用系统默认字体。</summary>
    public static void RefreshWindowChrome(Window window)
    {
        if (UseNewUi)
        {
            window.FontFamily = new FontFamily("Microsoft YaHei UI");
            window.SetResourceReference(Window.BackgroundProperty, "BgRootBrush");
            window.SetResourceReference(Window.ForegroundProperty, "TextPrimaryBrush");
        }
        else
        {
            window.FontFamily = SystemFonts.MessageFontFamily;
            window.Background = SystemColors.WindowBrush;
            window.Foreground = SystemColors.ControlTextBrush;
        }
    }
}
