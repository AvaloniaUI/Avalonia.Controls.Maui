using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Provides Avalonia styles resources for .NET MAUI control theming.
/// </summary>
public partial class ControlStyles : Styles
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ControlStyles"/> class and loads the associated XAML styles.
    /// </summary>
    /// <param name="serviceProvider">An optional service provider used by the Avalonia XAML loader.</param>
    public ControlStyles(IServiceProvider? serviceProvider = null)
    {
        AvaloniaXamlLoader.Load(serviceProvider, this);
    }
}
