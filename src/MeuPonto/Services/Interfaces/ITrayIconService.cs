namespace MeuPonto.Services.Interfaces;

/// <summary>
/// Serviço de ícone na bandeja do sistema (system tray).
/// </summary>
public interface ITrayIconService : IDisposable
{
    /// <summary>Inicializa o ícone na bandeja.</summary>
    void Initialize();

    /// <summary>Exibe uma notificação (balloon tip) na bandeja.</summary>
    void ShowNotification(string title, string message, bool isCritical = false, bool playSound = true);

    /// <summary>Atualiza o tooltip do ícone na bandeja.</summary>
    void UpdateTooltip(string text);

    /// <summary>Evento: usuário clicou em "Abrir" na bandeja.</summary>
    event EventHandler? ShowWindowRequested;

    /// <summary>Evento: usuário clicou em "Bater ponto de entrada" na bandeja.</summary>
    event EventHandler? ClockInRequested;

    /// <summary>Evento: usuário clicou em "Bater ponto de saída" na bandeja.</summary>
    event EventHandler? ClockOutRequested;

    /// <summary>Evento: usuário clicou em "Sair" na bandeja.</summary>
    event EventHandler? ExitRequested;
}
