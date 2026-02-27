using Microsoft.CodeAnalysis;

namespace Avalonia.Controls.Maui.SourceGenerators.Tests;

public class MauiAvaloniaBootstrapGeneratorTests
{
    private const string ValidMauiProgram = @"
using Microsoft.Maui.Hosting;

namespace TestApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        return null!;
    }
}
";

    [Fact]
    public void HappyPath_GeneratesBothFiles()
    {
        var result = GeneratorTestHelper.RunGenerator(
            ValidMauiProgram,
            GeneratorTestHelper.DefaultBuildProperties());

        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var generatedFiles = result.GeneratedTrees;
        Assert.Equal(2, generatedFiles.Length);

        var appFile = generatedFiles.FirstOrDefault(t => t.FilePath.Contains("AvaloniaApp.g.cs"));
        Assert.NotNull(appFile);
        var appText = appFile.GetText().ToString();
        Assert.Contains("internal partial class AvaloniaApp : MauiAvaloniaApplication", appText);
        Assert.Contains("global::TestApp.MauiProgram.CreateMauiApp()", appText);
        Assert.Contains("new global::Avalonia.Themes.Fluent.FluentTheme()", appText);
        Assert.Contains("ThemeVariant.Default", appText);

        var programFile = generatedFiles.FirstOrDefault(t => t.FilePath.Contains("AvaloniaDesktopProgram.g.cs"));
        Assert.NotNull(programFile);
        var programText = programFile.GetText().ToString();
        Assert.Contains("internal static partial class AvaloniaDesktopProgram", programText);
        Assert.Contains(".WithInterFont()", programText);
        Assert.Contains("AppBuilder.Configure<AvaloniaApp>()", programText);
    }

    [Fact]
    public void OptOut_GenerateBootstrapFalse_ProducesNoOutput()
    {
        var result = GeneratorTestHelper.RunGenerator(
            ValidMauiProgram,
            GeneratorTestHelper.DefaultBuildProperties(generateBootstrap: "false"));

        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void NoCreateMauiApp_ReportsWarning()
    {
        var source = @"
namespace TestApp;

public static class SomeClass
{
    public static void DoSomething() { }
}
";
        var result = GeneratorTestHelper.RunGenerator(
            source,
            GeneratorTestHelper.DefaultBuildProperties());

        Assert.Empty(result.GeneratedTrees);
        var warning = result.Diagnostics.FirstOrDefault(d => d.Id == "ACMSG001");
        Assert.NotNull(warning);
        Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
    }

    [Fact]
    public void MultipleCreateMauiApp_ReportsWarningAndUsesFirst()
    {
        var source = @"
using Microsoft.Maui.Hosting;

namespace TestApp;

public static class MauiProgram1
{
    public static MauiApp CreateMauiApp() => null!;
}

public static class MauiProgram2
{
    public static MauiApp CreateMauiApp() => null!;
}
";
        var result = GeneratorTestHelper.RunGenerator(
            source,
            GeneratorTestHelper.DefaultBuildProperties());

        Assert.Equal(2, result.GeneratedTrees.Length);
        var warning = result.Diagnostics.FirstOrDefault(d => d.Id == "ACMSG002");
        Assert.NotNull(warning);
        Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
    }

    [Fact]
    public void CustomTheme_Simple()
    {
        var result = GeneratorTestHelper.RunGenerator(
            ValidMauiProgram,
            GeneratorTestHelper.DefaultBuildProperties(theme: "Simple"));

        var appFile = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("AvaloniaApp.g.cs"));
        Assert.NotNull(appFile);
        var text = appFile.GetText().ToString();
        Assert.Contains("new global::Avalonia.Themes.Simple.SimpleTheme()", text);
    }

    [Fact]
    public void CustomThemeVariant_Dark()
    {
        var result = GeneratorTestHelper.RunGenerator(
            ValidMauiProgram,
            GeneratorTestHelper.DefaultBuildProperties(themeVariant: "Dark"));

        var appFile = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("AvaloniaApp.g.cs"));
        Assert.NotNull(appFile);
        var text = appFile.GetText().ToString();
        Assert.Contains("ThemeVariant.Dark", text);
    }

    [Fact]
    public void CustomThemeVariant_Light()
    {
        var result = GeneratorTestHelper.RunGenerator(
            ValidMauiProgram,
            GeneratorTestHelper.DefaultBuildProperties(themeVariant: "Light"));

        var appFile = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("AvaloniaApp.g.cs"));
        Assert.NotNull(appFile);
        var text = appFile.GetText().ToString();
        Assert.Contains("ThemeVariant.Light", text);
    }

    [Fact]
    public void InterFontDisabled_OmitsWithInterFont()
    {
        var result = GeneratorTestHelper.RunGenerator(
            ValidMauiProgram,
            GeneratorTestHelper.DefaultBuildProperties(includeInterFont: "false"));

        var programFile = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("AvaloniaDesktopProgram.g.cs"));
        Assert.NotNull(programFile);
        var text = programFile.GetText().ToString();
        Assert.DoesNotContain(".WithInterFont()", text);
    }

    [Fact]
    public void CustomAppClassName()
    {
        var result = GeneratorTestHelper.RunGenerator(
            ValidMauiProgram,
            GeneratorTestHelper.DefaultBuildProperties(appClassName: "MyCustomApp"));

        var appFile = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("MyCustomApp.g.cs"));
        Assert.NotNull(appFile);
        var text = appFile.GetText().ToString();
        Assert.Contains("internal partial class MyCustomApp : MauiAvaloniaApplication", text);

        var programFile = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("AvaloniaDesktopProgram.g.cs"));
        Assert.NotNull(programFile);
        var programText = programFile.GetText().ToString();
        Assert.Contains("AppBuilder.Configure<MyCustomApp>()", programText);
    }

    [Fact]
    public void CustomProgramClass_OverridesAutoDetect()
    {
        var source = @"
namespace TestApp;

public static class SomeClass
{
    public static void DoSomething() { }
}
";
        var result = GeneratorTestHelper.RunGenerator(
            source,
            GeneratorTestHelper.DefaultBuildProperties(programClass: "MyApp.CustomMauiProgram"));

        Assert.Equal(2, result.GeneratedTrees.Length);

        var appFile = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("AvaloniaApp.g.cs"));
        Assert.NotNull(appFile);
        var text = appFile.GetText().ToString();
        Assert.Contains("MyApp.CustomMauiProgram.CreateMauiApp()", text);

        // Should not report ACMSG001 since we explicitly set the class
        var noMauiAppWarning = result.Diagnostics.FirstOrDefault(d => d.Id == "ACMSG001");
        Assert.Null(noMauiAppWarning);
    }

    [Fact]
    public void CustomNamespace()
    {
        var result = GeneratorTestHelper.RunGenerator(
            ValidMauiProgram,
            GeneratorTestHelper.DefaultBuildProperties(rootNamespace: "My.Custom.Namespace"));

        var appFile = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("AvaloniaApp.g.cs"));
        Assert.NotNull(appFile);
        var text = appFile.GetText().ToString();
        Assert.Contains("namespace My.Custom.Namespace;", text);
    }

    [Fact]
    public void InfoDiagnostic_ReportedOnSuccess()
    {
        var result = GeneratorTestHelper.RunGenerator(
            ValidMauiProgram,
            GeneratorTestHelper.DefaultBuildProperties());

        var info = result.Diagnostics.FirstOrDefault(d => d.Id == "ACMSG003");
        Assert.NotNull(info);
        Assert.Equal(DiagnosticSeverity.Info, info.Severity);
    }

    [Fact]
    public void BootstrapOnly_GeneratesOnlyAppClass()
    {
        var result = GeneratorTestHelper.RunGenerator(
            ValidMauiProgram,
            GeneratorTestHelper.DefaultBuildProperties(generateDesktopProgram: "false"));

        // Should generate only the app class, not the desktop program
        Assert.Single(result.GeneratedTrees);

        var appFile = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("AvaloniaApp.g.cs"));
        Assert.NotNull(appFile);
        var appText = appFile.GetText().ToString();
        Assert.Contains("internal partial class AvaloniaApp : MauiAvaloniaApplication", appText);

        var programFile = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("AvaloniaDesktopProgram.g.cs"));
        Assert.Null(programFile);
    }

    [Fact]
    public void DesktopProgramWithoutBootstrap_ProducesNoOutput()
    {
        var result = GeneratorTestHelper.RunGenerator(
            ValidMauiProgram,
            GeneratorTestHelper.DefaultBuildProperties(generateBootstrap: "false", generateDesktopProgram: "true"));

        // Bootstrap is the gate — without it, nothing is generated even if desktop program is requested
        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void NonStaticCreateMauiApp_IsIgnored()
    {
        var source = @"
using Microsoft.Maui.Hosting;

namespace TestApp;

public class MauiProgram
{
    public MauiApp CreateMauiApp() => null!;
}
";
        var result = GeneratorTestHelper.RunGenerator(
            source,
            GeneratorTestHelper.DefaultBuildProperties());

        Assert.Empty(result.GeneratedTrees);
        var warning = result.Diagnostics.FirstOrDefault(d => d.Id == "ACMSG001");
        Assert.NotNull(warning);
    }

    [Fact]
    public void WrongReturnType_IsIgnored()
    {
        var source = @"
namespace TestApp;

public static class MauiProgram
{
    public static string CreateMauiApp() => """";
}
";
        var result = GeneratorTestHelper.RunGenerator(
            source,
            GeneratorTestHelper.DefaultBuildProperties());

        Assert.Empty(result.GeneratedTrees);
        var warning = result.Diagnostics.FirstOrDefault(d => d.Id == "ACMSG001");
        Assert.NotNull(warning);
    }
}
