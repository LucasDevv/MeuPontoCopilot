using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MeuPonto.Services.Interfaces;

namespace MeuPonto.Services;

/// <summary>
/// Implementação do serviço de bandeja do sistema usando H.NotifyIcon.WinUI.
/// Cria ícone na tray, menu de contexto, e exibe notificações balloon.
/// </summary>
public class TrayIconService : ITrayIconService
{
    private TaskbarIcon? _taskbarIcon;
    private bool _disposed;

    public event EventHandler? ShowWindowRequested;
    public event EventHandler? ClockInRequested;
    public event EventHandler? ClockOutRequested;
    public event EventHandler? ExitRequested;

    public void Initialize()
    {
        if (_taskbarIcon != null)
            return;

        _taskbarIcon = new TaskbarIcon();

        // Ícone gerado: "MP" em fundo azul
        _taskbarIcon.IconSource = new GeneratedIconSource
        {
            Text = "MP",
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
            FontSize = 18
        };

        _taskbarIcon.ToolTipText = "Meu Ponto — Fora do expediente";
        _taskbarIcon.NoLeftClickDelay = true;

        // Clique esquerdo: restaurar janela
        _taskbarIcon.LeftClickCommand = new RelayInputCommand(
            () => ShowWindowRequested?.Invoke(this, EventArgs.Empty));

        // Menu de contexto (clique direito)
        var flyout = new MenuFlyout();

        var openItem = new MenuFlyoutItem { Text = "Abrir" };
        openItem.Click += (_, _) => ShowWindowRequested?.Invoke(this, EventArgs.Empty);
        flyout.Items.Add(openItem);

        flyout.Items.Add(new MenuFlyoutSeparator());

        var clockInItem = new MenuFlyoutItem { Text = "Bater ponto de entrada" };
        clockInItem.Click += (_, _) => ClockInRequested?.Invoke(this, EventArgs.Empty);
        flyout.Items.Add(clockInItem);

        var clockOutItem = new MenuFlyoutItem { Text = "Bater ponto de saída" };
        clockOutItem.Click += (_, _) => ClockOutRequested?.Invoke(this, EventArgs.Empty);
        flyout.Items.Add(clockOutItem);

        flyout.Items.Add(new MenuFlyoutSeparator());

        var exitItem = new MenuFlyoutItem { Text = "Sair" };
        exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);
        flyout.Items.Add(exitItem);

        _taskbarIcon.ContextFlyout = flyout;
        _taskbarIcon.ForceCreate();
    }

    public void ShowNotification(string title, string message, bool isCritical = false, bool playSound = true)
    {
        if (_taskbarIcon == null)
            return;

        try
        {
            _taskbarIcon.ShowNotification(
                title: title,
                message: message,
                icon: isCritical ? NotificationIcon.Error : NotificationIcon.Info,
                sound: playSound);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TrayIconService] Erro ao exibir notificação: {ex.Message}");
        }
    }

    public void UpdateTooltip(string text)
    {
        if (_taskbarIcon != null)
        {
            _taskbarIcon.ToolTipText = text;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _taskbarIcon?.Dispose();
        _taskbarIcon = null;
    }

    /// <summary>
    /// Comando relay simples para LeftClickCommand.
    /// </summary>
    private sealed class RelayInputCommand(Action execute) : System.Windows.Input.ICommand
    {
#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute();
    }
}
