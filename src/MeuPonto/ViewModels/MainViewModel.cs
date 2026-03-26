using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using MeuPonto.Helpers;
using MeuPonto.Models;
using MeuPonto.Services.Interfaces;
using MeuPonto.Views;

namespace MeuPonto.ViewModels;

/// <summary>
/// ViewModel principal — exibe status, contador, ações e horários estimados.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IWorkSessionService _workSessionService;
    private readonly ISettingsService _settingsService;
    private readonly IHistoryService _historyService;
    private readonly IAlertService _alertService;
    private readonly ITrayIconService _trayIconService;

    private DispatcherQueueTimer? _uiTimer;
    private SecondaryWindow? _historyWindow;
    private SecondaryWindow? _settingsWindow;

    // ─── Status atual ────────────────────────────────────────────────

    [ObservableProperty]
    public partial string StatusText { get; set; } = "Fora do expediente";

    [ObservableProperty]
    public partial string CurrentTime { get; set; } = DateTime.Now.ToString("HH:mm:ss");

    [ObservableProperty]
    public partial string EntryTimeText { get; set; } = "--:--:--";

    [ObservableProperty]
    public partial string ElapsedTimeText { get; set; } = "00:00:00";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClockInCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClockOutCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResumeCommand))]
    [NotifyPropertyChangedFor(nameof(IsOffDuty))]
    [NotifyPropertyChangedFor(nameof(CanPause))]
    [NotifyPropertyChangedFor(nameof(CanResume))]
    public partial bool IsOnDuty { get; set; }

    public bool IsOffDuty => !IsOnDuty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResumeCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClockOutCommand))]
    [NotifyPropertyChangedFor(nameof(CanPause))]
    [NotifyPropertyChangedFor(nameof(CanResume))]
    public partial bool IsPaused { get; set; }

    [ObservableProperty]
    public partial string PauseTimeText { get; set; } = "";

    public bool CanPause => IsOnDuty && !IsPaused;
    public bool CanResume => IsOnDuty && IsPaused;

    // ─── Horários estimados de saída ─────────────────────────────────

    [ObservableProperty]
    public partial string EstimatedExitText { get; set; } = "--:--";

    [ObservableProperty]
    public partial string MaxExitText { get; set; } = "--:--";

    public MainViewModel(
        IWorkSessionService workSessionService,
        ISettingsService settingsService,
        IHistoryService historyService,
        IAlertService alertService,
        ITrayIconService trayIconService)
    {
        _workSessionService = workSessionService;
        _settingsService = settingsService;
        _historyService = historyService;
        _alertService = alertService;
        _trayIconService = trayIconService;

        _settingsService.SettingsChanged += (_, _) => RefreshEstimatedTimes();
    }

    /// <summary>
    /// Inicializa o ViewModel com o DispatcherQueue da UI.
    /// Deve ser chamado após a página carregar.
    /// </summary>
    public async Task InitializeAsync(DispatcherQueue dispatcherQueue)
    {
        // Restaurar sessão ativa (se houver)
        await _workSessionService.RestoreSessionAsync();
        if (_workSessionService.IsActive)
        {
            IsOnDuty = true;
            IsPaused = _workSessionService.IsPaused;
            EntryTimeText = TimeFormatHelper.FormatTime(_workSessionService.EntryTime!.Value);
            StatusText = _workSessionService.IsPaused ? "Pausado" : "Em jornada";
            RefreshEstimatedTimes();
            _alertService.StartMonitoring();
        }

        // Timer para atualizar UI a cada segundo
        _uiTimer = dispatcherQueue.CreateTimer();
        _uiTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTimer.Tick += OnTimerTick;
        _uiTimer.Start();
    }

    /// <summary>Cleanup ao sair da página.</summary>
    public void Cleanup()
    {
        _uiTimer?.Stop();
    }

    // ─── Bloco 2: Ações ─────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(IsOffDuty))]
    private async Task ClockIn()
    {
        var success = await _workSessionService.StartSessionAsync();
        if (!success) return;

        IsOnDuty = true;
        IsPaused = false;
        PauseTimeText = "";
        EntryTimeText = TimeFormatHelper.FormatTime(_workSessionService.EntryTime!.Value);
        StatusText = "Em jornada";
        ElapsedTimeText = "00:00:00";
        RefreshEstimatedTimes();

        _alertService.StartMonitoring();
        _trayIconService.UpdateTooltip("Meu Ponto — Em jornada");
    }

    [RelayCommand(CanExecute = nameof(IsOnDuty))]
    private async Task ClockOut()
    {
        var record = await _workSessionService.EndSessionAsync();
        if (record == null) return;

        IsOnDuty = false;
        IsPaused = false;
        PauseTimeText = "";
        StatusText = "Fora do expediente";
        EntryTimeText = "--:--:--";
        ElapsedTimeText = TimeFormatHelper.FormatDuration(record.TotalWorked);
        EstimatedExitText = "--:--";
        MaxExitText = "--:--";

        _alertService.StopMonitoring();
        _trayIconService.UpdateTooltip("Meu Ponto — Fora do expediente");
    }

    [RelayCommand(CanExecute = nameof(CanPause))]
    private async Task Pause()
    {
        var success = await _workSessionService.PauseSessionAsync();
        if (!success) return;

        IsPaused = true;
        StatusText = "Pausado";
        RefreshEstimatedTimes();
        _trayIconService.UpdateTooltip("Meu Ponto — Pausado");
    }

    [RelayCommand(CanExecute = nameof(CanResume))]
    private async Task Resume()
    {
        var success = await _workSessionService.ResumeSessionAsync();
        if (!success) return;

        IsPaused = false;
        StatusText = "Em jornada";
        RefreshEstimatedTimes();
        _trayIconService.UpdateTooltip("Meu Ponto — Em jornada");
    }

    [RelayCommand]
    private void OpenHistory()
    {
        if (_historyWindow != null)
        {
            _historyWindow.Activate();
            return;
        }

        _historyWindow = new SecondaryWindow(typeof(HistoryPage), "Histórico", 740, 520);
        _historyWindow.WindowClosed += (_, _) => _historyWindow = null;
        _historyWindow.Activate();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        if (_settingsWindow != null)
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SecondaryWindow(typeof(SettingsPage), "Configurações", 560, 680);
        _settingsWindow.WindowClosed += (_, _) => _settingsWindow = null;
        _settingsWindow.Activate();
    }

    // ─── Timer tick ──────────────────────────────────────────────────

    private void OnTimerTick(DispatcherQueueTimer sender, object args)
    {
        CurrentTime = DateTime.Now.ToString("HH:mm:ss");

        if (_workSessionService.IsActive)
        {
            var elapsed = _workSessionService.GetElapsedTime();
            ElapsedTimeText = TimeFormatHelper.FormatDuration(elapsed);

            var totalPause = _workSessionService.GetTotalPauseTime();
            PauseTimeText = totalPause > TimeSpan.Zero
                ? $"Pausa: {TimeFormatHelper.FormatDuration(totalPause)}"
                : "";

            RefreshEstimatedTimes();
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────

    /// <summary>Calcula e exibe os horários estimados de saída.</summary>
    private void RefreshEstimatedTimes()
    {
        if (_workSessionService.EntryTime == null)
        {
            EstimatedExitText = "--:--";
            MaxExitText = "--:--";
            return;
        }

        var entry = _workSessionService.EntryTime.Value;
        var settings = _settingsService.Settings;
        var totalPause = _workSessionService.GetTotalPauseTime();

        var estimatedExit = entry + settings.MainGoalHours + totalPause;
        var maxExit = entry + settings.MaxLimit + totalPause;

        EstimatedExitText = estimatedExit.ToString("HH:mm");
        MaxExitText = maxExit.ToString("HH:mm");
    }
}
