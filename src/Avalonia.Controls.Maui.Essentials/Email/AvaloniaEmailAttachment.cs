using Avalonia.Platform.Storage;
using Microsoft.Maui.ApplicationModel.Communication;

namespace Avalonia.Controls.Maui.Essentials;

class AvaloniaEmailAttachment : EmailAttachment
{
    private IStorageFile storageFile;
    public AvaloniaEmailAttachment(AvaloniaFileResult file)
        : base(file.FullPath, file.ContentType)
    {
        FileName = file.FileName;
        storageFile = file.StorageFile;
    }

    public override Task<Stream> PlatformOpenReadAsync()
    {
        return storageFile.OpenReadAsync();
    }
}
