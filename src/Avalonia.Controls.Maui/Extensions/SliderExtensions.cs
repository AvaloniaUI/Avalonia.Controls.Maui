using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Microsoft.Maui;
using AvaloniaSlider = Avalonia.Controls.Slider;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Platform extensions for applying .NET MAUI slider properties to Avalonia Slider.
/// </summary>
public static class SliderExtensions
{
    private const string MinimumTrackStyleTag = "__maui_min_track_style__";
    private const string MaximumTrackStyleTag = "__maui_max_track_style__";
    private const string ThumbStyleTag = "__maui_thumb_style__";
    private const string ThumbPointerOverStyleTag = "__maui_thumb_pointerover_style__";
    private const string ThumbPressedStyleTag = "__maui_thumb_pressed_style__";
    private const double ThumbSizeFallback = 16.0;
    private const double TrackHeightFallback = 4.0;

    /// <summary>Syncs the slider minimum value.</summary>
    /// <param name="control">Avalonia slider instance.</param>
    /// <param name="slider">MAUI slider providing the value.</param>
    public static void UpdateMinimum(this AvaloniaSlider control, ISlider slider) =>
        control.Minimum = slider.Minimum;

    /// <summary>Syncs the slider maximum value.</summary>
    /// <param name="control">Avalonia slider instance.</param>
    /// <param name="slider">MAUI slider providing the value.</param>
    public static void UpdateMaximum(this AvaloniaSlider control, ISlider slider) =>
        control.Maximum = slider.Maximum;

    /// <summary>Syncs the slider current value.</summary>
    /// <param name="control">Avalonia slider instance.</param>
    /// <param name="slider">MAUI slider providing the value.</param>
    public static void UpdateValue(this AvaloniaSlider control, ISlider slider) =>
        control.Value = slider.Value;

    /// <summary>Applies the minimum track color to the decrease track.</summary>
    /// <param name="control">Avalonia slider instance.</param>
    /// <param name="slider">MAUI slider providing the color.</param>
    public static void UpdateMinimumTrackColor(this AvaloniaSlider control, ISlider slider)
    {
        RemoveTaggedStyles(control, MinimumTrackStyleTag);

        if (slider.MinimumTrackColor == null)
        {
            ClearTrackResources(control, isMinimumTrack: true);
            return;
        }

        var brush = (IBrush)slider.MinimumTrackColor.ToPlatform();
        ApplyTrackResources(control, isMinimumTrack: true, brush);
        ApplyTrackStyles(control, MinimumTrackStyleTag, brush, "PART_DecreaseButton");
    }

    /// <summary>Applies the maximum track color to the increase track.</summary>
    /// <param name="control">Avalonia slider instance.</param>
    /// <param name="slider">MAUI slider providing the color.</param>
    public static void UpdateMaximumTrackColor(this AvaloniaSlider control, ISlider slider)
    {
        RemoveTaggedStyles(control, MaximumTrackStyleTag);

        if (slider.MaximumTrackColor == null)
        {
            ClearTrackResources(control, isMinimumTrack: false);
            return;
        }

        var brush = (IBrush)slider.MaximumTrackColor.ToPlatform();
        ApplyTrackResources(control, isMinimumTrack: false, brush);
        ApplyTrackStyles(control, MaximumTrackStyleTag, brush, "PART_IncreaseButton");
    }

    /// <summary>Applies the thumb color across all thumb visual states.</summary>
    /// <param name="control">Avalonia slider instance.</param>
    /// <param name="slider">MAUI slider providing the color.</param>
    public static void UpdateThumbColor(this AvaloniaSlider control, ISlider slider)
    {
        RemoveTaggedStyles(control, ThumbStyleTag);
        RemoveTaggedStyles(control, ThumbPointerOverStyleTag);
        RemoveTaggedStyles(control, ThumbPressedStyleTag);

        if (slider.ThumbColor == null)
        {
            ClearThumbResources(control);
            return;
        }

        var brush = (IBrush)slider.ThumbColor.ToPlatform();
        ApplyThumbResources(control, brush);

        AddThumbStyle(control, ThumbStyleTag, brush,
            x => x.OfType<AvaloniaSlider>().Template().OfType<Thumb>().Name("PART_Thumb"));
        AddThumbStyle(control, ThumbPointerOverStyleTag, brush,
            x => x.OfType<AvaloniaSlider>().Template().OfType<Thumb>().Name("PART_Thumb").Class(":pointerover"));
        AddThumbStyle(control, ThumbPressedStyleTag, brush,
            x => x.OfType<AvaloniaSlider>().Template().OfType<Thumb>().Name("PART_Thumb").Class(":pressed"));

        ApplyThumbTemplate(control, brush);
    }

