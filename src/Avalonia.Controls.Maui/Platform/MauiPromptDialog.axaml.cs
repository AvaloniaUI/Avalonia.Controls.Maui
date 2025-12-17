using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Platform;

public partial class MauiPromptDialog : Window
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

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        var tb = this.FindControl<TextBox>("InputTextBox");
        tb?.Focus();
        if (!string.IsNullOrEmpty(InputValue))
            tb?.SelectAll();
    }

    public string? Message { get; set; }
    public bool HasMessage => !string.IsNullOrEmpty(Message);

    public string? AcceptText { get; set; }
    public ICommand AcceptCommand => new RelayCommand<object>(_ => Close(InputValue));

    public string? CancelText { get; set; }
    public ICommand CancelCommand => new RelayCommand<object>(_ => Close(null));

    public string? Placeholder { get; set; }

    // Bindable property for InputValue
    public static readonly StyledProperty<string> InputValueProperty =
        AvaloniaProperty.Register<MauiPromptDialog, string>(nameof(InputValue));

    public string InputValue
    {
        get => GetValue(InputValueProperty);
        set => SetValue(InputValueProperty, value);
    }

    public int MaxLength { get; set; }
}
