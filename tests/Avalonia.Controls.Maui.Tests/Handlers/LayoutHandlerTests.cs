using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Headless.XUnit;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiLayoutHandler = Avalonia.Controls.Maui.Handlers.LayoutHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class LayoutHandlerTests : HandlerTestBase<MauiLayoutHandler, LayoutStub>
{
    [AvaloniaFact(DisplayName = "Background Initializes Correctly")]
    public async Task BackgroundInitializesCorrectly()
    {
        var color = Colors.Blue;
        var layout = new LayoutStub
        {
            Background = new SolidPaint(color)
        };

        var platformColor = await GetValueAsync(layout, GetNativeBackgroundColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    [AvaloniaFact(DisplayName = "Background Null Sets Transparent")]
    public async Task BackgroundNullSetsTransparent()
    {
        var layout = new LayoutStub
        {
            Background = null
        };

        var platformBackground = await GetValueAsync(layout, handler =>
        {
            if (handler.PlatformView is Panel panel)
            {
                return panel.Background;
            }
            return null;
        });

        // Should be transparent brush when null
        Assert.NotNull(platformBackground);
        Assert.Equal(Avalonia.Media.Brushes.Transparent, platformBackground);
    }

    [AvaloniaTheory(DisplayName = "ClipsToBounds Initializes Correctly")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ClipsToBoundsInitializesCorrectly(bool clipsToBounds)
    {
        var layout = new LayoutStub
        {
            ClipsToBounds = clipsToBounds
        };

        await ValidatePropertyInitValue(
            layout,
            () => layout.ClipsToBounds,
            GetNativeClipsToBounds,
            clipsToBounds);
    }

    [AvaloniaTheory(DisplayName = "ClipsToBounds Updates Correctly")]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ClipsToBoundsUpdatesCorrectly(bool initial, bool updated)
    {
        var layout = new LayoutStub
        {
            ClipsToBounds = initial
        };

        await ValidatePropertyUpdatesValue(
            layout,
            nameof(ILayout.ClipsToBounds),
            GetNativeClipsToBounds,
            updated,
            initial);
    }

    [AvaloniaFact(DisplayName = "Platform View Is LayoutPanel")]
    public async Task PlatformViewIsLayoutPanel()
    {
        var layout = new LayoutStub();

        var platformViewType = await GetValueAsync(layout, handler =>
        {
            return handler.PlatformView?.GetType();
        });

        Assert.NotNull(platformViewType);
        Assert.Equal(typeof(LayoutPanel), platformViewType);
    }

    [AvaloniaFact(DisplayName = "SetVirtualView Populates Children")]
    public async Task SetVirtualViewPopulatesChildren()
    {
        var layout = new LayoutStub();
        var child1 = new BoxViewStub { Color = Colors.Red };
        var child2 = new BoxViewStub { Color = Colors.Green };

        // Pre-populate children before handler is created
        layout.Add(child1);
        layout.Add(child2);

        var childCount = await GetValueAsync(layout, handler =>
        {
            if (handler.PlatformView is Panel panel)
            {
                return panel.Children.Count;
            }
            return -1;
        });

        // After SetVirtualView, all children should be added
        Assert.Equal(2, childCount);
    }

    [AvaloniaFact(DisplayName = "CrossPlatformMeasure Is Assigned")]
    public async Task CrossPlatformMeasureIsAssigned()
    {
        var layout = new LayoutStub();

        var hasDelegate = await GetValueAsync(layout, handler =>
        {
            if (handler.PlatformView is LayoutPanel layoutPanel)
            {
                return layoutPanel.CrossPlatformMeasure != null;
            }
            return false;
        });

        Assert.True(hasDelegate);
    }

    [AvaloniaFact(DisplayName = "CrossPlatformArrange Is Assigned")]
    public async Task CrossPlatformArrangeIsAssigned()
    {
        var layout = new LayoutStub();

        var hasDelegate = await GetValueAsync(layout, handler =>
        {
            if (handler.PlatformView is LayoutPanel layoutPanel)
            {
                return layoutPanel.CrossPlatformArrange != null;
            }
            return false;
        });

        Assert.True(hasDelegate);
    }

    [AvaloniaFact(DisplayName = "Empty Layout Has Zero Children")]
    public async Task EmptyLayoutHasZeroChildren()
    {
        var layout = new LayoutStub();

        var childCount = await GetValueAsync(layout, handler =>
        {
            if (handler.PlatformView is Panel panel)
            {
                return panel.Children.Count;
            }
            return -1;
        });

        Assert.Equal(0, childCount);
    }

    [AvaloniaFact(DisplayName = "Handler VirtualView Returns Correct Instance")]
    public async Task HandlerVirtualViewReturnsCorrectInstance()
    {
        var layout = new LayoutStub();

        var isSameInstance = await GetValueAsync(layout, handler =>
        {
            return object.ReferenceEquals(handler.VirtualView, layout);
        });

        Assert.True(isSameInstance);
    }

    [AvaloniaFact(DisplayName = "Handler PlatformView Is Not Null")]
    public async Task HandlerPlatformViewIsNotNull()
    {
        var layout = new LayoutStub();

        var isNotNull = await GetValueAsync(layout, handler =>
        {
            return handler.PlatformView != null;
        });

        Assert.True(isNotNull);
    }

    [AvaloniaFact(DisplayName = "Padding Initializes Correctly")]
    public async Task PaddingInitializesCorrectly()
    {
        var padding = new Microsoft.Maui.Thickness(10, 20, 30, 40);
        var layout = new LayoutStub
        {
            Padding = padding
        };

        var platformPadding = await GetValueAsync(layout, handler =>
        {
            return layout.Padding;
        });

        Assert.Equal(padding, platformPadding);
    }

    [AvaloniaFact(DisplayName = "IgnoreSafeArea Initializes Correctly")]
    public async Task IgnoreSafeAreaInitializesCorrectly()
    {
        var layout = new LayoutStub
        {
            IgnoreSafeArea = true
        };

        var ignoreSafeArea = await GetValueAsync(layout, handler =>
        {
            return layout.IgnoreSafeArea;
        });

        Assert.True(ignoreSafeArea);
    }

    [AvaloniaFact(DisplayName = "InputTransparent Initializes Correctly")]
    public async Task InputTransparentInitializesCorrectly()
    {
        var layout = new LayoutStub
        {
            InputTransparent = true
        };

        var inputTransparent = await GetValueAsync(layout, handler =>
        {
            return layout.InputTransparent;
        });

        Assert.True(inputTransparent);
    }

    [AvaloniaFact(DisplayName = "Layout Count Reflects Children")]
    public async Task LayoutCountReflectsChildren()
    {
        var layout = new LayoutStub();
        var child1 = new BoxViewStub { Color = Colors.Red };
        var child2 = new BoxViewStub { Color = Colors.Green };
        var child3 = new BoxViewStub { Color = Colors.Blue };

        layout.Add(child1);
        layout.Add(child2);
        layout.Add(child3);

        var (layoutCount, panelCount) = await GetValueAsync(layout, handler =>
        {
            var lc = layout.Count;
            var pc = handler.PlatformView is Panel panel ? panel.Children.Count : -1;
            return (lc, pc);
        });

        Assert.Equal(3, layoutCount);
        Assert.Equal(3, panelCount);
    }

    [AvaloniaFact(DisplayName = "Layout Contains Child")]
    public async Task LayoutContainsChild()
    {
        var layout = new LayoutStub();
        var child = new BoxViewStub { Color = Colors.Red };

        layout.Add(child);

        var contains = await GetValueAsync(layout, handler =>
        {
            return layout.Contains(child);
        });

        Assert.True(contains);
    }

    [AvaloniaFact(DisplayName = "Layout IndexOf Returns Correct Index")]
    public async Task LayoutIndexOfReturnsCorrectIndex()
    {
        var layout = new LayoutStub();
        var child1 = new BoxViewStub { Color = Colors.Red };
        var child2 = new BoxViewStub { Color = Colors.Green };
        var child3 = new BoxViewStub { Color = Colors.Blue };

        layout.Add(child1);
        layout.Add(child2);
        layout.Add(child3);

        var (index1, index2, index3) = await GetValueAsync(layout, handler =>
        {
            return (layout.IndexOf(child1), layout.IndexOf(child2), layout.IndexOf(child3));
        });

        Assert.Equal(0, index1);
        Assert.Equal(1, index2);
        Assert.Equal(2, index3);
    }

    [AvaloniaFact(DisplayName = "Layout Indexer Returns Correct Child")]
    public async Task LayoutIndexerReturnsCorrectChild()
    {
        var layout = new LayoutStub();
        var child1 = new BoxViewStub { Color = Colors.Red };
        var child2 = new BoxViewStub { Color = Colors.Green };

        layout.Add(child1);
        layout.Add(child2);

        var (first, second) = await GetValueAsync(layout, handler =>
        {
            return (layout[0], layout[1]);
        });

        Assert.Same(child1, first);
        Assert.Same(child2, second);
    }

    #region Edge Cases

    [AvaloniaFact(DisplayName = "Layout With Single Child Works")]
    public async Task LayoutWithSingleChildWorks()
    {
        var layout = new LayoutStub();
        var child = new BoxViewStub { Color = Colors.Red };
        layout.Add(child);

        var (count, panelCount) = await GetValueAsync(layout, handler =>
        {
            var lc = layout.Count;
            var pc = handler.PlatformView is Panel panel ? panel.Children.Count : -1;
            return (lc, pc);
        });

        Assert.Equal(1, count);
        Assert.Equal(1, panelCount);
    }

    [AvaloniaFact(DisplayName = "Layout With Many Children Works")]
    public async Task LayoutWithManyChildrenWorks()
    {
        var layout = new LayoutStub();
        const int childCount = 50;

        for (int i = 0; i < childCount; i++)
        {
            layout.Add(new BoxViewStub { Color = new Color(i / 255f, 0, 0) });
        }

        var (layoutCount, panelChildCount) = await GetValueAsync(layout, handler =>
        {
            var lc = layout.Count;
            var pc = handler.PlatformView is Panel panel ? panel.Children.Count : -1;
            return (lc, pc);
        });

        Assert.Equal(childCount, layoutCount);
        Assert.Equal(childCount, panelChildCount);
    }

    [AvaloniaFact(DisplayName = "IndexOf Returns Negative One For Missing Child")]
    public async Task IndexOfReturnsNegativeOneForMissingChild()
    {
        var layout = new LayoutStub();
        var child = new BoxViewStub { Color = Colors.Red };
        var missingChild = new BoxViewStub { Color = Colors.Blue };

        layout.Add(child);

        var index = await GetValueAsync(layout, handler =>
        {
            return layout.IndexOf(missingChild);
        });

        Assert.Equal(-1, index);
    }

    [AvaloniaFact(DisplayName = "Contains Returns False For Missing Child")]
    public async Task ContainsReturnsFalseForMissingChild()
    {
        var layout = new LayoutStub();
        var child = new BoxViewStub { Color = Colors.Red };
        var missingChild = new BoxViewStub { Color = Colors.Blue };

        layout.Add(child);

        var contains = await GetValueAsync(layout, handler =>
        {
            return layout.Contains(missingChild);
        });

        Assert.False(contains);
    }

    [AvaloniaFact(DisplayName = "Layout Range Accessor Works")]
    public async Task LayoutRangeAccessorWorks()
    {
        var layout = new LayoutStub();
        var child1 = new BoxViewStub { Color = Colors.Red };
        var child2 = new BoxViewStub { Color = Colors.Green };
        var child3 = new BoxViewStub { Color = Colors.Blue };
        var child4 = new BoxViewStub { Color = Colors.Yellow };

        layout.Add(child1);
        layout.Add(child2);
        layout.Add(child3);
        layout.Add(child4);

        var range = await GetValueAsync(layout, handler =>
        {
            return layout[1..3];
        });

        Assert.Equal(2, range.Count);
        Assert.Same(child2, range[0]);
        Assert.Same(child3, range[1]);
    }

    [AvaloniaFact(DisplayName = "Layout Clear Removes All Children")]
    public async Task LayoutClearRemovesAllChildren()
    {
        var layout = new LayoutStub();
        layout.Add(new BoxViewStub { Color = Colors.Red });
        layout.Add(new BoxViewStub { Color = Colors.Green });
        layout.Add(new BoxViewStub { Color = Colors.Blue });

        layout.Clear();

        var (count, isEmpty) = await GetValueAsync(layout, handler =>
        {
            return (layout.Count, layout.Count == 0);
        });

        Assert.Equal(0, count);
        Assert.True(isEmpty);
    }

    [AvaloniaFact(DisplayName = "Layout Remove Returns True When Child Exists")]
    public async Task LayoutRemoveReturnsTrueWhenChildExists()
    {
        var layout = new LayoutStub();
        var child = new BoxViewStub { Color = Colors.Red };
        layout.Add(child);

        var result = await GetValueAsync(layout, handler =>
        {
            return layout.Remove(child);
        });

        Assert.True(result);
    }

    [AvaloniaFact(DisplayName = "Layout Remove Returns False When Child Missing")]
    public async Task LayoutRemoveReturnsFalseWhenChildMissing()
    {
        var layout = new LayoutStub();
        var child = new BoxViewStub { Color = Colors.Red };
        var missingChild = new BoxViewStub { Color = Colors.Blue };
        layout.Add(child);

        var result = await GetValueAsync(layout, handler =>
        {
            return layout.Remove(missingChild);
        });

        Assert.False(result);
    }

    [AvaloniaFact(DisplayName = "Layout RemoveAt Updates Count")]
    public async Task LayoutRemoveAtUpdatesCount()
    {
        var layout = new LayoutStub();
        layout.Add(new BoxViewStub { Color = Colors.Red });
        layout.Add(new BoxViewStub { Color = Colors.Green });
        layout.Add(new BoxViewStub { Color = Colors.Blue });

        layout.RemoveAt(1);

        var count = await GetValueAsync(layout, handler =>
        {
            return layout.Count;
        });

        Assert.Equal(2, count);
    }

    [AvaloniaFact(DisplayName = "Layout Insert At Beginning Works")]
    public async Task LayoutInsertAtBeginningWorks()
    {
        var layout = new LayoutStub();
        var child1 = new BoxViewStub { Color = Colors.Red };
        var child2 = new BoxViewStub { Color = Colors.Green };
        var newChild = new BoxViewStub { Color = Colors.Blue };

        layout.Add(child1);
        layout.Add(child2);
        layout.Insert(0, newChild);

        var (first, count) = await GetValueAsync(layout, handler =>
        {
            return (layout[0], layout.Count);
        });

        Assert.Same(newChild, first);
        Assert.Equal(3, count);
    }

    [AvaloniaFact(DisplayName = "Layout Insert At End Works")]
    public async Task LayoutInsertAtEndWorks()
    {
        var layout = new LayoutStub();
        var child1 = new BoxViewStub { Color = Colors.Red };
        var child2 = new BoxViewStub { Color = Colors.Green };
        var newChild = new BoxViewStub { Color = Colors.Blue };

        layout.Add(child1);
        layout.Add(child2);
        layout.Insert(2, newChild);

        var (last, count) = await GetValueAsync(layout, handler =>
        {
            return (layout[2], layout.Count);
        });

        Assert.Same(newChild, last);
        Assert.Equal(3, count);
    }

    #endregion

    #region Background Color Tests

    [AvaloniaTheory(DisplayName = "Various Background Colors Initialize Correctly")]
    [InlineData(255, 0, 0)]   // Red
    [InlineData(0, 255, 0)]   // Green
    [InlineData(0, 0, 255)]   // Blue
    [InlineData(255, 255, 0)] // Yellow
    [InlineData(128, 128, 128)] // Gray
    public async Task VariousBackgroundColorsInitializeCorrectly(byte r, byte g, byte b)
    {
        var color = new Color(r / 255f, g / 255f, b / 255f);
        var layout = new LayoutStub
        {
            Background = new SolidPaint(color)
        };

        var platformColor = await GetValueAsync(layout, GetNativeBackgroundColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    [AvaloniaFact(DisplayName = "Background With Alpha Initializes Correctly")]
    public async Task BackgroundWithAlphaInitializesCorrectly()
    {
        var color = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red
        var layout = new LayoutStub
        {
            Background = new SolidPaint(color)
        };

        var platformColor = await GetValueAsync(layout, GetNativeBackgroundColor);

        Assert.NotNull(platformColor);
        Assert.Equal(color.Alpha, platformColor.Alpha, 2);
    }

    #endregion

    #region Padding Tests

    [AvaloniaTheory(DisplayName = "Uniform Padding Initializes Correctly")]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(50)]
    public async Task UniformPaddingInitializesCorrectly(double padding)
    {
        var uniformPadding = new Microsoft.Maui.Thickness(padding);
        var layout = new LayoutStub
        {
            Padding = uniformPadding
        };

        var result = await GetValueAsync(layout, handler =>
        {
            return layout.Padding;
        });

        Assert.Equal(padding, result.Left);
        Assert.Equal(padding, result.Top);
        Assert.Equal(padding, result.Right);
        Assert.Equal(padding, result.Bottom);
    }

    [AvaloniaFact(DisplayName = "Asymmetric Padding Initializes Correctly")]
    public async Task AsymmetricPaddingInitializesCorrectly()
    {
        var padding = new Microsoft.Maui.Thickness(5, 10, 15, 20);
        var layout = new LayoutStub
        {
            Padding = padding
        };

        var result = await GetValueAsync(layout, handler =>
        {
            return layout.Padding;
        });

        Assert.Equal(5, result.Left);
        Assert.Equal(10, result.Top);
        Assert.Equal(15, result.Right);
        Assert.Equal(20, result.Bottom);
    }

    [AvaloniaFact(DisplayName = "Horizontal Vertical Padding Initializes Correctly")]
    public async Task HorizontalVerticalPaddingInitializesCorrectly()
    {
        var padding = new Microsoft.Maui.Thickness(10, 20); // horizontal=10, vertical=20
        var layout = new LayoutStub
        {
            Padding = padding
        };

        var result = await GetValueAsync(layout, handler =>
        {
            return layout.Padding;
        });

        Assert.Equal(10, result.Left);
        Assert.Equal(20, result.Top);
        Assert.Equal(10, result.Right);
        Assert.Equal(20, result.Bottom);
    }

    #endregion

    #region Handler Lifecycle Tests

    [AvaloniaFact(DisplayName = "Handler Can Be Created Multiple Times")]
    public async Task HandlerCanBeCreatedMultipleTimes()
    {
        var layout = new LayoutStub { Background = new SolidPaint(Colors.Red) };

        var (handler1Type, handler2Type) = await InvokeOnMainThreadAsync(() =>
        {
            var h1 = CreateHandler<MauiLayoutHandler>(layout);
            var t1 = h1.PlatformView?.GetType();

            // Disconnect and create new handler
            var h2 = CreateHandler<MauiLayoutHandler>(layout);
            var t2 = h2.PlatformView?.GetType();

            return (t1, t2);
        });

        Assert.Equal(typeof(LayoutPanel), handler1Type);
        Assert.Equal(typeof(LayoutPanel), handler2Type);
    }

    [AvaloniaFact(DisplayName = "Children Are Synced On SetVirtualView")]
    public async Task ChildrenAreSyncedOnSetVirtualView()
    {
        var layout = new LayoutStub();

        // Add children before handler
        layout.Add(new BoxViewStub { Color = Colors.Red });
        layout.Add(new BoxViewStub { Color = Colors.Green });

        var panelCount = await GetValueAsync(layout, handler =>
        {
            if (handler.PlatformView is Panel panel)
            {
                return panel.Children.Count;
            }
            return -1;
        });

        Assert.Equal(2, panelCount);
    }

    #endregion

    #region CopyTo Tests

    [AvaloniaFact(DisplayName = "CopyTo Copies All Children")]
    public async Task CopyToCopiesAllChildren()
    {
        var layout = new LayoutStub();
        var child1 = new BoxViewStub { Color = Colors.Red };
        var child2 = new BoxViewStub { Color = Colors.Green };
        var child3 = new BoxViewStub { Color = Colors.Blue };

        layout.Add(child1);
        layout.Add(child2);
        layout.Add(child3);

        var array = new IView[3];

        var result = await GetValueAsync(layout, handler =>
        {
            layout.CopyTo(array, 0);
            return array;
        });

        Assert.Same(child1, result[0]);
        Assert.Same(child2, result[1]);
        Assert.Same(child3, result[2]);
    }

    [AvaloniaFact(DisplayName = "CopyTo With Offset Works")]
    public async Task CopyToWithOffsetWorks()
    {
        var layout = new LayoutStub();
        var child1 = new BoxViewStub { Color = Colors.Red };
        var child2 = new BoxViewStub { Color = Colors.Green };

        layout.Add(child1);
        layout.Add(child2);

        var array = new IView[5];

        var result = await GetValueAsync(layout, handler =>
        {
            layout.CopyTo(array, 2);
            return array;
        });

        Assert.Null(result[0]);
        Assert.Null(result[1]);
        Assert.Same(child1, result[2]);
        Assert.Same(child2, result[3]);
        Assert.Null(result[4]);
    }

    #endregion

    #region Enumeration Tests

    [AvaloniaFact(DisplayName = "GetEnumerator Enumerates All Children")]
    public async Task GetEnumeratorEnumeratesAllChildren()
    {
        var layout = new LayoutStub();
        var child1 = new BoxViewStub { Color = Colors.Red };
        var child2 = new BoxViewStub { Color = Colors.Green };
        var child3 = new BoxViewStub { Color = Colors.Blue };

        layout.Add(child1);
        layout.Add(child2);
        layout.Add(child3);

        var items = await GetValueAsync(layout, handler =>
        {
            var list = new System.Collections.Generic.List<IView>();
            foreach (var item in layout)
            {
                list.Add(item);
            }
            return list;
        });

        Assert.Equal(3, items.Count);
        Assert.Same(child1, items[0]);
        Assert.Same(child2, items[1]);
        Assert.Same(child3, items[2]);
    }

    #endregion

    #region IsReadOnly Tests

    [AvaloniaFact(DisplayName = "IsReadOnly Returns False")]
    public async Task IsReadOnlyReturnsFalse()
    {
        var layout = new LayoutStub();

        var isReadOnly = await GetValueAsync(layout, handler =>
        {
            return layout.IsReadOnly;
        });

        Assert.False(isReadOnly);
    }

    #endregion

    // Platform-specific helper methods
    Color? GetNativeBackgroundColor(MauiLayoutHandler handler) =>
        AvaloniaPropertyHelpers.GetNativeBackgroundColor(handler);

    bool GetNativeClipsToBounds(MauiLayoutHandler handler)
    {
        if (handler.PlatformView is Panel panel)
        {
            return panel.ClipToBounds;
        }
        return false;
    }
}

