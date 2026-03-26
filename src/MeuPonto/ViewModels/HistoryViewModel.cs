using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using MeuPonto.Helpers;
using MeuPonto.Models;
using MeuPonto.Services.Interfaces;

namespace MeuPonto.ViewModels;

/// <summary>
/// ViewModel da tela de histórico de registros.
/// </summary>
public partial class HistoryViewModel : ObservableObject
{
    private readonly IHistoryService _historyService;

    [ObservableProperty]
    public partial ObservableCollection<TimeRecordDisplay> Records { get; set; } = [];

    [ObservableProperty]
    public partial DateTimeOffset? FilterDate { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string RecordCount { get; set; } = "0 registros";

    public HistoryViewModel(IHistoryService historyService)
    {
        _historyService = historyService;
    }

    /// <summary>Carrega registros ao entrar na tela.</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            await _historyService.LoadRecordsAsync();
            RefreshRecords();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ApplyFilter()
    {
        RefreshRecords();
    }

    [RelayCommand]
    private void ClearFilter()
    {
        FilterDate = null;
        RefreshRecords();
    }

    [RelayCommand]
    private void OpenDataFolder()
    {
        try
        {
            var path = _historyService.GetDataFolderPath();
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryViewModel] Erro ao abrir pasta: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ExportCsv()
    {
        try
        {
            var folder = _historyService.GetDataFolderPath();
            var fileName = $"meu-ponto-export-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
            var filePath = Path.Combine(folder, fileName);
            await _historyService.ExportCsvAsync(filePath);

            // Abre a pasta com o arquivo
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryViewModel] Erro ao exportar CSV: {ex.Message}");
        }
    }

    private void RefreshRecords()
    {
        var all = _historyService.GetRecords();

        if (FilterDate.HasValue)
        {
            var filterDateOnly = FilterDate.Value.Date;
            all = all.Where(r => r.Date.Date == filterDateOnly).ToList();
        }

        Records = new ObservableCollection<TimeRecordDisplay>(
            all.Select(r => new TimeRecordDisplay
            {
                Date = TimeFormatHelper.FormatDate(r.Date),
                EntryTime = r.EntryTime.HasValue ? TimeFormatHelper.FormatTime(r.EntryTime.Value) : "--:--:--",
                ExitTime = r.ExitTime.HasValue ? TimeFormatHelper.FormatTime(r.ExitTime.Value) : "--:--:--",
                TotalWorked = TimeFormatHelper.FormatDuration(r.TotalWorked),
                PauseTime = r.PauseCount > 0
                    ? $"{TimeFormatHelper.FormatDurationShort(r.TotalPauseTime)} ({r.PauseCount}x)"
                    : "--",
                Status = r.Status == RecordStatus.Complete ? "Completo" : "Incompleto",
                IsComplete = r.Status == RecordStatus.Complete
            }));

        RecordCount = $"{Records.Count} registro{(Records.Count != 1 ? "s" : "")}";
    }
}

/// <summary>
/// Modelo de exibição para registro no histórico (usado na ListView).
/// </summary>
public class TimeRecordDisplay
{
    private static readonly SolidColorBrush CompleteBrush = new(ColorHelper.FromArgb(255, 56, 142, 60));     // verde
    private static readonly SolidColorBrush IncompleteBrush = new(ColorHelper.FromArgb(255, 245, 124, 0));   // laranja

    public string Date { get; set; } = "";
    public string EntryTime { get; set; } = "";
    public string ExitTime { get; set; } = "";
    public string TotalWorked { get; set; } = "";
    public string PauseTime { get; set; } = "";
    public string Status { get; set; } = "";
    public bool IsComplete { get; set; }
    public SolidColorBrush StatusBadgeColor => IsComplete ? CompleteBrush : IncompleteBrush;
}
