using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Platform control for rendering a MAUI EntryCell.
/// </summary>
public class MauiEntryCell : Border
{
    /// <summary>
    /// Gets the label text block.
    /// </summary>
    public TextBlock Label { get; }

    /// <summary>
    /// Gets the input text box.
    /// </summary>
    public TextBox Input { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiEntryCell"/> class.
    /// </summary>
    public MauiEntryCell()
    {
        Label = new TextBlock
        {
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 100,
            Margin = new Thickness(0, 0, 12, 0)
        };

        Input = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            Margin = new Thickness(16, 8)
        };

        Grid.SetColumn(Label, 0);
        Grid.SetColumn(Input, 1);

        grid.Children.Add(Label);
        grid.Children.Add(Input);

        MinHeight = 44;
        Background = Brushes.Transparent;
        Child = grid;
    }
}
