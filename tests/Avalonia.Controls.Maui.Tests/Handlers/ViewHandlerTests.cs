using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using MauiColors = Microsoft.Maui.Graphics.Colors;
using MauiContentView = Microsoft.Maui.Controls.ContentView;
using MauiEllipseGeometry = Microsoft.Maui.Controls.Shapes.EllipseGeometry;
using MauiVirtualEntry = Microsoft.Maui.Controls.Entry;
using MauiPoint = Microsoft.Maui.Graphics.Point;
using MauiSolidPaint = Microsoft.Maui.Graphics.SolidPaint;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ViewHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "UpdateClip applies geometry to control")]
    public void UpdateClipAppliesGeometry()
    {
        var view = new StubBase
        {
            Clip = new MauiEllipseGeometry
            {
                Center = new MauiPoint(50, 40),
                RadiusX = 50,
                RadiusY = 40
            },
            Width = 100,
            Height = 80
        };

        var control = new Control();
        control.UpdateClip(view);

        var clip = Assert.IsType<EllipseGeometry>(control.Clip);
        Assert.Equal(new Rect(0, 0, 100, 80), clip.Rect);
    }

    [AvaloniaFact(DisplayName = "UpdateShadow applies DropShadowEffect")]
    public void UpdateShadowAppliesDropShadowEffect()
    {
        var view = new StubBase
        {
            Shadow = new ShadowStub
            {
                Paint = new MauiSolidPaint(MauiColors.Red),
                Offset = new MauiPoint(6, 8),
                Opacity = 0.5f,
                Radius = 12f
            }
        };

        var control = new global::Avalonia.Controls.Control();

        control.UpdateShadow(view);

        var effect = Assert.IsType<DropShadowEffect>(control.Effect);

        Assert.Equal((byte)(255 * 0.5f), effect.Color.A);
        Assert.Equal((byte)(MauiColors.Red.Red * 255), effect.Color.R);
        Assert.Equal((byte)(MauiColors.Red.Green * 255), effect.Color.G);
        Assert.Equal((byte)(MauiColors.Red.Blue * 255), effect.Color.B);
        Assert.Equal(6, effect.OffsetX, 2);
        Assert.Equal(8, effect.OffsetY, 2);
        Assert.Equal(12, effect.BlurRadius, 2);
    }

    [AvaloniaFact(DisplayName = "UpdateShadow clears effect when null")]
    public void UpdateShadowClearsWhenNull()
    {
        var view = new StubBase
        {
            Shadow = new ShadowStub
            {
                Paint = new MauiSolidPaint(MauiColors.Red),
                Offset = new MauiPoint(2, 2),
                Opacity = 1f,
                Radius = 6f
            }
        };

        var control = new Control();
        control.UpdateShadow(view);

        view.Shadow = null;
        control.UpdateShadow(view);

        Assert.Null(control.Effect);
    }

    [AvaloniaFact(DisplayName = "Loaded and Unloaded fire on attach/detach")]
    public async Task LoadedAndUnloadedFireOnAttachDetach()
    {
        var view = new MauiContentView();
        var loadedCount = 0;
        var unloadedCount = 0;

        view.Loaded += (_, _) => loadedCount++;
        view.Unloaded += (_, _) => unloadedCount++;

        var handler = await CreateHandlerAsync<ContentViewHandler>(view);
        var platformView = handler.PlatformView;

        var window = new Window { Content = platformView, Width = 200, Height = 200 };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, loadedCount);
            Assert.Equal(0, unloadedCount);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, loadedCount);
            Assert.Equal(1, unloadedCount);
        }
        finally
        {
            window.Close();
        }
    }
    
    [AvaloniaFact(DisplayName = "Focused/Unfocused reflect platform focus")]
    public async Task FocusedAndUnfocusedReflectPlatformFocus()
    {
        var entry = new MauiVirtualEntry();
        var focusedCount = 0;
        var unfocusedCount = 0;

        entry.Focused += (_, _) => focusedCount++;
        entry.Unfocused += (_, _) => unfocusedCount++;

        var handler = await CreateHandlerAsync<EntryHandler>(entry);
        var platformView = handler.PlatformView;
        platformView.Focusable = true;

        var window = new Window { Content = platformView, Width = 200, Height = 80 };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            platformView.Focus();
            Dispatcher.UIThread.RunJobs();

            Assert.True(platformView.IsFocused, "Platform view should accept focus");
            Assert.True(entry.IsFocused, "Entry should reflect focus state");
            Assert.True(focusedCount > 0, "Focused event should fire");

            window.FocusManager?.ClearFocus();
            Dispatcher.UIThread.RunJobs();

            Assert.False(entry.IsFocused, "Entry should clear focus state");
            Assert.True(unfocusedCount > 0, "Unfocused event should fire");
        }
        finally
        {
            window.Close();
        }
    }
}
