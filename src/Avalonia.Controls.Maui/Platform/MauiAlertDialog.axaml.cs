using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Avalonia dialog control that presents a MAUI alert message with optional accept and cancel buttons.
/// </summary>
public partial class MauiAlertDialog : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MauiAlertDialog"/> class.
    /// </summary>
    public MauiAlertDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiAlertDialog"/> class with the specified title, message, and button texts.
    /// </summary>
    /// <param name="title">The title displayed at the top of the alert dialog.</param>
    /// <param name="message">The message body displayed in the alert dialog.</param>
    /// <param name="acceptText">The text for the accept button, or <see langword="null"/> to hide it.</param>
    /// <param name="cancelText">The text for the cancel button.</param>
    public MauiAlertDialog(string title, string message, string? acceptText, string cancelText)
        : this()
    {
        AlertTitle = title;
        AlertMessage = message;
        AcceptText = acceptText;
        CancelText = cancelText;
        HasAcceptButton = !string.IsNullOrEmpty(acceptText);

        DataContext = this;
    }

    /// <summary>
    /// Gets or sets the title text displayed at the top of the alert dialog.
    /// </summary>
    public string AlertTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message body text displayed in the alert dialog.
    /// </summary>
    public string AlertMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text displayed on the accept button.
    /// </summary>
    public string? AcceptText { get; set; }

    /// <summary>
    /// Gets or sets the text displayed on the cancel button.
    /// </summary>
    public string CancelText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the accept button is visible.
    /// </summary>
    public bool HasAcceptButton { get; set; }

    /// <summary>
    /// Occurs when the dialog is closed, providing <see langword="true"/> if accepted or <see langword="false"/> if cancelled.
    /// </summary>
    public event Action<bool?>? OnResult;

    private void OnAcceptClicked(object? sender, RoutedEventArgs e)
    {
        OnResult?.Invoke(true);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        OnResult?.Invoke(false);
    }
}
