using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace Avalonia.Controls.Maui.Essentials;

partial class AvaloniaEmail
{
    private static async Task PlatformComposeAsync(EmailMessage? message)
    {
        var uri = await GetUri(message);
        bool isMailTo = uri.StartsWith("mailto:");
        if (!isMailTo)
        {
            var path = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.eml");
            await File.WriteAllTextAsync(path, uri);
            uri = path;
        }

        try
        {
            ProcessStartInfo? info = null;

            if (OperatingSystem.IsWindows())
            {
                info = new ProcessStartInfo(uri) { UseShellExecute = true };
            }
            else if (OperatingSystem.IsLinux())
            {
                info = new ProcessStartInfo("xdg-open", $"\"{uri}\"") { UseShellExecute = false };
            }
            else if (OperatingSystem.IsMacOS())
            {
                info = new ProcessStartInfo("open", $"\"{uri}\"") { UseShellExecute = false };
            }

            if (info == null)
                return;

            using var process = Process.Start(info);
            if (process != null) await process.WaitForExitAsync();
        }
        finally
        {
            if (!isMailTo)
                File.Delete(uri);
        }
    }
}
