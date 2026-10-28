using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Avalonia.Controls.Maui.SourceGenerators;

/// <summary>
/// Reports handlers that derive from the portable .NET MAUI ViewHandler with an Avalonia control as
/// their platform view. That base class does no layout and maps no view properties, so the control
/// does not appear; the Avalonia.Controls.Maui ViewHandler does both.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PortableViewHandlerAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Diagnostics.PortableViewHandler);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            var mauiViewHandler = start.Compilation.GetTypeByMetadataName("Microsoft.Maui.Handlers.ViewHandler`2");
            var avaloniaControl = start.Compilation.GetTypeByMetadataName("Avalonia.Controls.Control");
            if (mauiViewHandler is null || avaloniaControl is null)
                return;

            start.RegisterSymbolAction(
                symbolContext => AnalyzeType(symbolContext, mauiViewHandler, avaloniaControl),
                SymbolKind.NamedType);
        });
    }

    private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol mauiViewHandler, INamedTypeSymbol avaloniaControl)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        // Only the class that derives directly, so subclasses of a reported handler are not reported again
        if (type.TypeKind != TypeKind.Class
            || type.BaseType is not { } baseType
            || !SymbolEqualityComparer.Default.Equals(baseType.OriginalDefinition, mauiViewHandler))
            return;

        var platformView = baseType.TypeArguments[1];
        if (!DerivesFrom(platformView, avaloniaControl))
            return;

        context.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.PortableViewHandler,
            type.Locations.Length > 0 ? type.Locations[0] : Location.None,
            type.Name,
            baseType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            platformView.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
    }

    private static bool DerivesFrom(ITypeSymbol type, INamedTypeSymbol baseType)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
        }

        return false;
    }
}
