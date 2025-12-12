using Avalonia;
using Avalonia.Controls.Maui.LifecycleEvents;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

/// <summary>
/// Extension methods for registering Avalonia styles and control templates used by Avalonia.Controls.Maui.
/// </summary>
public static class AvaloniaResourcesBuilderExtensions
{
    private const string RegistrationKey = "__AvaloniaControlsMauiResourcesRegistered";

    /// <summary>
    /// Registers the built-in Avalonia themes and all control templates/styles
    /// shipped with Avalonia.Controls.Maui.
    /// </summary>
    public static MauiAppBuilder UseAvaloniaResources(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(avalonia =>
            {
                avalonia.OnLaunching((application, _) => RegisterResources(application));
            });
        });

        return builder;
    }

    private static void RegisterResources(Application application)
    {
        if (application.Resources.ContainsKey(RegistrationKey))
            return;

        application.Resources[RegistrationKey] = true;

        application.Resources.MergedDictionaries.Add(new Avalonia.Controls.Maui.MauiRadioButtonResources());
        application.Resources.MergedDictionaries.Add(new Avalonia.Controls.Maui.ProgressRingResources());
        application.Resources.MergedDictionaries.Add(new Avalonia.Controls.Maui.MauiComboBoxResources());

        application.Styles.Add(new Avalonia.Controls.Maui.ControlStyles());
    }
}