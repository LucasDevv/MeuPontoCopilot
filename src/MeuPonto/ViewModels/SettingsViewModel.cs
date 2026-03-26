using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuPonto.Helpers;
using MeuPonto.Models;
using MeuPonto.Services.Interfaces;

namespace MeuPonto.ViewModels;

/// <summary>
/// ViewModel da tela de configurações.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    // Campos de configuração (mapeados como HH:mm string para edição fácil)

    [ObservableProperty]
    public partial string MainGoalHours { get; set; } = "08:00";

    [ObservableProperty]
    public partial string AlertStartTime { get; set; } = "08:30";

    [ObservableProperty]
    public partial string AlertInterval { get; set; } = "00:15";

    [ObservableProperty]
    public partial string MaxLimit { get; set; } = "10:00";

    [ObservableProperty]
    public partial string HistoryPath { get; set; } = "";

    [ObservableProperty]
    public partial bool StartWithWindows { get; set; }

    [ObservableProperty]
    public partial bool MinimizeToTrayOnClose { get; set; } = true;

    [ObservableProperty]
    public partial bool EnableSound { get; set; } = true;

    [ObservableProperty]
    public partial string PlatformUrl { get; set; } = "";

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "";

    [ObservableProperty]
    public partial bool HasError { get; set; }

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>Carrega as configurações atuais nos campos.</summary>
    public void Load()
    {
        var s = _settingsService.Settings;
        MainGoalHours = TimeFormatHelper.FormatDurationShort(s.MainGoalHours);
        AlertStartTime = TimeFormatHelper.FormatDurationShort(s.AlertStartTime);
        AlertInterval = TimeFormatHelper.FormatDurationShort(s.AlertInterval);
        MaxLimit = TimeFormatHelper.FormatDurationShort(s.MaxLimit);
        HistoryPath = s.HistoryPath;
        StartWithWindows = s.StartWithWindows;
        MinimizeToTrayOnClose = s.MinimizeToTrayOnClose;
        EnableSound = s.EnableSound;
        PlatformUrl = s.PlatformUrl;
        StatusMessage = "";
        HasError = false;
    }

    [RelayCommand]
    private async Task Save()
    {
        // Validar campos
        if (!TryParseTime(MainGoalHours, out var goal))
        {
            SetError("Meta principal inválida. Use formato HH:mm.");
            return;
        }
        if (!TryParseTime(AlertStartTime, out var alertStart))
        {
            SetError("Início de alerta extra inválido. Use formato HH:mm.");
            return;
        }
        if (!TryParseTime(AlertInterval, out var interval))
        {
            SetError("Intervalo de alertas inválido. Use formato HH:mm.");
            return;
        }
        if (!TryParseTime(MaxLimit, out var maxLimit))
        {
            SetError("Limite máximo inválido. Use formato HH:mm.");
            return;
        }

        // Validar regras de negócio
        if (goal <= TimeSpan.Zero)
        {
            SetError("Meta principal deve ser maior que zero.");
            return;
        }
        if (alertStart < goal)
        {
            SetError("Início de alerta extra deve ser >= meta principal.");
            return;
        }
        if (interval <= TimeSpan.Zero)
        {
            SetError("Intervalo de alertas deve ser maior que zero.");
            return;
        }
        if (maxLimit <= goal)
        {
            SetError("Limite máximo deve ser maior que a meta principal.");
            return;
        }

        var settings = new AppSettings
        {
            MainGoalHours = goal,
            AlertStartTime = alertStart,
            AlertInterval = interval,
            MaxLimit = maxLimit,
            HistoryPath = HistoryPath?.Trim() ?? "",
            StartWithWindows = StartWithWindows,
            MinimizeToTrayOnClose = MinimizeToTrayOnClose,
            EnableSound = EnableSound,
            PlatformUrl = PlatformUrl?.Trim() ?? ""
        };

        await _settingsService.SaveAsync(settings);

        HasError = false;
        StatusMessage = "✓ Configurações salvas com sucesso!";
    }

    [RelayCommand]
    private void Cancel()
    {
        // Janela secundária — o usuário fecha pelo botão X
    }

    private static bool TryParseTime(string input, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Aceita "HH:mm" ou "HH:mm:ss"
        if (TimeSpan.TryParseExact(input.Trim(), [@"hh\:mm", @"hh\:mm\:ss", @"h\:mm"], null, out result))
            return true;

        if (TimeSpan.TryParse(input.Trim(), out result))
            return true;

        return false;
    }

    private void SetError(string message)
    {
        HasError = true;
        StatusMessage = message;
    }
}
