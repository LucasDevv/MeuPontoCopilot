namespace MeuPonto.Models;

/// <summary>
/// Representa um registro de pausa dentro de uma sessão de trabalho.
/// </summary>
public class PauseRecord
{
    /// <summary>Momento em que a pausa foi iniciada.</summary>
    public DateTime Start { get; set; }

    /// <summary>Momento em que a pausa foi encerrada. Null se ainda ativa.</summary>
    public DateTime? End { get; set; }

    /// <summary>Duração da pausa (parcial se ainda ativa).</summary>
    public TimeSpan Duration => (End ?? DateTime.Now) - Start;
}
