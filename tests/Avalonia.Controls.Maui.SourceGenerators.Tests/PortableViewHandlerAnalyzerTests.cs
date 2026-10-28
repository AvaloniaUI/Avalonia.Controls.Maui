using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Avalonia.Controls.Maui.SourceGenerators.Tests;

public class PortableViewHandlerAnalyzerTests
{
    // Minimal stand-ins for the .NET MAUI and Avalonia types the analyzer looks for
    private const string Stubs = @"
namespace Microsoft.Maui { public interface IView { } }
namespace Microsoft.Maui.Handlers
{
    public abstract class ViewHandler<TVirtualView, TPlatformView> where TPlatformView : class { }
}
namespace Avalonia.Controls
{
    public class Control { }
    public class StackPanel : Control { }
}
namespace Avalonia.Controls.Maui.Handlers
{
    public abstract class ViewHandler<TVirtualView, TPlatformView> where TPlatformView : Avalonia.Controls.Control { }
}
public class RatingView : Microsoft.Maui.IView { }
";

    [Fact]
    public async Task ReportsHandlerDerivingFromMauiViewHandlerWithAvaloniaControl()
    {
        var diagnostics = await GetDiagnosticsAsync(@"
public class RatingViewHandler : Microsoft.Maui.Handlers.ViewHandler<RatingView, Avalonia.Controls.StackPanel> { }
");

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ACMSG004", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Contains("RatingViewHandler", diagnostic.GetMessage());
        Assert.Equal("RatingViewHandler", diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan));
    }

    [Fact]
    public async Task DoesNotReportHandlerDerivingFromAvaloniaViewHandler()
    {
        var diagnostics = await GetDiagnosticsAsync(@"
public class RatingViewHandler : Avalonia.Controls.Maui.Handlers.ViewHandler<RatingView, Avalonia.Controls.StackPanel> { }
");

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReportMauiViewHandlerForOtherPlatformViews()
    {
        var diagnostics = await GetDiagnosticsAsync(@"
public class NativeView { }
public class RatingViewHandler : Microsoft.Maui.Handlers.ViewHandler<RatingView, NativeView> { }
");

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ReportsOnlyTheClassThatDerivesFromMauiViewHandler()
    {
        var diagnostics = await GetDiagnosticsAsync(@"
public class RatingViewHandler : Microsoft.Maui.Handlers.ViewHandler<RatingView, Avalonia.Controls.StackPanel> { }
public class FancyRatingViewHandler : RatingViewHandler { }
");

        var diagnostic = Assert.Single(diagnostics);
        Assert.Contains("RatingViewHandler", diagnostic.GetMessage());
        Assert.DoesNotContain("FancyRatingViewHandler", diagnostic.GetMessage());
    }

    [Fact]
    public async Task DoesNotReportWithoutAvaloniaTypes()
    {
        var diagnostics = await GetDiagnosticsAsync(@"
namespace Microsoft.Maui.Handlers { public abstract class ViewHandler<TVirtualView, TPlatformView> { } }
public class Handler : Microsoft.Maui.Handlers.ViewHandler<object, string> { }
", includeStubs: false);

        Assert.Empty(diagnostics);
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source, bool includeStubs = true)
    {
        var trees = includeStubs
            ? new[] { CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(Stubs) }
            : new[] { CSharpSyntaxTree.ParseText(source) };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            trees,
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));

        return await compilation
            .WithAnalyzers([new PortableViewHandlerAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();
    }
}
