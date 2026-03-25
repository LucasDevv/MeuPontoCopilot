namespace MeuPonto.Models;

/// <summary>
/// Status atual da jornada de trabalho.
/// </summary>
public enum WorkStatus
{
    /// <summary>Fora do expediente — sem jornada ativa.</summary>
    OffDuty,

    /// <summary>Em jornada — entrada registrada.</summary>
    OnDuty
}
