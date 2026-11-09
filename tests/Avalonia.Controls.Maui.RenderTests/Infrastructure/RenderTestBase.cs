using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia.Headless;
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

        // Use invariant culture for consistent rendering across locales.
        // Also set the process-wide defaults: since Avalonia 12.1, controls such as
        // DatePicker read CultureInfo.CurrentCulture from the dispatcher thread,
        // which is not necessarily the thread running this method.
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

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

    protected Task RenderToFile(View view, Action<IViewHandler>? setupPlatformView = null, [CallerMemberName] string testName = "") =>
        RenderToFile(view, 1, setupPlatformView, testName);

    protected async Task RenderToFile(View view, double scale, Action<IViewHandler>? setupPlatformView = null, [CallerMemberName] string testName = "")
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

            await RenderControlToFile(platformView, width, height, scale, testName);
        });
    }

    /// <summary>
    /// Shows <paramref name="content"/> in a window at <paramref name="scale"/>, and saves the window
    /// rendered at that scale.
    /// </summary>
    protected async Task RenderControlToFile(Control content, double width, double height, double scale, string testName)
    {
        content.Margin = new Avalonia.Thickness(20);

        var container = new Panel
        {
            Children = { content },
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
            SystemDecorations = WindowDecorations.None,
            Background = Brushes.White,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.Manual
        };

        window.SetRenderScaling(scale);
        window.Show();

        // Give layout system time to run
        for (int i = 0; i < 5; i++)
        {
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Normal);
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);
            await Task.Delay(20);
        }

        // Render
        var pixelSize = new PixelSize((int)Math.Round((width + 40) * scale), (int)Math.Round((height + 40) * scale));
        var dpi = new Vector(96 * scale, 96 * scale);
        using var bitmap = new RenderTargetBitmap(pixelSize, dpi);
        bitmap.Render(window);

        bitmap.Save(GetOutputPath(testName));

        window.Close();
    }

    protected string GetOutputPath(string testName) =>
        Path.Combine(Directory.GetCurrentDirectory(), $"{GetType().Name}_{testName}.out.png");

    protected void CompareImages(double? tolerance = null, [CallerMemberName] string testName = "")
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

        TestRenderHelper.AssertCompareImages(actualPath, expectedPath, tolerance);
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

