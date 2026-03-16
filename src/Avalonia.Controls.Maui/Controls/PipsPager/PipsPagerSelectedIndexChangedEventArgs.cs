using Avalonia.Interactivity;

// When Avalonia.Controls.PipsPager ships, remove this file entirely.
namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// Provides data for the <see cref="PipsPager.SelectedIndexChanged"/> event.
/// </summary>
public class PipsPagerSelectedIndexChangedEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of <see cref="PipsPagerSelectedIndexChangedEventArgs"/>.</summary>
    /// <param name="oldIndex">The previous selected index.</param>
    /// <param name="newIndex">The new selected index.</param>
    public PipsPagerSelectedIndexChangedEventArgs(int oldIndex, int newIndex)
        : base(PipsPager.SelectedIndexChangedEvent)
    {
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }

    /// <summary>Gets the previous selected index.</summary>
    public int OldIndex { get; }

    /// <summary>Gets the new selected index.</summary>
    public int NewIndex { get; }
}
