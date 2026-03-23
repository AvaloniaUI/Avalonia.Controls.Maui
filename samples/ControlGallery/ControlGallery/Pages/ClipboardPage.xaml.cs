namespace ControlGallery.Pages;

public partial class ClipboardPage : ContentPage
{
    public ClipboardPage()
    {
        InitializeComponent();
        Clipboard.Default.ClipboardContentChanged += OnClipboardContentChanged;
    }

    async void OnCopyClicked(object? sender, EventArgs e)
    {
        var text = CopyEntry.Text;
        if (string.IsNullOrEmpty(text))
            return;

        await Clipboard.Default.SetTextAsync(text);
    }

    async void OnPasteClicked(object? sender, EventArgs e)
    {
        var text = await Clipboard.Default.GetTextAsync();
        PasteResultLabel.Text = $"Pasted: {text ?? "(empty)"}";
    }

    void OnCheckHasTextClicked(object? sender, EventArgs e)
    {
        HasTextLabel.Text = $"HasText: {Clipboard.Default.HasText}";
    }

    void OnClipboardContentChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            EventLabel.Text = $"Last event: {DateTime.Now:T}";
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Clipboard.Default.ClipboardContentChanged -= OnClipboardContentChanged;
    }
}
