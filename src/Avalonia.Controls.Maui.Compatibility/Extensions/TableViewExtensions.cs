using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping TableView properties to Avalonia MauiTableView.
/// </summary>
public static class TableViewExtensions
{
    /// <summary>
    /// Updates the TableView root content on the platform view.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiTableView control.</param>
    /// <param name="tableView">The MAUI TableView providing the content.</param>
    public static void UpdateRoot(this MauiTableView platformView, TableView tableView)
    {
        platformView.TableView = tableView;
    }

    /// <summary>
    /// Updates the uniform row height for cells.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiTableView control.</param>
    /// <param name="tableView">The MAUI TableView providing the row height.</param>
    public static void UpdateRowHeight(this MauiTableView platformView, TableView tableView)
    {
        platformView.RowHeight = tableView.RowHeight;
    }

    /// <summary>
    /// Updates whether rows can have variable heights.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiTableView control.</param>
    /// <param name="tableView">The MAUI TableView providing the setting.</param>
    public static void UpdateHasUnevenRows(this MauiTableView platformView, TableView tableView)
    {
        platformView.HasUnevenRows = tableView.HasUnevenRows;
    }

    /// <summary>
    /// Updates the table intent (Data, Form, Settings, Menu) which affects styling.
    /// </summary>
    /// <param name="platformView">The Avalonia MauiTableView control.</param>
    /// <param name="tableView">The MAUI TableView providing the intent.</param>
    public static void UpdateIntent(this MauiTableView platformView, TableView tableView)
    {
        platformView.Intent = tableView.Intent;
    }
}
