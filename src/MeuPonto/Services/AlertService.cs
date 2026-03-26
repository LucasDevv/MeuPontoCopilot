using MeuPonto.Models;
using MeuPonto.Services.Interfaces;

namespace MeuPonto.Services;

/// <summary>
/// Implementação do serviço de alertas.
/// Monitora a jornada ativa e dispara notificações conforme regras configuradas.
/// Usa PeriodicTimer para verificação periódica independente da UI.
/// </summary>
public class AlertService : IAlertService
{
    private readonly IWorkSessionService _workSessionService;
    private readonly ISettingsService _settingsService;
    private readonly ITrayIconService _trayIconService;

    private readonly AlertState _alertState = new();
    private CancellationTokenSource? _cts;
    private Task? _monitorTask;

    public AlertService(
        IWorkSessionService workSessionService,
        ISettingsService settingsService,
        ITrayIconService trayIconService)
    {
        _workSessionService = workSessionService;
        _settingsService = settingsService;
        _trayIconService = trayIconService;
    }

    public void StartMonitoring()
    {
        StopMonitoring();
        ResetAlerts();

        _cts = new CancellationTokenSource();
        _monitorTask = MonitorLoopAsync(_cts.Token);
    }

    public void StopMonitoring()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        _monitorTask = null;
    }

    public void ResetAlerts()
    {
        _alertState.Reset();
    }

    private async Task MonitorLoopAsync(CancellationToken ct)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (await timer.WaitForNextTickAsync(ct))
            {
                if (!_workSessionService.IsActive || _workSessionService.IsPaused)
                    continue;

                CheckAlerts();
            }
        }
        catch (OperationCanceledException)
        {
            // Esperado ao parar o monitoramento
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AlertService] Erro no loop de monitoramento: {ex.Message}");
        }
    }

    private void CheckAlerts()
    {
        var elapsed = _workSessionService.GetElapsedTime();
        var settings = _settingsService.Settings;
        var enableSound = settings.EnableSound;

        // 1. Alerta de meta principal (dispara 1 vez)
        if (!_alertState.GoalAlerted && elapsed >= settings.MainGoalHours)
        {
            _alertState.GoalAlerted = true;
            _trayIconService.ShowNotification(
                "🎯 Meta atingida!",
                $"Você completou {Helpers.TimeFormatHelper.FormatDurationShort(settings.MainGoalHours)} de jornada. " +
                "Hora de bater o ponto de saída!",
                isCritical: false,
                playSound: enableSound);
        }

        // 2. Alertas extras periódicos (após o AlertStartTime)
        if (_alertState.GoalAlerted && !_alertState.MaxLimitAlerted && elapsed >= settings.AlertStartTime)
        {
            var timeSinceAlertStart = elapsed - settings.AlertStartTime;
            var expectedAlerts = (int)(timeSinceAlertStart / settings.AlertInterval) + 1;

            if (expectedAlerts > _alertState.ExtraAlertsCount)
            {
                _alertState.ExtraAlertsCount = expectedAlerts;
                _alertState.LastExtraAlertTime = DateTime.Now;

                var overtime = elapsed - settings.MainGoalHours;
                _trayIconService.ShowNotification(
                    "⏰ Hora extra!",
                    $"Jornada: {Helpers.TimeFormatHelper.FormatDurationShort(elapsed)}. " +
                    $"Hora extra: +{Helpers.TimeFormatHelper.FormatDurationShort(overtime)}.",
                    isCritical: false,
                    playSound: enableSound);
            }
        }

        // 3. Alerta de limite máximo (dispara 1 vez, mais forte)
        if (!_alertState.MaxLimitAlerted && elapsed >= settings.MaxLimit)
        {
            _alertState.MaxLimitAlerted = true;
            _trayIconService.ShowNotification(
                "🚨 LIMITE MÁXIMO ATINGIDO!",
                $"Você atingiu o limite de {Helpers.TimeFormatHelper.FormatDurationShort(settings.MaxLimit)}! " +
                "Bata o ponto de saída AGORA!",
                isCritical: true,
                playSound: enableSound);
        }
    }
}
