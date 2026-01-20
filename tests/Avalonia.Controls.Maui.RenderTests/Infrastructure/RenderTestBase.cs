using System.Runtime.CompilerServices;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Controls.Maui.RenderTests.Infrastructure;

public class RenderTestBase : IAsyncDisposable
{
    private MauiApp? _mauiApp;
    private IServiceProvider? _servicesProvider;
    private IMauiContext? _mauiContext;
    private bool _isCreated;

    protected void EnsureHandlerCreated()
    {
        if (_isCreated) return;
        _isCreated = true;

        var appBuilder = MauiApp.CreateBuilder();
        appBuilder.ConfigureTestBuilder();
        
        _mauiApp = appBuilder.Build();
        _servicesProvider = _mauiApp.Services;
        _mauiContext = new ContextStub(_servicesProvider);
    }

    protected IMauiContext MauiContext
    {
        get
        {
            EnsureHandlerCreated();
            return _mauiContext!;
        }
    }

    protected async Task RenderToFile(View view, Action<IViewHandler>? setupPlatformView = null, [CallerMemberName] string testName = "")
    {
        EnsureHandlerCreated();
        
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var mauiContext = MauiContext;
            var handler = view.ToHandler(mauiContext);
            var platformView = (Control)((IViewHandler)handler).ContainerView! ?? (Control)handler.PlatformView!;

            setupPlatformView?.Invoke((IViewHandler)handler);

            // Measure and Arrange
            var width = view.WidthRequest > 0 ? view.WidthRequest : 250;
            var height = view.HeightRequest > 0 ? view.HeightRequest : 120;

            view.Measure(width, height);
            view.Arrange(new Microsoft.Maui.Graphics.Rect(0, 0, width, height));

            // Wrap in Window for Rendering
            platformView.Margin = new Avalonia.Thickness(20);
            
            var container = new Panel
            {
                Children = { platformView },
                Background = Brushes.White,
                Width = width + 40,
                Height = height + 40
            };

            var window = new Window
            {
                Content = container,
                Width = width + 40,
                Height = height + 40,
                Title = testName,
                SystemDecorations = SystemDecorations.None,
                Background = Brushes.White,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.Manual
            };
            
            window.Show();

            // Give layout system time to run
            await Task.Delay(100);

            // Render
            var pixelSize = new PixelSize((int)width + 40, (int)height + 40);
            var dpi = new Vector(96, 96);
            using var bitmap = new RenderTargetBitmap(pixelSize, dpi);
            bitmap.Render(window);
            
            var fileName = $"{GetType().Name}_{testName}.out.png";
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            
            bitmap.Save(outputPath);
            
            window.Close();
        });
    }



    protected void CompareImages([CallerMemberName] string testName = "")
    {
        var className = GetType().Name;
        var actualParams = $"{className}_{testName}.out.png";
        var expectedParams = $"{className}_{testName}.expected.png";
        
        var actualPath = Path.Combine(Directory.GetCurrentDirectory(), actualParams);
        var expectedPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", expectedParams);

        if (!File.Exists(actualPath))
        {
            Assert.Fail($"Actual image not found: {actualPath}");
        }
        
        if (!File.Exists(expectedPath))
        {
            // For now, if expected image doesn't exist, we just pass (or fail if strict). 
            // In first run, we usually want to see the output.
            // Failing so we are forced to generate it.
            Assert.Fail($"Expected image not found: {expectedPath}. Actual image saved to: {actualPath}");
        }

        TestRenderHelper.AssertCompareImages(actualPath, expectedPath);
    }

    public virtual ValueTask DisposeAsync()
    {
        _mauiApp?.Dispose();
        return ValueTask.CompletedTask;
    }

    private class ContextStub : IMauiContext
    {
        public ContextStub(IServiceProvider services) => Services = services;
        public IServiceProvider Services { get; }
        public IMauiHandlersFactory Handlers => Services.GetRequiredService<IMauiHandlersFactory>();
    }
}

