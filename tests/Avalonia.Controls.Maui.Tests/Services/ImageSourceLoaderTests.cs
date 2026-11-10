using Avalonia.Controls.Maui.Services;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using AvaloniaIImage = Avalonia.Media.IImage;
using AvaloniaImage = Avalonia.Controls.Image;
using ImageSourceServiceResult = Avalonia.Controls.Maui.Services.ImageSourceServiceResult;

namespace Avalonia.Controls.Maui.Tests.Services;

public class ImageSourceLoaderTests
{
    [AvaloniaFact(DisplayName = "A bitmap image loads again at the new scale")]
    public void BitmapImageLoadsAgainAtNewScale()
    {
        var service = new RecordingImageSourceService(resolutionDependent: true);
        var image = new AvaloniaImage();
        var window = Show(image);
        using var loader = new ImageSourceLoader(image, CreateContext(service), source => image.Source = source);

        loader.SetSource(new StreamImageSource());
        window.SetRenderScaling(2);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal([1f, 2f], service.Scales);
        Assert.Same(service.Results[1].Value, image.Source);
        Assert.True(service.Results[0].IsDisposed);
        Assert.Equal(2, loader.ResultScale);
        window.Close();
    }

    [AvaloniaFact(DisplayName = "A font image does not load again when the scale changes")]
    public async Task FontImageDoesNotLoadAgain()
    {
        var service = new RecordingImageSourceService(new AvaloniaFontImageSourceService());
        var image = new AvaloniaImage();
        var window = Show(image);
        using var loader = new ImageSourceLoader(image, CreateContext(service), source => image.Source = source);

        loader.SetSource(new FontImageSource { Glyph = "M", Size = 20 });
        await WaitForAsync(() => image.Source != null);
        window.SetRenderScaling(2);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal([1f], service.Scales);
        Assert.IsType<Avalonia.Media.DrawingImage>(image.Source);
        window.Close();
    }

    [AvaloniaFact(DisplayName = "The loader waits for its target to attach")]
    public void LoaderWaitsForAttach()
    {
        var service = new RecordingImageSourceService(resolutionDependent: true);
        var image = new AvaloniaImage();
        using var loader = new ImageSourceLoader(image, CreateContext(service), source => image.Source = source);

        loader.SetSource(new StreamImageSource());
        Assert.Empty(service.Scales);

        var window = Show(image, scale: 2);

        Assert.Equal([2f], service.Scales);
        Assert.NotNull(image.Source);
        window.Close();
    }

    [AvaloniaFact(DisplayName = "A slow load replaced by a newer source never applies, and its result is disposed")]
    public async Task StaleLoadIsDiscarded()
    {
        var service = new RecordingImageSourceService(resolutionDependent: true) { Pending = true };
        var image = new AvaloniaImage();
        var applied = new List<AvaloniaIImage?>();
        var window = Show(image);
        using var loader = new ImageSourceLoader(image, CreateContext(service), source =>
        {
            applied.Add(source);
            image.Source = source;
        });

        loader.SetSource(new StreamImageSource());
        loader.SetSource(new StreamImageSource());

        service.CompleteAll();
        await WaitForAsync(() => applied.Count == 1 && service.Results[0].IsDisposed);

        Assert.Equal(2, service.Results.Count);
        Assert.True(service.Results[0].IsDisposed);
        Assert.False(service.Results[1].IsDisposed);
        Assert.Equal([service.Results[1].Value], applied);
        window.Close();
    }

    [AvaloniaFact(DisplayName = "A detached target stops listening for scale changes")]
    public void DetachedTargetUnsubscribes()
    {
        var service = new RecordingImageSourceService(resolutionDependent: true);
        var image = new AvaloniaImage();
        var window = Show(image);
        using var loader = new ImageSourceLoader(image, CreateContext(service), source => image.Source = source);
        loader.SetSource(new StreamImageSource());
        Assert.True(loader.IsSubscribed);

        window.Content = null;
        window.SetRenderScaling(2);
        Dispatcher.UIThread.RunJobs();

        Assert.False(loader.IsSubscribed);
        Assert.Equal([1f], service.Scales);
        window.Close();
    }

