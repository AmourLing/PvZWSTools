using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using PvZWSTools_Shared.Helpers;

namespace PvZWSTools_WPF.Themes;

/// <summary>UI 风格与主题的运行时管理：
/// 经典 UI = master 原生外观（白天原生覆写 / 夜间零模板深色换色），
/// NewUI = 自定义圆角主题（黑夜/白天两套）。</summary>
public static class UiThemeManager
{
    private static readonly string ConfigPath =
        Path.Combine(AppContext.BaseDirectory, "ui_theme.cfg");

    /// <summary>当前是否使用 NewUI（false = 经典 UI）。</summary>
    public static bool UseNewUi { get; private set; } = true;

    /// <summary>主题：true = 黑夜，false = 白天。</summary>
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
        catch (Exception ex)
        {
            Log.Error("UI 配置读取失败: " + ex);
        }
        Log.Info(string.Format("UI 模式: UseNewUi={0}, IsDark={1}", UseNewUi, IsDark));
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

    /// <summary>按当前选择应用主题资源：
    /// NewUI = 黑夜/白天地图；经典 = 白天原生覆写、夜间零模板深色换色
    /// （只换颜色不动模板，勾叉等控件几何与 master 完全一致）。</summary>
    public static void Apply()
    {
        var merged = Application.Current.Resources.MergedDictionaries;
        merged.Clear();
        string source = UseNewUi
            ? (IsDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml")
            : (IsDark ? "Themes/ClassicDarkOverrides.xaml" : "Themes/ClassicOverrides.xaml");
        merged.Add(new ResourceDictionary { Source = new Uri(source, UriKind.Relative) });
        if(UseNewUi)
        {
            // 单元卡片模板与配色无关，黑夜/白天共用一份
            merged.Add(new ResourceDictionary { Source = new Uri("Themes/UnitTemplates.xaml", UriKind.Relative) });
        }
    }

    /// <summary>窗口底色/文字随模式刷新：NewUI 用主题画刷；
    /// 经典白天用系统默认；经典夜间用深色（与 ClassicDarkOverrides 一致）。</summary>
    public static void RefreshWindowChrome(Window window)
    {
        if (UseNewUi)
        {
            window.SetResourceReference(Window.BackgroundProperty, "BgRootBrush");
            window.SetResourceReference(Window.ForegroundProperty, "TextPrimaryBrush");
        }
        else if (IsDark)
        {
            window.Background = new SolidColorBrush(Color.FromRgb(0x1B, 0x1E, 0x24));
            window.Foreground = new SolidColorBrush(Color.FromRgb(0xE8, 0xEA, 0xED));
        }
        else
        {
            window.Background = SystemColors.WindowBrush;
            window.Foreground = SystemColors.ControlTextBrush;
        }
    }
}
