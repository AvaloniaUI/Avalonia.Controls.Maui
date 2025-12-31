using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Primitives;
using PlatformView = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for Avalonia platform views to support .NET MAUI view property mapping.
/// </summary>
public static class ViewExtensions
{
    /// <summary>
    /// Updates the transformation properties (scale, rotation, translation) of a platform view.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing transformation properties.</param>
    /// <remarks>This method creates a TransformGroup with ScaleTransform, RotateTransform, and TranslateTransform
    /// to handle all transformation properties from the .NET MAUI view.</remarks>
    public static void UpdateTransformation(this PlatformView control, IView view)
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (control.RenderTransform is not global::Avalonia.Media.TransformGroup group)
            {
                group = new global::Avalonia.Media.TransformGroup();
                group.Children.Add(new global::Avalonia.Media.ScaleTransform());
                group.Children.Add(new global::Avalonia.Media.RotateTransform());
                group.Children.Add(new global::Avalonia.Media.TranslateTransform());
                control.RenderTransform = group;
                control.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            }
            if (group.Children[0] is global::Avalonia.Media.ScaleTransform scale)
            {
                double radX = view.RotationX * (Math.PI / 180.0);
                double radY = view.RotationY * (Math.PI / 180.0);
                
                scale.ScaleX = view.ScaleX * view.Scale * Math.Cos(radY);
                scale.ScaleY = view.ScaleY * view.Scale * Math.Cos(radX);
            }
            if (group.Children[1] is global::Avalonia.Media.RotateTransform rotate)
            {
                rotate.Angle = view.Rotation;
            }
            if (group.Children[2] is global::Avalonia.Media.TranslateTransform translate)
            {
                translate.X = view.TranslationX;
                translate.Y = view.TranslationY;
            }
        });
    }

    /// <summary>
    /// Updates the width of a platform view based on the .NET MAUI view's width property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the width property.</param>
    /// <remarks>If the width is NaN, the property is cleared to use default sizing.</remarks>
    public static void UpdateWidth(this PlatformView control, IView view)
    {
        if (!double.IsNaN(view.Width))
            control.Width = view.Width;
        else
            control.ClearValue(PlatformView.WidthProperty);
    }

    /// <summary>
    /// Updates the height of a platform view based on the .NET MAUI view's height property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the height property.</param>
    /// <remarks>If the height is NaN, the property is cleared to use default sizing.</remarks>
    public static void UpdateHeight(this PlatformView control, IView view)
    {
        if (!double.IsNaN(view.Height))
            control.Height = view.Height;
        else
            control.ClearValue(PlatformView.HeightProperty);
    }

    /// <summary>
    /// Updates the minimum height of a platform view based on the .NET MAUI view's minimum height property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The MAUI view containing the minimum height property.</param>
    /// <remarks>If the minimum height is NaN, the property is cleared to use default sizing.</remarks>
    public static void UpdateMinimumHeight(this PlatformView control, IView view)
    {
        if (!double.IsNaN(view.MinimumHeight))
            control.MinHeight = view.MinimumHeight;
        else
            control.ClearValue(PlatformView.MinHeightProperty);
    }

    /// <summary>
    /// Updates the maximum height of a platform view based on the .NET MAUI view's maximum height property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the maximum height property.</param>
    /// <remarks>If the maximum height is positive infinity, the property is cleared to use default sizing.</remarks>
    public static void UpdateMaximumHeight(this PlatformView control, IView view)
    {
        if (!double.IsPositiveInfinity(view.MaximumHeight))
            control.MaxHeight = view.MaximumHeight;
        else
            control.ClearValue(PlatformView.MaxHeightProperty);
    }

    /// <summary>
    /// Updates the minimum width of a platform view based on the .NET MAUI view's minimum width property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the minimum width property.</param>
    /// <remarks>If the minimum width is NaN, the property is cleared to use default sizing.</remarks>
    public static void UpdateMinimumWidth(this PlatformView control, IView view)
    {
        if (!double.IsNaN(view.MinimumWidth))
            control.MinWidth = view.MinimumWidth;
        else
            control.ClearValue(PlatformView.MinWidthProperty);
    }

    /// <summary>
    /// Updates the maximum width of a platform view based on the .NET MAUI view's maximum width property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the maximum width property.</param>
    /// <remarks>If the maximum width is positive infinity, the property is cleared to use default sizing.</remarks>
    public static void UpdateMaximumWidth(this PlatformView control, IView view)
    {
        if (!double.IsPositiveInfinity(view.MaximumWidth))
            control.MaxWidth = view.MaximumWidth;
        else
            control.ClearValue(PlatformView.MaxWidthProperty);
    }

    /// <summary>
    /// Updates the enabled state of a platform view based on the MAUI view's enabled property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the enabled property.</param>
    public static void UpdateIsEnabled(this PlatformView control, IView view)
    {
        control.IsEnabled = view.IsEnabled;
    }

    /// <summary>
    /// Updates the opacity of a platform view based on the .NET MAUI view's opacity property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the opacity property.</param>
    /// <remarks>This method posts the update to the UI thread to ensure thread safety.</remarks>
    public static void UpdateOpacity(this PlatformView control, IView view)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            control.Opacity = view.Opacity;
        });
    }

    /// <summary>
    /// Updates the opacity of a platform view with a specific opacity value.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="opacity">The opacity value to set.</param>
    /// <remarks>This method posts the update to the UI thread to ensure thread safety.</remarks>
    public static void UpdateOpacity(this PlatformView control, double opacity)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            control.Opacity = opacity;
        });
    }

    /// <summary>
    /// Updates the automation ID of a platform view based on the .NET MAUI view's automation ID property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the automation ID property.</param>
    /// <remarks>If the automation ID is null or empty, the property is cleared.</remarks>
    public static void UpdateAutomationId(this PlatformView control, IView view)
    {
        if (!string.IsNullOrEmpty(view.AutomationId))
            global::Avalonia.Automation.AutomationProperties.SetAutomationId(control, view.AutomationId);
        else
            control.ClearValue(global::Avalonia.Automation.AutomationProperties.AutomationIdProperty);
    }

    /// <summary>
    /// Updates the input transparency of a platform view based on the .NET MAUI view's input transparent property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="handler">The view handler for context.</param>
    /// <param name="view">The .NET MAUI view containing the input transparent property.</param>
    /// <remarks>In Avalonia, InputTransparent is mapped to IsHitTestVisible property.</remarks>
    public static void UpdateInputTransparent(this PlatformView control, IViewHandler handler, IView view)
    {
        // In Avalonia, there is no direct equivalent to InputTransparent.
        // However, we can achieve similar behavior by setting IsHitTestVisible.
        control.IsHitTestVisible = !view.InputTransparent;
    }

    /// <summary>
    /// Sets focus to the platform view.
    /// </summary>
    /// <param name="control">The platform view to focus.</param>
    /// <param name="request">The focus request containing focus parameters.</param>
    public static void Focus(this PlatformView control, FocusRequest request)
    {
        control.Focus();
    }

    /// <summary>
    /// Removes focus from the platform view.
    /// </summary>
    /// <param name="control">The platform view to unfocus.</param>
    /// <param name="view">The .NET MAUI view associated with the control.</param>
    /// <remarks>Implementation pending - unfocus logic needs to be added.</remarks>
    public static void Unfocus(this PlatformView control, IView view)
    {
        var topLevel = TopLevel.GetTopLevel(control);
        topLevel?.FocusManager?.ClearFocus();
    }

    /// <summary>
    /// Invalidates the measure of a platform view to trigger layout recalculation.
    /// </summary>
    /// <param name="control">The platform view to invalidate.</param>
    /// <param name="view">The .NET MAUI view associated with the control.</param>
    public static void InvalidateMeasure(this PlatformView control, IView view)
    {
        control.InvalidateMeasure();
    }

    /// <summary>
    /// Updates the visibility of a platform view based on the MAUI view's visibility property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the visibility property.</param>
    /// <remarks>Maps .NET MAUI Visibility enum to Avalonia IsVisible boolean property.</remarks>
    public static void UpdateVisibility(this PlatformView control, IView view)
    {
        switch (view.Visibility)
        {
            case Visibility.Visible:
                control.IsVisible = true;
                break;
            case Visibility.Hidden:
            case Visibility.Collapsed:
                control.IsVisible = false;
                break;
        }
    }

    /// <summary>
    /// Updates the tooltip of a platform view.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="toolTip">The tooltip text to set.</param>
    /// <remarks>If the tooltip is null or empty, the property is cleared.</remarks>
    public static void UpdateToolTip(this PlatformView control, string? toolTip)
    {
        if (!string.IsNullOrEmpty(toolTip))
            ToolTip.SetTip(control, toolTip);
        else
            control.ClearValue(ToolTip.TipProperty);
    }

    /// <summary>
    /// Updates the horizontal layout alignment of a platform view based on the MAUI view's alignment property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the horizontal layout alignment property.</param>
    public static void UpdateHorizontalLayoutAlignment(this PlatformView control, IView view)
    {
        control.HorizontalAlignment = view.HorizontalLayoutAlignment.ToAvaloniaHorizontalAlignment();
    }

    /// <summary>
    /// Updates the vertical layout alignment of a platform view based on the MAUI view's alignment property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The MAUI view containing the vertical layout alignment property.</param>
    public static void UpdateVerticalLayoutAlignment(this PlatformView control, IView view)
    {
        control.VerticalAlignment = view.VerticalLayoutAlignment.ToAvaloniaVerticalAlignment();
    }

    /// <summary>
    /// Updates the accessibility semantics of a platform view based on the MAUI view's semantics property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the semantics property.</param>
    /// <remarks>Sets accessibility properties like description and hint for screen readers.</remarks>
    public static void UpdateSemantics(this PlatformView control, IView view)
    {
        if (view.Semantics?.Description is not null)
            global::Avalonia.Automation.AutomationProperties.SetHelpText(control, view.Semantics.Description);
        else
            control.ClearValue(global::Avalonia.Automation.AutomationProperties.HelpTextProperty);
        if (view.Semantics?.Hint is not null)
            global::Avalonia.Automation.AutomationProperties.SetItemStatus(control, view.Semantics.Hint);
        else
            control.ClearValue(global::Avalonia.Automation.AutomationProperties.ItemStatusProperty);
        if (view.Semantics?.HeadingLevel is not null)
        {
        }
    }

    /// <summary>
    /// Invalidates the clip of a platform view by setting it to null.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    public static void InvalidateClip(this PlatformView control)
    {
        control.Clip = null;
    }

    /// <summary>
    /// Updates the clip geometry of a platform view based on the MAUI view's clip property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the clip property.</param>
    /// <remarks>
    /// <para><b>Supported Clip Geometries:</b></para>
    /// <list type="bullet">
    ///   <item>RectangleGeometry - Sharp rectangular clip</item>
    ///   <item>RoundRectangleGeometry - Rounded corners (uses largest corner radius)</item>
    ///   <item>EllipseGeometry - Elliptical/circular clip</item>
    ///   <item>PathGeometry - Custom path-based clipping via ToPlatform()</item>
    /// </list>
    /// <para><b>Timing Issue (Known Bug):</b></para>
    /// When the window is initially large, controls may not have their bounds calculated yet,
    /// causing clips to not render until a resize occurs. This happens because:
    /// <list type="number">
    ///   <item>GetClipSize() returns 0x0 when bounds aren't available</item>
    ///   <item>Method subscribes to BoundsProperty changes to retry</item>
    ///   <item>Sometimes the bounds change event doesn't fire on initial layout</item>
    ///   <item>Resizing the window triggers a new layout pass, which fires the event</item>
    /// </list>
    /// <para><b>Workaround:</b></para>
    /// The method uses a PropertyChangedSubscription to automatically retry when bounds
    /// become available. If clips don't appear initially, they should render after the
    /// first layout pass or window resize.
    /// </remarks>
    public static void UpdateClip(this PlatformView control, IView view)
    {
        // Clean up any existing subscription before processing new clip
        DisposeClipSubscription(control);

        if (view.Clip is null)
        {
            // No clip defined, clear any existing clip geometry
            control.InvalidateClip();
            return;
        }

        // Attempt to get control dimensions from various sources
        // This tries: control.Bounds → view.Width/Height → view.Frame → control.DesiredSize
        var (width, height) = GetClipSize(control, view);

        if (width <= 0 || height <= 0)
        {
            // TIMING ISSUE: Control doesn't have valid dimensions yet
            // This commonly happens on initial load with wide windows where layout
            // hasn't completed. We subscribe to bounds changes to retry later.

            var subscription = new PropertyChangedSubscription(control, e =>
            {
                // Only react to bounds changes (ignore other properties)
                if (e.Property != global::Avalonia.Visual.BoundsProperty)
                    return;

                // Check if we now have valid dimensions
                var (currentWidth, currentHeight) = GetClipSize(control, view);
                if (currentWidth > 0 && currentHeight > 0)
                {
                    // Success! Clean up subscription and apply the clip
                    DisposeClipSubscription(control);
                    control.UpdateClip(view);
                }
                // If still invalid, keep subscription active for next bounds change
            });

            // Store subscription in weak table to prevent memory leaks
            SetClipSubscription(control, subscription);

            // POTENTIAL FIX: Force a layout pass to ensure bounds get calculated
            // This should trigger the BoundsProperty change we're subscribed to above
            // Note: InvalidateMeasure may not immediately solve the issue if the parent
            // container hasn't been measured yet, but it helps in many cases
            control.InvalidateMeasure();

            return;
        }

        // At this point we have valid dimensions, apply the appropriate clip geometry
        // Try to use ToPlatform() extension method which properly handles:
        // - EllipseGeometry (with Center/RadiusX/RadiusY properties)
        // - RectangleGeometry (with Rect property)
        // - RoundRectangleGeometry (with Rect + CornerRadius properties)
        // - PathGeometry, LineGeometry, GeometryGroup
        var avaloniaGeometry = (view.Clip as Geometry)?.ToPlatform();

        if (avaloniaGeometry is not null)
        {
            // Successfully converted using geometry's explicit properties
            control.Clip = avaloniaGeometry;
        }
        else if (view.Clip is RoundRectangleGeometry roundRect)
        {
            // RoundRectangleGeometry without explicit Rect - use container bounds
            // This happens when XAML has <RoundRectangleGeometry CornerRadius="32" />
            // without a Rect property, meaning it should clip to the container's bounds
            var radius = Math.Max(
                roundRect.CornerRadius.TopLeft,
                Math.Max(
                    roundRect.CornerRadius.TopRight,
                    Math.Max(roundRect.CornerRadius.BottomLeft,
                             roundRect.CornerRadius.BottomRight)));

            control.Clip = new global::Avalonia.Media.RectangleGeometry(
                new Rect(0, 0, width, height),
                radius,
                radius);
        }
        else
        {
            // Conversion failed or geometry type not supported, clear clip
            control.InvalidateClip();
        }
    }

    /// <summary>
    /// Gets the clip size for a platform view based on various size sources.
    /// </summary>
    /// <param name="control">The platform view.</param>
    /// <param name="view">The .NET MAUI view.</param>
    /// <returns>A tuple containing width and height for clipping.</returns>
    /// <remarks>
    /// <para><b>Size Source Priority (fallback chain):</b></para>
    /// <list type="number">
    ///   <item><b>control.Bounds</b> - Most reliable, from Avalonia's layout system</item>
    ///   <item><b>view.Width/Height</b> - Explicit MAUI size properties</item>
    ///   <item><b>view.Frame</b> - MAUI's calculated frame rectangle</item>
    ///   <item><b>control.DesiredSize</b> - Avalonia's measure pass result</item>
    /// </list>
    /// <para><b>Why Multiple Sources?</b></para>
    /// Different layout phases provide size information at different times:
    /// <list type="bullet">
    ///   <item>Early: DesiredSize available after measure pass</item>
    ///   <item>Middle: Width/Height if explicitly set in XAML/code</item>
    ///   <item>Late: Bounds available after arrange pass (most reliable)</item>
    /// </list>
    /// <para><b>Common Failure Scenario:</b></para>
    /// On initial load with wide windows, none of these sources may have valid values yet,
    /// resulting in 0x0 size. This triggers the retry subscription mechanism in UpdateClip.
    /// </remarks>
    static (double width, double height) GetClipSize(PlatformView control, IView view)
    {
        // Start with Avalonia's layout-calculated bounds (most reliable when available)
        double width = control.Bounds.Width;
        double height = control.Bounds.Height;

        // Fallback 1: Try explicit MAUI Width/Height properties
        if ((double.IsNaN(width) || width <= 0) && !double.IsNaN(view.Width) && view.Width > 0)
            width = view.Width;
        if ((double.IsNaN(height) || height <= 0) && !double.IsNaN(view.Height) && view.Height > 0)
            height = view.Height;

        // Fallback 2: Try MAUI's Frame (calculated position and size)
        if ((double.IsNaN(width) || width <= 0) && view.Frame.Width > 0)
            width = view.Frame.Width;
        if ((double.IsNaN(height) || height <= 0) && view.Frame.Height > 0)
            height = view.Frame.Height;

        // Fallback 3: Try Avalonia's DesiredSize (from measure pass)
        if ((double.IsNaN(width) || width <= 0) && control.DesiredSize.Width > 0)
            width = control.DesiredSize.Width;
        if ((double.IsNaN(height) || height <= 0) && control.DesiredSize.Height > 0)
            height = control.DesiredSize.Height;

        return (width, height);
    }

    static readonly ConditionalWeakTable<PlatformView, IDisposable> ClipSubscriptions = new();

    /// <summary>
    /// Sets a clip subscription for a platform view.
    /// </summary>
    /// <param name="control">The platform view.</param>
    /// <param name="subscription">The subscription to set.</param>
    static void SetClipSubscription(PlatformView control, IDisposable subscription)
    {
        DisposeClipSubscription(control);
        ClipSubscriptions.Add(control, subscription);
    }

    /// <summary>
    /// Disposes the clip subscription for a platform view.
    /// </summary>
    /// <param name="control">The platform view.</param>
    static void DisposeClipSubscription(PlatformView control)
    {
        if (ClipSubscriptions.TryGetValue(control, out var disposable))
        {
            disposable.Dispose();
            ClipSubscriptions.Remove(control);
        }
    }

    /// <summary>
    /// Handles property changed subscriptions for clip retry logic.
    /// </summary>
    sealed class PropertyChangedSubscription : IDisposable
    {
        readonly PlatformView _control;
        readonly Action<AvaloniaPropertyChangedEventArgs> _onChanged;

        public PropertyChangedSubscription(PlatformView control, Action<AvaloniaPropertyChangedEventArgs> onChanged)
        {
            _control = control;
            _onChanged = onChanged;
            _control.PropertyChanged += OnPropertyChanged;
        }

        void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) =>
            _onChanged(e);
        
        public void Dispose() =>
            _control.PropertyChanged -= OnPropertyChanged;
    }

    /// <summary>
    /// Updates the shadow effect of a platform view based on the MAUI view's shadow property.
    /// </summary>
    /// <param name="control">The platform view to apply the shadow to.</param>
    /// <param name="view">The .NET MAUI view containing shadow configuration.</param>
    /// <remarks>
    /// <para><b>How it works:</b></para>
    /// <list type="number">
    ///   <item>Converts MAUI's IShadow to Avalonia's DropShadowEffect via ToAvalonia() extension</item>
    ///   <item>Maps shadow properties:
    ///     <list type="bullet">
    ///       <item>Shadow.Paint.Color → DropShadowEffect.Color (with opacity applied)</item>
    ///       <item>Shadow.Offset.X/Y → DropShadowEffect.OffsetX/OffsetY</item>
    ///       <item>Shadow.Radius → DropShadowEffect.BlurRadius</item>
    ///     </list>
    ///   </item>
    ///   <item>If shadow is null, clears the effect to remove any existing shadow</item>
    ///   <item>Applies the effect to control.Effect property (GPU-accelerated rendering)</item>
    /// </list>
    /// <para><b>Implementation details:</b></para>
    /// <list type="bullet">
    ///   <item>Uses Avalonia's IEffect system for efficient shadow rendering</item>
    ///   <item>DropShadowEffect is GPU-accelerated when possible</item>
    ///   <item>Shadow opacity is combined with paint color alpha channel</item>
    ///   <item>Only SolidPaint is supported; gradient shadows default to black</item>
    /// </list>
    /// <para><b>Performance notes:</b></para>
    /// <list type="bullet">
    ///   <item>Shadows can impact rendering performance, especially with large blur radius</item>
    ///   <item>Effect is applied to the container view when NeedsContainer is true</item>
    ///   <item>Changes are immediate; no animation or transitions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// MAUI XAML:
    /// <code>
    /// &lt;BoxView BackgroundColor="Blue"&gt;
    ///   &lt;BoxView.Shadow&gt;
    ///     &lt;Shadow Brush="Black" Opacity="0.8" Radius="10" Offset="5,5" /&gt;
    ///   &lt;/BoxView.Shadow&gt;
    /// &lt;/BoxView&gt;
    /// </code>
    /// This produces a soft black shadow offset 5px right and 5px down with 10px blur.
    /// </example>
    public static void UpdateShadow(this PlatformView control, IView view)
    {
        // Convert MAUI shadow to Avalonia DropShadowEffect
        // This handles color conversion, offset mapping, and blur radius
        var shadow = view.Shadow.ToPlatform();

        if (shadow is null)
        {
            // Clear any existing shadow effect when MAUI shadow is removed
            control.Effect = null;
            return;
        }

        // Apply the DropShadowEffect to the control
        // Avalonia will render this as a GPU-accelerated drop shadow
        control.Effect = shadow;
    }

    /// <summary>
    /// Updates the background of a platform view based on the .NET MAUI view's background property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the background property.</param>
    /// <remarks>Handles both Panel and TemplatedControl types for background setting.</remarks>
    public static void UpdateBackground(this PlatformView control, IView view)
    {
        if (control is Panel panel)
        {
            // Background is handled by specific control types (Panel, TextBlock, etc.)
            // For Panel-based controls (like ContentView), set the background
            if (view.Background != null)
            {
                panel.Background = view.Background.ToPlatform();
            }
            else
            {
                panel.ClearValue(Panel.BackgroundProperty);
            }
        }
        else if (control is TemplatedControl templatedControl)
        {
            if (view.Background != null)
            {
                templatedControl.Background = view.Background.ToPlatform();
            }
            else
            {
                templatedControl.ClearValue(TemplatedControl.BackgroundProperty);
            }
        }
    }

    /// <summary>
    /// Updates the background image of a platform view asynchronously.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="imageSource">The image source to set as background.</param>
    /// <param name="provider">The image source service provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>Implementation pending - image background logic needs to be added.</remarks>
    [NotImplemented("Pending to implement image background logic.")]
    public static Task UpdateBackgroundImageSourceAsync(this PlatformView control, IImageSource? imageSource, IImageSourceServiceProvider provider)
    {
        // TODO: Implement image background logic
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the border of a platform view based on the MAUI view's border properties.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing border properties.</param>
    /// <remarks>Implementation pending - border logic needs to be added.</remarks>
    [NotImplemented("Type or member is obsolete.")]
    public static void UpdateBorder(this PlatformView control, IView view)
    {
        // Type or member is obsolete.
    }

    /// <summary>
    /// Updates the flow direction of a platform view based on the MAUI view's flow direction property.
    /// </summary>
    /// <param name="control">The platform view to update.</param>
    /// <param name="view">The .NET MAUI view containing the flow direction property.</param>
    /// <remarks>Maps .NET MAUI FlowDirection enum to Avalonia FlowDirection enum.</remarks>
    public static void UpdateFlowDirection(this PlatformView control, IView view)
    {
        switch (view.FlowDirection)
        {
            case FlowDirection.MatchParent:
            case FlowDirection.LeftToRight:
                control.FlowDirection = Media.FlowDirection.LeftToRight;
                break;
            case FlowDirection.RightToLeft:
                control.FlowDirection = Media.FlowDirection.RightToLeft;
                break;
        }
    }
}