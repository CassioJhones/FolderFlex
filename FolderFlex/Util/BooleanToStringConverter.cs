using System.Windows.Data;

namespace FolderFlex.Util;
public class BooleanToStringConverter : IValueConverter
{
    public required string TrueValue { get; set; }
    public required string FalseValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => (bool)value ? TrueValue : FalseValue;
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}
