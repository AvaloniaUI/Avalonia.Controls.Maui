
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace BenchmarkApp.Diagnostics;

/// <summary>
/// Tracks allocation events during test execution using <see cref="EventListener"/>.
/// Subscribes to the .NET runtime's <c>GCAllocationTick</c> events to capture
/// allocation amounts and LOH/SOH classification.
/// </summary>
/// <remarks>
/// This tracker uses <see cref="EventListener"/> which works via virtual method overrides
/// and does not require runtime reflection, making it AOT-compatible.
/// If the runtime EventSource is not available, <see cref="IsAvailable"/> returns <c>false</c>
/// and <see cref="Stop"/> returns a zeroed summary.
/// </remarks>
public sealed class AllocationTracker : EventListener
{
    private const string RuntimeEventSourceName = "Microsoft-Windows-DotNETRuntime";
    private const int GcAllocationTickEventId = 10;
    private const long AllocationTickKeyword = 0x20; // GCAllocationTick keyword

    private long _totalAllocatedBytes;
    private long _lohAllocatedBytes;
    private long _allocationCount;
    private bool _tracking;
    private Stopwatch? _stopwatch;
    private EventSource? _runtimeSource;

    /// <summary>
    /// Gets a value indicating whether the runtime EventSource was found and is available.
    /// </summary>
    public bool IsAvailable => _runtimeSource is not null;

    /// <summary>
    /// Starts tracking allocations. Call <see cref="Stop"/> to end tracking and get results.
    /// </summary>
    public void Start()
    {
        _totalAllocatedBytes = 0;
        _lohAllocatedBytes = 0;
        _allocationCount = 0;
        _stopwatch = Stopwatch.StartNew();
        _tracking = true;
    }

    /// <summary>
    /// Stops tracking and returns the allocation summary.
    /// </summary>
    /// <returns>An <see cref="AllocationSummary"/> with the collected data.</returns>
    public AllocationSummary Stop()
    {
        _tracking = false;
        _stopwatch?.Stop();

        return new AllocationSummary(
            Interlocked.Read(ref _totalAllocatedBytes),
            Interlocked.Read(ref _lohAllocatedBytes),
            Interlocked.Read(ref _allocationCount),
            _stopwatch?.Elapsed ?? TimeSpan.Zero);
    }

    /// <inheritdoc/>
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == RuntimeEventSourceName)
        {
            _runtimeSource = eventSource;
            EnableEvents(eventSource, EventLevel.Informational, (EventKeywords)AllocationTickKeyword);
        }
    }

    /// <inheritdoc/>
    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (!_tracking)
            return;

        if (eventData.EventId != GcAllocationTickEventId)
            return;

        if (eventData.Payload is null || eventData.Payload.Count < 4)
            return;

        // Payload[0] = AllocationAmount (uint), Payload[1] = AllocationKind (uint: 0=small, 1=large)
        if (eventData.Payload[0] is uint allocationAmount)
        {
            Interlocked.Add(ref _totalAllocatedBytes, allocationAmount);
            Interlocked.Increment(ref _allocationCount);

            if (eventData.Payload[1] is uint kind && kind == 1)
            {
                Interlocked.Add(ref _lohAllocatedBytes, allocationAmount);
            }
        }
    }
}
