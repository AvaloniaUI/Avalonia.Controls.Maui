

namespace BenchmarkApp;

/// <summary>
/// Represents the outcome of a benchmark test run.
/// </summary>
public sealed class BenchmarkResult
{
    private BenchmarkResult(bool passed, string? failureReason, string? warningReason, IReadOnlyDictionary<string, object>? metrics)
    {
        Passed = passed;
        FailureReason = failureReason;
        WarningReason = warningReason;
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
    /// Gets a warning message for non-fatal threshold breaches, or <c>null</c> if none.
    /// Warnings do not cause test failure but are recorded in test output for tracking.
    /// </summary>
    public string? WarningReason { get; }

    /// <summary>
    /// Gets the metrics collected during the benchmark run.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metrics { get; }

    /// <summary>
    /// Creates a passing result with optional metrics.
    /// </summary>
    public static BenchmarkResult Pass(IReadOnlyDictionary<string, object>? metrics = null)
        => new(true, null, null, metrics);

    /// <summary>
    /// Creates a failing result with a reason and optional metrics.
    /// </summary>
    public static BenchmarkResult Fail(string reason, IReadOnlyDictionary<string, object>? metrics = null)
        => new(false, reason, null, metrics);

    /// <summary>
    /// Creates a passing result with a warning. The test will not fail, but the warning
    /// reason is recorded in test output for monitoring and tracking purposes.
    /// </summary>
    public static BenchmarkResult Warn(string reason, IReadOnlyDictionary<string, object>? metrics = null)
        => new(true, null, reason, metrics);
}
