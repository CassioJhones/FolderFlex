using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FolderFlex.Util;

public sealed class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string str && !string.IsNullOrEmpty(str) ? Visibility.Visible : Visibility.Hidden;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
