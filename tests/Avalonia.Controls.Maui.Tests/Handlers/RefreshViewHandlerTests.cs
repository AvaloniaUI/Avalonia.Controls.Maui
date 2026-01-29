using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiRefreshViewHandler = Avalonia.Controls.Maui.Handlers.RefreshViewHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class RefreshViewHandlerTests : HandlerTestBase<MauiRefreshViewHandler, RefreshViewStub>
{
    [AvaloniaFact(DisplayName = "Handler Creates Platform View")]
    public async Task HandlerCreatesPlatformView()
    {
        var refreshView = new RefreshViewStub();

        var handler = await CreateHandlerAsync(refreshView);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<RefreshContainer>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "IsRefreshing Initializes Correctly With False")]
    public async Task IsRefreshingInitializesCorrectlyWithFalse()
    {
        var refreshView = new RefreshViewStub
        {
            IsRefreshing = false
        };

        await ValidatePropertyInitValue(
            refreshView,
            () => refreshView.IsRefreshing,
            _ => refreshView.IsRefreshing,
            false);
    }

    [AvaloniaFact(DisplayName = "IsRefreshing Initializes Correctly With True")]
    public async Task IsRefreshingInitializesCorrectlyWithTrue()
    {
        var refreshView = new RefreshViewStub
        {
            IsRefreshing = true
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView);
        Assert.True(refreshView.IsRefreshing);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Initializes Correctly With True")]
    public async Task IsEnabledInitializesCorrectlyWithTrue()
    {
        var refreshView = new RefreshViewStub
        {
            IsEnabled = true
        };

        await ValidatePropertyInitValue(
            refreshView,
            () => refreshView.IsEnabled,
            GetNativeIsEnabled,
            true);
    }

    [AvaloniaFact(DisplayName = "IsEnabled Initializes Correctly With False")]
    public async Task IsEnabledInitializesCorrectlyWithFalse()
    {
        var refreshView = new RefreshViewStub
        {
            IsEnabled = false
        };

        await ValidatePropertyInitValue(
            refreshView,
            () => refreshView.IsEnabled,
            GetNativeIsEnabled,
            false);
    }

    [AvaloniaTheory(DisplayName = "IsEnabled Updates Correctly")]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task IsEnabledUpdatesCorrectly(bool initial, bool updated)
    {
        var refreshView = new RefreshViewStub
        {
            IsEnabled = initial
        };

        await ValidatePropertyUpdatesValue(
            refreshView,
            nameof(IView.IsEnabled),
            GetNativeIsEnabled,
            updated,
            initial);
    }

    [AvaloniaFact(DisplayName = "RefreshColor Initializes Correctly")]
    public async Task RefreshColorInitializesCorrectly()
    {
        var color = Colors.Blue;
        var refreshView = new RefreshViewStub
        {
            RefreshColor = new SolidPaint(color)
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView);
        Assert.NotNull(refreshView.RefreshColor);
    }

    [AvaloniaFact(DisplayName = "RefreshColor Updates Correctly")]
    public async Task RefreshColorUpdatesCorrectly()
    {
        var initialColor = Colors.Blue;
        var newColor = Colors.Red;

        var refreshView = new RefreshViewStub
        {
            RefreshColor = new SolidPaint(initialColor)
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(refreshView.RefreshColor);

        await InvokeOnMainThreadAsync(() =>
        {
            refreshView.RefreshColor = new SolidPaint(newColor);
            handler.UpdateValue(nameof(IRefreshView.RefreshColor));
        });

        Assert.NotNull(refreshView.RefreshColor);
    }

    [AvaloniaTheory(DisplayName = "RefreshColor Supports Various Colors")]
    [InlineData(255, 0, 0)]
    [InlineData(0, 255, 0)]
    [InlineData(0, 0, 255)]
    [InlineData(255, 165, 0)]
    [InlineData(128, 0, 128)]
    public async Task RefreshColorSupportsVariousColors(byte r, byte g, byte b)
    {
        var color = Color.FromRgb(r, g, b);
        var refreshView = new RefreshViewStub
        {
            RefreshColor = new SolidPaint(color)
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView);
        Assert.NotNull(refreshView.RefreshColor);
    }

    [AvaloniaFact(DisplayName = "Null RefreshColor Does Not Throw")]
    public async Task NullRefreshColorDoesNotThrow()
    {
        var refreshView = new RefreshViewStub
        {
            RefreshColor = null
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView);
        Assert.Null(refreshView.RefreshColor);
    }

    [AvaloniaFact(DisplayName = "Content Initializes Correctly")]
    public async Task ContentInitializesCorrectly()
    {
        var content = new LabelStub { Text = "Test Content" };
        var refreshView = new RefreshViewStub
        {
            Content = content
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView);
        Assert.NotNull(handler.PlatformView.Content);
    }

    [AvaloniaFact(DisplayName = "Null Content Sets Null")]
    public async Task NullContentSetsNull()
    {
        var refreshView = new RefreshViewStub
        {
            Content = null
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView);
        Assert.Null(handler.PlatformView.Content);
    }

    [AvaloniaFact(DisplayName = "Content Updates Correctly")]
    public async Task ContentUpdatesCorrectly()
    {
        var initialContent = new LabelStub { Text = "Initial" };
        var refreshView = new RefreshViewStub
        {
            Content = initialContent
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView.Content);

        var newContent = new LabelStub { Text = "Updated" };
        await InvokeOnMainThreadAsync(() =>
        {
            refreshView.Content = newContent;
            handler.UpdateValue(nameof(IRefreshView.Content));
        });

        Assert.NotNull(handler.PlatformView.Content);
    }

    [AvaloniaFact(DisplayName = "Content Can Be Cleared")]
    public async Task ContentCanBeCleared()
    {
        var content = new LabelStub { Text = "Content" };
        var refreshView = new RefreshViewStub
        {
            Content = content
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView.Content);

        await InvokeOnMainThreadAsync(() =>
        {
            refreshView.Content = null;
            handler.UpdateValue(nameof(IRefreshView.Content));
        });

        Assert.Null(handler.PlatformView.Content);
    }

    [AvaloniaFact(DisplayName = "Background Initializes Correctly")]
    public async Task BackgroundInitializesCorrectly()
    {
        var color = Colors.LightGray;
        var refreshView = new RefreshViewStub
        {
            Background = new SolidPaint(color)
        };

        var platformColor = await GetValueAsync(refreshView, GetNativeBackgroundColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    [AvaloniaFact(DisplayName = "Background Updates Correctly")]
    public async Task BackgroundUpdatesCorrectly()
    {
        var initialColor = Colors.White;
        var newColor = Colors.LightBlue;

        var refreshView = new RefreshViewStub
        {
            Background = new SolidPaint(initialColor)
        };

        var handler = await CreateHandlerAsync(refreshView);
        var initialNative = GetNativeBackgroundColor(handler);
        Assert.NotNull(initialNative);

        refreshView.Background = new SolidPaint(newColor);
        handler.UpdateValue(nameof(IView.Background));

        var newNative = GetNativeBackgroundColor(handler);
        Assert.NotNull(newNative);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, newNative);
    }

    [AvaloniaFact(DisplayName = "Null Background Sets Transparent")]
    public async Task NullBackgroundSetsTransparent()
    {
        var refreshView = new RefreshViewStub
        {
            Background = null
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "IsRefreshing Changes Update RefreshingCount")]
    public async Task IsRefreshingChangesUpdateRefreshingCount()
    {
        var refreshView = new RefreshViewStub
        {
            IsRefreshing = false
        };

        await CreateHandlerAsync(refreshView);
        Assert.Equal(0, refreshView.RefreshingCount);

        await InvokeOnMainThreadAsync(() =>
        {
            refreshView.IsRefreshing = true;
        });

        Assert.Equal(1, refreshView.RefreshingCount);

        await InvokeOnMainThreadAsync(() =>
        {
            refreshView.IsRefreshing = false;
            refreshView.IsRefreshing = true;
        });

        Assert.Equal(2, refreshView.RefreshingCount);
    }

    [AvaloniaFact(DisplayName = "Setting IsRefreshing To Same Value Does Not Increment Count")]
    public async Task SettingIsRefreshingToSameValueDoesNotIncrementCount()
    {
        var refreshView = new RefreshViewStub
        {
            IsRefreshing = true
        };

        await CreateHandlerAsync(refreshView);
        Assert.Equal(1, refreshView.RefreshingCount);

        await InvokeOnMainThreadAsync(() =>
        {
            refreshView.IsRefreshing = true;
        });

        Assert.Equal(1, refreshView.RefreshingCount);
    }

    [AvaloniaFact(DisplayName = "Handler With Custom Mapper Works")]
    public async Task HandlerWithCustomMapperWorks()
    {
        var customMapper = new PropertyMapper<IRefreshView, MauiRefreshViewHandler>(MauiRefreshViewHandler.Mapper);
        var handler = new MauiRefreshViewHandler(customMapper);
        var refreshView = new RefreshViewStub();

        await InvokeOnMainThreadAsync(() =>
        {
            InitializeViewHandler(refreshView, handler);
        });

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Handler With Null Mapper Uses Default")]
    public async Task HandlerWithNullMapperUsesDefault()
    {
        var handler = new MauiRefreshViewHandler(null);
        var refreshView = new RefreshViewStub();

        await InvokeOnMainThreadAsync(() =>
        {
            InitializeViewHandler(refreshView, handler);
        });

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Handler With Null Mappers Uses Defaults")]
    public async Task HandlerWithNullMappersUsesDefaults()
    {
        var handler = new MauiRefreshViewHandler(null, null);
        var refreshView = new RefreshViewStub();

        await InvokeOnMainThreadAsync(() =>
        {
            InitializeViewHandler(refreshView, handler);
        });

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Multiple Property Updates Work Correctly")]
    public async Task MultiplePropertyUpdatesWorkCorrectly()
    {
        var refreshView = new RefreshViewStub
        {
            IsEnabled = true,
            RefreshColor = new SolidPaint(Colors.Blue),
            Background = new SolidPaint(Colors.White)
        };

        var handler = await CreateHandlerAsync(refreshView);

        await InvokeOnMainThreadAsync(() =>
        {
            refreshView.IsEnabled = false;
            handler.UpdateValue(nameof(IView.IsEnabled));

            refreshView.RefreshColor = new SolidPaint(Colors.Red);
            handler.UpdateValue(nameof(IRefreshView.RefreshColor));

            refreshView.Background = new SolidPaint(Colors.Gray);
            handler.UpdateValue(nameof(IView.Background));
        });

        Assert.False(GetNativeIsEnabled(handler));
        Assert.NotNull(refreshView.RefreshColor);
    }

    [AvaloniaFact(DisplayName = "Unrelated Property Update Does Not Affect IsEnabled")]
    public async Task UnrelatedPropertyUpdateDoesNotAffectIsEnabled()
    {
        var refreshView = new RefreshViewStub
        {
            IsEnabled = true,
            RefreshColor = new SolidPaint(Colors.Blue)
        };

        await ValidateUnrelatedPropertyUnaffected(
            refreshView,
            GetNativeIsEnabled,
            nameof(IRefreshView.RefreshColor),
            () => refreshView.RefreshColor = new SolidPaint(Colors.Red));
    }

    [AvaloniaFact(DisplayName = "Visibility Initializes Correctly With Visible")]
    public async Task VisibilityInitializesCorrectlyWithVisible()
    {
        var refreshView = new RefreshViewStub
        {
            Visibility = Visibility.Visible
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.True(handler.PlatformView.IsVisible);
    }

    [AvaloniaFact(DisplayName = "Visibility Initializes Correctly With Hidden")]
    public async Task VisibilityInitializesCorrectlyWithHidden()
    {
        var refreshView = new RefreshViewStub
        {
            Visibility = Visibility.Hidden
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.False(handler.PlatformView.IsVisible);
    }

    [AvaloniaFact(DisplayName = "Visibility Initializes Correctly With Collapsed")]
    public async Task VisibilityInitializesCorrectlyWithCollapsed()
    {
        var refreshView = new RefreshViewStub
        {
            Visibility = Visibility.Collapsed
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.False(handler.PlatformView.IsVisible);
    }

    [AvaloniaFact(DisplayName = "Default Values Are Set Correctly")]
    public async Task DefaultValuesAreSetCorrectly()
    {
        var refreshView = new RefreshViewStub();

        var handler = await CreateHandlerAsync(refreshView);

        Assert.NotNull(handler.PlatformView);
        Assert.False(refreshView.IsRefreshing);
        Assert.True(refreshView.IsEnabled);
        Assert.Null(refreshView.RefreshColor);
        Assert.Null(refreshView.Content);
        Assert.Equal(0, refreshView.RefreshingCount);
    }

    [AvaloniaFact(DisplayName = "RefreshColor With Alpha Channel")]
    public async Task RefreshColorWithAlphaChannel()
    {
        var color = Color.FromRgba(255, 0, 0, 128);
        var refreshView = new RefreshViewStub
        {
            RefreshColor = new SolidPaint(color)
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView);
        Assert.NotNull(refreshView.RefreshColor);
    }

    [AvaloniaFact(DisplayName = "Content With Nested Views")]
    public async Task ContentWithNestedViews()
    {
        var innerLabel = new LabelStub { Text = "Inner" };
        var refreshView = new RefreshViewStub
        {
            Content = innerLabel
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView);
        Assert.NotNull(handler.PlatformView.Content);
    }

    [AvaloniaFact(DisplayName = "Rapid IsRefreshing Toggle Does Not Throw")]
    public async Task RapidIsRefreshingToggleDoesNotThrow()
    {
        var refreshView = new RefreshViewStub();
        var handler = await CreateHandlerAsync(refreshView);

        await InvokeOnMainThreadAsync(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                refreshView.IsRefreshing = true;
                handler.UpdateValue(nameof(IRefreshView.IsRefreshing));
                refreshView.IsRefreshing = false;
                handler.UpdateValue(nameof(IRefreshView.IsRefreshing));
            }
        });

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Rapid Content Changes Do Not Throw")]
    public async Task RapidContentChangesDoNotThrow()
    {
        var refreshView = new RefreshViewStub();
        var handler = await CreateHandlerAsync(refreshView);

        await InvokeOnMainThreadAsync(() =>
        {
            for (int i = 0; i < 50; i++)
            {
                refreshView.Content = new LabelStub { Text = $"Content {i}" };
                handler.UpdateValue(nameof(IRefreshView.Content));
                refreshView.Content = null;
                handler.UpdateValue(nameof(IRefreshView.Content));
            }
        });

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Gradient RefreshColor Initializes Correctly")]
    public async Task GradientRefreshColorInitializesCorrectly()
    {
        var gradientPaint = new LinearGradientPaint
        {
            StartColor = Colors.Red,
            EndColor = Colors.Blue
        };

        var refreshView = new RefreshViewStub
        {
            RefreshColor = gradientPaint
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.NotNull(handler.PlatformView);
        Assert.NotNull(refreshView.RefreshColor);
    }

    [AvaloniaFact(DisplayName = "IsRefreshing False To True Transition")]
    public async Task IsRefreshingFalseToTrueTransition()
    {
        var refreshView = new RefreshViewStub
        {
            IsRefreshing = false
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.False(refreshView.IsRefreshing);

        await InvokeOnMainThreadAsync(() =>
        {
            refreshView.IsRefreshing = true;
            handler.UpdateValue(nameof(IRefreshView.IsRefreshing));
        });

        Assert.True(refreshView.IsRefreshing);
    }

    [AvaloniaFact(DisplayName = "IsRefreshing True To False Transition")]
    public async Task IsRefreshingTrueToFalseTransition()
    {
        var refreshView = new RefreshViewStub
        {
            IsRefreshing = true
        };

        var handler = await CreateHandlerAsync(refreshView);
        Assert.True(refreshView.IsRefreshing);

        await InvokeOnMainThreadAsync(() =>
        {
            refreshView.IsRefreshing = false;
            handler.UpdateValue(nameof(IRefreshView.IsRefreshing));
        });

        Assert.False(refreshView.IsRefreshing);
    }

    [AvaloniaFact(DisplayName = "RefreshColor Reapplies When Visualizer Changes")]
    public async Task RefreshColorReappliesWhenVisualizerChanges()
    {
        var color = Colors.Red;
        var refreshView = new RefreshViewStub
        {
            RefreshColor = new SolidPaint(color)
        };

        var handler = await CreateHandlerAsync(refreshView);
        var platformView = handler.PlatformView;

        var newVisualizer = new RefreshVisualizer();
        
        await InvokeOnMainThreadAsync(() =>
        {
            platformView.Visualizer = newVisualizer;
        });

        var brush = newVisualizer.Foreground as Media.SolidColorBrush;
        Assert.NotNull(brush);
        Assert.Equal(Media.Colors.Red, brush.Color);
    }

    private bool GetNativeIsEnabled(MauiRefreshViewHandler handler)
    {
        return handler.PlatformView?.IsEnabled ?? false;
    }

    private Color? GetNativeBackgroundColor(MauiRefreshViewHandler handler) =>
        AvaloniaPropertyHelpers.GetNativeBackgroundColor(handler);
}