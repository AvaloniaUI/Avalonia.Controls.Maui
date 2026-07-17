using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Service resolution helpers mirroring MAUI's internal ServiceProviderExtensions and
/// ElementHandlerExtensions, which are not accessible outside the MAUI assemblies.
/// </summary>
internal static class MauiServiceProviderExtensions
{
    /// <summary>Creates a typed logger from the service provider, if logging is registered.</summary>
    internal static ILogger<T>? CreateLogger<T>(this IServiceProvider services) =>
        services.GetService<ILoggerFactory>()?.CreateLogger<T>();

    /// <summary>Creates a logger for the given type from the service provider, if logging is registered.</summary>
    internal static ILogger? CreateLogger(this IServiceProvider services, Type type) =>
        services.GetService<ILoggerFactory>()?.CreateLogger(type.FullName ?? type.Name);

    /// <summary>Creates a typed logger from the handler's MAUI context, if logging is registered.</summary>
    internal static ILogger<T>? CreateLogger<T>(this IMauiContext context) =>
        context.Services.CreateLogger<T>();

    /// <summary>Resolves a required service from the handler's MAUI context.</summary>
    internal static T GetRequiredService<T>(this IElementHandler handler) where T : notnull
    {
        var context = handler.MauiContext ??
            throw new InvalidOperationException($"Unable to find the context. The {nameof(IElementHandler.MauiContext)} property should have been set by the host.");

        var services = context.Services ??
            throw new InvalidOperationException($"Unable to find the service provider. The {nameof(IElementHandler.MauiContext)} property should have been set by the host.");

        return services.GetRequiredService<T>();
    }
}
