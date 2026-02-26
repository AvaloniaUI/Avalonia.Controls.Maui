using Microsoft.CodeAnalysis.Diagnostics;

namespace Avalonia.Controls.Maui.SourceGenerators;

internal sealed class GeneratorOptions
{
    public bool GenerateBootstrap { get; }
    public string Theme { get; }
    public string ThemeVariant { get; }
    public bool IncludeInterFont { get; }
    public string? ProgramClass { get; }
    public string AppClassName { get; }
    public string RootNamespace { get; }

    private GeneratorOptions(
        bool generateBootstrap,
        string theme,
        string themeVariant,
        bool includeInterFont,
        string? programClass,
        string appClassName,
        string rootNamespace)
    {
        GenerateBootstrap = generateBootstrap;
        Theme = theme;
        ThemeVariant = themeVariant;
        IncludeInterFont = includeInterFont;
        ProgramClass = programClass;
        AppClassName = appClassName;
        RootNamespace = rootNamespace;
    }

    public static GeneratorOptions? FromOptions(AnalyzerConfigOptionsProvider optionsProvider)
    {
        var globalOptions = optionsProvider.GlobalOptions;

        if (!globalOptions.TryGetValue("build_property.AvaloniaControlsMauiGenerateBootstrap", out var generateBootstrapStr)
            || !string.Equals(generateBootstrapStr, "true", System.StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        globalOptions.TryGetValue("build_property.AvaloniaControlsMauiTheme", out var theme);
        globalOptions.TryGetValue("build_property.AvaloniaControlsMauiThemeVariant", out var themeVariant);
        globalOptions.TryGetValue("build_property.AvaloniaControlsMauiIncludeInterFont", out var includeInterFontStr);
        globalOptions.TryGetValue("build_property.AvaloniaControlsMauiProgramClass", out var programClass);
        globalOptions.TryGetValue("build_property.AvaloniaControlsMauiAppClassName", out var appClassName);
        globalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);

        return new GeneratorOptions(
            generateBootstrap: true,
            theme: string.IsNullOrEmpty(theme) ? "Fluent" : theme!,
            themeVariant: string.IsNullOrEmpty(themeVariant) ? "Default" : themeVariant!,
            includeInterFont: string.IsNullOrEmpty(includeInterFontStr) || string.Equals(includeInterFontStr, "true", System.StringComparison.OrdinalIgnoreCase),
            programClass: string.IsNullOrEmpty(programClass) ? null : programClass,
            appClassName: string.IsNullOrEmpty(appClassName) ? "AvaloniaApp" : appClassName!,
            rootNamespace: string.IsNullOrEmpty(rootNamespace) ? "GeneratedAvaloniaBootstrap" : rootNamespace!);
    }
}
