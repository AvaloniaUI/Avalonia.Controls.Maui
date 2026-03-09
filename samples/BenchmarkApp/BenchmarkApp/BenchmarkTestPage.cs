

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp;

/// <summary>
/// Base class for benchmark test pages. Subclasses set up their MAUI control tree
/// in the constructor and perform measurements in <see cref="RunAsync"/>.
/// </summary>
public abstract class BenchmarkTestPage : ContentPage
{
    /// <summary>
    /// Runs the benchmark and returns the result.
    /// </summary>
    /// <param name="window">The window hosting the test page. Tests that need to swap the
    /// window's page (e.g. to test <see cref="TabbedPage"/>) can use this.</param>
    /// <param name="logger">Logger for diagnostic output during the run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The benchmark result indicating pass/fail and any collected metrics.</returns>
    public abstract Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken);
}
