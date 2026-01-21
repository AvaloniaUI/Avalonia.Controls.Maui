using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Platform control for rendering a MAUI SwitchCell.
/// </summary>
public class MauiSwitchCell : Border
{
    /// <summary>
    /// Gets the label text block.
    /// </summary>
    public TextBlock Label { get; }

    /// <summary>
    /// Gets the toggle switch.
    /// </summary>
    public ToggleSwitch ToggleSwitch { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiSwitchCell"/> class.
    /// </summary>
    public MauiSwitchCell()
    {
        Label = new TextBlock
        {
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center
        };

        ToggleSwitch = new ToggleSwitch
        {
            VerticalAlignment = VerticalAlignment.Center
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(16, 8)
        };

        Grid.SetColumn(Label, 0);
        Grid.SetColumn(ToggleSwitch, 1);

        grid.Children.Add(Label);
        grid.Children.Add(ToggleSwitch);

        MinHeight = 44;
        Background = Brushes.Transparent;
        Child = grid;
    }
}
