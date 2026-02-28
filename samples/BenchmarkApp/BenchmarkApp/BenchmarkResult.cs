

namespace BenchmarkApp;

/// <summary>
/// Represents the outcome of a benchmark test run.
/// </summary>
public sealed class BenchmarkResult
{
    private BenchmarkResult(bool passed, string? failureReason, IReadOnlyDictionary<string, object>? metrics)
    {
        Passed = passed;
        FailureReason = failureReason;
        Metrics = metrics ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets a value indicating whether the benchmark passed.
    /// </summary>
    public bool Passed { get; }

    /// <summary>
    /// Gets the reason the benchmark failed, or <c>null</c> if it passed.
    /// </summary>
    public string? FailureReason { get; }

    /// <summary>
    /// Gets the metrics collected during the benchmark run.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metrics { get; }

    /// <summary>
    /// Creates a passing result with optional metrics.
    /// </summary>
    public static BenchmarkResult Pass(IReadOnlyDictionary<string, object>? metrics = null)
        => new(true, null, metrics);

    /// <summary>
    /// Creates a failing result with a reason and optional metrics.
    /// </summary>
    public static BenchmarkResult Fail(string reason, IReadOnlyDictionary<string, object>? metrics = null)
        => new(false, reason, metrics);
}
