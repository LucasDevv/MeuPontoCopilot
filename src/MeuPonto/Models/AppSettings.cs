using System.Text.Json.Serialization;
using MeuPonto.Helpers;

namespace MeuPonto.Models;

/// <summary>
/// Configurações do aplicativo, persistidas em settings.json.
/// </summary>
public class AppSettings
{
    /// <summary>Meta/carga principal de horas (ex.: 08:00).</summary>
    [JsonConverter(typeof(TimeSpanJsonConverter))]
    public TimeSpan MainGoalHours { get; set; } = TimeSpan.FromHours(8);

    /// <summary>Horário para começar alertas extras (ex.: 08:30).</summary>
    [JsonConverter(typeof(TimeSpanJsonConverter))]
    public TimeSpan AlertStartTime { get; set; } = new TimeSpan(8, 30, 0);

    /// <summary>Intervalo entre alertas extras (ex.: 15 min).</summary>
    [JsonConverter(typeof(TimeSpanJsonConverter))]
    public TimeSpan AlertInterval { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Limite máximo absoluto (ex.: 10:00).</summary>
    [JsonConverter(typeof(TimeSpanJsonConverter))]
    public TimeSpan MaxLimit { get; set; } = TimeSpan.FromHours(10);

    /// <summary>Pasta do arquivo de histórico. Vazio = pasta padrão.</summary>
    public string HistoryPath { get; set; } = string.Empty;

    /// <summary>Iniciar com o Windows.</summary>
    public bool StartWithWindows { get; set; }

    /// <summary>Minimizar para bandeja ao fechar a janela.</summary>
    public bool MinimizeToTrayOnClose { get; set; } = true;

    /// <summary>Habilitar som nas notificações.</summary>
    public bool EnableSound { get; set; } = true;

    /// <summary>Cria uma cópia profunda das configurações.</summary>
    public AppSettings Clone()
    {
        return new AppSettings
        {
            MainGoalHours = MainGoalHours,
            AlertStartTime = AlertStartTime,
            AlertInterval = AlertInterval,
            MaxLimit = MaxLimit,
            HistoryPath = HistoryPath,
            StartWithWindows = StartWithWindows,
            MinimizeToTrayOnClose = MinimizeToTrayOnClose,
            EnableSound = EnableSound
        };
    }
}
