

using System.Diagnostics;

namespace BenchmarkApp;

/// <summary>
/// Captures GC memory state, heap generation details, and process-level native memory at a point in time for before/after comparison.
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

    /// <summary>
    /// Gets the total physical memory (working set) used by the process in bytes.
    /// </summary>
    public long WorkingSet { get; }

    /// <summary>
    /// Gets the private memory committed to the process in bytes.
    /// </summary>
    public long PrivateMemory { get; }

    /// <summary>
    /// Gets the generation 0 heap size in bytes after the last GC.
    /// </summary>
    public long Gen0HeapSize { get; }

    /// <summary>
    /// Gets the generation 1 heap size in bytes after the last GC.
    /// </summary>
    public long Gen1HeapSize { get; }

    /// <summary>
    /// Gets the generation 2 heap size in bytes after the last GC.
    /// </summary>
    public long Gen2HeapSize { get; }

    /// <summary>
    /// Gets the large object heap (LOH) size in bytes after the last GC.
    /// </summary>
    public long LargeObjectHeapSize { get; }

    /// <summary>
    /// Gets the pinned object heap (POH) size in bytes after the last GC.
    /// </summary>
    public long PinnedObjectHeapSize { get; }

    /// <summary>
    /// Gets the number of pinned objects at the time of capture.
    /// </summary>
    public long PinnedObjectsCount { get; }

    /// <summary>
    /// Gets the number of fragmented bytes across all heaps.
    /// </summary>
    public long FragmentedBytes { get; }

    /// <summary>
    /// Gets the total bytes promoted during the last GC.
    /// </summary>
    public long PromotedBytes { get; }

    /// <summary>
    /// Gets the number of objects waiting for finalization.
    /// </summary>
    public long FinalizationPendingCount { get; }

    /// <summary>
    /// Gets the percentage of time spent in GC pauses since process start.
    /// </summary>
    public double GcPauseTimePercentage { get; }

    /// <summary>
    /// Gets the thread pool thread count at the time of capture.
    /// </summary>
    public int ThreadPoolThreadCount { get; }

    /// <summary>
    /// Gets the number of pending thread pool work items at the time of capture.
    /// </summary>
    public long ThreadPoolPendingWorkItems { get; }

    private MemorySnapshot(
        long totalMemory,
        int gen0,
        int gen1,
        int gen2,
        long workingSet,
        long privateMemory,
        long gen0HeapSize,
        long gen1HeapSize,
        long gen2HeapSize,
        long largeObjectHeapSize,
        long pinnedObjectHeapSize,
        long pinnedObjectsCount,
        long fragmentedBytes,
        long promotedBytes,
        long finalizationPendingCount,
        double gcPauseTimePercentage,
        int threadPoolThreadCount,
        long threadPoolPendingWorkItems)
    {
        TotalMemory = totalMemory;
        Gen0Collections = gen0;
        Gen1Collections = gen1;
        Gen2Collections = gen2;
        WorkingSet = workingSet;
        PrivateMemory = privateMemory;
        Gen0HeapSize = gen0HeapSize;
        Gen1HeapSize = gen1HeapSize;
        Gen2HeapSize = gen2HeapSize;
        LargeObjectHeapSize = largeObjectHeapSize;
        PinnedObjectHeapSize = pinnedObjectHeapSize;
        PinnedObjectsCount = pinnedObjectsCount;
        FragmentedBytes = fragmentedBytes;
        PromotedBytes = promotedBytes;
        FinalizationPendingCount = finalizationPendingCount;
        GcPauseTimePercentage = gcPauseTimePercentage;
        ThreadPoolThreadCount = threadPoolThreadCount;
        ThreadPoolPendingWorkItems = threadPoolPendingWorkItems;
    }

    /// <summary>
    /// Captures the current GC memory state, heap generation details, and process-level native memory.
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

        using var process = Process.GetCurrentProcess();
        var gcInfo = GC.GetGCMemoryInfo();

        // Extract per-generation heap sizes safely
        var genInfo = gcInfo.GenerationInfo;
        long gen0Heap = genInfo.Length > 0 ? genInfo[0].SizeAfterBytes : 0;
        long gen1Heap = genInfo.Length > 1 ? genInfo[1].SizeAfterBytes : 0;
        long gen2Heap = genInfo.Length > 2 ? genInfo[2].SizeAfterBytes : 0;
        long lohSize = genInfo.Length > 3 ? genInfo[3].SizeAfterBytes : 0;
        long pohSize = genInfo.Length > 4 ? genInfo[4].SizeAfterBytes : 0;

        return new MemorySnapshot(
            GC.GetTotalMemory(false),
            GC.CollectionCount(0),
            GC.CollectionCount(1),
            GC.CollectionCount(2),
            process.WorkingSet64,
            process.PrivateMemorySize64,
            gen0Heap,
            gen1Heap,
            gen2Heap,
            lohSize,
            pohSize,
            gcInfo.PinnedObjectsCount,
            gcInfo.FragmentedBytes,
            gcInfo.PromotedBytes,
            gcInfo.FinalizationPendingCount,
            gcInfo.PauseTimePercentage,
            ThreadPool.ThreadCount,
            ThreadPool.PendingWorkItemCount);
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
            Gen2Collections - before.Gen2Collections,
            WorkingSet - before.WorkingSet,
            PrivateMemory - before.PrivateMemory,
            Gen0HeapSize - before.Gen0HeapSize,
            Gen1HeapSize - before.Gen1HeapSize,
            Gen2HeapSize - before.Gen2HeapSize,
            LargeObjectHeapSize - before.LargeObjectHeapSize,
            PinnedObjectHeapSize - before.PinnedObjectHeapSize,
            PinnedObjectsCount - before.PinnedObjectsCount,
            FragmentedBytes - before.FragmentedBytes,
            PromotedBytes - before.PromotedBytes,
            FinalizationPendingCount - before.FinalizationPendingCount,
            before.GcPauseTimePercentage,
            GcPauseTimePercentage,
            ThreadPoolThreadCount - before.ThreadPoolThreadCount,
            ThreadPoolPendingWorkItems - before.ThreadPoolPendingWorkItems);
    }
}
