using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Slider;

namespace Avalonia.Controls.Maui.Handlers;

public class SliderHandler : ViewHandler<ISlider, PlatformView>, ISliderHandler
{
    public static IPropertyMapper<ISlider, ISliderHandler> Mapper = new PropertyMapper<ISlider, ISliderHandler>(ViewHandler.ViewMapper)
    {
        [nameof(ISlider.Maximum)] = MapMaximum,
        [nameof(ISlider.MaximumTrackColor)] = MapMaximumTrackColor,
        [nameof(ISlider.Minimum)] = MapMinimum,
        [nameof(ISlider.MinimumTrackColor)] = MapMinimumTrackColor,
        [nameof(ISlider.ThumbColor)] = MapThumbColor,
        [nameof(ISlider.ThumbImageSource)] = MapThumbImageSource,
        [nameof(ISlider.Value)] = MapValue,
    };

    public static CommandMapper<ISlider, ISliderHandler> CommandMapper = new(ViewCommandMapper)
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

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    public override bool NeedsContainer => false;

    protected override void ConnectHandler(PlatformView platformView)
    {
        platformView.PropertyChanged += OnSliderPropertyChanged;
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.PropertyChanged -= OnSliderPropertyChanged;
        base.DisconnectHandler(platformView);
    }

    public static void MapMinimum(ISliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMinimum(slider);
        }
    }

    public static void MapMaximum(ISliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMaximum(slider);
        }
    }

    public static void MapValue(ISliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateValue(slider);
        }
    }

    public static void MapMinimumTrackColor(ISliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMinimumTrackColor(slider);
        }
    }

    public static void MapMaximumTrackColor(ISliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMaximumTrackColor(slider);
        }
    }

    public static void MapThumbColor(ISliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateThumbColor(slider);
        }
    }

    [NotImplemented("Implement proper image source loading when image infrastructure is ready")]
    public static void MapThumbImageSource(ISliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateThumbImageSource(slider);
        }
    }  
    
    private void OnSliderPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == PlatformView.ValueProperty && VirtualView != null)
        {
            VirtualView.Value = (double)(e.NewValue ?? 0);
        }
    }
}