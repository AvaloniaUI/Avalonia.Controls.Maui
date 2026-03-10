using Avalonia.Controls.Maui.SkiaSharp.Views.Handlers;
using Avalonia.Controls.Maui.SkiaSharp.Views.Services;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

/// <summary>
/// Extension methods for configuring SkiaSharp views with Avalonia handlers in a MAUI application.
/// </summary>
public static class AppHostBuilderExtensions
{
    /// <summary>
    /// Registers Avalonia-backed SkiaSharp view handlers and image source services.
    /// </summary>
    /// <param name="builder">The MAUI app builder to configure.</param>
    /// <param name="forceSoftwareRendering">
    /// When <see langword="true"/>, GL views will use CPU raster rendering instead of GPU.
    /// This is useful for platforms like Browser/WASM where GPU rendering may not be reliable.
    /// </param>
    /// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
    public static MauiAppBuilder UseAvaloniaSkiaSharp(this MauiAppBuilder builder, bool forceSoftwareRendering = false) =>
        builder
            .ConfigureMauiHandlers(handlers =>
            {
                SKGLViewHandler.ForceSoftwareRendering = forceSoftwareRendering;
                handlers.AddHandler<SKCanvasView, SKCanvasViewHandler>();
                handlers.AddHandler<SKGLView, SKGLViewHandler>();
            })
            .ConfigureImageSources(sources =>
            {
                sources.AddService<ISKImageImageSource, AvaloniaSKImageSourceService>();
                sources.AddService<ISKBitmapImageSource, AvaloniaSKImageSourceService>();
                sources.AddService<ISKPixmapImageSource, AvaloniaSKImageSourceService>();
                sources.AddService<ISKPictureImageSource, AvaloniaSKImageSourceService>();
            });
}
