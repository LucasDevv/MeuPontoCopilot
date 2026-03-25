using System.Text.Json;
using MeuPonto.Models;
using MeuPonto.Services.Interfaces;

namespace MeuPonto.Services;

/// <summary>
/// Implementação do serviço de configurações.
/// Persiste em settings.json no %LOCALAPPDATA%\MeuPonto\.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private AppSettings _settings = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public AppSettings Settings => _settings;

    public event EventHandler? SettingsChanged;

    public SettingsService()
    {
        var folder = GetDataFolder();
        _settingsPath = Path.Combine(folder, "settings.json");
    }

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (loaded != null)
                {
                    _settings = loaded;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsService] Erro ao carregar settings: {ex.Message}");
        }

        // Se não existir ou der erro, usa configurações padrão e salva
        _settings = new AppSettings();
        await SaveAsync(_settings);
    }

    public async Task SaveAsync(AppSettings settings)
    {
        try
        {
            _settings = settings;
            var json = JsonSerializer.Serialize(_settings, JsonOptions);
            var folder = Path.GetDirectoryName(_settingsPath)!;
            Directory.CreateDirectory(folder);
            await File.WriteAllTextAsync(_settingsPath, json);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsService] Erro ao salvar settings: {ex.Message}");
        }
    }

    private static string GetDataFolder()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(localAppData, "MeuPonto");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
