#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace MyConference.Services;

[SupportedOSPlatform("browser")]
public static partial class ImageInterop
{
    [JSImport("globalThis.fetchImageAsBase64")]
    public static partial Task<string> FetchImageAsBase64(string url);
}
#endif
