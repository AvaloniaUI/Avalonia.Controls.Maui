using Avalonia.Controls.Primitives;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using AvaloniaSlider = global::Avalonia.Controls.Slider;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI ISlider to Avalonia Slider mapping
/// </summary>
public class SliderHandler : ViewHandler<Microsoft.Maui.ISlider, AvaloniaSlider>, ISliderHandler
{
    public static IPropertyMapper<Microsoft.Maui.ISlider, ISliderHandler> Mapper = new PropertyMapper<Microsoft.Maui.ISlider, ISliderHandler>(ViewHandler.ViewMapper)
    {
        [nameof(Microsoft.Maui.ISlider.Maximum)] = MapMaximum,
        [nameof(Microsoft.Maui.ISlider.MaximumTrackColor)] = MapMaximumTrackColor,
        [nameof(Microsoft.Maui.ISlider.Minimum)] = MapMinimum,
        [nameof(Microsoft.Maui.ISlider.MinimumTrackColor)] = MapMinimumTrackColor,
        [nameof(Microsoft.Maui.ISlider.ThumbColor)] = MapThumbColor,
        [nameof(Microsoft.Maui.ISlider.ThumbImageSource)] = MapThumbImageSource,
        [nameof(Microsoft.Maui.ISlider.Value)] = MapValue,
    };

    public static CommandMapper<Microsoft.Maui.ISlider, ISliderHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public SliderHandler() : base(Mapper, CommandMapper)
    {
    }

    public SliderHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public SliderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    Microsoft.Maui.ISlider ISliderHandler.VirtualView => VirtualView;

    System.Object ISliderHandler.PlatformView => PlatformView;

    protected override AvaloniaSlider CreatePlatformView()
    {
        return new AvaloniaSlider();
    }

    public override bool NeedsContainer => false;

    protected override void ConnectHandler(AvaloniaSlider platformView)
    {
        platformView.PropertyChanged += OnSliderPropertyChanged;
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(AvaloniaSlider platformView)
    {
        platformView.PropertyChanged -= OnSliderPropertyChanged;
        base.DisconnectHandler(platformView);
    }

    private void OnSliderPropertyChanged(object? sender, global::Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == AvaloniaSlider.ValueProperty && VirtualView != null)
        {
            VirtualView.Value = (double)(e.NewValue ?? 0);
        }
    }

    public static void MapMinimum(ISliderHandler handler, Microsoft.Maui.ISlider slider)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((AvaloniaSlider)handler.PlatformView).Minimum = slider.Minimum;
    }

    public static void MapMaximum(ISliderHandler handler, Microsoft.Maui.ISlider slider)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((AvaloniaSlider)handler.PlatformView).Maximum = slider.Maximum;
    }

    public static void MapValue(ISliderHandler handler, Microsoft.Maui.ISlider slider)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((AvaloniaSlider)handler.PlatformView).Value = slider.Value;
    }

    public static void MapMinimumTrackColor(ISliderHandler handler, Microsoft.Maui.ISlider slider)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // Note: Avalonia Slider doesn't have separate MinimumTrackColor property out of the box
        // This would require custom styling/templating to implement
        // For now, we can set the Foreground which affects the filled portion
        if (slider.MinimumTrackColor != null)
        {
            ((AvaloniaSlider)handler.PlatformView).Foreground = slider.MinimumTrackColor.ToPlatform();
        }
    }

    public static void MapMaximumTrackColor(ISliderHandler handler, Microsoft.Maui.ISlider slider)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // Note: Avalonia Slider doesn't have separate MaximumTrackColor property out of the box
        // This would require custom styling/templating to implement
        // For now, we can set the Background which affects the unfilled portion
        if (slider.MaximumTrackColor != null)
        {
            ((AvaloniaSlider)handler.PlatformView).Background = slider.MaximumTrackColor.ToPlatform();
        }
    }

    public static void MapThumbColor(ISliderHandler handler, Microsoft.Maui.ISlider slider)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // Note: Avalonia Slider thumb color customization requires template modification
        // This is a limitation that would need custom styling to fully support
        // TODO: Implement custom styling for thumb color
    }

    public static void MapThumbImageSource(ISliderHandler handler, Microsoft.Maui.ISlider slider)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // Note: Custom thumb image would require template modification in Avalonia
        // TODO: Implement custom thumb image support
    }
}
