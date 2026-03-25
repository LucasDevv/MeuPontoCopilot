using System.Text.Json;
using MeuPonto.Models;
using MeuPonto.Services.Interfaces;

namespace MeuPonto.Services;

/// <summary>
/// Implementação do serviço de controle de jornada.
/// Persiste sessão ativa em active-session.json para recuperação.
/// </summary>
public class WorkSessionService : IWorkSessionService
{
    private readonly IHistoryService _historyService;
    private ActiveSession? _activeSession;
    private readonly string _sessionPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public bool IsActive => _activeSession?.IsActive == true;
    public DateTime? EntryTime => _activeSession?.IsActive == true ? _activeSession.EntryTime : null;

    public event EventHandler? SessionChanged;

    public WorkSessionService(IHistoryService historyService)
    {
        _historyService = historyService;
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(localAppData, "MeuPonto");
        Directory.CreateDirectory(folder);
        _sessionPath = Path.Combine(folder, "active-session.json");
    }

    public async Task<bool> StartSessionAsync()
    {
        if (IsActive)
            return false;

        _activeSession = new ActiveSession
        {
            EntryTime = DateTime.Now,
            IsActive = true
        };

        await PersistSessionAsync();

        // Cria registro incompleto no histórico imediatamente
        var record = new TimeRecord
        {
            Date = _activeSession.EntryTime.Date,
            EntryTime = _activeSession.EntryTime,
            Status = RecordStatus.Incomplete
        };
        await _historyService.SaveRecordAsync(record);

        SessionChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public async Task<TimeRecord?> EndSessionAsync()
    {
        if (!IsActive || _activeSession == null)
            return null;

        var exitTime = DateTime.Now;
        var totalWorked = exitTime - _activeSession.EntryTime;

        var record = new TimeRecord
        {
            Date = _activeSession.EntryTime.Date,
            EntryTime = _activeSession.EntryTime,
            ExitTime = exitTime,
            TotalWorked = totalWorked,
            Status = RecordStatus.Complete
        };

        await _historyService.SaveRecordAsync(record);

        // Limpa sessão ativa
        _activeSession = null;
        await ClearSessionAsync();

        SessionChanged?.Invoke(this, EventArgs.Empty);
        return record;
    }

    public TimeSpan GetElapsedTime()
    {
        if (!IsActive || _activeSession == null)
            return TimeSpan.Zero;

        // Sempre calcula com DateTime real, nunca com timer
        return DateTime.Now - _activeSession.EntryTime;
    }

    public async Task RestoreSessionAsync()
    {
        try
        {
            if (File.Exists(_sessionPath))
            {
                var json = await File.ReadAllTextAsync(_sessionPath);
                var session = JsonSerializer.Deserialize<ActiveSession>(json, JsonOptions);
                if (session is { IsActive: true })
                {
                    _activeSession = session;
                    SessionChanged?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WorkSessionService] Erro ao restaurar sessão: {ex.Message}");
        }

        _activeSession = null;
    }

    private async Task PersistSessionAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_activeSession, JsonOptions);
            await File.WriteAllTextAsync(_sessionPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WorkSessionService] Erro ao persistir sessão: {ex.Message}");
        }
    }

    private async Task ClearSessionAsync()
    {
        try
        {
            if (File.Exists(_sessionPath))
                await Task.Run(() => File.Delete(_sessionPath));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WorkSessionService] Erro ao limpar sessão: {ex.Message}");
        }
    }
}
