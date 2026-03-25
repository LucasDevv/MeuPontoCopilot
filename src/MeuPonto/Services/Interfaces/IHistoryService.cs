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

    /// <summary>Retorna todos os registros em memória.</summary>
    List<TimeRecord> GetRecords();

    /// <summary>Retorna o último registro salvo.</summary>
    TimeRecord? GetLastRecord();

    /// <summary>Exporta registros para CSV.</summary>
    Task ExportCsvAsync(string filePath);

    /// <summary>Retorna o caminho da pasta de dados.</summary>
    string GetDataFolderPath();
}
