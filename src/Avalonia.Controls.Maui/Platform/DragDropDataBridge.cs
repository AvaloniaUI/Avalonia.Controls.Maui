using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Static side-channel for passing MAUI DataPackage between drag source and drop target.
/// Only one drag operation can be active at a time and all operations run on the UI thread,
/// so static state is safe.
/// </summary>
internal static class DragDropDataBridge
{
    /// <summary>
    /// The active DataPackage from the current drag operation.
    /// </summary>
    public static DataPackage? ActiveDataPackage { get; set; }

    /// <summary>
    /// The MAUI View that is the source of the current drag operation.
    /// </summary>
    public static View? ActiveDragSourceView { get; set; }

    /// <summary>
    /// The DragGestureRecognizers from the source view.
    /// </summary>
    public static List<DragGestureRecognizer>? ActiveDragRecognizers { get; set; }

    /// <summary>
    /// Clears all active drag state.
    /// </summary>
    public static void Clear()
    {
        ActiveDataPackage = null;
        ActiveDragSourceView = null;
        ActiveDragRecognizers = null;
    }
}
