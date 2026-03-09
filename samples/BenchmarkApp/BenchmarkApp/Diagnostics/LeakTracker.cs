
namespace BenchmarkApp.Diagnostics;

/// <summary>
/// Information about an object that survived GC when it was expected to be collected.
/// </summary>
/// <param name="Name">Descriptive name of the tracked object.</param>
/// <param name="InitialGeneration">The GC generation the object was in when first tracked.</param>
/// <param name="CurrentGeneration">The GC generation the object is in after collection attempt, or -1 if collected.</param>
/// <param name="WasPromoted">Whether the object was promoted to a higher generation than it started in.</param>
public record LeakSurvivor(string Name, int InitialGeneration, int CurrentGeneration, bool WasPromoted);

/// <summary>
/// Result of a leak check, containing counts and details of surviving objects.
/// </summary>
/// <param name="TotalTracked">Total number of objects tracked.</param>
/// <param name="CollectedCount">Number of objects that were successfully collected.</param>
/// <param name="Survivors">Details of objects that survived GC.</param>
public record LeakCheckResult(int TotalTracked, int CollectedCount, IReadOnlyList<LeakSurvivor> Survivors)
{
    /// <summary>
    /// Converts the result to a metrics dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, object> ToMetrics()
    {
        var metrics = new Dictionary<string, object>
        {
            ["LeakTracker.TotalTracked"] = TotalTracked,
            ["LeakTracker.CollectedCount"] = CollectedCount,
            ["LeakTracker.SurvivorCount"] = Survivors.Count,
        };

        foreach (var survivor in Survivors)
        {
            metrics[$"LeakTracker.Survivor.{survivor.Name}.InitialGen"] = survivor.InitialGeneration;
            metrics[$"LeakTracker.Survivor.{survivor.Name}.CurrentGen"] = survivor.CurrentGeneration;
            metrics[$"LeakTracker.Survivor.{survivor.Name}.WasPromoted"] = survivor.WasPromoted;
        }

        return metrics;
    }
}

/// <summary>
/// Tracks objects with long weak references and generation information to detect memory leaks
/// and unexpected object promotion.
/// </summary>
/// <remarks>
/// Uses <c>WeakReference(target, trackResurrection: true)</c> (long weak reference) so that
/// objects with custom finalizers are also tracked through finalization.
/// </remarks>
public sealed class LeakTracker
{
    private readonly List<(string Name, WeakReference Reference, int InitialGeneration)> _tracked = new();

    /// <summary>
    /// Begins tracking the specified object.
    /// </summary>
    /// <param name="name">A descriptive name for the object being tracked.</param>
    /// <param name="target">The object to track.</param>
    public void Track(string name, object target)
    {
        var gen = GC.GetGeneration(target);
        _tracked.Add((name, new WeakReference(target, trackResurrection: true), gen));
    }

    /// <summary>
    /// Checks which tracked objects survived GC.
    /// Call <see cref="ForceFullGcAsync"/> before this for accurate results.
    /// </summary>
    /// <returns>A <see cref="LeakCheckResult"/> with collection and survivor details.</returns>
    public LeakCheckResult Check()
    {
        int collected = 0;
        var survivors = new List<LeakSurvivor>();

        foreach (var (name, weakRef, initialGen) in _tracked)
        {
            if (!weakRef.IsAlive)
            {
                collected++;
            }
            else
            {
                var currentGen = GC.GetGeneration(weakRef.Target!);
                survivors.Add(new LeakSurvivor(name, initialGen, currentGen, currentGen > initialGen));
            }
        }

        return new LeakCheckResult(_tracked.Count, collected, survivors);
    }

    /// <summary>
    /// Performs the standard triple-GC pattern with finalizer processing to ensure all
    /// collectable objects are reclaimed.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public static async Task ForceFullGcAsync(CancellationToken cancellationToken = default)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
