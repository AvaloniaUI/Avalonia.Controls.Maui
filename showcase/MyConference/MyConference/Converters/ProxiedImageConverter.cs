using System.Globalization;

namespace MyConference.Converters;

public class ProxiedImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string url || string.IsNullOrEmpty(url))
            return null;

#if BROWSER
        if (url.Contains("sessionize.com"))
        {
            return new StreamImageSource
            {
                Stream = async _ =>
                {
                    var base64 = await Services.ImageInterop.FetchImageAsBase64(url);
                    var bytes = System.Convert.FromBase64String(base64);
                    return new MemoryStream(bytes);
                }
            };
        }
#endif

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
