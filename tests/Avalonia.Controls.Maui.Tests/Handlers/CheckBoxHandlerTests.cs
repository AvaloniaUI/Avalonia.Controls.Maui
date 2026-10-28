using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiCheckBoxHandler = Avalonia.Controls.Maui.Handlers.CheckBoxHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class CheckBoxHandlerTests : HandlerTestBase<MauiCheckBoxHandler, CheckBoxStub>
{
    [AvaloniaFact(DisplayName = "Foreground Colors The Box In Every State")]
    public async Task ForegroundColorsTheBoxInEveryState()
    {
        var checkBox = new CheckBoxStub { Foreground = new SolidPaint(Colors.Red) };
        var handler = await CreateHandlerAsync(checkBox);

        await InvokeOnMainThreadAsync(() =>
        {
            foreach (var key in new[]
            {
                "CheckBoxCheckBackgroundStrokeUnchecked", "CheckBoxCheckBackgroundStrokeUncheckedDisabled",
                "CheckBoxCheckBackgroundStrokeChecked", "CheckBoxCheckBackgroundStrokeIndeterminate",
                "CheckBoxCheckBackgroundFillChecked", "CheckBoxCheckBackgroundFillCheckedPointerOver",
            })
            {
                var brush = Assert.IsAssignableFrom<Avalonia.Media.ISolidColorBrush>(handler.PlatformView.Resources[key]);
                Assert.Equal(Avalonia.Media.Colors.Red, brush.Color);
            }

            checkBox.Foreground = null;
            handler.UpdateValue(nameof(ICheckBox.Foreground));

            Assert.False(handler.PlatformView.Resources.ContainsKey("CheckBoxCheckBackgroundFillChecked"));
        });
    }

    [AvaloniaFact(DisplayName = "IsChecked Initializes Correctly")]
    public async Task IsCheckedInitializesCorrectly()
    {
        var checkBox = new CheckBoxStub
        {
            IsChecked = true
        };

        await ValidatePropertyInitValue(checkBox, () => checkBox.IsChecked, GetNativeIsChecked, checkBox.IsChecked);
    }

    [AvaloniaTheory(DisplayName = "CheckBox Stays Inside A Small Grid Cell")]
    [InlineData(-1, 20, 20)]
    [InlineData(44, 24, 24)]
    public async Task CheckBoxStaysInsideSmallGridCell(double minimumRequest, double column, double row)
    {
        var checkBox = new Microsoft.Maui.Controls.CheckBox { IsChecked = true };
        if (minimumRequest > 0)
        {
            checkBox.MinimumWidthRequest = minimumRequest;
            checkBox.MinimumHeightRequest = minimumRequest;
        }
        var grid = new Microsoft.Maui.Controls.Grid
        {
            ColumnDefinitions = new Microsoft.Maui.Controls.ColumnDefinitionCollection(new Microsoft.Maui.Controls.ColumnDefinition(column), new Microsoft.Maui.Controls.ColumnDefinition(Microsoft.Maui.GridLength.Star)),
            RowDefinitions = new Microsoft.Maui.Controls.RowDefinitionCollection(new Microsoft.Maui.Controls.RowDefinition(row), new Microsoft.Maui.Controls.RowDefinition(Microsoft.Maui.GridLength.Star)),
        };
        grid.Add(checkBox);

        var bounds = await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<Avalonia.Controls.Maui.Handlers.LayoutHandler>(grid);
            var window = new Window { Width = 200, Height = 200, Content = handler.PlatformView };
            window.Show();
            try
            {
                window.UpdateLayout();
                var platformView = (Control)checkBox.Handler!.PlatformView!;
                var origin = platformView.TranslatePoint(default, (Visual)handler.PlatformView!)!.Value;
                return new Rect(origin, platformView.Bounds.Size);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(bounds.X >= 0 && bounds.Y >= 0 && bounds.Right <= column + 0.01 && bounds.Bottom <= row + 0.01,
            $"CheckBox {bounds} spills out of its {column}x{row} cell");
    }

    [AvaloniaFact(DisplayName = "MinimumWidthRequest Still Widens The CheckBox When There Is Room")]
    public async Task MinimumWidthRequestStillWidensCheckBox()
    {
        var checkBox = new Microsoft.Maui.Controls.CheckBox { MinimumWidthRequest = 44 };
        var layout = new Microsoft.Maui.Controls.HorizontalStackLayout { checkBox };

        var width = await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<Avalonia.Controls.Maui.Handlers.LayoutHandler>(layout);
            var window = new Window { Width = 200, Height = 100, Content = handler.PlatformView };
            window.Show();
            try
            {
                window.UpdateLayout();
                return ((Control)checkBox.Handler!.PlatformView!).Bounds.Width;
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(checkBox.Frame.Width >= 44, $"CheckBox frame is {checkBox.Frame.Width} wide");
        Assert.True(width >= 44, $"CheckBox is {width} wide");
    }

    [AvaloniaFact(DisplayName = "Box Is Vertically Centered When Taller Than Template")]
    public async Task BoxIsVerticallyCenteredWhenTallerThanTemplate()
    {
        var checkBox = new CheckBoxStub
        {
            Height = Microsoft.Maui.Primitives.Dimension.Unset,
            MinimumHeight = 44
        };

        var handler = await CreateHandlerAsync(checkBox);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            var window = new Window
            {
                Width = 200,
                Height = 200,
                Content = new Avalonia.Controls.Canvas { Children = { platformView } }
            };
            window.Show();

            try
            {
                platformView.Measure(new global::Avalonia.Size(100, 44));
                platformView.Arrange(new global::Avalonia.Rect(0, 0, 100, 44));

                Assert.Equal(44, platformView.Bounds.Height, 3);

                var box = platformView.GetVisualDescendants()
                    .OfType<Border>()
                    .Single(b => b.Name == "NormalRectangle");
                var boxTop = box.TranslatePoint(new global::Avalonia.Point(0, 0), platformView)!.Value.Y;
                var boxCenter = boxTop + box.Bounds.Height / 2;

                Assert.Equal(platformView.Bounds.Height / 2, boxCenter, 1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaTheory(DisplayName = "IsChecked Updates Correctly")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsCheckedUpdatesCorrectly(bool isChecked)
    {
        var checkBox = new CheckBoxStub
        {
            IsChecked = false
        };

        await ValidatePropertyUpdatesValue(
            checkBox,
            nameof(ICheckBox.IsChecked),
            GetNativeIsChecked,
            isChecked,
            false);
    }

    [AvaloniaFact(DisplayName = "Foreground Initializes Correctly")]
    public async Task ForegroundInitializesCorrectly()
    {
        var checkBox = new CheckBoxStub
        {
            Foreground = new SolidPaint(Colors.Red)
        };

        var values = await GetValueAsync(checkBox, (handler) =>
        {
            return new
            {
                ViewValue = checkBox.Foreground,
                PlatformViewValue = GetNativeForeground(handler)
            };
        });

        Assert.NotNull(values.PlatformViewValue);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, values.PlatformViewValue);
    }

    [AvaloniaFact(DisplayName = "Null Foreground Doesn't Crash")]
    public async Task NullForegroundDoesntCrash()
    {
        var checkBox = new CheckBoxStub
        {
            Foreground = null!
        };

        await CreateHandlerAsync(checkBox);
    }

    [AvaloniaTheory(DisplayName = "Foreground Updates Correctly")]
    [InlineData(255, 0, 0)]      // Red
    [InlineData(0, 255, 0)]      // Green
    [InlineData(0, 0, 255)]      // Blue
    public async Task ForegroundUpdatesCorrectly(byte r, byte g, byte b)
    {
        var color = Color.FromRgb(r, g, b);
        var checkBox = new CheckBoxStub
        {
            Foreground = new SolidPaint(Colors.Black)
        };

        var handler = await CreateHandlerAsync(checkBox);

        await InvokeOnMainThreadAsync(() =>
        {
            checkBox.Foreground = new SolidPaint(color);
            handler.UpdateValue(nameof(ICheckBox.Foreground));
        });

        var nativeColor = await GetValueAsync(checkBox, GetNativeForeground);
        
        Assert.NotNull(nativeColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, nativeColor);
    }

    [AvaloniaFact(DisplayName = "BorderBrush Initializes With Foreground")]
    public async Task BorderBrushInitializesWithForeground()
    {
        var checkBox = new CheckBoxStub
        {
            Foreground = new SolidPaint(Colors.Blue)
        };

        var values = await GetValueAsync(checkBox, (handler) =>
        {
            return new
            {
                ViewValue = checkBox.Foreground,
                PlatformBorderBrushValue = GetNativeBorderBrush(handler)
            };
        });

        Assert.NotNull(values.PlatformBorderBrushValue);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Blue, values.PlatformBorderBrushValue);
    }

    [AvaloniaFact(DisplayName = "Background Initializes Correctly")]
    public async Task BackgroundInitializesCorrectly()
    {
        var color = Colors.Green;
        var checkBox = new CheckBoxStub
        {
            Background = new SolidPaint(color)
        };

        var platformColor = await GetValueAsync(checkBox, GetNativeBackgroundColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    [AvaloniaFact(DisplayName = "Null Background Doesn't Crash")]
    public async Task NullBackgroundDoesntCrash()
    {
        var checkBox = new CheckBoxStub
        {
            Background = null!
        };

        await CreateHandlerAsync(checkBox);
    }

    [AvaloniaTheory(DisplayName = "Background Updates Correctly")]
    [InlineData(255, 255, 0)]    // Yellow
    [InlineData(255, 0, 255)]    // Magenta
    [InlineData(0, 255, 255)]    // Cyan
    public async Task BackgroundUpdatesCorrectly(byte r, byte g, byte b)
    {
        var color = Color.FromRgb(r, g, b);
        var checkBox = new CheckBoxStub
        {
            Background = new SolidPaint(Colors.White)
        };

        var handler = await CreateHandlerAsync(checkBox);

        await InvokeOnMainThreadAsync(() =>
        {
            checkBox.Background = new SolidPaint(color);
            handler.UpdateValue(nameof(ICheckBox.Background));
        });

        var nativeColor = await GetValueAsync(checkBox, GetNativeBackgroundColor);
        
        Assert.NotNull(nativeColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, nativeColor);
    }

    [AvaloniaTheory(DisplayName = "Updating Foreground Does Not Affect IsChecked")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ForegroundDoesNotAffectIsChecked(bool isChecked)
    {
        var checkBox = new CheckBoxStub
        {
            IsChecked = isChecked,
            Foreground = new SolidPaint(Colors.Red)
        };

        await ValidateUnrelatedPropertyUnaffected(
            checkBox,
            GetNativeIsChecked,
            nameof(ICheckBox.Foreground),
            () => checkBox.Foreground = new SolidPaint(Colors.Blue));
    }

    [AvaloniaTheory(DisplayName = "Updating IsChecked Does Not Affect Foreground")]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task IsCheckedDoesNotAffectForeground(bool initialChecked, bool newChecked)
    {
        var checkBox = new CheckBoxStub
        {
            IsChecked = initialChecked,
            Foreground = new SolidPaint(Colors.Red)
        };

        await ValidateUnrelatedPropertyUnaffected(
            checkBox,
            GetNativeForeground,
            nameof(ICheckBox.IsChecked),
            () => checkBox.IsChecked = newChecked);
    }

    [AvaloniaTheory(DisplayName = "Updating Background Does Not Affect IsChecked")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task BackgroundDoesNotAffectIsChecked(bool isChecked)
    {
        var checkBox = new CheckBoxStub
        {
            IsChecked = isChecked,
            Background = new SolidPaint(Colors.White)
        };

        await ValidateUnrelatedPropertyUnaffected(
            checkBox,
            GetNativeIsChecked,
            nameof(ICheckBox.Background),
            () => checkBox.Background = new SolidPaint(Colors.Black));
    }

    [AvaloniaFact(DisplayName = "Handler Connects Event Correctly")]
    public async Task HandlerConnectsEventCorrectly()
    {
        var checkBox = new CheckBoxStub
        {
            IsChecked = false
        };

        var handler = await CreateHandlerAsync(checkBox);
        var platformView = handler.PlatformView as CheckBox;

        Assert.NotNull(platformView);
        
        // Simulate user checking the box through the platform view
        platformView.IsChecked = true;
        
        // The handler should propagate this back to the virtual view
        Assert.True(checkBox.IsChecked);
    }
    
    [AvaloniaFact(DisplayName = "CheckedChanged Event Fires When Checked")]
    public async Task CheckedChangedEventFiresWhenChecked()
    {
        var checkBox = new CheckBoxStub
        {
            IsChecked = false
        };

        bool eventFired = false;
        bool eventValue = false;

        checkBox.CheckedChanged += (sender, e) =>
        {
            eventFired = true;
            eventValue = e.Value;
        };

        var handler = await CreateHandlerAsync(checkBox);

        await InvokeOnMainThreadAsync(() =>
        {
            checkBox.IsChecked = true;
        });

        Assert.True(eventFired, "CheckedChanged event should fire");
        Assert.True(eventValue, "Event value should be true");
    }

    [AvaloniaFact(DisplayName = "CheckedChanged Event Fires When Unchecked")]
    public async Task CheckedChangedEventFiresWhenUnchecked()
    {
        var checkBox = new CheckBoxStub
        {
            IsChecked = true
        };

        bool eventFired = false;
        bool eventValue = true;

        checkBox.CheckedChanged += (sender, e) =>
        {
            eventFired = true;
            eventValue = e.Value;
        };

        var handler = await CreateHandlerAsync(checkBox);

        await InvokeOnMainThreadAsync(() =>
        {
            checkBox.IsChecked = false;
        });

        Assert.True(eventFired, "CheckedChanged event should fire");
        Assert.False(eventValue, "Event value should be false");
    }

    [AvaloniaFact(DisplayName = "Color Initializes Correctly")]
    public async Task ColorInitializesCorrectly()
    {
        var checkBox = new CheckBoxStub
        {
            Color = Colors.Red
        };

        var values = await GetValueAsync(checkBox, (handler) =>
        {
            return new
            {
                ViewValue = checkBox.Color,
                PlatformViewValue = GetNativeForeground(handler)
            };
        });

        Assert.NotNull(values.PlatformViewValue);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, values.PlatformViewValue);
    }

    [AvaloniaTheory(DisplayName = "Color Updates Correctly")]
    [InlineData(255, 0, 0)]      // Red
    [InlineData(0, 255, 0)]      // Green
    [InlineData(0, 0, 255)]      // Blue
    [InlineData(128, 0, 128)]    // Purple
    public async Task ColorUpdatesCorrectly(byte r, byte g, byte b)
    {
        var color = Color.FromRgb(r, g, b);
        var checkBox = new CheckBoxStub
        {
            Color = Colors.Black
        };

        var handler = await CreateHandlerAsync(checkBox);

        await InvokeOnMainThreadAsync(() =>
        {
            checkBox.Color = color;
            handler.UpdateValue("Color"); // Color is on CheckBox class, not ICheckBox interface
        });

        var nativeColor = await GetValueAsync(checkBox, GetNativeForeground);

        Assert.NotNull(nativeColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, nativeColor);
    }

    [AvaloniaFact(DisplayName = "Color Updates BorderBrush")]
    public async Task ColorUpdatesBorderBrush()
    {
        var checkBox = new CheckBoxStub
        {
            Color = Colors.Green
        };

        var values = await GetValueAsync(checkBox, (handler) =>
        {
            return new
            {
                ViewValue = checkBox.Color,
                PlatformBorderBrushValue = GetNativeBorderBrush(handler)
            };
        });

        Assert.NotNull(values.PlatformBorderBrushValue);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Green, values.PlatformBorderBrushValue);
    }
    
    bool GetNativeIsChecked(MauiCheckBoxHandler handler)
    {
        var checkBox = handler.PlatformView;
        return checkBox?.IsChecked ?? false;
    }
    
    Color? GetNativeForeground(MauiCheckBoxHandler handler)
    {
        var checkBox = handler.PlatformView;
        if (checkBox?.Foreground is Media.ISolidColorBrush brush)
        {
            var avaloniaColor = brush.Color;
            return Color.FromRgba(avaloniaColor.R, avaloniaColor.G, avaloniaColor.B, avaloniaColor.A);
        }
        return null;
    }
    
    Color? GetNativeBorderBrush(MauiCheckBoxHandler handler)
    {
        var checkBox = handler.PlatformView;
        if (checkBox?.BorderBrush is Media.ISolidColorBrush brush)
        {
            var avaloniaColor = brush.Color;
            return Color.FromRgba(avaloniaColor.R, avaloniaColor.G, avaloniaColor.B, avaloniaColor.A);
        }
        return null;
    }
    
    Color? GetNativeBackgroundColor(MauiCheckBoxHandler handler)
    {
        var checkBox = handler.PlatformView;
        if (checkBox?.Background is Media.ISolidColorBrush brush)
        {
            var avaloniaColor = brush.Color;
            return Color.FromRgba(avaloniaColor.R, avaloniaColor.G, avaloniaColor.B, avaloniaColor.A);
        }
        return null;
    }
}