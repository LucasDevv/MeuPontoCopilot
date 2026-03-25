using Microsoft.UI.Xaml.Data;

namespace MeuPonto.Converters;

/// <summary>
/// Converte WorkStatus para texto legível em português.
/// </summary>
public class WorkStatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isOnDuty)
            return isOnDuty ? "Em jornada" : "Fora do expediente";
        return "Desconhecido";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
