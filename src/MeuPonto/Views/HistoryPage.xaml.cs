using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MeuPonto.ViewModels;

namespace MeuPonto.Views;

/// <summary>
/// Página de histórico de registros de ponto.
/// </summary>
public sealed partial class HistoryPage : Page
{
    public HistoryViewModel ViewModel { get; }

    public HistoryPage()
    {
        ViewModel = App.GetService<HistoryViewModel>();
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }
}
