using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Exposes the connection state of a handler so property mappers can skip default values
/// during the initial connection. Mirrors MAUI's internal ElementHandlerState tracking,
/// which is not accessible outside the MAUI assemblies.
/// </summary>
internal interface IHandlerStateExhibitor
{
    /// <summary>
    /// Gets a value indicating whether the handler is connecting to the element for the first time.
    /// </summary>
    bool IsConnectingHandler { get; }

    /// <summary>
    /// Gets a value indicating whether the handler is currently mapping all properties to the element.
    /// </summary>
    bool IsMappingProperties { get; }
}

internal static class HandlerStateExtensions
{
    /// <summary>
    /// Indicates whether the handler is connecting for the first time to the element and mapping all properties.
    /// </summary>
    internal static bool IsConnectingHandler(this IViewHandler handler) =>
        (handler as IHandlerStateExhibitor)?.IsConnectingHandler ?? false;

    /// <summary>
    /// Indicates whether the handler is currently mapping all properties to the element.
    /// </summary>
    internal static bool IsMappingProperties(this IViewHandler handler) =>
        (handler as IHandlerStateExhibitor)?.IsMappingProperties ?? false;
}