    /// <summary>Updates the thumb to display a custom image.</summary>
    /// <param name="control">Avalonia slider instance.</param>
    /// <param name="image">Resolved platform image for the thumb.</param>
    public static void UpdateThumbImageSource(this AvaloniaSlider control, Avalonia.Media.IImage? image)
    {
        RemoveTaggedStyles(control, ThumbStyleTag);
        RemoveTaggedStyles(control, ThumbPointerOverStyleTag);
        RemoveTaggedStyles(control, ThumbPressedStyleTag);

        if (image == null || image is not IImageBrushSource brushSource)
        {
            // Reset to default visuals; thumb color mapping (if any) will reapply styling.
            ClearThumbResources(control);
            return;
        }

        var brush = new ImageBrush
        {
            Source = brushSource,
            Stretch = Stretch.UniformToFill
        };

        ApplyThumbImage(control, brush, image);
    }

    private static void ApplyTrackResources(AvaloniaSlider control, bool isMinimumTrack, IBrush brush)
    {
        if (isMinimumTrack)
        {
            control.Resources["SliderTrackValueFill"] = brush;
            control.Resources["SliderTrackValueFillPointerOver"] = brush;
            control.Resources["SliderTrackValueFillPressed"] = brush;
        }
        else
        {
            control.Resources["SliderTrackFill"] = brush;
            control.Resources["SliderTrackFillPointerOver"] = brush;
            control.Resources["SliderTrackFillPressed"] = brush;
        }
    }

    private static void ApplyTrackStyles(AvaloniaSlider control, string tag, IBrush brush, string partName)
    {
        var trackStyle = new Style(x => x
            .OfType<AvaloniaSlider>()
            .Template()
            .OfType<RepeatButton>()
            .Name(partName)
            .Template()
            .OfType<Border>()
            .Name("VisualTrack"));
        trackStyle.Setters.Add(new Setter(Border.BackgroundProperty, brush));
        trackStyle.Setters.Add(new Setter(Border.BorderBrushProperty, brush));
        AddTaggedStyle(control, trackStyle, tag);

        // Fallback coloring when VisualTrack is missing (e.g., Simple theme)
        var repeatButtonStyle = new Style(x => x
            .OfType<AvaloniaSlider>()
            .Template()
            .OfType<RepeatButton>()
            .Name(partName));
        repeatButtonStyle.Setters.Add(new Setter(TemplatedControl.BackgroundProperty, brush));
        repeatButtonStyle.Setters.Add(new Setter(TemplatedControl.BorderBrushProperty, brush));
        AddTaggedStyle(control, repeatButtonStyle, tag);

        // Fallback template when VisualTrack is missing (thin track with themed height)
        var repeatButtonTemplateStyle = new Style(x => x
            .OfType<RepeatButton>()
            .Name(partName));
        repeatButtonTemplateStyle.Setters.Add(new Setter(TemplatedControl.TemplateProperty,
            CreateThinTrackTemplate(brush, control)));
        AddTaggedStyle(control, repeatButtonTemplateStyle, tag);
    }

    private static void AddThumbStyle(
        AvaloniaSlider control,
        string tag,
        IBrush brush,
        Func<Selector?, Selector> selectorBuilder)
    {
        var style = new Style(selectorBuilder);
        style.Setters.Add(new Setter(TemplatedControl.BackgroundProperty, brush));
        style.Setters.Add(new Setter(TemplatedControl.BorderBrushProperty, brush));
        AddTaggedStyle(control, style, tag);
    }

    private static void ApplyThumbResources(AvaloniaSlider control, IBrush brush)
    {
        control.Resources["SliderThumbBackground"] = brush;
        control.Resources["SliderThumbBackgroundPointerOver"] = brush;
        control.Resources["SliderThumbBackgroundPressed"] = brush;

        control.Resources["SliderThumbFill"] = brush;
        control.Resources["SliderThumbFillPointerOver"] = brush;
        control.Resources["SliderThumbFillPressed"] = brush;

        control.Resources["SystemControlForegroundAccentBrush"] = brush;
    }

