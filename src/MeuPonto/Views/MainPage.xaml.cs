using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MeuPonto.ViewModels;

namespace MeuPonto.Views;

/// <summary>
/// Página principal — status, contador, ações e resumo.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync(DispatcherQueue);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.Cleanup();
    }
}
