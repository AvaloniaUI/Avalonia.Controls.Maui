using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Storage;
using System.Security.Cryptography;
using System.Text;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Provides extension methods for sending emails with attachments and converting emails to different formats.
/// </summary>
public static class AvaloniaEmailExtensions
{
    /// <summary>
    /// Composes an email with attachments asynchronously, handling different attachment types.
    /// </summary>
    /// <param name="email">The email service implementation.</param>
    /// <param name="files">The collection of files to attach.</param>
    /// <param name="message">The email message to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// <b>Note:</b> For <see cref="EmailBodyFormat.Html"/> The email client must be associated with the MIME type <c>message/rfc822</c> for .eml files to open correctly.
    /// On Linux, this can be configured with: <c>xdg-mime default thunderbird.desktop message/rfc822</c>
    /// On Mac, command+shift+d to enter compose mode
    /// </remarks>
    public static Task ComposeAsync(this IEmail email, IEnumerable<FileBase> files, EmailMessage message)
    {
        var attachments = new List<EmailAttachment>();
        foreach (var file in files ?? [])
        {
            if (file is AvaloniaFileResult result)
                attachments.Add(new AvaloniaEmailAttachment(result));
            else
                attachments.Add(new EmailAttachment(file.FullPath));
        }
        message.Attachments = attachments;
        return email.ComposeAsync(message);
    }

    /// <summary>
    /// Converts an <see cref="EmailMessage"/> to a <c>mailto:</c> URI string.
    /// </summary>
    /// <param name="message">The email message to convert.</param>
    /// <returns>A properly formatted <c>mailto:</c> URI.</returns>
    internal static Task<string> ConvertToMailTo(this EmailMessage message)
    {
        var query = new List<string>();

        if (!string.IsNullOrEmpty(message.Subject))
            query.Add("subject=" + Uri.EscapeDataString(message.Subject));

        if (!string.IsNullOrEmpty(message.Body))
            query.Add("body=" + Uri.EscapeDataString(message.Body));

        if (message.Cc?.Any() == true)
            query.Add("cc=" + Uri.EscapeDataString(string.Join(",", message.Cc)));

        if (message.Bcc?.Any() == true)
            query.Add("bcc=" + Uri.EscapeDataString(string.Join(",", message.Bcc)));

        var recipients = string.Join(",", message.To?.Select(Uri.EscapeDataString) ?? []);

        var uri = $"mailto:{recipients}";
        if (query.Count > 0)
            uri += "?" + string.Join("&", query);

        return Task.FromResult(uri);
    }

