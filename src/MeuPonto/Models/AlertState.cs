namespace MeuPonto.Models;

/// <summary>
/// Controle interno de quais alertas já foram disparados na sessão atual.
/// Evita repetição indevida de notificações.
/// </summary>
public class AlertState
{
    /// <summary>Alerta de meta principal já enviado.</summary>
    public bool GoalAlerted { get; set; }

    /// <summary>Alerta de limite máximo já enviado.</summary>
    public bool MaxLimitAlerted { get; set; }

    /// <summary>Quantidade de alertas extras já enviados.</summary>
    public int ExtraAlertsCount { get; set; }

    /// <summary>Horário do último alerta extra enviado.</summary>
    public DateTime? LastExtraAlertTime { get; set; }

    /// <summary>Reseta todos os estados de alerta.</summary>
    public void Reset()
    {
        GoalAlerted = false;
        MaxLimitAlerted = false;
        ExtraAlertsCount = 0;
        LastExtraAlertTime = null;
    }
}
