using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PvZWSTools_WPF.Converters;

/// <summary>把日志行的控制台颜色换成界面用的画刷。
/// 默认色走主题画刷（浅色主题下控制台的"灰"会变成看不见），其余颜色用一组
/// 深浅背景都能读的中途色，而不是控制台那套原色。</summary>
public sealed class ConsoleColorConverter:IValueConverter
{
    private static readonly Dictionary<ConsoleColor, string> Themed = new()
    {
        [ConsoleColor.Black] = "TextPrimaryBrush",
        [ConsoleColor.DarkGray] = "TextSecondaryBrush",
        [ConsoleColor.Gray] = "TextPrimaryBrush",
        [ConsoleColor.White] = "TextPrimaryBrush",
        [ConsoleColor.DarkRed] = "DangerBrush",
        [ConsoleColor.Red] = "DangerBrush",
    };

    private static readonly Dictionary<ConsoleColor, Color> Fixed = new()
    {
        [ConsoleColor.DarkYellow] = Color.FromRgb(0xB5, 0x89, 0x00),
        [ConsoleColor.Yellow] = Color.FromRgb(0xD7, 0xA2, 0x1B),
        [ConsoleColor.DarkGreen] = Color.FromRgb(0x2E, 0x7D, 0x32),
        [ConsoleColor.Green] = Color.FromRgb(0x4C, 0x9A, 0x2A),
        [ConsoleColor.DarkCyan] = Color.FromRgb(0x1B, 0x5F, 0xB5),
        [ConsoleColor.Cyan] = Color.FromRgb(0x2F, 0x80, 0xED),
        [ConsoleColor.DarkMagenta] = Color.FromRgb(0x7B, 0x33, 0x9B),
        [ConsoleColor.Magenta] = Color.FromRgb(0xB3, 0x4F, 0xD1),
        [ConsoleColor.Blue] = Color.FromRgb(0x2F, 0x80, 0xED),
        [ConsoleColor.DarkBlue] = Color.FromRgb(0x1B, 0x5F, 0xB5),
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is ConsoleColor color
           && Themed.TryGetValue(color, out string key)
           && Application.Current.TryFindResource(key) is Brush themed)
            return themed;

        Color plain = value is ConsoleColor c && Fixed.TryGetValue(c, out Color hit)
            ? hit
            : Color.FromRgb(0x88, 0x88, 0x88);
        return new SolidColorBrush(plain);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
