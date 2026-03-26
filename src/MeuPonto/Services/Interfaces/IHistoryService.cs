using MeuPonto.Models;

namespace MeuPonto.Services.Interfaces;

/// <summary>
/// Serviço de histórico de registros de ponto.
/// </summary>
public interface IHistoryService
{
    /// <summary>Carrega todos os registros do disco.</summary>
    Task<List<TimeRecord>> LoadRecordsAsync();

    /// <summary>Adiciona ou atualiza um registro no histórico.</summary>
    Task SaveRecordAsync(TimeRecord record);

    /// <summary>Atualiza um registro existente identificado pela data original.</summary>
    Task UpdateRecordAsync(DateTime originalDate, DateTime? originalEntryTime, TimeRecord updated);

    /// <summary>Retorna todos os registros em memória.</summary>
    List<TimeRecord> GetRecords();

    /// <summary>Retorna o último registro salvo.</summary>
    TimeRecord? GetLastRecord();

    /// <summary>Exporta registros para CSV.</summary>
    Task ExportCsvAsync(string filePath);

    /// <summary>Retorna o caminho da pasta de dados.</summary>
    string GetDataFolderPath();
}
