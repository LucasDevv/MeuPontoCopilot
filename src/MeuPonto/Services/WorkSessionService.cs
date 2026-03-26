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
    public bool IsPaused => _activeSession?.IsPaused == true;
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
            IsActive = true,
            IsPaused = false,
            Pauses = []
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

        // Se estiver pausado, encerra a pausa ativa antes de finalizar
        if (_activeSession.IsPaused)
        {
            var activePause = _activeSession.Pauses.LastOrDefault(p => p.End == null);
            if (activePause != null)
                activePause.End = DateTime.Now;

            _activeSession.IsPaused = false;
        }

        var exitTime = DateTime.Now;
        var totalPauseTime = CalculateTotalPauseTime();
        var totalWorked = (exitTime - _activeSession.EntryTime) - totalPauseTime;
        if (totalWorked < TimeSpan.Zero)
            totalWorked = TimeSpan.Zero;

        var record = new TimeRecord
        {
            Date = _activeSession.EntryTime.Date,
            EntryTime = _activeSession.EntryTime,
            ExitTime = exitTime,
            TotalWorked = totalWorked,
            TotalPauseTime = totalPauseTime,
            PauseCount = _activeSession.Pauses.Count,
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

        var totalPauseTime = CalculateTotalPauseTime();
        var elapsed = (DateTime.Now - _activeSession.EntryTime) - totalPauseTime;
        return elapsed > TimeSpan.Zero ? elapsed : TimeSpan.Zero;
    }

    public TimeSpan GetTotalPauseTime()
    {
        if (!IsActive || _activeSession == null)
            return TimeSpan.Zero;

        return CalculateTotalPauseTime();
    }

    public async Task<bool> PauseSessionAsync()
    {
        if (!IsActive || _activeSession == null || _activeSession.IsPaused)
            return false;

        _activeSession.IsPaused = true;
        _activeSession.Pauses.Add(new PauseRecord { Start = DateTime.Now });

        await PersistSessionAsync();
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public async Task<bool> ResumeSessionAsync()
    {
        if (!IsActive || _activeSession == null || !_activeSession.IsPaused)
            return false;

        var activePause = _activeSession.Pauses.LastOrDefault(p => p.End == null);
        if (activePause != null)
            activePause.End = DateTime.Now;

        _activeSession.IsPaused = false;

        await PersistSessionAsync();
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return true;
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

    private TimeSpan CalculateTotalPauseTime()
    {
        if (_activeSession?.Pauses == null || _activeSession.Pauses.Count == 0)
            return TimeSpan.Zero;

        return _activeSession.Pauses.Aggregate(TimeSpan.Zero, (total, p) => total + p.Duration);
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
