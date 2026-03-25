using MeuPonto.Models;

namespace MeuPonto.Services.Interfaces;

/// <summary>
/// Serviço de configurações do usuário.
/// </summary>
public interface ISettingsService
{
    /// <summary>Configurações atuais.</summary>
    AppSettings Settings { get; }

    /// <summary>Carrega configurações do disco.</summary>
    Task LoadAsync();

    /// <summary>Salva configurações no disco.</summary>
    Task SaveAsync(AppSettings settings);

    /// <summary>Evento disparado quando as configurações mudam.</summary>
    event EventHandler? SettingsChanged;
}
