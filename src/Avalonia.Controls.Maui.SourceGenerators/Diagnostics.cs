using Microsoft.CodeAnalysis;

namespace Avalonia.Controls.Maui.SourceGenerators;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor NoCreateMauiAppFound = new(
        id: "ACMSG001",
        title: "No CreateMauiApp method found",
        messageFormat: "No static 'CreateMauiApp()' method returning 'MauiApp' was found. The generated bootstrap code will not compile. Define a static 'CreateMauiApp()' method or set 'AvaloniaControlsMauiProgramClass' in your project file.",
        category: "Avalonia.Controls.Maui",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleCreateMauiAppFound = new(
        id: "ACMSG002",
        title: "Multiple CreateMauiApp candidates found",
        messageFormat: "Multiple static 'CreateMauiApp()' methods were found: {0}. Set 'AvaloniaControlsMauiProgramClass' in your project file to specify which one to use.",
        category: "Avalonia.Controls.Maui",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PortableViewHandler = new(
        id: "ACMSG004",
        title: "Handler derives from the .NET MAUI ViewHandler",
        messageFormat: "'{0}' derives from Microsoft.Maui.Handlers.ViewHandler<{1}, {2}>, which does not measure or arrange its Avalonia control or map view properties to it, so the control does not appear. Derive from Avalonia.Controls.Maui.Handlers.ViewHandler<{1}, {2}> instead.",
        category: "Avalonia.Controls.Maui",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/AvaloniaUI/Avalonia.Controls.Maui/blob/main/docs/config-and-setup.md#create-a-handler");

    public static readonly DiagnosticDescriptor BootstrapGenerated = new(
        id: "ACMSG003",
        title: "Bootstrap code generated",
        messageFormat: "Generated Avalonia bootstrap using '{0}.CreateMauiApp()'",
        category: "Avalonia.Controls.Maui",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}