    private static void ClearThumbResources(AvaloniaSlider control)
    {
        control.Resources.Remove("SliderThumbBackground");
        control.Resources.Remove("SliderThumbBackgroundPointerOver");
        control.Resources.Remove("SliderThumbBackgroundPressed");
        control.Resources.Remove("SliderThumbFill");
        control.Resources.Remove("SliderThumbFillPointerOver");
        control.Resources.Remove("SliderThumbFillPressed");
        control.Resources.Remove("SystemControlForegroundAccentBrush");
    }

    private static void ClearTrackResources(AvaloniaSlider control, bool isMinimumTrack)
    {
        var prefix = isMinimumTrack ? "SliderTrackValueFill" : "SliderTrackFill";
        control.Resources.Remove(prefix);
        control.Resources.Remove($"{prefix}PointerOver");
        control.Resources.Remove($"{prefix}Pressed");
    }

    private static void AddTaggedStyle(AvaloniaSlider control, Style style, string tag)
    {
        style.Resources[tag] = true;
        control.Styles.Insert(0, style);
    }

    private static void RemoveTaggedStyles(AvaloniaSlider control, string tag)
    {
        for (int i = control.Styles.Count - 1; i >= 0; i--)
        {
            if (control.Styles[i] is Style style && style.Resources?.ContainsKey(tag) == true)
                control.Styles.RemoveAt(i);
        }
    }

    private static double GetTrackHeight(AvaloniaSlider slider)
    {
        if (slider.TryGetResource("SliderOutsideTickBarThemeHeight", out var value) && value is double h && h > 0)
            return h;

        return TrackHeightFallback;
    }

    private static FuncControlTemplate<RepeatButton> CreateThinTrackTemplate(IBrush brush, AvaloniaSlider slider)
    {
        var height = GetTrackHeight(slider);
        return new FuncControlTemplate<RepeatButton>((_, __) =>
            new Border
            {
                Name = "VisualTrack",
                Height = height,
                CornerRadius = new CornerRadius(height / 2),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = brush
            });
    }

    private static void ApplyThumbTemplate(AvaloniaSlider control, IBrush brush)
    {
        void Apply(AvaloniaSlider slider, TemplateAppliedEventArgs? args = null)
        {
            var thumb = args?.NameScope.Find<Thumb>("PART_Thumb") ??
                        slider.GetVisualDescendants().OfType<Thumb>().FirstOrDefault();

            if (thumb == null)
                return;

            var width = thumb.Bounds.Width > 0
                ? thumb.Bounds.Width
                : thumb.Width > 0
                    ? thumb.Width
                    : thumb.MinWidth > 0
                        ? thumb.MinWidth
                        : ThumbSizeFallback;

            var height = thumb.Bounds.Height > 0
                ? thumb.Bounds.Height
                : thumb.Height > 0
                    ? thumb.Height
                    : thumb.MinHeight > 0
                        ? thumb.MinHeight
                        : ThumbSizeFallback;

            var radius = Math.Min(width, height) / 2;

            thumb.Template = new FuncControlTemplate<Thumb>((_, __) =>
                new Border
                {
                    Width = width,
                    Height = height,
                    CornerRadius = new CornerRadius(radius),
                    Background = brush,
                    BorderBrush = brush
                });
        }

        Apply(control);
        control.TemplateApplied += OnTemplateApplied;

        void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            Apply(control, e);
            control.TemplateApplied -= OnTemplateApplied;
        }
    }

    private static void ApplyThumbImage(AvaloniaSlider control, IBrush brush, Avalonia.Media.IImage image)
    {
        void Apply(AvaloniaSlider slider, TemplateAppliedEventArgs? args = null)
        {
            var thumb = args?.NameScope.Find<Thumb>("PART_Thumb") ??
                        slider.GetVisualDescendants().OfType<Thumb>().FirstOrDefault();

            if (thumb == null)
                return;

            var size = image.Size;
            // Use the original image size without clamping or resizing
            var width = size.Width > 0 && !double.IsNaN(size.Width) ? size.Width : ThumbSizeFallback;
            var height = size.Height > 0 && !double.IsNaN(size.Height) ? size.Height : ThumbSizeFallback;

            var radius = Math.Min(width, height) / 2;

            thumb.Template = new FuncControlTemplate<Thumb>((_, __) =>
                new Border
                {
                    Width = width,
                    Height = height,
                    Background = brush
                });
        }

        Apply(control);
        control.TemplateApplied += OnTemplateApplied;

        void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            Apply(control, e);
            control.TemplateApplied -= OnTemplateApplied;
        }
    }
}
