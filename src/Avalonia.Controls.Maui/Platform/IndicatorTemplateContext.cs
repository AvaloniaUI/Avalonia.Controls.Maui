namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Data context for the IndicatorTemplate.
/// </summary>
public class IndicatorTemplateContext
{
    /// <summary>
    /// Gets or sets the index of the indicator.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the indicator is currently selected.
    /// </summary>
    public bool IsSelected { get; set; }
}