    [AvaloniaFact(DisplayName = "A target moved to a window at another scale loads again")]
    public void TargetMovedToAnotherScaleLoadsAgain()
    {
        var service = new RecordingImageSourceService(resolutionDependent: true);
        var image = new AvaloniaImage();
        var first = Show(image);
        using var loader = new ImageSourceLoader(image, CreateContext(service), source => image.Source = source);
        loader.SetSource(new StreamImageSource());

        first.Content = null;
        var second = Show(image, scale: 2);

        Assert.Equal([1f, 2f], service.Scales);
        first.Close();
        second.Close();
    }

    [AvaloniaFact(DisplayName = "Disposing the loader disposes its result and clears the image")]
    public void DisposeReleasesResult()
    {
        var service = new RecordingImageSourceService(resolutionDependent: true);
        var image = new AvaloniaImage();
        var window = Show(image);
        var loader = new ImageSourceLoader(image, CreateContext(service), source => image.Source = source);
        loader.SetSource(new StreamImageSource());

        loader.Dispose();

        Assert.Null(image.Source);
        Assert.True(service.Results[0].IsDisposed);
        window.Close();
    }

    [AvaloniaFact(DisplayName = "A source with no Avalonia service gives no image")]
    public void SourceWithoutServiceGivesNoImage()
    {
        var image = new AvaloniaImage { Source = new WriteableBitmap(new PixelSize(1, 1), new Vector(96, 96)) };
        var window = Show(image);
        var applied = new List<AvaloniaIImage?>();
        using var loader = new ImageSourceLoader(image, CreateContext(service: null), applied.Add);

        loader.SetSource(new StreamImageSource());

        Assert.Equal([null], applied);
        window.Close();
    }

    static Window Show(Control content, double scale = 1)
    {
        var window = new Window { Content = content, Width = 100, Height = 100 };
        window.SetRenderScaling(scale);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    static async Task WaitForAsync(Func<bool> condition)
    {
        for (var i = 0; i < 50 && !condition(); i++)
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        Assert.True(condition());
    }

    static IMauiContext CreateContext(IImageSourceService? service)
    {
        var provider = Substitute.For<IImageSourceServiceProvider>();
        provider.GetImageSourceService(Arg.Any<Type>()).Returns(service);

        var services = new ServiceCollection()
            .AddSingleton(provider)
            .BuildServiceProvider();

        var context = Substitute.For<IMauiContext>();
        context.Services.Returns(services);
        return context;
    }

    sealed class RecordingImageSourceService : IAvaloniaImageSourceService
    {
        readonly IAvaloniaImageSourceService? _inner;
        readonly bool _resolutionDependent;
        readonly List<TaskCompletionSource<IImageSourceServiceResult<AvaloniaIImage>?>> _pending = new();

        public RecordingImageSourceService(bool resolutionDependent) => _resolutionDependent = resolutionDependent;

        public RecordingImageSourceService(IAvaloniaImageSourceService inner) => _inner = inner;

        public bool Pending { get; init; }

        public List<float> Scales { get; } = new();

        public List<ImageSourceServiceResult> Results { get; } = new();

        public Task<IImageSourceServiceResult<AvaloniaIImage>?> GetImageAsync(
            IImageSource imageSource,
            float scale = 1,
            CancellationToken cancellationToken = default)
        {
            Scales.Add(scale);

            if (_inner != null)
                return _inner.GetImageAsync(imageSource, scale, cancellationToken);

            var bitmap = new WriteableBitmap(
                new PixelSize((int)(4 * scale), (int)(4 * scale)),
                new Vector(96 * scale, 96 * scale));
            var result = new ImageSourceServiceResult(bitmap, _resolutionDependent);
            Results.Add(result);

            if (!Pending)
                return Task.FromResult<IImageSourceServiceResult<AvaloniaIImage>?>(result);

            // Like a download, the result arrives whether or not the caller has cancelled
            var completion = new TaskCompletionSource<IImageSourceServiceResult<AvaloniaIImage>?>();
            _pending.Add(completion);
            return completion.Task.ContinueWith(_ => (IImageSourceServiceResult<AvaloniaIImage>?)result, TaskScheduler.Default);
        }

        public void CompleteAll()
        {
            foreach (var completion in _pending)
                completion.TrySetResult(null);
        }
    }
}
