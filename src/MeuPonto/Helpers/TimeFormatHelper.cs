using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeuPonto.Helpers;

/// <summary>
/// Utilitários para formatação de tempo no app.
/// </summary>
public static class TimeFormatHelper
{
    /// <summary>Formata TimeSpan como "HH:mm:ss".</summary>
    public static string FormatDuration(TimeSpan ts)
    {
        var totalHours = (int)ts.TotalHours;
        return $"{totalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    /// <summary>Formata TimeSpan como "HH:mm" (sem segundos).</summary>
    public static string FormatDurationShort(TimeSpan ts)
    {
        var totalHours = (int)ts.TotalHours;
        return $"{totalHours:D2}:{ts.Minutes:D2}";
    }

    /// <summary>Formata DateTime como "HH:mm:ss".</summary>
    public static string FormatTime(DateTime dt)
    {
        return dt.ToString("HH:mm:ss");
    }

    /// <summary>Formata DateTime como "dd/MM/yyyy".</summary>
    public static string FormatDate(DateTime dt)
    {
        return dt.ToString("dd/MM/yyyy");
    }
}

/// <summary>
/// Conversor JSON para TimeSpan (formato "hh:mm:ss" ou "d.hh:mm:ss").
/// </summary>
public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrEmpty(str))
            return TimeSpan.Zero;

        if (TimeSpan.TryParse(str, out var result))
            return result;

        return TimeSpan.Zero;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
    }
}
