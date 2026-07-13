using Avalonia.Controls.Maui.Essentials;
using Microsoft.Maui.ApplicationModel.Communication;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaEmailTests
{
    // 1. Null message → returns "mailto:"
    [Fact]
    public async Task GetUri_NullMessage_ReturnsMailtoOnly()
    {
        // Arrange
        EmailMessage? message = null;

        // Act
        string uri = await AvaloniaEmail.GetUri(message);

        // Assert
        Assert.Equal("mailto:", uri);
    }

    // 2. Plain text, no attachments → ConvertToMailTo()
    [Fact]
    public async Task GetUri_PlainTextNoAttachments_ReturnsMailToUri()
    {
        // Arrange
        var message = new EmailMessage
        {
            To = new List<string> { "test@example.com" },
            Subject = "Hello",
            Body = "World",
            BodyFormat = EmailBodyFormat.PlainText,
            Attachments = null // no attachments
        };

        // Act
        string uri = await AvaloniaEmail.GetUri(message);

        // Assert
        // The exact format depends on the extension, but we expect it to start with "mailto:"
        Assert.StartsWith("mailto:", uri);
        // Optionally check that it contains the subject/body encoded
        Assert.Contains("subject=Hello", uri);
        Assert.Contains("body=World", uri);
    }

    [Fact]
    public async Task GetUri_HtmlBody_ReturnsEmlUri()
    {
        // Arrange
        var message = new EmailMessage
        {
            To = new List<string> { "test@example.com" },
            Subject = "HTML email",
            Body = "<html><body><h1>Hi</h1></body></html>",
            BodyFormat = EmailBodyFormat.Html,
            Attachments = null
        };

        // Act
        string uri = await AvaloniaEmail.GetUri(message);

        // Assert
        // Should not be mailto: because BodyFormat is not PlainText
        Assert.False(uri.StartsWith("mailto:"));
        Assert.NotNull(uri);
        Assert.NotEmpty(uri);
    }
}
