using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Services;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Slider;
using System.Threading;
using System.Threading.Tasks;

namespace Avalonia.Controls.Maui.Handlers;

public class SliderHandler : ViewHandler<ISlider, PlatformView>
{
    private CancellationTokenSource? _thumbImageCts;

    public static IPropertyMapper<ISlider, SliderHandler> Mapper = new PropertyMapper<ISlider, SliderHandler>(ViewHandler.ViewMapper)
    {
        [nameof(ISlider.Maximum)] = MapMaximum,
        [nameof(ISlider.MaximumTrackColor)] = MapMaximumTrackColor,
        [nameof(ISlider.Minimum)] = MapMinimum,
        [nameof(ISlider.MinimumTrackColor)] = MapMinimumTrackColor,
        [nameof(ISlider.ThumbColor)] = MapThumbColor,
        [nameof(ISlider.ThumbImageSource)] = MapThumbImageSource,
        [nameof(ISlider.Value)] = MapValue,
    };

    public static CommandMapper<ISlider, SliderHandler> CommandMapper = new(ViewCommandMapper)
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
        _thumbImageCts?.Cancel();
        _thumbImageCts = null;
        base.DisconnectHandler(platformView);
    }

    public static void MapMinimum(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMinimum(slider);
        }
    }

    public static void MapMaximum(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMaximum(slider);
        }
    }

    public static void MapValue(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateValue(slider);
        }
    }

    public static void MapMinimumTrackColor(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMinimumTrackColor(slider);
        }
    }

    public static void MapMaximumTrackColor(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMaximumTrackColor(slider);
        }
    }

    public static void MapThumbColor(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateThumbColor(slider);
        }
    }

    public static void MapThumbImageSource(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            if (handler is not SliderHandler sh)
                return;

            sh._thumbImageCts?.Cancel();
            sh._thumbImageCts = null;

            if (slider.ThumbImageSource == null)
            {
                platformView.UpdateThumbImageSource(null);
                platformView.UpdateThumbColor(slider);
                return;
            }

            var cts = new CancellationTokenSource();
            sh._thumbImageCts = cts;
            _ = sh.LoadThumbImageAsync(slider.ThumbImageSource, cts.Token);
        }
    }  
    
    private void OnSliderPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == PlatformView.ValueProperty && VirtualView != null)
        {
            VirtualView.Value = (double)(e.NewValue ?? 0);
        }
    }

    private async Task LoadThumbImageAsync(IImageSource imageSource, CancellationToken token)
    {
        try
        {
            var provider = this.GetRequiredService<IImageSourceServiceProvider>();
            if (provider.GetImageSourceService(imageSource.GetType()) is IAvaloniaImageSourceService service)
            {
                var result = await service.GetImageAsync(imageSource, 1.0f, token);
                if (token.IsCancellationRequested)
                    return;

                PlatformView?.UpdateThumbImageSource(result?.Value as Avalonia.Media.IImage);
            }
            else
            {
                PlatformView?.UpdateThumbImageSource(null);
            }
        }
        catch
        {
            PlatformView?.UpdateThumbImageSource(null);
        }
    }
}
