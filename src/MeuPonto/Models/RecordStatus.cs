namespace MeuPonto.Models;

/// <summary>
/// Status de um registro de ponto no histórico.
/// </summary>
public enum RecordStatus
{
    /// <summary>Registro completo — entrada e saída registradas.</summary>
    Complete,

    /// <summary>Registro incompleto — falta saída ou houve problema.</summary>
    Incomplete
}
