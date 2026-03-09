using System.Diagnostics.CodeAnalysis;
using Avalonia.Media;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Custom Entry control for .NET MAUI integration.
/// </summary>
public class MauiEntry : TextBox
{
    /// <summary>
    /// Defines the <see cref="ClearButtonVisibility"/> property.
    /// </summary>
    public static readonly StyledProperty<ClearButtonVisibility> ClearButtonVisibilityProperty =
        AvaloniaProperty.Register<MauiEntry, ClearButtonVisibility>(nameof(ClearButtonVisibility));

    /// <summary>
    /// Gets or sets the clear button visibility.
    /// </summary>
    public ClearButtonVisibility ClearButtonVisibility
    {
        get => GetValue(ClearButtonVisibilityProperty);
        set => SetValue(ClearButtonVisibilityProperty, value);
    }

    /// <summary>
    /// Occurs when the text selection changes.
    /// </summary>
    public event EventHandler<Interactivity.RoutedEventArgs>? SelectionChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiEntry"/> class.
    /// </summary>
    public MauiEntry()
    {
        AcceptsReturn = false;
        TextWrapping = TextWrapping.NoWrap;

        // Ensure selection is visible
        SelectionBrush = Brushes.Blue;
        SelectionForegroundBrush = Brushes.White;
    }

    /// <summary>
    /// Defines the <see cref="IsClearButtonVisible"/> property.
    /// </summary>
    public static readonly DirectProperty<MauiEntry, bool> IsClearButtonVisibleProperty =
        AvaloniaProperty.RegisterDirect<MauiEntry, bool>(nameof(IsClearButtonVisible), o => o.IsClearButtonVisible);

    private bool _isClearButtonVisible;

    /// <summary>
    /// Gets a value indicating whether the clear button is visible.
    /// </summary>
    public bool IsClearButtonVisible
    {
        get => _isClearButtonVisible;
        private set => SetAndRaise(IsClearButtonVisibleProperty, ref _isClearButtonVisible, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty ||
            change.Property == IsFocusedProperty ||
            change.Property == ClearButtonVisibilityProperty)
        {
            UpdateIsClearButtonVisible();
        }

        if (change.Property == CaretIndexProperty ||
            change.Property == SelectionStartProperty ||
            change.Property == SelectionEndProperty)
        {
            RaiseSelectionChanged();
        }
    }

    private void UpdateIsClearButtonVisible()
    {
        IsClearButtonVisible = ClearButtonVisibility == ClearButtonVisibility.WhileEditing
                               && !string.IsNullOrEmpty(Text)
                               && IsFocused;
    }

    /// <summary>
    /// Raises the <see cref="SelectionChanged"/> event.
    /// </summary>
    public void RaiseSelectionChanged()
    {
        SelectionChanged?.Invoke(this, new Interactivity.RoutedEventArgs());
    }

    /// <summary>
    /// Clears the text.
    /// </summary>
    public new void Clear()
    {
        Text = string.Empty;
    }

    /// <summary>
    /// Gets the command to clear the text.
    /// </summary>
    [field: AllowNull, MaybeNull]
    public System.Windows.Input.ICommand ClearCommand => field ??= new ActionCommand(Clear);

    private class ActionCommand : System.Windows.Input.ICommand
    {
        private readonly Action _action;
        public ActionCommand(Action action) => _action = action;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _action();

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
