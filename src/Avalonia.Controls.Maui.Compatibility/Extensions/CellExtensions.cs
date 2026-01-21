using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Shared extension methods for all cell types.
/// </summary>
public static class CellExtensions
{
    /// <summary>
    /// Builds a context menu from the cell's context actions.
    /// </summary>
    /// <param name="cell">The MAUI cell.</param>
    /// <returns>A configured ContextMenu, or null if no context actions exist.</returns>
    public static ContextMenu? BuildContextMenu(Cell cell)
    {
        if (!cell.HasContextActions)
        {
            return null;
        }

        var contextMenu = new ContextMenu();
        var menuItems = new List<MenuItem>();

        foreach (var action in cell.ContextActions)
        {
            var menuItem = new MenuItem
            {
                Header = action.Text,
            };

            // Capture action for closure
            var capturedAction = action;
            menuItem.Click += (s, e) =>
            {
                if (capturedAction.Command?.CanExecute(capturedAction.CommandParameter) == true)
                {
                    capturedAction.Command.Execute(capturedAction.CommandParameter);
                }
            };

            // Apply destructive styling if marked
            if (action.IsDestructive)
            {
                menuItem.Foreground = Media.Brushes.Red;
            }

            menuItems.Add(menuItem);
        }

        contextMenu.ItemsSource = menuItems;
        return contextMenu;
    }
}
