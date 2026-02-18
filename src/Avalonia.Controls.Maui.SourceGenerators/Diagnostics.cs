using Microsoft.CodeAnalysis;

namespace Avalonia.Controls.Maui.SourceGenerators;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor MultipleAvaloniaMauiAppMethods = new(
        id: "ACMSRC001",
        title: "Multiple [AvaloniaMauiApp] methods found",
        messageFormat: "Only one method may be marked with [AvaloniaMauiApp], but multiple were found",
        category: "Avalonia.Controls.Maui",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodMustBePublicStatic = new(
        id: "ACMSRC002",
        title: "Method must be public static",
        messageFormat: "The method '{0}' marked with [AvaloniaMauiApp] must be 'public static'",
        category: "Avalonia.Controls.Maui",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodMustReturnMauiApp = new(
        id: "ACMSRC003",
        title: "Method must return MauiApp",
        messageFormat: "The method '{0}' marked with [AvaloniaMauiApp] must return 'Microsoft.Maui.Hosting.MauiApp'",
        category: "Avalonia.Controls.Maui",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MethodMustBeParameterless = new(
        id: "ACMSRC004",
        title: "Method must be parameterless",
        messageFormat: "The method '{0}' marked with [AvaloniaMauiApp] must have no parameters",
        category: "Avalonia.Controls.Maui",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
