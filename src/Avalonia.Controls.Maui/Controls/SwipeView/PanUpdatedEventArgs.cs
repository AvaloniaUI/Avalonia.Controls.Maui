using System;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Contains the pan updates event data 
/// </summary>
public class PanUpdatedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of <see cref="PanUpdatedEventArgs"/> with the specified status, and total X and Y offsets.
    /// </summary>
    /// <param name="statusType">The current status of the pan gesture.</param>
    /// <param name="totalX">The total horizontal distance of the pan gesture.</param>
    /// <param name="totalY">The total vertical distance of the pan gesture.</param>
    public PanUpdatedEventArgs(PanGestureStatus statusType, double totalX, double totalY)
    {
        StatusType = statusType;
        TotalX = totalX;
        TotalY = totalY;
    }

    /// <summary>
    /// Gets or sets the current status of the pan gesture.
    /// </summary>
    public PanGestureStatus StatusType { get; set; }

    /// <summary>
    /// Gets or sets the total horizontal distance of the pan gesture.
    /// </summary>
    public double TotalX { get; set; }

    /// <summary>
    /// Gets or sets the total vertical distance of the pan gesture.
    /// </summary>
    public double TotalY { get; set; }
}
