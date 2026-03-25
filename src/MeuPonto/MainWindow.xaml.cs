using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using MeuPonto.Services.Interfaces;
using MeuPonto.Views;
using Windows.Graphics;

namespace MeuPonto;

/// <summary>
/// Janela principal do aplicativo.
/// Janela compacta, sem maximizar, com titlebar integrada.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly ISettingsService _settingsService;
    private AppWindow? _appWindow;
    private bool _isHidingToTray;

    public MainWindow()
    {
        InitializeComponent();

        _settingsService = App.GetService<ISettingsService>();

        // Estender conteúdo até a titlebar (mesma cor do background)
        ExtendsContentIntoTitleBar = true;

        // Backdrop Acrylic para visual translúcido moderno
        SystemBackdrop = new DesktopAcrylicBackdrop();

        // Configurar janela
        ConfigureWindow();

        // Navegar para página principal
        ContentFrame.Navigate(typeof(MainPage));

        // Interceptar fechamento
        Closed += OnWindowClosed;
    }

    /// <summary>Mostra a janela (restaura de minimizado/oculto).</summary>
    public void Show()
    {
        if (_appWindow != null)
        {
            _appWindow.Show();
        }
    }

    private void ConfigureWindow()
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        if (_appWindow != null)
        {
            // Tamanho compacto — app de uso rápido
            _appWindow.Resize(new SizeInt32(410, 490));

            _appWindow.Title = "Meu Ponto";

            // Remover botão de maximizar
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsMaximizable = false;
                presenter.IsResizable = true;
            }

            // Interceptar minimização → esconder da taskbar (ficar só na bandeja)
            _appWindow.Changed += OnAppWindowChanged;

            CenterOnScreen(_appWindow);
        }

        Title = "Meu Ponto";
    }

    private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (_isHidingToTray) return;

        if (sender.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Minimized })
        {
            _isHidingToTray = true;
            sender.Hide();
            _isHidingToTray = false;
        }
    }

    private static void CenterOnScreen(AppWindow appWindow)
    {
        var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
        if (displayArea != null)
        {
            var workArea = displayArea.WorkArea;
            var size = appWindow.Size;
            var x = (workArea.Width - size.Width) / 2 + workArea.X;
            var y = (workArea.Height - size.Height) / 2 + workArea.Y;
            appWindow.Move(new PointInt32(x, y));
        }
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        if (_settingsService.Settings.MinimizeToTrayOnClose)
        {
            args.Handled = true;
            _appWindow?.Hide();
        }
    }
}
