

namespace BenchmarkApp;

/// <summary>
/// Captures GC memory state at a point in time for before/after comparison.
/// </summary>
public readonly struct MemorySnapshot
{
    /// <summary>
    /// Gets the total managed memory in bytes at the time of capture.
    /// </summary>
    public long TotalMemory { get; }

    /// <summary>
    /// Gets the number of generation 0 collections at the time of capture.
    /// </summary>
    public int Gen0Collections { get; }

    /// <summary>
    /// Gets the number of generation 1 collections at the time of capture.
    /// </summary>
    public int Gen1Collections { get; }

    /// <summary>
    /// Gets the number of generation 2 collections at the time of capture.
    /// </summary>
    public int Gen2Collections { get; }

    private MemorySnapshot(long totalMemory, int gen0, int gen1, int gen2)
    {
        TotalMemory = totalMemory;
        Gen0Collections = gen0;
        Gen1Collections = gen1;
        Gen2Collections = gen2;
    }

    /// <summary>
    /// Captures the current GC memory state.
    /// </summary>
    /// <param name="forceGC">If <c>true</c>, forces a full GC before capturing.</param>
    /// <returns>A snapshot of the current memory state.</returns>
    public static MemorySnapshot Capture(bool forceGC = false)
    {
        if (forceGC)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        return new MemorySnapshot(
            GC.GetTotalMemory(false),
            GC.CollectionCount(0),
            GC.CollectionCount(1),
            GC.CollectionCount(2));
    }

    /// <summary>
    /// Computes the delta between this snapshot and an earlier one.
    /// </summary>
    /// <param name="before">The snapshot taken before the measured operation.</param>
    /// <returns>The difference between the two snapshots.</returns>
    public MemoryDelta Compare(MemorySnapshot before)
    {
        return new MemoryDelta(
            TotalMemory - before.TotalMemory,
            Gen0Collections - before.Gen0Collections,
            Gen1Collections - before.Gen1Collections,
            Gen2Collections - before.Gen2Collections);
    }
}
