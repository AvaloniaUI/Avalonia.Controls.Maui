

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

    /// <summary>
    /// Creates a benchmark failure when native memory growth exceeds the given threshold using
    /// <see cref="MemoryDelta.NativeMemoryEstimate"/> (PrivateMemory minus GC committed bytes),
    /// which excludes shared libraries, GPU buffers, and OS page cache that make WorkingSet
    /// unreliable on CI.
    /// </summary>
    /// <returns>
    /// A failing <see cref="BenchmarkResult"/> when native memory growth exceeds the threshold;
    /// otherwise, <c>null</c>.
    /// </returns>
    protected static BenchmarkResult? CreateNativeMemoryFailure(
        MemoryDelta memoryDelta,
        ILogger logger,
        IReadOnlyDictionary<string, object>? metrics = null,
        long thresholdBytes = 50 * 1024 * 1024)
    {
        var nativeGrowth = memoryDelta.NativeMemoryEstimate;

        if (nativeGrowth > thresholdBytes)
        {
            logger.LogWarning(
                "Native memory growth {Growth:F1} MB exceeds {Threshold:F0} MB threshold",
                nativeGrowth / (1024.0 * 1024),
                thresholdBytes / (1024.0 * 1024));

            return BenchmarkResult.Fail(
                $"Native memory growth {nativeGrowth / (1024.0 * 1024):F1} MB exceeds {thresholdBytes / (1024.0 * 1024):F0} MB threshold",
                metrics);
        }

        if (memoryDelta.WorkingSetDelta > thresholdBytes)
        {
            logger.LogWarning(
                "Working set grew {Growth:F1} MB (exceeds {Threshold:F0} MB) but native estimate is {Native:F1} MB, within bounds",
                memoryDelta.WorkingSetDelta / (1024.0 * 1024),
                thresholdBytes / (1024.0 * 1024),
                nativeGrowth / (1024.0 * 1024));
        }

        return null;
    }
}
