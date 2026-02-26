using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Avalonia.Controls.Maui.SourceGenerators.Tests;

internal static class GeneratorTestHelper
{
    public static GeneratorDriverRunResult RunGenerator(
        string source,
        Dictionary<string, string>? buildProperties = null)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        };

        // Add a minimal MauiApp type so the generator can resolve the return type
        var mauiStubSource = CSharpSyntaxTree.ParseText(@"
namespace Microsoft.Maui.Hosting
{
    public class MauiApp { }
    public class MauiAppBuilder { }
}
");

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree, mauiStubSource],
            references,
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        var generator = new MauiAvaloniaBootstrapGenerator();

        var optionsProvider = new TestAnalyzerConfigOptionsProvider(buildProperties ?? new Dictionary<string, string>());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            optionsProvider: optionsProvider);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        return driver.GetRunResult();
    }

    public static Dictionary<string, string> DefaultBuildProperties(
        string rootNamespace = "TestApp",
        string generateBootstrap = "true",
        string? theme = null,
        string? themeVariant = null,
        string? includeInterFont = null,
        string? programClass = null,
        string? appClassName = null)
    {
        var props = new Dictionary<string, string>
        {
            ["build_property.AvaloniaControlsMauiGenerateBootstrap"] = generateBootstrap,
            ["build_property.RootNamespace"] = rootNamespace,
        };

        if (theme is not null)
            props["build_property.AvaloniaControlsMauiTheme"] = theme;
        if (themeVariant is not null)
            props["build_property.AvaloniaControlsMauiThemeVariant"] = themeVariant;
        if (includeInterFont is not null)
            props["build_property.AvaloniaControlsMauiIncludeInterFont"] = includeInterFont;
        if (programClass is not null)
            props["build_property.AvaloniaControlsMauiProgramClass"] = programClass;
        if (appClassName is not null)
            props["build_property.AvaloniaControlsMauiAppClassName"] = appClassName;

        return props;
    }

    private sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly TestAnalyzerConfigOptions _globalOptions;

        public TestAnalyzerConfigOptionsProvider(Dictionary<string, string> buildProperties)
        {
            _globalOptions = new TestAnalyzerConfigOptions(buildProperties);
        }

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => TestAnalyzerConfigOptions.Empty;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => TestAnalyzerConfigOptions.Empty;
    }

    private sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        public static readonly TestAnalyzerConfigOptions Empty = new(new Dictionary<string, string>());

        private readonly Dictionary<string, string> _values;

        public TestAnalyzerConfigOptions(Dictionary<string, string> values)
        {
            _values = values;
        }

        public override bool TryGetValue(string key,
#if NET
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            out string? value) => _values.TryGetValue(key, out value);
    }
}
