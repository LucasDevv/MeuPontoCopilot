namespace MeuPonto.Services.Interfaces;

/// <summary>
/// Serviço de navegação entre páginas.
/// </summary>
public interface INavigationService
{
    /// <summary>Navega para uma página pelo tipo.</summary>
    void NavigateTo(Type pageType);

    /// <summary>Volta para a página anterior.</summary>
    void GoBack();

    /// <summary>Indica se é possível voltar.</summary>
    bool CanGoBack { get; }

    /// <summary>Define o Frame usado para navegação.</summary>
    void SetFrame(Microsoft.UI.Xaml.Controls.Frame frame);
}
