using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MeuPonto.Converters;

/// <summary>
/// Converte bool para Visibility invertido (true → Collapsed, false → Visible).
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
            return b ? Visibility.Collapsed : Visibility.Visible;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility v && v == Visibility.Collapsed;
    }
}
