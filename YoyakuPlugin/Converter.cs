using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
namespace YoyakuPlugin;

/// <summary>
/// boolを反転させる
/// </summary>
public class NotConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Not((bool)value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Not((bool)value);
    }
    private bool Not(bool b)
    {
        return !b;
    }
}
/// <summary>
/// DataGridLengthとdoubleの相互変換
/// </summary>
public class DataGridLengthValueConverter : IValueConverter
{
    private readonly DataGridLengthConverter _converter = new DataGridLengthConverter();
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return _converter.ConvertFrom(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var length = (DataGridLength)value;
        return length.Value;
    }
}
