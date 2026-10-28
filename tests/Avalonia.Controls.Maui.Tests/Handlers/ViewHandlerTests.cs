using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Maui.Controls.Hosting;
using System.ComponentModel;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using MauiColors = Microsoft.Maui.Graphics.Colors;
using MauiContentView = Microsoft.Maui.Controls.ContentView;
using MauiEllipseGeometry = Microsoft.Maui.Controls.Shapes.EllipseGeometry;
using MauiVirtualEntry = Microsoft.Maui.Controls.Entry;
using MauiWindow = Microsoft.Maui.Controls.Window;
using MauiPoint = Microsoft.Maui.Graphics.Point;
using MauiRect = Microsoft.Maui.Graphics.Rect;
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

    [AvaloniaFact(DisplayName = "Margin maps to platform control")]
    public async Task MarginMapsToPlatformControl()
    {
        var margin = new Microsoft.Maui.Thickness(1, 2, 3, 4);
        var view = new ContentViewStub
        {
            Margin = margin
        };

        var platformMargin = await GetValueAsync<global::Avalonia.Thickness, ContentViewHandler>(view, handler => handler.PlatformView.Margin);

        Assert.Equal(new global::Avalonia.Thickness(1, 2, 3, 4), platformMargin);
    }

    [AvaloniaFact(DisplayName = "Margin is subtracted from measure constraints only once")]
    public async Task MarginIsSubtractedFromMeasureConstraintsOnlyOnce()
    {
        const string text = "The quick brown fox jumps over the lazy dog and keeps on running far away";

        var withMargin = new Microsoft.Maui.Controls.Label
        {
            Text = text,
            LineBreakMode = Microsoft.Maui.LineBreakMode.WordWrap,
            Margin = new Microsoft.Maui.Thickness(50, 5, 50, 5)
        };
        var withoutMargin = new Microsoft.Maui.Controls.Label
        {
            Text = text,
            LineBreakMode = Microsoft.Maui.LineBreakMode.WordWrap
        };

        var (marginSize, plainSize) = await InvokeOnMainThreadAsync(() =>
        {
            CreateHandler<LabelHandler>(withMargin);
            CreateHandler<LabelHandler>(withoutMargin);

            // MAUI deflates the constraints by the margin before asking the handler, so the
            // label with a margin must see the same 150-wide space as the plain one.
            var marginSize = ((Microsoft.Maui.IView)withMargin).Measure(250, double.PositiveInfinity);
            var plainSize = ((Microsoft.Maui.IView)withoutMargin).Measure(150, double.PositiveInfinity);
            return (marginSize, plainSize);
        });

        Assert.Equal(plainSize.Width + 100, marginSize.Width, 3);
        Assert.Equal(plainSize.Height + 10, marginSize.Height, 3);
    }

    [AvaloniaFact(DisplayName = "Derived handler can override GetDesiredSize")]
    public async Task DerivedHandlerCanOverrideGetDesiredSize()
    {
        var view = new MauiContentView
        {
            Margin = new Microsoft.Maui.Thickness(5)
        };

        var size = await InvokeOnMainThreadAsync(() =>
        {
            CreateHandler<FixedSizeContentViewHandler>(view);
            return ((Microsoft.Maui.IView)view).Measure(500, 500);
        });

        Assert.Equal(FixedSizeContentViewHandler.FixedSize.Width + 10, size.Width, 3);
        Assert.Equal(FixedSizeContentViewHandler.FixedSize.Height + 10, size.Height, 3);
    }

    [AvaloniaFact(DisplayName = "Clip added later wraps ScrollView content in a container")]
    public async Task ClipAddedLaterWrapsScrollViewContent()
    {
        var content = new MauiContentView();
        var scrollView = new Microsoft.Maui.Controls.ScrollView { Content = content };

        await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<ScrollViewHandler>(scrollView);
            var contentHandler = (ContentViewHandler)content.Handler!;

            content.Clip = new MauiEllipseGeometry(new MauiPoint(10, 10), 10, 10);

            var container = Assert.IsAssignableFrom<Control>(contentHandler.ContainerView);
            Assert.Same(container, handler.PlatformView.Content);
            Assert.Same(container, contentHandler.PlatformView.Parent);

            content.Clip = null;

            Assert.Null(contentHandler.ContainerView);
            Assert.Same(contentHandler.PlatformView, handler.PlatformView.Content);
        });
    }

    [AvaloniaFact(DisplayName = "Clip added later wraps Border content in a container")]
    public async Task ClipAddedLaterWrapsBorderContent()
    {
        var content = new MauiContentView();
        var border = new Microsoft.Maui.Controls.Border { Content = content };

        await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<BorderHandler>(border);
            var contentHandler = (ContentViewHandler)content.Handler!;
            var host = Assert.IsAssignableFrom<Decorator>(contentHandler.PlatformView.Parent);

            content.Clip = new MauiEllipseGeometry(new MauiPoint(10, 10), 10, 10);

            var container = Assert.IsAssignableFrom<Control>(contentHandler.ContainerView);
            Assert.Same(container, host.Child);
            Assert.Same(container, contentHandler.PlatformView.Parent);

            content.Clip = null;

            Assert.Null(contentHandler.ContainerView);
            Assert.Same(contentHandler.PlatformView, host.Child);
        });
    }

    [AvaloniaFact(DisplayName = "Clip added later wraps window content in a container")]
    public async Task ClipAddedLaterWrapsWindowContent()
    {
        var content = new MauiContentView();

        await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<ContentViewHandler>(content);
            var window = new Window { Content = handler.PlatformView };
            window.Show();

            try
            {
                content.Clip = new MauiEllipseGeometry(new MauiPoint(10, 10), 10, 10);

                var container = Assert.IsAssignableFrom<Control>(handler.ContainerView);
                Assert.Same(container, window.Content);

                content.Clip = null;

                Assert.Null(handler.ContainerView);
                Assert.Same(handler.PlatformView, window.Content);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "InputTransparent maps to IsHitTestVisible")]
    public async Task InputTransparentMapsToIsHitTestVisible()
    {
        var view = new ContentViewStub
        {
            InputTransparent = true
        };

        var handler = await CreateHandlerAsync<ContentViewHandler>(view);

        var isHitTestVisible = await InvokeOnMainThreadAsync(() => handler.PlatformView.IsHitTestVisible);
        Assert.False(isHitTestVisible);

        await InvokeOnMainThreadAsync(() =>
        {
            view.InputTransparent = false;
            handler.UpdateValue(nameof(Microsoft.Maui.IView.InputTransparent));
        });

        isHitTestVisible = await InvokeOnMainThreadAsync(() => handler.PlatformView.IsHitTestVisible);
        Assert.True(isHitTestVisible);
    }

    [AvaloniaFact(DisplayName = "Handler is assigned to view")]
    public async Task HandlerIsAssignedToView()
    {
        var view = new ContentViewStub();
        var handler = await CreateHandlerAsync<ContentViewHandler>(view);

        Assert.Same(handler, view.Handler);
    }

    [AvaloniaFact(DisplayName = "BindingContext and Resources inherit through MAUI tree")]
    public void BindingContextAndResourcesInheritThroughMauiTree()
    {
        var viewModel = new InheritedPropertyViewModel("Inherited binding");
        var label = new Microsoft.Maui.Controls.Label();

        label.SetBinding(
            Microsoft.Maui.Controls.Label.TextProperty,
            new Microsoft.Maui.Controls.Binding(nameof(InheritedPropertyViewModel.Text)));
        label.SetDynamicResource(Microsoft.Maui.Controls.Label.TextColorProperty, "InheritedTextColor");

        var page = new Microsoft.Maui.Controls.ContentPage
        {
            BindingContext = viewModel,
            Resources =
            {
                ["InheritedTextColor"] = MauiColors.MediumSeaGreen
            },
            Content = label
        };

        Assert.Same(viewModel, label.BindingContext);
        Assert.Equal("Inherited binding", label.Text);
        Assert.Equal(MauiColors.MediumSeaGreen, label.TextColor);
        Assert.True(page.Resources.ContainsKey("InheritedTextColor"));
    }

    [AvaloniaFact(DisplayName = "Parent and Window are assigned by MAUI tree")]
    public void ParentAndWindowAreAssignedByMauiTree()
    {
        var content = new MauiContentView();
        var page = new Microsoft.Maui.Controls.ContentPage
        {
            Content = content
        };
        var window = new MauiWindow(page);

        Assert.Same(page, content.Parent);
        Assert.Same(window, page.Window);
        Assert.Same(window, content.Window);
    }

    [AvaloniaFact(DisplayName = "Effects collection retains routing effects")]
    public void EffectsCollectionRetainsRoutingEffects()
    {
        var view = new MauiContentView();
        var effect = new TestRoutingEffect();

        view.Effects.Add(effect);

        Assert.Single(view.Effects);
        Assert.Same(effect, view.Effects[0]);

        view.Effects.Remove(effect);

        Assert.Empty(view.Effects);
    }

    [AvaloniaFact(DisplayName = "Registered PlatformEffect can update Avalonia control")]
    public async Task RegisteredPlatformEffectCanUpdateAvaloniaControl()
    {
        TestFocusBackgroundPlatformEffect.Reset();

        EnsureHandlerCreated(builder =>
        {
            builder.ConfigureEffects(effects =>
            {
                effects.Add<TestFocusBackgroundRoutingEffect, TestFocusBackgroundPlatformEffect>();
            });
        });

        var entry = new MauiVirtualEntry { Text = "Effect target" };
        entry.Effects.Add(new TestFocusBackgroundRoutingEffect());

        var handler = await CreateHandlerAsync<EntryHandler>(entry);
        var platformView = handler.PlatformView;
        platformView.Focusable = true;

        var window = new Window { Content = platformView, Width = 200, Height = 80 };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, TestFocusBackgroundPlatformEffect.AttachedCount);

            platformView.Focus();
            Dispatcher.UIThread.RunJobs();

            Assert.True(entry.IsFocused, "Entry should reflect platform focus before the effect updates the background.");
            var brush = Assert.IsAssignableFrom<ISolidColorBrush>(platformView.Background);
            Assert.Equal(TestFocusBackgroundPlatformEffect.FocusedBackgroundColor, brush.Color);

            window.FocusManager?.Focus(null);
            Dispatcher.UIThread.RunJobs();

            Assert.False(entry.IsFocused, "Entry should reflect platform unfocus before the effect restores the background.");
            Assert.Same(TestFocusBackgroundPlatformEffect.OriginalBackground, platformView.Background);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact(DisplayName = "Bounds, Frame, X, and Y update after arrange")]
    public async Task BoundsFrameXAndYUpdateAfterArrange()
    {
        var view = new MauiContentView
        {
            WidthRequest = 100,
            HeightRequest = 50
        };
        var handler = await CreateHandlerAsync<ContentViewHandler>(view);

        await InvokeOnMainThreadAsync(() =>
        {
            ((Microsoft.Maui.IView)view).Arrange(new MauiRect(12, 34, 100, 50));
        });

        Assert.Equal(12, view.X, precision: 3);
        Assert.Equal(34, view.Y, precision: 3);
        Assert.Equal(100, view.Width, precision: 3);
        Assert.Equal(50, view.Height, precision: 3);

        Assert.Equal(12, view.Bounds.X, precision: 3);
        Assert.Equal(34, view.Bounds.Y, precision: 3);
        Assert.Equal(100, view.Bounds.Width, precision: 3);
        Assert.Equal(50, view.Bounds.Height, precision: 3);

        Assert.Equal(12, view.Frame.X, precision: 3);
        Assert.Equal(34, view.Frame.Y, precision: 3);
        Assert.Equal(100, view.Frame.Width, precision: 3);
        Assert.Equal(50, view.Frame.Height, precision: 3);

        var platformBounds = await InvokeOnMainThreadAsync(() => handler.PlatformView.Bounds);
        Assert.Equal(12, platformBounds.X, precision: 3);
        Assert.Equal(34, platformBounds.Y, precision: 3);
        Assert.Equal(100, platformBounds.Width, precision: 3);
        Assert.Equal(50, platformBounds.Height, precision: 3);
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

    [AvaloniaFact(DisplayName = "Loaded handler can change its row while the row's binding context is applied")]
    public async Task LoadedHandlerCanChangeRowWhileBindingContextIsApplied()
    {
        // The row's content comes from a binding, so it is created and attached while MAUI
        // passes the row's binding context to the row's children
        var row = new Microsoft.Maui.Controls.Grid();
        var content = new MauiContentView();
        content.SetBinding(MauiContentView.ContentProperty, new Microsoft.Maui.Controls.Binding(".", converter: new LabelWithLoadedHandler(row)));
        row.Add(content);
        row.Add(new Microsoft.Maui.Controls.Label { Text = "Sibling" });

        await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<LayoutHandler>(row);
            var window = new Window { Width = 200, Height = 200, Content = handler.PlatformView };
            window.Show();
            try
            {
                row.BindingContext = "Title";
                Dispatcher.UIThread.RunJobs();

                Assert.Equal(3, row.Count);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private sealed class LabelWithLoadedHandler(Microsoft.Maui.Controls.Grid row) : Microsoft.Maui.Controls.IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            var label = new Microsoft.Maui.Controls.Label { Text = value as string };
            label.Loaded += (_, _) => row.Add(new Microsoft.Maui.Controls.BoxView { HeightRequest = 2 });
            return label;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
            throw new NotSupportedException();
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

            window.FocusManager?.Focus(null);
            Dispatcher.UIThread.RunJobs();

            Assert.False(entry.IsFocused, "Entry should clear focus state");
            Assert.True(unfocusedCount > 0, "Unfocused event should fire");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact(DisplayName = "Focus/Unfocus update IsFocused")]
    public async Task FocusAndUnfocusUpdateIsFocused()
    {
        var entry = new MauiVirtualEntry();
        var handler = await CreateHandlerAsync<EntryHandler>(entry);
        var platformView = handler.PlatformView;
        platformView.Focusable = true;

        var window = new Window { Content = platformView, Width = 200, Height = 80 };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var focused = await InvokeOnMainThreadAsync(() => entry.Focus());
            Dispatcher.UIThread.RunJobs();

            Assert.True(focused, "Focus should return true");
            Assert.True(entry.IsFocused, "Entry should report focused");

            await InvokeOnMainThreadAsync(() => entry.Unfocus());
            Dispatcher.UIThread.RunJobs();

            Assert.False(entry.IsFocused, "Entry should report unfocused");
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class FixedSizeContentViewHandler : ContentViewHandler
    {
        public static readonly Microsoft.Maui.Graphics.Size FixedSize = new(80, 30);

        public override Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
            FixedSize;
    }

    private sealed class InheritedPropertyViewModel(string text)
    {
        public string Text { get; } = text;
    }

    private sealed class TestRoutingEffect : Microsoft.Maui.Controls.RoutingEffect
    {
    }

    private sealed class TestFocusBackgroundRoutingEffect : Microsoft.Maui.Controls.RoutingEffect
    {
    }

    private sealed class TestFocusBackgroundPlatformEffect : Microsoft.Maui.Controls.Platform.PlatformEffect
    {
        public static readonly Color FocusedBackgroundColor = Color.FromRgb(220, 245, 220);

        public static int AttachedCount { get; private set; }

        public static IBrush? OriginalBackground { get; private set; }

        public static void Reset()
        {
            AttachedCount = 0;
            OriginalBackground = null;
        }

        protected override void OnAttached()
        {
            AttachedCount++;

            if (Control is AvaloniaTextBox textBox)
            {
                OriginalBackground = textBox.Background;
            }

            UpdateBackground();
        }

        protected override void OnDetached()
        {
            ClearBackground();
            AttachedCount = Math.Max(0, AttachedCount - 1);
            OriginalBackground = null;
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);

            if (args.PropertyName == Microsoft.Maui.Controls.VisualElement.IsFocusedProperty.PropertyName)
            {
                UpdateBackground();
            }
        }

        private void UpdateBackground()
        {
            if (Control is AvaloniaTextBox textBox)
            {
                textBox.Background = Element is Microsoft.Maui.Controls.VisualElement { IsFocused: true }
                    ? new SolidColorBrush(FocusedBackgroundColor)
                    : OriginalBackground;
            }
        }

        private void ClearBackground()
        {
            if (Control is AvaloniaTextBox textBox)
            {
                textBox.Background = OriginalBackground;
            }
        }
    }
}
