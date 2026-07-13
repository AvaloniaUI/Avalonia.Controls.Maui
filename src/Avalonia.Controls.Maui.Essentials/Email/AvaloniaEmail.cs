using Microsoft.Maui.ApplicationModel.Communication;

namespace Avalonia.Controls.Maui.Essentials;

partial class AvaloniaEmail : IEmail
{
    public bool IsComposeSupported => true;

    public Task ComposeAsync(EmailMessage? message) =>
        PlatformComposeAsync(message);

    internal static async Task<string> GetUri(EmailMessage? message)
    {
        if (message == null)
        {
            return "mailto:";
        }
        else if (message.BodyFormat == EmailBodyFormat.PlainText &&
            (message.Attachments == null || message.Attachments.Count == 0))
        {
            return await message.ConvertToMailTo();
        }
        else
        {
            return await message.ConvertToEml();
        }
    }
}
