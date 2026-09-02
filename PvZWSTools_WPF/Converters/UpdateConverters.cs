using System;
using System.Globalization;
using System.Windows.Data;

namespace PvZWSTools_WPF.Converters;

/// <summary>double → bool：value == 0 → true（用于 IsIndeterminate 绑定，0 表示 Content-Length 未知）。</summary>
public class ZeroToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is double d) return d <= 0;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
