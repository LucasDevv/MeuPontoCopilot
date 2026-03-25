using Microsoft.UI.Xaml.Controls;
using MeuPonto.Services.Interfaces;

namespace MeuPonto.Services;

/// <summary>
/// Implementação do serviço de navegação usando Frame do WinUI 3.
/// </summary>
public class NavigationService : INavigationService
{
    private Frame? _frame;

    public bool CanGoBack => _frame?.CanGoBack == true;

    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }

    public void NavigateTo(Type pageType)
    {
        _frame?.Navigate(pageType);
    }

    public void GoBack()
    {
        if (_frame?.CanGoBack == true)
            _frame.GoBack();
    }
}
