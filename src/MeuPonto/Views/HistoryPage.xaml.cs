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

    private async void OnEditButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: TimeRecordDisplay record })
        {
            ViewModel.BeginEditCommand.Execute(record);
            if (ViewModel.IsEditing)
            {
                EditDialog.XamlRoot = XamlRoot;
                await EditDialog.ShowAsync();
            }
        }
    }

    private async void OnEditDialogSave(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Defer para poder aguardar operação async
        var deferral = args.GetDeferral();
        try
        {
            await ViewModel.SaveEditCommand.ExecuteAsync(null);

            // Se ainda está editando, houve erro de validação — impede fechar
            if (ViewModel.IsEditing)
                args.Cancel = true;
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void OnEditDialogCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        ViewModel.CancelEditCommand.Execute(null);
    }

    private void OnEditTimeChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.RecalculateEditTotal();
    }
}
