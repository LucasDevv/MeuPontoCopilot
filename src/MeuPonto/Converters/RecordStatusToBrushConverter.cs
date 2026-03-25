using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MeuPonto.Converters;

/// <summary>
/// Converte status de registro (bool IsComplete) para cor de fundo.
/// Completo → verde suave, Incompleto → amarelo suave.
/// </summary>
public class RecordStatusToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush CompleteBrush =
        new(ColorHelper.FromArgb(255, 220, 245, 220)); // verde suave

    private static readonly SolidColorBrush IncompleteBrush =
        new(ColorHelper.FromArgb(255, 255, 243, 205)); // amarelo suave

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isComplete)
            return isComplete ? CompleteBrush : IncompleteBrush;
        return IncompleteBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
