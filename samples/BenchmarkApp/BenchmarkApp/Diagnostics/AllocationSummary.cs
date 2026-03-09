
namespace BenchmarkApp.Diagnostics;

/// <summary>
/// Summarizes allocation data collected during a benchmark test run.
/// </summary>
/// <param name="TotalAllocatedBytes">Total bytes allocated (small object + large object heap).</param>
/// <param name="LohAllocatedBytes">Bytes allocated on the large object heap.</param>
/// <param name="AllocationCount">Number of allocation events observed.</param>
/// <param name="ElapsedTime">Duration of the tracking period.</param>
public readonly record struct AllocationSummary(
    long TotalAllocatedBytes,
    long LohAllocatedBytes,
    long AllocationCount,
    TimeSpan ElapsedTime)
{
    /// <summary>
    /// Gets the allocation rate in bytes per second, or zero if elapsed time is zero.
    /// </summary>
    public double AllocationRateBytesPerSecond =>
        ElapsedTime.TotalSeconds > 0 ? TotalAllocatedBytes / ElapsedTime.TotalSeconds : 0;

    /// <summary>
    /// Converts the summary to a metrics dictionary with prefixed keys.
    /// </summary>
    public IReadOnlyDictionary<string, object> ToMetrics()
    {
        return new Dictionary<string, object>
        {
            ["AllocationTracker.TotalAllocatedBytes"] = TotalAllocatedBytes,
            ["AllocationTracker.LohAllocatedBytes"] = LohAllocatedBytes,
            ["AllocationTracker.AllocationCount"] = AllocationCount,
            ["AllocationTracker.ElapsedMs"] = ElapsedTime.TotalMilliseconds,
            ["AllocationTracker.AllocationRateBytesPerSecond"] = AllocationRateBytesPerSecond,
        };
    }
}
