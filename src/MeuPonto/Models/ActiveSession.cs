namespace MeuPonto.Models;

/// <summary>
/// Sessão ativa de jornada (persistida para recuperação após fechar o app).
/// </summary>
public class ActiveSession
{
    /// <summary>Horário de entrada da sessão ativa.</summary>
    public DateTime EntryTime { get; set; }

    /// <summary>Indica se há uma jornada ativa.</summary>
    public bool IsActive { get; set; }

    /// <summary>Indica se a sessão está pausada.</summary>
    public bool IsPaused { get; set; }

    /// <summary>Registros de pausas da sessão.</summary>
    public List<PauseRecord> Pauses { get; set; } = [];
}
