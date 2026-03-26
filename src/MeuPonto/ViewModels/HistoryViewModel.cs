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
    private readonly IWorkSessionService _workSessionService;

    [ObservableProperty]
    public partial ObservableCollection<TimeRecordDisplay> Records { get; set; } = [];

    [ObservableProperty]
    public partial DateTimeOffset? FilterDate { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string RecordCount { get; set; } = "0 registros";

    // ─── Estado de edição ────────────────────────────────────────────

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string EditEntryTime { get; set; } = "";

    [ObservableProperty]
    public partial string EditExitTime { get; set; } = "";

    [ObservableProperty]
    public partial string EditTotalWorked { get; set; } = "";

    [ObservableProperty]
    public partial string EditValidationError { get; set; } = "";

    private TimeRecordDisplay? _editingRecord;
    private TimeRecord? _editingSource;

    public HistoryViewModel(IHistoryService historyService, IWorkSessionService workSessionService)
    {
        _historyService = historyService;
        _workSessionService = workSessionService;
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

    // ─── Edição de registros ─────────────────────────────────────────

    [RelayCommand]
    private void BeginEdit(TimeRecordDisplay? record)
    {
        if (record == null || !record.CanEdit)
            return;

        _editingRecord = record;
        _editingSource = record.SourceRecord;

        EditEntryTime = _editingSource?.EntryTime?.ToString("HH:mm") ?? "";
        EditExitTime = _editingSource?.ExitTime?.ToString("HH:mm") ?? "";
        EditValidationError = "";
        RecalculateEditTotal();

        IsEditing = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        _editingRecord = null;
        _editingSource = null;
        EditValidationError = "";
    }

    [RelayCommand]
    private async Task SaveEdit()
    {
        if (_editingSource == null || _editingRecord == null)
            return;

        var validation = ValidateEdit();
        if (validation != null)
        {
            EditValidationError = validation;
            return;
        }

        var originalDate = _editingSource.Date;
        var originalEntry = _editingSource.EntryTime;

        var entryTime = ParseTime(_editingSource.Date, EditEntryTime);
        var exitTime = string.IsNullOrWhiteSpace(EditExitTime) ? null : ParseTime(_editingSource.Date, EditExitTime);

        _editingSource.EntryTime = entryTime;
        _editingSource.ExitTime = exitTime;

        if (entryTime.HasValue && exitTime.HasValue)
        {
            _editingSource.TotalWorked = exitTime.Value - entryTime.Value;
            _editingSource.Status = RecordStatus.Complete;
        }
        else
        {
            _editingSource.TotalWorked = TimeSpan.Zero;
            _editingSource.Status = RecordStatus.Incomplete;
        }

        _editingSource.IsManuallyEdited = true;
        _editingSource.LastEditedAt = DateTime.Now;

        await _historyService.UpdateRecordAsync(originalDate, originalEntry, _editingSource);

        IsEditing = false;
        _editingRecord = null;
        _editingSource = null;

        RefreshRecords();
    }

    /// <summary>Recalcula o total exibido durante a edição.</summary>
    public void RecalculateEditTotal()
    {
        var entry = ParseTime(DateTime.Today, EditEntryTime);
        var exit = ParseTime(DateTime.Today, EditExitTime);

        if (entry.HasValue && exit.HasValue && exit.Value >= entry.Value)
        {
            var total = exit.Value - entry.Value;
            EditTotalWorked = TimeFormatHelper.FormatDuration(total);
        }
        else
        {
            EditTotalWorked = "--:--:--";
        }
    }

    private string? ValidateEdit()
    {
        if (string.IsNullOrWhiteSpace(EditEntryTime))
            return "Informe a hora de entrada.";

        var entry = ParseTime(DateTime.Today, EditEntryTime);
        if (entry == null)
            return "Hora de entrada inválida.";

        if (!string.IsNullOrWhiteSpace(EditExitTime))
        {
            var exit = ParseTime(DateTime.Today, EditExitTime);
            if (exit == null)
                return "Hora de saída inválida.";

            if (exit.Value < entry.Value)
                return "Saída deve ser maior ou igual à entrada.";
        }

        return null;
    }

    private static DateTime? ParseTime(DateTime date, string time)
    {
        if (string.IsNullOrWhiteSpace(time))
            return null;

        if (TimeSpan.TryParse(time, out var ts))
            return date.Date + ts;

        return null;
    }

    private void RefreshRecords()
    {
        var all = _historyService.GetRecords();

        if (FilterDate.HasValue)
        {
            var filterDateOnly = FilterDate.Value.Date;
            all = all.Where(r => r.Date.Date == filterDateOnly).ToList();
        }

        // Verifica se há sessão ativa para bloquear edição do dia atual
        var activeEntryDate = _workSessionService.IsActive
            ? _workSessionService.EntryTime?.Date
            : (DateTime?)null;

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
                IsComplete = r.Status == RecordStatus.Complete,
                IsManuallyEdited = r.IsManuallyEdited,
                EditedAtTooltip = r.LastEditedAt.HasValue
                    ? $"Editado em {r.LastEditedAt.Value:dd/MM/yyyy HH:mm}"
                    : "",
                CanEdit = activeEntryDate == null || r.Date.Date != activeEntryDate.Value.Date,
                SourceRecord = r
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
    public bool IsManuallyEdited { get; set; }
    public string EditedAtTooltip { get; set; } = "";
    public bool CanEdit { get; set; } = true;
    public TimeRecord? SourceRecord { get; set; }
    public SolidColorBrush StatusBadgeColor => IsComplete ? CompleteBrush : IncompleteBrush;
}
