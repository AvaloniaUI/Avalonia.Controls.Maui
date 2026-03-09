using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Services;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Slider;
using System.Threading;
using System.Threading.Tasks;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="ISlider"/>.</summary>
public class SliderHandler : ViewHandler<ISlider, PlatformView>
{
    private CancellationTokenSource? _thumbImageCts;

    /// <summary>Property mapper for <see cref="SliderHandler"/>.</summary>
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

    /// <summary>Command mapper for <see cref="SliderHandler"/>.</summary>
    public static CommandMapper<ISlider, SliderHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="SliderHandler"/>.</summary>
    public SliderHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SliderHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public SliderHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="SliderHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public SliderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <summary>Gets a value indicating whether this handler requires a container view.</summary>
    public override bool NeedsContainer => false;

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        platformView.PropertyChanged += OnSliderPropertyChanged;
        base.ConnectHandler(platformView);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.PropertyChanged -= OnSliderPropertyChanged;
        _thumbImageCts?.Cancel();
        _thumbImageCts = null;
        base.DisconnectHandler(platformView);
    }

    /// <summary>Maps the Minimum property to the platform view.</summary>
    /// <param name="handler">The handler for the slider.</param>
    /// <param name="slider">The virtual view.</param>
    public static void MapMinimum(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMinimum(slider);
        }
    }

    /// <summary>Maps the Maximum property to the platform view.</summary>
    /// <param name="handler">The handler for the slider.</param>
    /// <param name="slider">The virtual view.</param>
    public static void MapMaximum(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMaximum(slider);
        }
    }

    /// <summary>Maps the Value property to the platform view.</summary>
    /// <param name="handler">The handler for the slider.</param>
    /// <param name="slider">The virtual view.</param>
    public static void MapValue(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateValue(slider);
        }
    }

    /// <summary>Maps the MinimumTrackColor property to the platform view.</summary>
    /// <param name="handler">The handler for the slider.</param>
    /// <param name="slider">The virtual view.</param>
    public static void MapMinimumTrackColor(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMinimumTrackColor(slider);
        }
    }

    /// <summary>Maps the MaximumTrackColor property to the platform view.</summary>
    /// <param name="handler">The handler for the slider.</param>
    /// <param name="slider">The virtual view.</param>
    public static void MapMaximumTrackColor(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateMaximumTrackColor(slider);
        }
    }

    /// <summary>Maps the ThumbColor property to the platform view.</summary>
    /// <param name="handler">The handler for the slider.</param>
    /// <param name="slider">The virtual view.</param>
    public static void MapThumbColor(SliderHandler handler, ISlider slider)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateThumbColor(slider);
        }
    }

    /// <summary>Maps the ThumbImageSource property to the platform view.</summary>
    /// <param name="handler">The handler for the slider.</param>
    /// <param name="slider">The virtual view.</param>
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
