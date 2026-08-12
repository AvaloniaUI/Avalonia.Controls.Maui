using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.LifecycleEvents;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Helpers mirroring small internal MAUI extension methods that are
/// not accessible outside the MAUI assemblies.
/// </summary>
internal static class MauiInternalExtensions
{
    /// <summary>Observes the task, logging any exception instead of letting it go unobserved.</summary>
    public static async void FireAndForget(this Task task, ILogger? logger, [CallerMemberName] string? callerName = null)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Unexpected exception in {Member}.", callerName);
        }
    }

    /// <summary>Invokes all registered lifecycle event delegates of the given type.</summary>
    public static void InvokeLifecycleEvents<TDelegate>(this IServiceProvider services, Action<TDelegate> action)
        where TDelegate : Delegate
    {
        if (services.GetService<ILifecycleEventService>() is not ILifecycleEventService lifecycleService)
            return;

        foreach (var del in lifecycleService.GetEventDelegates<TDelegate>(typeof(TDelegate).Name))
            action?.Invoke(del);
    }

    /// <summary>Returns the index of an item in a sequence, or -1 when not found.</summary>
    public static int IndexOf(this IEnumerable enumerable, object? item)
    {
        var i = 0;
        foreach (var element in enumerable)
        {
            if (Equals(element, item))
                return i;
            i++;
        }

        return -1;
    }

    /// <summary>
    /// Returns the element's composite gesture recognizers, or <see langword="null"/> when the element does not
    /// expose them.
    /// </summary>
    public static IList<Microsoft.Maui.Controls.IGestureRecognizer>? GetCompositeGestureRecognizers(this Element element)
        => (element as IGestureController)?.CompositeGestureRecognizers;

    /// <summary>
    /// Indicates whether the view requires a container view for Clip/Shadow support.
    /// </summary>
    public static bool NeedsContainer(this IView? view)
    {
        return view?.Clip != null || view?.Shadow != null;
    }
}
