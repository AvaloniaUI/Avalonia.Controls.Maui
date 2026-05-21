using Microsoft.Maui.Controls.Platform;
using System.ComponentModel;

namespace ControlGallery.Effects;

public class FocusRoutingEffect : RoutingEffect
{
}

public class FocusPlatformEffect : PlatformEffect
{
    public static int AttachedCount { get; private set; }

    public static event EventHandler? DiagnosticsChanged;

#if !WINDOWS && !ANDROID && !IOS && !MACCATALYST
    private global::Avalonia.Media.IBrush? _originalBackground;
#endif

    protected override void OnAttached()
    {
        AttachedCount++;
#if !WINDOWS && !ANDROID && !IOS && !MACCATALYST
        if (Control is global::Avalonia.Controls.TextBox textBox)
        {
            _originalBackground = textBox.Background;
        }
#endif
        UpdateBackground();
        DiagnosticsChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnDetached()
    {
        ClearBackground();
        AttachedCount = Math.Max(0, AttachedCount - 1);
        DiagnosticsChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
    {
        base.OnElementPropertyChanged(args);

        if (args.PropertyName == VisualElement.IsFocusedProperty.PropertyName)
        {
            UpdateBackground();
        }
    }

    private void UpdateBackground()
    {
        var isFocused = Element is VisualElement { IsFocused: true };

#if WINDOWS
        if (Control is Microsoft.UI.Xaml.Controls.Control control)
        {
            control.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                isFocused ? Microsoft.UI.Colors.LightGreen : Microsoft.UI.Colors.Transparent);
        }
#elif ANDROID
        Control?.SetBackgroundColor(isFocused ? Android.Graphics.Color.LightGreen : Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
        if (Control is UIKit.UIView view)
        {
            view.BackgroundColor = isFocused ? UIKit.UIColor.FromRGB(220, 245, 220) : UIKit.UIColor.Clear;
        }
#else
        if (Control is global::Avalonia.Controls.TextBox textBox)
        {
            textBox.Background = isFocused
                ? new global::Avalonia.Media.SolidColorBrush(global::Avalonia.Media.Color.FromRgb(220, 245, 220))
                : _originalBackground;
        }
#endif
    }

    private void ClearBackground()
    {
#if WINDOWS
        if (Control is Microsoft.UI.Xaml.Controls.Control control)
        {
            control.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
#elif ANDROID
        Control?.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
        if (Control is UIKit.UIView view)
        {
            view.BackgroundColor = UIKit.UIColor.Clear;
        }
#else
        if (Control is global::Avalonia.Controls.TextBox textBox)
        {
            textBox.Background = _originalBackground;
        }
#endif
    }
}
