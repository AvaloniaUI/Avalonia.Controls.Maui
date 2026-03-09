

namespace BenchmarkApp;

/// <summary>
/// Represents the difference between two <see cref="MemorySnapshot"/> captures,
/// including heap generation details, native memory breakdown, and platform handle counts.
/// </summary>
public readonly struct MemoryDelta
{
    /// <summary>
    /// Gets the change in total managed memory in bytes (positive means growth).
    /// </summary>
    public long BytesDelta { get; }

    /// <summary>
    /// Gets the number of generation 0 collections that occurred.
    /// </summary>
    public int Gen0Delta { get; }

    /// <summary>
    /// Gets the number of generation 1 collections that occurred.
    /// </summary>
    public int Gen1Delta { get; }

    /// <summary>
    /// Gets the number of generation 2 collections that occurred.
    /// </summary>
    public int Gen2Delta { get; }

    /// <summary>
    /// Gets the change in process working set (total physical memory) in bytes.
    /// </summary>
    public long WorkingSetDelta { get; }

    /// <summary>
    /// Gets the change in process private memory in bytes.
    /// </summary>
    public long PrivateMemoryDelta { get; }

    /// <summary>
    /// Gets the estimated native-only memory growth in bytes.
    /// Calculated as <c>PrivateMemoryDelta - GcCommittedBytesDelta</c>, which is more
    /// accurate than the working set approach because it subtracts the GC's total
    /// committed virtual memory (including bookkeeping and segment overhead) rather
    /// than just the live heap size.
    /// </summary>
    public long NativeMemoryEstimate => PrivateMemoryDelta - GcCommittedBytesDelta;

    /// <summary>
    /// Gets the legacy estimated native-only memory growth (working set minus managed).
    /// Prefer <see cref="NativeMemoryEstimate"/> for more accurate results.
    /// </summary>
    public long EstimatedNativeGrowth => WorkingSetDelta - BytesDelta;

    /// <summary>
    /// Gets the change in generation 0 heap size in bytes.
    /// </summary>
    public long Gen0HeapSizeDelta { get; }

    /// <summary>
    /// Gets the change in generation 1 heap size in bytes.
    /// </summary>
    public long Gen1HeapSizeDelta { get; }

    /// <summary>
    /// Gets the change in generation 2 heap size in bytes.
    /// </summary>
    public long Gen2HeapSizeDelta { get; }

    /// <summary>
    /// Gets the change in large object heap size in bytes.
    /// </summary>
    public long LargeObjectHeapSizeDelta { get; }

    /// <summary>
    /// Gets the change in pinned object heap size in bytes.
    /// </summary>
    public long PinnedObjectHeapSizeDelta { get; }

    /// <summary>
    /// Gets the change in the number of pinned objects.
    /// </summary>
    public long PinnedObjectsCountDelta { get; }

    /// <summary>
    /// Gets the change in fragmented bytes across all heaps.
    /// </summary>
    public long FragmentedBytesDelta { get; }

    /// <summary>
    /// Gets the change in promoted bytes.
    /// </summary>
    public long PromotedBytesDelta { get; }

    /// <summary>
    /// Gets the change in the number of objects waiting for finalization.
    /// </summary>
    public long FinalizationPendingCountDelta { get; }

    /// <summary>
    /// Gets the GC pause time percentage captured before the measured operation.
    /// </summary>
    public double GcPauseTimePercentageBefore { get; }

    /// <summary>
    /// Gets the GC pause time percentage captured after the measured operation.
    /// </summary>
    public double GcPauseTimePercentageAfter { get; }

    /// <summary>
    /// Gets the change in thread pool thread count.
    /// </summary>
    public int ThreadPoolThreadCountDelta { get; }

    /// <summary>
    /// Gets the change in pending thread pool work items.
    /// </summary>
    public long ThreadPoolPendingWorkItemsDelta { get; }

    /// <summary>
    /// Gets the change in GC committed virtual memory in bytes.
    /// </summary>
    public long GcCommittedBytesDelta { get; }

    /// <summary>
    /// Gets the change in total managed heap size (including fragmentation) in bytes.
    /// </summary>
    public long GcHeapSizeBytesDelta { get; }

    /// <summary>
    /// Gets the change in GDI object count (Windows only).
    /// </summary>
    public int GdiObjectDelta { get; }

    /// <summary>
    /// Gets the change in USER object count (Windows only).
    /// </summary>
    public int UserObjectDelta { get; }

    /// <summary>
    /// Gets the peak GDI object count observed during the snapshot window (Windows only).
    /// </summary>
    public uint GdiObjectsPeak { get; }

    /// <summary>
    /// Gets the peak USER object count observed during the snapshot window (Windows only).
    /// </summary>
    public uint UserObjectsPeak { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryDelta"/> struct.
    /// </summary>
    public MemoryDelta(
        long bytesDelta,
        int gen0Delta,
        int gen1Delta,
        int gen2Delta,
        long workingSetDelta,
        long privateMemoryDelta,
        long gen0HeapSizeDelta,
        long gen1HeapSizeDelta,
        long gen2HeapSizeDelta,
        long largeObjectHeapSizeDelta,
        long pinnedObjectHeapSizeDelta,
        long pinnedObjectsCountDelta,
        long fragmentedBytesDelta,
        long promotedBytesDelta,
        long finalizationPendingCountDelta,
        double gcPauseTimePercentageBefore,
        double gcPauseTimePercentageAfter,
        int threadPoolThreadCountDelta,
        long threadPoolPendingWorkItemsDelta,
        long gcCommittedBytesDelta,
        long gcHeapSizeBytesDelta,
        int gdiObjectDelta,
        int userObjectDelta,
        uint gdiObjectsPeak,
        uint userObjectsPeak)
    {
        BytesDelta = bytesDelta;
        Gen0Delta = gen0Delta;
        Gen1Delta = gen1Delta;
        Gen2Delta = gen2Delta;
        WorkingSetDelta = workingSetDelta;
        PrivateMemoryDelta = privateMemoryDelta;
        Gen0HeapSizeDelta = gen0HeapSizeDelta;
        Gen1HeapSizeDelta = gen1HeapSizeDelta;
        Gen2HeapSizeDelta = gen2HeapSizeDelta;
        LargeObjectHeapSizeDelta = largeObjectHeapSizeDelta;
        PinnedObjectHeapSizeDelta = pinnedObjectHeapSizeDelta;
        PinnedObjectsCountDelta = pinnedObjectsCountDelta;
        FragmentedBytesDelta = fragmentedBytesDelta;
        PromotedBytesDelta = promotedBytesDelta;
        FinalizationPendingCountDelta = finalizationPendingCountDelta;
        GcPauseTimePercentageBefore = gcPauseTimePercentageBefore;
        GcPauseTimePercentageAfter = gcPauseTimePercentageAfter;
        ThreadPoolThreadCountDelta = threadPoolThreadCountDelta;
        ThreadPoolPendingWorkItemsDelta = threadPoolPendingWorkItemsDelta;
        GcCommittedBytesDelta = gcCommittedBytesDelta;
        GcHeapSizeBytesDelta = gcHeapSizeBytesDelta;
        GdiObjectDelta = gdiObjectDelta;
        UserObjectDelta = userObjectDelta;
        GdiObjectsPeak = gdiObjectsPeak;
        UserObjectsPeak = userObjectsPeak;
    }

    /// <summary>
    /// Converts the delta to a metrics dictionary suitable for inclusion in <see cref="BenchmarkResult"/>.
    /// </summary>
    public IReadOnlyDictionary<string, object> ToMetrics()
    {
        var metrics = new Dictionary<string, object>
        {
            ["MemoryBytesAllocated"] = BytesDelta,
            ["Gen0Collections"] = Gen0Delta,
            ["Gen1Collections"] = Gen1Delta,
            ["Gen2Collections"] = Gen2Delta,
            ["NativeWorkingSetDelta"] = WorkingSetDelta,
            ["NativePrivateMemoryDelta"] = PrivateMemoryDelta,
            ["EstimatedNativeGrowth"] = EstimatedNativeGrowth,
            ["NativeMemoryEstimate"] = NativeMemoryEstimate,
            ["GcCommittedBytesDelta"] = GcCommittedBytesDelta,
            ["GcHeapSizeBytesDelta"] = GcHeapSizeBytesDelta,
            ["Gen0HeapSizeDelta"] = Gen0HeapSizeDelta,
            ["Gen1HeapSizeDelta"] = Gen1HeapSizeDelta,
            ["Gen2HeapSizeDelta"] = Gen2HeapSizeDelta,
            ["LargeObjectHeapSizeDelta"] = LargeObjectHeapSizeDelta,
            ["PinnedObjectHeapSizeDelta"] = PinnedObjectHeapSizeDelta,
            ["PinnedObjectsCountDelta"] = PinnedObjectsCountDelta,
            ["FragmentedBytesDelta"] = FragmentedBytesDelta,
            ["PromotedBytesDelta"] = PromotedBytesDelta,
            ["FinalizationPendingCountDelta"] = FinalizationPendingCountDelta,
            ["GcPauseTimePercentageBefore"] = GcPauseTimePercentageBefore,
            ["GcPauseTimePercentageAfter"] = GcPauseTimePercentageAfter,
            ["ThreadPoolThreadCountDelta"] = ThreadPoolThreadCountDelta,
            ["ThreadPoolPendingWorkItemsDelta"] = ThreadPoolPendingWorkItemsDelta,
        };

        // Only include handle metrics on Windows (non-zero values)
        if (GdiObjectDelta != 0 || UserObjectDelta != 0 || GdiObjectsPeak > 0)
        {
            metrics["GdiObjectDelta"] = GdiObjectDelta;
            metrics["UserObjectDelta"] = UserObjectDelta;
            metrics["GdiObjectsPeak"] = GdiObjectsPeak;
            metrics["UserObjectsPeak"] = UserObjectsPeak;
        }

        return metrics;
    }
}
