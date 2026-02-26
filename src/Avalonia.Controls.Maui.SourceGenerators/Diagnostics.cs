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

    public static readonly DiagnosticDescriptor BootstrapGenerated = new(
        id: "ACMSG003",
        title: "Bootstrap code generated",
        messageFormat: "Generated Avalonia desktop bootstrap using '{0}.CreateMauiApp()'",
        category: "Avalonia.Controls.Maui",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}
