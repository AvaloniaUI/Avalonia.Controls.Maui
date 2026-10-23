using Avalonia.Controls.Maui.Services;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Avalonia.Controls.Maui.Tests.Services;

public class ImageSourceServiceResolutionTests
{
    public static IEnumerable<object[]> ImageSources()
    {
        yield return [new FileImageSource { File = "icon.png" }, typeof(AvaloniaFileImageSourceService)];
        yield return [new FontImageSource { Glyph = "A" }, typeof(AvaloniaFontImageSourceService)];
        yield return [new UriImageSource { Uri = new Uri("https://example.com/icon.png") }, typeof(AvaloniaUriImageSourceService)];
        yield return [new StreamImageSource { Stream = _ => Task.FromResult<Stream>(new MemoryStream()) }, typeof(AvaloniaStreamImageSourceService)];
    }

    [AvaloniaTheory(DisplayName = "Image sources resolve to the Avalonia image source services")]
    [MemberData(nameof(ImageSources))]
    public void ImageSourcesResolveToAvaloniaServices(ImageSource imageSource, Type expectedServiceType)
    {
        using var app = CreateApp();
        var provider = app.Services.GetRequiredService<IImageSourceServiceProvider>();

        var service = provider.ResolveImageSourceService(imageSource);

        Assert.IsType(expectedServiceType, service);
    }

    [AvaloniaFact(DisplayName = "Resolving a FileImageSource by concrete type returns the MAUI service")]
    public void ConcreteFileImageSourceTypeResolvesToMauiService()
    {
        // Documents why ResolveImageSourceService exists: by concrete type, the provider
        // returns MAUI's FileImageSourceService, which cannot load Avalonia bitmaps.
        using var app = CreateApp();
        var provider = app.Services.GetRequiredService<IImageSourceServiceProvider>();

        var service = provider.GetImageSourceService(typeof(FileImageSource));

        Assert.IsNotAssignableFrom<IAvaloniaImageSourceService>(service);
    }

    private static MauiApp CreateApp()
    {
        // Same registrations as a real app: UseMauiApp registers MAUI's services for the
        // concrete Controls image source types, then UseAvaloniaApp registers the Avalonia
        // services for the image source interfaces.
        var builder = MauiApp.CreateBuilder();
        builder.ConfigureImageSources(services =>
        {
            services.AddService<FileImageSource>(_ => new FileImageSourceService());
            services.AddService<FontImageSource>(sp => new FontImageSourceService(sp.GetRequiredService<IFontManager>()));
            services.AddService<StreamImageSource>(_ => new StreamImageSourceService());
            services.AddService<UriImageSource>(_ => new UriImageSourceService());
        });
        builder.UseAvaloniaApp();
        return builder.Build();
    }
}
