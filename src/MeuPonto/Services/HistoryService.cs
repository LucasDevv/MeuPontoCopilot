using System.Text;
using System.Text.Json;
using MeuPonto.Models;
using MeuPonto.Services.Interfaces;

namespace MeuPonto.Services;

/// <summary>
/// Implementação do serviço de histórico.
/// Persiste em time-records.json no %LOCALAPPDATA%\MeuPonto\.
/// </summary>
public class HistoryService : IHistoryService
{
    private readonly ISettingsService _settingsService;
    private List<TimeRecord> _records = [];
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public HistoryService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<List<TimeRecord>> LoadRecordsAsync()
    {
        try
        {
            var path = GetRecordsPath();
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);
                var loaded = JsonSerializer.Deserialize<List<TimeRecord>>(json, JsonOptions);
                if (loaded != null)
                {
                    lock (_lock)
                    {
                        _records = loaded;
                    }
                    return _records;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] Erro ao carregar registros: {ex.Message}");
        }

        lock (_lock)
        {
            _records = [];
        }
        return _records;
    }

    public async Task SaveRecordAsync(TimeRecord record)
    {
        lock (_lock)
        {
            // Evita duplicidade — busca registro existente pela data + entrada
            var existing = _records.FindIndex(r =>
                r.Date.Date == record.Date.Date &&
                r.EntryTime == record.EntryTime);

            if (existing >= 0)
                _records[existing] = record;
            else
                _records.Add(record);
        }

        await PersistRecordsAsync();
    }

    public async Task UpdateRecordAsync(DateTime originalDate, DateTime? originalEntryTime, TimeRecord updated)
    {
        lock (_lock)
        {
            var index = _records.FindIndex(r =>
                r.Date.Date == originalDate.Date &&
                r.EntryTime == originalEntryTime);

            if (index >= 0)
                _records[index] = updated;
        }

        await PersistRecordsAsync();
    }

    public List<TimeRecord> GetRecords()
    {
        lock (_lock)
        {
            return [.. _records.OrderByDescending(r => r.Date).ThenByDescending(r => r.EntryTime)];
        }
    }

    public TimeRecord? GetLastRecord()
    {
        lock (_lock)
        {
            return _records
                .OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.EntryTime)
                .FirstOrDefault();
        }
    }

    public async Task ExportCsvAsync(string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Data;Entrada;Saída;Total;Pausas;Status;Editado");

        List<TimeRecord> snapshot;
        lock (_lock)
        {
            snapshot = [.. _records.OrderByDescending(r => r.Date)];
        }

        foreach (var r in snapshot)
        {
            var date = r.Date.ToString("dd/MM/yyyy");
            var entry = r.EntryTime?.ToString("HH:mm:ss") ?? "--:--:--";
            var exit = r.ExitTime?.ToString("HH:mm:ss") ?? "--:--:--";
            var total = Helpers.TimeFormatHelper.FormatDuration(r.TotalWorked);
            var pauses = r.PauseCount > 0
                ? $"{Helpers.TimeFormatHelper.FormatDurationShort(r.TotalPauseTime)} ({r.PauseCount}x)"
                : "--";
            var status = r.Status == RecordStatus.Complete ? "Completo" : "Incompleto";
            var edited = r.IsManuallyEdited ? $"Sim ({r.LastEditedAt:dd/MM/yyyy HH:mm})" : "Não";
            sb.AppendLine($"{date};{entry};{exit};{total};{pauses};{status};{edited}");
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }

    public string GetDataFolderPath()
    {
        return GetDataFolder();
    }

    private async Task PersistRecordsAsync()
    {
        try
        {
            var path = GetRecordsPath();
            var folder = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(folder);

            List<TimeRecord> snapshot;
            lock (_lock)
            {
                snapshot = [.. _records];
            }

            var json = JsonSerializer.Serialize(snapshot, JsonOptions);
            await File.WriteAllTextAsync(path, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] Erro ao persistir registros: {ex.Message}");
        }
    }

    private string GetRecordsPath()
    {
        var folder = GetDataFolder();
        return Path.Combine(folder, "time-records.json");
    }

    private string GetDataFolder()
    {
        var custom = _settingsService.Settings.HistoryPath;
        if (!string.IsNullOrWhiteSpace(custom) && Directory.Exists(custom))
            return custom;

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(localAppData, "MeuPonto");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
