using MeuPonto.Models;

namespace MeuPonto.Services.Interfaces;

/// <summary>
/// Serviço de controle de jornada de trabalho.
/// </summary>
public interface IWorkSessionService
{
    /// <summary>Indica se há uma jornada ativa.</summary>
    bool IsActive { get; }

    /// <summary>Indica se a sessão está pausada.</summary>
    bool IsPaused { get; }

    /// <summary>Horário de entrada da sessão ativa, ou null.</summary>
    DateTime? EntryTime { get; }

    /// <summary>Inicia uma nova jornada. Retorna false se já houver jornada ativa.</summary>
    Task<bool> StartSessionAsync();

    /// <summary>Encerra a jornada ativa. Retorna o registro ou null se não houver jornada.</summary>
    Task<TimeRecord?> EndSessionAsync();

    /// <summary>Pausa a jornada ativa. Retorna false se não houver jornada ou já estiver pausada.</summary>
    Task<bool> PauseSessionAsync();

    /// <summary>Retoma a jornada pausada. Retorna false se não estiver pausada.</summary>
    Task<bool> ResumeSessionAsync();

    /// <summary>Calcula o tempo efetivamente trabalhado (descontando pausas).</summary>
    TimeSpan GetElapsedTime();

    /// <summary>Calcula o tempo total em pausas (incluindo pausa ativa).</summary>
    TimeSpan GetTotalPauseTime();

    /// <summary>Restaura a sessão ativa do disco (ao abrir o app).</summary>
    Task RestoreSessionAsync();

    /// <summary>Evento disparado quando o estado da sessão muda.</summary>
    event EventHandler? SessionChanged;
}
