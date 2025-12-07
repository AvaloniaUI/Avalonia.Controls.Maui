namespace Avalonia.Controls.Maui;

/// <summary>
/// Helper class to wrap items in a grouped collection view
/// </summary>
internal class GroupItem
{
    public object? Data { get; set; }
    public bool IsHeader { get; set; }
    public bool IsFooter { get; set; }
}