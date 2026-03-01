

using System.Diagnostics.Metrics;

namespace BenchmarkApp;

/// <summary>
/// Publishes benchmark metrics via <see cref="System.Diagnostics.Metrics"/> for
/// monitoring with <c>dotnet-counters</c>.
/// </summary>
/// <remarks>
/// Usage: <c>dotnet-counters monitor --process-id &lt;PID&gt; BenchmarkApp</c>
/// </remarks>
public static class BenchmarkMetrics
{
    private static readonly Meter Meter = new("BenchmarkApp");

    /// <summary>
    /// Records the elapsed time in milliseconds per benchmark iteration.
    /// </summary>
    public static readonly Histogram<double> BenchmarkDuration =
        Meter.CreateHistogram<double>("benchmark.duration", "ms", "Elapsed time per iteration");

    /// <summary>
    /// Counts the total number of benchmark iterations executed.
    /// </summary>
    public static readonly Counter<int> IterationCount =
        Meter.CreateCounter<int>("benchmark.iterations", "{iterations}", "Total iterations executed");

    /// <summary>
    /// Counts the number of passing benchmark iterations.
    /// </summary>
    public static readonly Counter<int> PassCount =
        Meter.CreateCounter<int>("benchmark.passed", "{iterations}", "Passed iterations");

    /// <summary>
    /// Counts the number of failing benchmark iterations.
    /// </summary>
    public static readonly Counter<int> FailCount =
        Meter.CreateCounter<int>("benchmark.failed", "{iterations}", "Failed iterations");

    /// <summary>
    /// Records the memory byte delta per benchmark iteration.
    /// </summary>
    public static readonly Histogram<long> MemoryDelta =
        Meter.CreateHistogram<long>("benchmark.memory_delta", "bytes", "Memory delta per iteration");

    /// <summary>
    /// Records the allocation rate in bytes per second when allocation tracking is enabled.
    /// </summary>
    public static readonly Histogram<double> AllocationRate =
        Meter.CreateHistogram<double>("benchmark.allocation_rate", "bytes/s", "Allocation rate per iteration");

    /// <summary>
    /// Records the total bytes allocated during a benchmark iteration when allocation tracking is enabled.
    /// </summary>
    public static readonly Histogram<long> TotalAllocated =
        Meter.CreateHistogram<long>("benchmark.total_allocated", "bytes", "Total allocated bytes per iteration");

    /// <summary>
    /// Records the number of pending thread pool work items per benchmark iteration.
    /// </summary>
    public static readonly Histogram<int> ThreadPoolPending =
        Meter.CreateHistogram<int>("benchmark.threadpool_pending", "{items}", "Pending thread pool work items per iteration");

    /// <summary>
    /// Records all instruments for a single benchmark iteration.
    /// </summary>
    /// <param name="testName">The name of the benchmark test.</param>
    /// <param name="elapsedMs">Elapsed time in milliseconds.</param>
    /// <param name="memoryBytes">Memory delta in bytes.</param>
    /// <param name="passed">Whether the iteration passed.</param>
    public static void RecordIteration(string testName, double elapsedMs, long memoryBytes, bool passed)
    {
        var tag = new KeyValuePair<string, object?>("benchmark.test", testName);

        BenchmarkDuration.Record(elapsedMs, tag);
        IterationCount.Add(1, tag);
        MemoryDelta.Record(memoryBytes, tag);

        if (passed)
        {
            PassCount.Add(1, tag);
        }
        else
        {
            FailCount.Add(1, tag);
        }
    }
}
