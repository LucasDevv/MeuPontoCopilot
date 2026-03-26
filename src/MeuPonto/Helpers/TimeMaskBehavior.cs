using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MeuPonto.Helpers;

/// <summary>
/// Attached behavior que aplica máscara HH:mm (99:99) em um TextBox.
/// Aceita apenas dígitos e insere ':' automaticamente após o 2º dígito.
/// </summary>
public static class TimeMaskBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(TimeMaskBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.TextChanged += OnTextChanged;
            }
            else
            {
                textBox.TextChanged -= OnTextChanged;
            }
        }
    }

    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        var text = textBox.Text;

        // Permitir campo vazio
        if (string.IsNullOrEmpty(text))
            return;

        // Extrair apenas dígitos
        var digits = string.Empty;
        foreach (var c in text)
        {
            if (char.IsDigit(c))
                digits += c;
        }

        // Máximo 4 dígitos (HHmm)
        if (digits.Length > 4)
            digits = digits[..4];

        // Formatar com ':'
        string formatted;
        if (digits.Length <= 2)
        {
            formatted = digits;
        }
        else
        {
            formatted = digits[..2] + ":" + digits[2..];
        }

        // Só alterar se necessário (evita loop infinito)
        if (formatted != text)
        {
            // Desconectar para evitar reentrância
            textBox.TextChanged -= OnTextChanged;
            textBox.Text = formatted;
            textBox.SelectionStart = formatted.Length;
            textBox.TextChanged += OnTextChanged;
        }
    }
}
