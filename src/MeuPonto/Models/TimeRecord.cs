namespace MeuPonto.Models;

/// <summary>
/// Representa um registro de ponto no histórico local.
/// </summary>
public class TimeRecord
{
    /// <summary>Data do registro (somente data, sem hora).</summary>
    public DateTime Date { get; set; }

    /// <summary>Horário de entrada.</summary>
    public DateTime? EntryTime { get; set; }

    /// <summary>Horário de saída.</summary>
    public DateTime? ExitTime { get; set; }

    /// <summary>Total trabalhado nessa sessão.</summary>
    public TimeSpan TotalWorked { get; set; }

    /// <summary>Tempo total em pausas.</summary>
    public TimeSpan TotalPauseTime { get; set; }

    /// <summary>Quantidade de pausas realizadas.</summary>
    public int PauseCount { get; set; }

    /// <summary>Status do registro (completo / incompleto).</summary>
    public RecordStatus Status { get; set; } = RecordStatus.Incomplete;
}
