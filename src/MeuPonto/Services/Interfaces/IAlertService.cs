namespace MeuPonto.Services.Interfaces;

/// <summary>
/// Serviço de alertas e notificações da jornada.
/// </summary>
public interface IAlertService
{
    /// <summary>Inicia o monitoramento de alertas para a sessão ativa.</summary>
    void StartMonitoring();

    /// <summary>Para o monitoramento de alertas.</summary>
    void StopMonitoring();

    /// <summary>Reseta o estado dos alertas (para nova sessão).</summary>
    void ResetAlerts();
}