    /// <summary>
    /// Converts an <see cref="EmailMessage"/> to a complete EML (RFC 822) file content as a string.
    /// </summary>
    /// <param name="message">The email message to convert.</param>
    /// <returns>A string containing the full EML file content, including MIME headers, body, and attachments.</returns>
    /// <remarks>
    /// <para>
    /// This method generates a standard .eml file that can be:
    /// <list type="bullet">
    /// <item><description>Saved to disk and opened in email clients (Outlook, Thunderbird, Apple Mail)</description></item>
    /// <item><description>Sent as an attachment</description></item>
    /// <item><description>Opened via <c>xdg-open</c> (Linux), <c>open</c> (macOS), or default association (Windows)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The generated EML complies with RFC 822 and MIME standards. It includes:
    /// <list type="bullet">
    /// <item><description>Required headers (Date, MIME-Version, Subject, recipients)</description></item>
    /// <item><description>X-Unsent header (marks email as draft)</description></item>
    /// <item><description>Multipart structure for HTML + plain text (when applicable)</description></item>
    /// <item><description>Base64-encoded attachments</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Note:</b> Attachments are read asynchronously; the method calls <see cref="FileBase.OpenReadAsync"/> for each attachment.
    /// For large attachments, consider streaming to disk rather than building in-memory.
    /// </para>
    /// </remarks>
    internal static async Task<string> ConvertToEml(this EmailMessage message)
    {
        var builder = new StringBuilder();
        var boundary = $"----=_NextPart_{Guid.NewGuid():N}";
        var altBoundary = $"{boundary}_alt";

        // === HEADERS SECTION ===        
        if (message.To?.Count > 0)
            builder.AppendLine($"To: {string.Join(",", message.To)}");

        if (message.Cc?.Count > 0)
            builder.AppendLine($"Cc: {string.Join(",", message.Cc)}");

        if (message.Bcc?.Count > 0)
            builder.AppendLine($"Bcc: {string.Join(",", message.Bcc)}");

        if (!string.IsNullOrWhiteSpace(message.Subject))
            builder.AppendLine($"Subject: {EncodeSubject(message.Subject)}");

        builder.AppendLine($"Date: {DateTime.Now.ToString("R")}");
        builder.AppendLine($"MIME-Version: 1.0");
        builder.AppendLine($"X-Unsent: 1");

        // Set Content-Type based on message structure
        if (message.BodyFormat == EmailBodyFormat.Html || message.Attachments?.Count > 0)
        {
            builder.AppendLine($"Content-Type: multipart/mixed; boundary=\"{boundary}\"");
        }
        else
        {
            builder.AppendLine($"Content-Type: text/plain; charset=utf-8");
        }

        // IMPORTANT: Empty line separating headers from body
        builder.AppendLine();

        // === BODY SECTION (starts after the empty line) ===
        if (message.BodyFormat == EmailBodyFormat.Html || message.Attachments?.Count > 0)
        {
            // Start multipart message
            builder.AppendLine($"This is a multipart message in MIME format.");
            builder.AppendLine($"--{boundary}");

            // Body part with alternative views
            builder.AppendLine($"Content-Type: multipart/alternative; boundary=\"{altBoundary}\"");
            builder.AppendLine();

            // Plain text version (if not HTML or if explicitly provided)
            if (!string.IsNullOrEmpty(message.Body))
            {
                var encoding = message.BodyFormat == EmailBodyFormat.PlainText ?
                    "text/plain" : "text/html";

                builder.AppendLine($"--{altBoundary}");
                builder.AppendLine($"Content-Type: {encoding}; charset=utf-8");
                builder.AppendLine($"Content-Transfer-Encoding: quoted-printable");
                builder.AppendLine();
                builder.AppendLine(message.Body);
                builder.AppendLine();
            }
            builder.AppendLine($"--{altBoundary}--");
            builder.AppendLine();

            // Attachments
            foreach (var attachment in message.Attachments ?? [])
            {
                if (attachment is AvaloniaEmailAttachment)
                {
                    using var stream = await attachment.OpenReadAsync();
                    await AppendAttachement(attachment.ContentType, stream);
                }
                else if (!OperatingSystem.IsBrowser())
                {
                    using var stream = new FileStream(attachment.FullPath, FileMode.Open, FileAccess.Read);
                    await AppendAttachement(AvaloniaFileResult.ResolveContentType(attachment.FileName), stream);
                }
                else
                {
                    throw new InvalidOperationException($"Only {nameof(ComposeAsync)} is supported");
                }

                async Task AppendAttachement(string contentType, Stream stream)
                {
                    builder.AppendLine($"--{boundary}");
                    builder.AppendLine($"Content-Type: {contentType}; name=\"{attachment.FileName}\"");
                    builder.AppendLine($"Content-Transfer-Encoding: base64");
                    builder.AppendLine($"Content-Disposition: attachment; filename=\"{attachment.FileName}\"");
                    builder.AppendLine();

                    //using var memoryStream = new MemoryStream();
                    //await stream.CopyToAsync(memoryStream);
                    //builder.AppendLine(Convert.ToBase64String(memoryStream.ToArray()));
                    //builder.AppendLine();

                    using var base64Stream = new CryptoStream(stream, new ToBase64Transform(), CryptoStreamMode.Read);

                    var buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = await base64Stream.ReadAsync(buffer)) > 0)
                    {
                        builder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                        if (builder.Length % 76 == 0) // Proper base64 line wrapping
                            builder.AppendLine();
                    }
                    builder.AppendLine();
                }
            }

            // End of multipart message
            builder.AppendLine($"--{boundary}--");
        }
        else if (!string.IsNullOrEmpty(message.Body))
        {
            // Simple text message
            builder.AppendLine($"Content-Transfer-Encoding: quoted-printable");
            builder.AppendLine();
            builder.AppendLine(message.Body);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Encodes a subject line for inclusion in an email header using RFC 2047 quoted-printable or Base64 encoding.
    /// </summary>
    /// <param name="subject">The subject text to encode.</param>
    /// <returns>
    /// The original subject if it contains only ASCII characters (0-127);
    /// otherwise, a UTF-8 Base64 encoded string in the format <c>=?utf-8?B?...?=</c>.
    /// </returns>
    /// <remarks>
    /// This method ensures that non-ASCII characters (e.g., accented letters, Chinese, emojis) are properly encoded
    /// according to email standards, preventing garbled text in older email clients.
    /// </remarks>
    private static string EncodeSubject(string subject)
    {
        if (subject.Any(c => c > 127))
        {
            var bytes = Encoding.UTF8.GetBytes(subject);
            var base64 = Convert.ToBase64String(bytes);
            return $"=?utf-8?B?{base64}?=";
        }
        return subject;
    }
}