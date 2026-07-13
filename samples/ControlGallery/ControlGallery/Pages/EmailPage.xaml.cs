using Avalonia.Controls.Maui.Essentials;
using System.Collections.ObjectModel;

namespace ControlGallery.Pages;

public partial class EmailPage : ContentPage
{
    // Observable collection to hold selected attachments
    private ObservableCollection<FileResult> _attachments = new();

    public EmailPage()
	{
		InitializeComponent();
        AttachmentsCollection.ItemsSource = _attachments;
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        // Collect data
        string? to = ToEntry.Text?.Trim();
        string subject = SubjectEntry.Text?.Trim() ?? string.Empty;
        string body = BodyEditor.Text ?? string.Empty;
        bool isHtml = HtmlCheckBox.IsChecked;

        // Basic validation
        if (string.IsNullOrEmpty(to))
        {
            await DisplayAlert("Missing recipient", "Please enter an email address.", "OK");
            return;
        }

        try
        {
            var message = new EmailMessage
            {
                Subject = subject,
                Body = body,
                To = new List<string> { to },
                BodyFormat = isHtml ? EmailBodyFormat.Html : EmailBodyFormat.PlainText                
            };

            await Email.Default.ComposeAsync(_attachments, message);
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Not supported", "Email is not supported on this device.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not send email: {ex.Message}", "OK");
        }
    }

    // --- Add Attachments ---
    private async void OnAddAttachmentsClicked(object sender, EventArgs e)
    {
        try
        {
            // Pick multiple files
            var options = new PickOptions
            {
                PickerTitle = "Select files to attach",
                // Optional: restrict file types
                // FileTypes = new FilePickerFileType(...)
            };

            var result = await FilePicker.PickMultipleAsync(options);

            if (result != null && result.Any())
            {
                foreach (var file in result)
                {
                    _attachments.Add(file);
                }

                UpdateAttachmentsUI();
            }
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permission Denied", "Unable to access files. Please grant storage permission.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not pick files: {ex.Message}", "OK");
        }
    }

    // --- Clear Attachments ---
    private void OnClearAttachmentsClicked(object sender, EventArgs e)
    {
        _attachments.Clear();
        UpdateAttachmentsUI();
    }

    private void UpdateAttachmentsUI()
    {
        bool hasAttachments = _attachments.Any();
        AttachmentsCollection.IsVisible = hasAttachments;
        ClearAttachmentsButton.IsVisible = hasAttachments;
    }
}