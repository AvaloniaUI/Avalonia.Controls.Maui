using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Threading;

namespace Avalonia.Controls.Maui.Platform;

public partial class MauiPromptDialog : UserControl
{
    public MauiPromptDialog()
    {
        InitializeComponent();
    }

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
    
    public string? Title { get; set; }

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

    public string? Message { get; set; }
    public bool HasMessage => !string.IsNullOrEmpty(Message);

    public string? AcceptText { get; set; }
    public string? CancelText { get; set; }

    public string? Placeholder { get; set; }

    // Bindable property for InputValue
    public static readonly StyledProperty<string> InputValueProperty =
        AvaloniaProperty.Register<MauiPromptDialog, string>(nameof(InputValue));

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

    public int MaxLength { get; set; }
    
    public event Action<string?>? OnResult;

    private void Close(string? result)
    {
        OnResult?.Invoke(result);
    }
}
