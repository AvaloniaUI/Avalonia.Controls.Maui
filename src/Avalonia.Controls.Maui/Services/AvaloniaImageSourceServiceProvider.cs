using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Services;

public class AvaloniaImageSourceServiceProvider : IImageSourceServiceProvider
{
    private readonly IServiceProvider _serviceProvider;

    public AvaloniaImageSourceServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IServiceProvider HostServiceProvider => _serviceProvider;

    public IImageSourceService? GetImageSourceService(Type imageSource)
    {
        if (imageSource == null)
            return null;

        // Determine the appropriate service type based on the image source type
        Type? serviceType = null;

        if (typeof(IUriImageSource).IsAssignableFrom(imageSource))
        {
            serviceType = typeof(IImageSourceService<IUriImageSource>);
        }
        else if (typeof(IFileImageSource).IsAssignableFrom(imageSource))
        {
            serviceType = typeof(IImageSourceService<IFileImageSource>);
        }
        else if (typeof(IStreamImageSource).IsAssignableFrom(imageSource))
        {
            serviceType = typeof(IImageSourceService<IStreamImageSource>);
        }
        else if (typeof(IFontImageSource).IsAssignableFrom(imageSource))
        {
            serviceType = typeof(IImageSourceService<IFontImageSource>);
        }

        if (serviceType != null)
        {
            return _serviceProvider.GetRequiredService(serviceType) as IImageSourceService;
        }

        return null;
    }

    public object? GetService(Type serviceType)
    {
        return _serviceProvider.GetService(serviceType);
    }
}