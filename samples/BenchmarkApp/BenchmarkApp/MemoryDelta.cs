

namespace BenchmarkApp;

/// <summary>
/// Represents the difference between two <see cref="MemorySnapshot"/> captures.
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
    /// Initializes a new instance of the <see cref="MemoryDelta"/> struct.
    /// </summary>
    public MemoryDelta(long bytesDelta, int gen0Delta, int gen1Delta, int gen2Delta)
    {
        BytesDelta = bytesDelta;
        Gen0Delta = gen0Delta;
        Gen1Delta = gen1Delta;
        Gen2Delta = gen2Delta;
    }

    /// <summary>
    /// Converts the delta to a metrics dictionary suitable for inclusion in <see cref="BenchmarkResult"/>.
    /// </summary>
    public IReadOnlyDictionary<string, object> ToMetrics()
    {
        return new Dictionary<string, object>
        {
            ["MemoryBytesAllocated"] = BytesDelta,
            ["Gen0Collections"] = Gen0Delta,
            ["Gen1Collections"] = Gen1Delta,
            ["Gen2Collections"] = Gen2Delta,
        };
    }
}
