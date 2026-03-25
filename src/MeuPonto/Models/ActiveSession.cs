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
}
