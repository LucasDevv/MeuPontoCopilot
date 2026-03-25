using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MeuPonto.ViewModels;

namespace MeuPonto.Views;

/// <summary>
/// Página de configurações do aplicativo.
/// </summary>
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Load();
    }
}
