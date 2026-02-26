using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Avalonia dialog control that presents a MAUI action sheet with optional title, cancel, destruction, and additional action buttons.
/// </summary>
public partial class MauiActionSheetDialog : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MauiActionSheetDialog"/> class.
    /// </summary>
    public MauiActionSheetDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets the text of the button that was selected to close the dialog.
    /// </summary>
    public string? Result { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the dialog has a non-empty title.
    /// </summary>
    public bool HasTitle => !string.IsNullOrEmpty(Title);

    /// <summary>
    /// Gets or sets the title displayed at the top of the action sheet.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the text for the cancel button.
    /// </summary>
    public string? CancelButtonText { get; set; }

    /// <summary>
    /// Gets a value indicating whether the cancel button should be displayed.
    /// </summary>
    public bool HasCancelButton => !string.IsNullOrEmpty(CancelButtonText);

    /// <summary>
    /// Gets the command that closes the dialog with the cancel button text as the result.
    /// </summary>
    public ICommand CancelCommand => new RelayCommand<object>(_ => CloseWithResult(CancelButtonText));

    /// <summary>
    /// Gets or sets the text for the destructive action button.
    /// </summary>
    public string? DestructionButtonText { get; set; }

    /// <summary>
    /// Gets the formatted text for the destructive action button.
    /// </summary>
    public string? DestructionButtonFormatted => DestructionButtonText;

    /// <summary>
    /// Gets a value indicating whether the destructive action button should be displayed.
    /// </summary>
    public bool HasDestructionButton => !string.IsNullOrEmpty(DestructionButtonText);

    /// <summary>
    /// Gets the command that closes the dialog with the destruction button text as the result.
    /// </summary>
    public ICommand DestructionCommand => new RelayCommand<object>(_ => CloseWithResult(DestructionButtonText));

    /// <summary>
    /// Gets the collection of additional action buttons displayed in the dialog.
    /// </summary>
    public ObservableCollection<ActionSheetButtonViewModel>? Buttons { get; private set; }

    /// <summary>
    /// Occurs when the dialog is closed with a result, providing the selected button text.
    /// </summary>
    public event Action<string?>? OnResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiActionSheetDialog"/> class with the specified title, cancel text, destruction text, and additional buttons.
    /// </summary>
    /// <param name="title">The title to display at the top of the action sheet.</param>
    /// <param name="cancel">The text for the cancel button, or an empty string to hide it.</param>
    /// <param name="destruction">The text for the destructive action button, or an empty string to hide it.</param>
    /// <param name="buttons">An optional array of additional button labels to display.</param>
    public MauiActionSheetDialog(string title, string cancel, string destruction, string[]? buttons) : this()
    {
        Title = title;
        CancelButtonText = string.IsNullOrEmpty(cancel) ? null : cancel;
        DestructionButtonText = string.IsNullOrEmpty(destruction) ? null : destruction;
        
        Buttons = new ObservableCollection<ActionSheetButtonViewModel>();
        if (buttons != null)
        {
            foreach (var btn in buttons)
            {
                Buttons.Add(new ActionSheetButtonViewModel(btn, new RelayCommand<string>(OnButtonClicked)));
            }
        }

        DataContext = null;
        DataContext = this;
    }

    private void OnButtonClicked(string text)
    {
        CloseWithResult(text);
    }

    private void OnButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var text = button.CommandParameter as string ?? button.Content?.ToString();
            CloseWithResult(text);
        }
    }

    private void CloseWithResult(string? result)
    {
        Result = result;
        OnResult?.Invoke(result);
    }
}

/// <summary>
/// View model representing a single button in an action sheet dialog.
/// </summary>
public class ActionSheetButtonViewModel
{
    /// <summary>
    /// Gets the display text of the button.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the command executed when the button is clicked.
    /// </summary>
    public ICommand Command { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionSheetButtonViewModel"/> class.
    /// </summary>
    /// <param name="text">The display text of the button.</param>
    /// <param name="command">The command to execute when the button is clicked.</param>
    public ActionSheetButtonViewModel(string text, ICommand command)
    {
        Text = text;
        Command = command;
    }
}

// Simple RelayCommand implementation to avoid dependencies if not available
internal class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T>? _canExecute;

    public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

#pragma warning disable CS0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute((T)parameter!);
    }

    public void Execute(object? parameter)
    {
        _execute((T)parameter!);
    }
}
