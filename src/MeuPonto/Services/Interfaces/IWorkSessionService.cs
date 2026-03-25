using MeuPonto.Models;

namespace MeuPonto.Services.Interfaces;

/// <summary>
/// Serviço de controle de jornada de trabalho.
/// </summary>
public interface IWorkSessionService
{
    /// <summary>Indica se há uma jornada ativa.</summary>
    bool IsActive { get; }

    /// <summary>Horário de entrada da sessão ativa, ou null.</summary>
    DateTime? EntryTime { get; }

    /// <summary>Inicia uma nova jornada. Retorna false se já houver jornada ativa.</summary>
    Task<bool> StartSessionAsync();

    /// <summary>Encerra a jornada ativa. Retorna o registro ou null se não houver jornada.</summary>
    Task<TimeRecord?> EndSessionAsync();

    /// <summary>Calcula o tempo decorrido desde a entrada até agora.</summary>
    TimeSpan GetElapsedTime();

    /// <summary>Restaura a sessão ativa do disco (ao abrir o app).</summary>
    Task RestoreSessionAsync();

    /// <summary>Evento disparado quando o estado da sessão muda.</summary>
    event EventHandler? SessionChanged;
}
