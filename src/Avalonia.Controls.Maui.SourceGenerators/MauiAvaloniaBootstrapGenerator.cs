using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Avalonia.Controls.Maui.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class MauiAvaloniaBootstrapGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Read MSBuild properties
        var optionsProvider = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) => GeneratorOptions.FromOptions(provider));

        // Find all static methods named "CreateMauiApp" that return MauiApp
        var createMauiAppMethods = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCreateMauiAppCandidate(node),
                transform: static (ctx, _) => GetCreateMauiAppInfo(ctx))
            .Where(static info => info is not null)
            .Collect();

        // Combine options and methods
        var combined = optionsProvider.Combine(createMauiAppMethods);

        context.RegisterSourceOutput(combined, static (ctx, source) =>
        {
            var (options, methods) = source;

            // Early exit if generation is disabled
            if (options is null)
            {
                return;
            }

            var candidates = methods.Where(m => m is not null).Select(m => m!).ToList();

            string mauiProgramClass;

            if (!string.IsNullOrEmpty(options.ProgramClass))
            {
                // User explicitly specified the class
                mauiProgramClass = options.ProgramClass!;
            }
            else if (candidates.Count == 0)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Diagnostics.NoCreateMauiAppFound, Location.None));
                return;
            }
            else if (candidates.Count > 1)
            {
                var names = string.Join(", ", candidates.Select(c => c.FullyQualifiedClassName));
                ctx.ReportDiagnostic(Diagnostic.Create(Diagnostics.MultipleCreateMauiAppFound, Location.None, names));
                // Use the first one found but still warn
                mauiProgramClass = $"global::{candidates[0].FullyQualifiedClassName}";
            }
            else
            {
                mauiProgramClass = $"global::{candidates[0].FullyQualifiedClassName}";
            }

            // Generate AvaloniaApp
            var appSource = SourceGenerationHelper.GenerateAvaloniaApp(options, mauiProgramClass);
            ctx.AddSource($"{options.AppClassName}.g.cs", appSource);

            // Generate Program
            var programSource = SourceGenerationHelper.GenerateDesktopProgram(options);
            ctx.AddSource("AvaloniaDesktopProgram.g.cs", programSource);

            ctx.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.BootstrapGenerated,
                Location.None,
                mauiProgramClass));
        });
    }

    private static bool IsCreateMauiAppCandidate(SyntaxNode node)
    {
        return node is MethodDeclarationSyntax method
            && method.Identifier.ValueText == "CreateMauiApp"
            && method.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword);
    }

    private static CreateMauiAppInfo? GetCreateMauiAppInfo(GeneratorSyntaxContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(method) is not IMethodSymbol symbol || !symbol.IsStatic)
        {
            return null;
        }

        // Check the return type is MauiApp
        var returnType = symbol.ReturnType;
        if (returnType.Name != "MauiApp")
        {
            return null;
        }

        // Check it's Microsoft.Maui.Hosting.MauiApp
        var containingNamespace = GetFullNamespace(returnType.ContainingNamespace);
        if (containingNamespace != "Microsoft.Maui.Hosting")
        {
            return null;
        }

        // Get the containing class name
        var containingType = symbol.ContainingType;
        if (containingType is null)
        {
            return null;
        }

        var fullyQualifiedClassName = GetFullyQualifiedName(containingType);

        return new CreateMauiAppInfo(fullyQualifiedClassName);
    }

    private static string GetFullNamespace(INamespaceSymbol? namespaceSymbol)
    {
        if (namespaceSymbol is null || namespaceSymbol.IsGlobalNamespace)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        BuildNamespace(namespaceSymbol, sb);
        return sb.ToString();
    }

    private static void BuildNamespace(INamespaceSymbol namespaceSymbol, StringBuilder sb)
    {
        if (namespaceSymbol.ContainingNamespace is not null && !namespaceSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            BuildNamespace(namespaceSymbol.ContainingNamespace, sb);
            sb.Append('.');
        }

        sb.Append(namespaceSymbol.Name);
    }

    private static string GetFullyQualifiedName(INamedTypeSymbol typeSymbol)
    {
        var ns = GetFullNamespace(typeSymbol.ContainingNamespace);
        return string.IsNullOrEmpty(ns) ? typeSymbol.Name : $"{ns}.{typeSymbol.Name}";
    }

    private sealed class CreateMauiAppInfo
    {
        public string FullyQualifiedClassName { get; }

        public CreateMauiAppInfo(string fullyQualifiedClassName)
        {
            FullyQualifiedClassName = fullyQualifiedClassName;
        }
    }
}
