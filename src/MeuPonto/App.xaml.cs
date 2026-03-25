using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MeuPonto.Services;
using MeuPonto.Services.Interfaces;
using MeuPonto.ViewModels;

namespace MeuPonto;

/// <summary>
/// Ponto de entrada do aplicativo.
/// Configura DI, carrega serviços e cria a janela principal.
/// </summary>
public partial class App : Application
{
    private static IServiceProvider _serviceProvider = null!;
    private MainWindow? _mainWindow;

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Configurar DI
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Carregar configurações e histórico
        var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        await settingsService.LoadAsync();

        var historyService = _serviceProvider.GetRequiredService<IHistoryService>();
        await historyService.LoadRecordsAsync();

        // Inicializar ícone na bandeja
        var trayService = _serviceProvider.GetRequiredService<ITrayIconService>();
        trayService.Initialize();

        // Conectar eventos da bandeja
        WireTrayEvents(trayService);

        // Criar janela principal
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }

    /// <summary>Resolve serviço do container DI.</summary>
    public static T GetService<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    /// <summary>Acesso à janela principal.</summary>
    public static MainWindow? MainWindowInstance =>
        (Current as App)?._mainWindow;

    private static void ConfigureServices(IServiceCollection services)
    {
        // Serviços (singletons — mantêm estado)
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IHistoryService, HistoryService>();
        services.AddSingleton<IWorkSessionService, WorkSessionService>();
        services.AddSingleton<ITrayIconService, TrayIconService>();
        services.AddSingleton<IAlertService, AlertService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels (transient — nova instância por resolução)
        services.AddTransient<MainViewModel>();
        services.AddTransient<HistoryViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    private void WireTrayEvents(ITrayIconService tray)
    {
        tray.ShowWindowRequested += (_, _) =>
        {
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.Activate();
            }
        };

        tray.ClockInRequested += async (_, _) =>
        {
            var session = _serviceProvider.GetRequiredService<IWorkSessionService>();
            if (!session.IsActive)
            {
                await session.StartSessionAsync();
                var alert = _serviceProvider.GetRequiredService<IAlertService>();
                alert.StartMonitoring();
                tray.UpdateTooltip("Meu Ponto — Em jornada");
            }
        };

        tray.ClockOutRequested += async (_, _) =>
        {
            var session = _serviceProvider.GetRequiredService<IWorkSessionService>();
            if (session.IsActive)
            {
                await session.EndSessionAsync();
                var alert = _serviceProvider.GetRequiredService<IAlertService>();
                alert.StopMonitoring();
                tray.UpdateTooltip("Meu Ponto — Fora do expediente");
            }
        };

        tray.ExitRequested += (_, _) =>
        {
            tray.Dispose();
            _mainWindow?.Close();
            Environment.Exit(0);
        };
    }
}
