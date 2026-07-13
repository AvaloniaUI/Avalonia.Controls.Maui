using Microsoft.Maui.ApplicationModel.Communication;
using System.Runtime.InteropServices.JavaScript;

namespace Avalonia.Controls.Maui.Essentials;

partial class AvaloniaEmail
{
    public AvaloniaEmail()
    {
        _ = JSHost.ImportAsync("email", "/email.browser.js");
    }
    
    [JSImport("emailInterop.openEml", "email")]
    internal static partial void OpenEml(string uri);

    [JSImport("globalThis.eval")]
    internal static partial void Open(string uri);

    private async Task PlatformComposeAsync(EmailMessage? message)
    {
        var uri = await GetUri(message);

        if (uri.StartsWith("mailto:"))
        {
            Open($"window.location.href='{uri}'");
        }
        else
        {
            OpenEml(uri);
        }
    }
}
