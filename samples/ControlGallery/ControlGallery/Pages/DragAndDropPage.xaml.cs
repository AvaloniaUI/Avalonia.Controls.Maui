namespace ControlGallery.Pages;

public partial class DragAndDropPage : ContentPage
{
    public DragAndDropPage()
    {
        InitializeComponent();
    }

    // --- Text Drag/Drop ---

    private void OnDragStarting(object? sender, DragStartingEventArgs e)
    {
        e.Data.Text = "Hello from drag source!";
        DragDropStatus1.Text = "Status: Dragging...";
    }

    private void OnDropCompleted(object? sender, DropCompletedEventArgs e)
    {
        DragDropStatus1.Text = "Status: Drop completed";
    }

    private void OnDragOver1(object? sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void OnDrop1(object? sender, DropEventArgs e)
    {
        var text = await e.Data.GetTextAsync();
        DropLabel1.Text = text ?? "(empty)";
        DragDropStatus1.Text = $"Status: Dropped '{text}'";
    }

    // --- Multiple Sources ---

    private void OnMultiDragStarting(object? sender, DragStartingEventArgs e)
    {
        if (sender is Border border)
        {
            var label = border.Content as Label;
            e.Data.Text = label?.Text ?? "Unknown";
        }
    }

    private async void OnMultiDrop(object? sender, DropEventArgs e)
    {
        var text = await e.Data.GetTextAsync();
        MultiDropLabel.Text = $"Dropped: {text}";
    }

    // --- DragOver Feedback ---

    private void OnFeedbackDragStarting(object? sender, DragStartingEventArgs e)
    {
        e.Data.Text = "Feedback test";
    }

    private void OnFeedbackDragOver(object? sender, DragEventArgs e)
    {
        FeedbackDropTarget.BackgroundColor = Colors.LightGreen;
        FeedbackDropLabel.Text = "Release to drop!";
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void OnFeedbackDragLeave(object? sender, DragEventArgs e)
    {
        FeedbackDropTarget.BackgroundColor = Colors.LightGray;
        FeedbackDropLabel.Text = "Hover over me";
    }

    private async void OnFeedbackDrop(object? sender, DropEventArgs e)
    {
        FeedbackDropTarget.BackgroundColor = Colors.LightBlue;
        var text = await e.Data.GetTextAsync();
        FeedbackDropLabel.Text = $"Got: {text}";
    }

    // --- Cancel Drag ---

    private void OnCancelledDragStarting(object? sender, DragStartingEventArgs e)
    {
        e.Cancel = true;
        CancelDragStatus.Text = "Status: Drag was cancelled";
    }

    // --- CanDrag/AllowDrop Toggles ---

    private void OnToggleDragStarting(object? sender, DragStartingEventArgs e)
    {
        e.Data.Text = "Toggle drag data";
        ToggleStatus.Text = "Status: Dragging...";
    }

    private async void OnToggleDrop(object? sender, DropEventArgs e)
    {
        var text = await e.Data.GetTextAsync();
        ToggleDropLabel.Text = text ?? "Dropped!";
        ToggleStatus.Text = "Status: Drop received";
    }

    private void OnCanDragToggled(object? sender, ToggledEventArgs e)
    {
        ToggleDragRecognizer.CanDrag = e.Value;
        ToggleStatus.Text = $"Status: CanDrag = {e.Value}";
    }

    private void OnAllowDropToggled(object? sender, ToggledEventArgs e)
    {
        ToggleDropRecognizer.AllowDrop = e.Value;
        ToggleStatus.Text = $"Status: AllowDrop = {e.Value}";
    }

    // --- External File Drop ---

    private void OnFileDragOver(object? sender, DragEventArgs e)
    {
        FileDropTarget.BackgroundColor = Colors.LightGreen;
        FileDropLabel.Text = "Release to drop files!";
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void OnFileDragLeave(object? sender, DragEventArgs e)
    {
        FileDropTarget.BackgroundColor = Colors.LightGray;
        FileDropLabel.Text = "Drop files here";
    }

    private async void OnFileDrop(object? sender, DropEventArgs e)
    {
        FileDropTarget.BackgroundColor = Colors.LightBlue;

        var text = await e.Data.GetTextAsync();
        if (!string.IsNullOrEmpty(text))
        {
            var paths = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var fileNames = paths.Select(p => System.IO.Path.GetFileName(p)).ToArray();
            FileDropLabel.Text = string.Join(", ", fileNames);
            FileDropStatus.Text = $"Dropped {paths.Length} file(s)";
        }
        else
        {
            FileDropLabel.Text = "(no data)";
            FileDropStatus.Text = "Drop contained no text or file paths";
        }
    }
}
