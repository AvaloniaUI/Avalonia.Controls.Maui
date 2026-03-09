using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Threading;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Avalonia dialog control that presents a MAUI prompt for user text input with configurable title, message, and validation options.
/// </summary>
public partial class MauiPromptDialog : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MauiPromptDialog"/> class.
    /// </summary>
    public MauiPromptDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiPromptDialog"/> class with the specified prompt configuration.
    /// </summary>
    /// <param name="title">The title displayed at the top of the prompt dialog.</param>
    /// <param name="message">The message displayed below the title.</param>
    /// <param name="accept">The text for the accept button, defaulting to "OK" if empty.</param>
    /// <param name="cancel">The text for the cancel button, defaulting to "Cancel" if empty.</param>
    /// <param name="placeholder">The placeholder text displayed in the input field when empty.</param>
    /// <param name="maxLength">The maximum number of characters allowed, or -1 for unlimited.</param>
    /// <param name="keyboard">The <see cref="Keyboard"/> type to use for the input field.</param>
    /// <param name="initialValue">The initial text value pre-populated in the input field.</param>
    public MauiPromptDialog(string title, string message, string accept, string cancel, string? placeholder = null, int maxLength = -1, Keyboard? keyboard = null, string initialValue = "") : this()
    {
        Title = title;
        Message = message;
        AcceptText = string.IsNullOrEmpty(accept) ? "OK" : accept;
        CancelText = string.IsNullOrEmpty(cancel) ? "Cancel" : cancel;
        Placeholder = placeholder;
        InputValue = initialValue;
        
        // MAUI uses -1 for unlimited, Avalonia uses 0
        MaxLength = maxLength > -1 ? maxLength : 0;

        DataContext = null;
        DataContext = this;
    }
    
    /// <summary>
    /// Gets or sets the title displayed at the top of the prompt dialog.
    /// </summary>
    public string? Title { get; set; }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var tb = this.FindControl<TextBox>("InputTextBox");
        Dispatcher.UIThread.Post(() => 
        {
            tb?.Focus();
            if (!string.IsNullOrEmpty(InputValue))
                tb?.SelectAll();
        });
    }

    /// <summary>
    /// Gets or sets the descriptive message displayed in the prompt dialog.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets a value indicating whether the dialog has a non-empty message to display.
    /// </summary>
    public bool HasMessage => !string.IsNullOrEmpty(Message);

    /// <summary>
    /// Gets or sets the text displayed on the accept button.
    /// </summary>
    public string? AcceptText { get; set; }

    /// <summary>
    /// Gets or sets the text displayed on the cancel button.
    /// </summary>
    public string? CancelText { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text shown in the input field when it is empty.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Defines the <see cref="InputValue"/> styled property.
    /// </summary>
    public static readonly StyledProperty<string> InputValueProperty =
        AvaloniaProperty.Register<MauiPromptDialog, string>(nameof(InputValue));

    /// <summary>
    /// Gets or sets the current text value entered by the user in the input field.
    /// </summary>
    public string InputValue
    {
        get => GetValue(InputValueProperty);
        set => SetValue(InputValueProperty, value);
    }
    
    private void OnAcceptClicked(object? sender, RoutedEventArgs e)
    {
        Close(InputValue);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    /// <summary>
    /// Gets or sets the maximum number of characters allowed in the input field, where 0 indicates no limit.
    /// </summary>
    public int MaxLength { get; set; }

    /// <summary>
    /// Occurs when the dialog is closed, providing the entered text or <see langword="null"/> if cancelled.
    /// </summary>
    public event Action<string?>? OnResult;

    private void Close(string? result)
    {
        OnResult?.Invoke(result);
    }
}
