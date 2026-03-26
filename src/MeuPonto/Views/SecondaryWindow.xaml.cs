using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

namespace MeuPonto.Views;

/// <summary>
/// Janela secundária genérica para abrir Histórico e Configurações.
/// </summary>
public sealed partial class SecondaryWindow : Window
{
    private AppWindow? _appWindow;

    /// <summary>Disparado quando a janela é fechada.</summary>
    public event EventHandler? WindowClosed;

    public SecondaryWindow(Type pageType, string title, int width, int height)
    {
        InitializeComponent();

        // Configurar janela
        ConfigureWindow(title, width, height);

        // Estender conteúdo até a titlebar (mesma cor)
        ExtendsContentIntoTitleBar = true;

        // Backdrop Acrylic
        SystemBackdrop = new DesktopAcrylicBackdrop();

        // Navegar para a página solicitada
        ContentFrame.Navigate(pageType);
    }

    public Frame GetFrame() => ContentFrame;

    private void ConfigureWindow(string title, int width, int height)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        if (_appWindow != null)
        {
            _appWindow.Resize(new SizeInt32(width, height));
            _appWindow.Title = title;

            // Sempre por cima (fica acima da janela principal)
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsAlwaysOnTop = true;
            }

            _appWindow.Closing += (_, _) => WindowClosed?.Invoke(this, EventArgs.Empty);

            CenterOnScreen(_appWindow);
        }

        Title = title;
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
}
