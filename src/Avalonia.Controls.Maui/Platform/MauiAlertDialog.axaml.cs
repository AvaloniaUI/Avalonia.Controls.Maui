using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Avalonia.Controls.Maui.Platform;

public partial class MauiAlertDialog : Window
{
    public MauiAlertDialog()
    {
        InitializeComponent();
    }

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

    public string AlertTitle { get; set; } = string.Empty;
    public string AlertMessage { get; set; } = string.Empty;
    public string? AcceptText { get; set; }
    public string CancelText { get; set; } = string.Empty;
    public bool HasAcceptButton { get; set; }

    private void OnAcceptClicked(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
