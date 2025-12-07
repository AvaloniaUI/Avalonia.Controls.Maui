using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia.Markup.Xaml.Styling;
using System.Linq;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Extension methods for configuring Avalonia AppBuilder with MAUI fonts.
/// </summary>
public static class AvaloniaAppBuilderExtensions
{
    /// <summary>
    /// Configures Avalonia controls themes used in Handlers.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026", 
        Justification = "ResourceInclude is safe when using embedded resources from the same assembly")]
    public static AppBuilder WithControls(this AppBuilder builder)
    {
        return builder.AfterSetup(_ =>
        {
            if (Application.Current != null)
            {
                var themeUris = new[]
                {
                    "avares://Avalonia.Controls.Maui/Controls/MauiComboBox/MauiComboBox.axaml",
                    "avares://Avalonia.Controls.Maui/Controls/ProgressRing/ProgressRing.axaml",
                    "avares://Avalonia.Controls.Maui/Controls/RadioButton/MauiRadioButton.axaml",
                };

                foreach (var uriString in themeUris)
                {
                    var themeUri = new Uri(uriString);
                    
                    // Check if already added to avoid duplicates
                    var alreadyAdded = Application.Current.Resources.MergedDictionaries
                        .OfType<ResourceInclude>()
                        .Any(r => r.Source == themeUri);
                    
                    if (!alreadyAdded)
                    {
                        var theme = new ResourceInclude(themeUri)
                        {
                            Source = themeUri
                        };
                        
                        Application.Current.Resources.MergedDictionaries.Add(theme);
                    }
                }
            }
        });
    }
}