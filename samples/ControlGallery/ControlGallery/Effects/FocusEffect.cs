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

#if WINDOWS
    private Microsoft.UI.Xaml.Media.Brush? _originalBackground;
#elif ANDROID
    private Android.Graphics.Drawables.Drawable? _originalBackground;
#elif IOS || MACCATALYST
    private UIKit.UIColor? _originalBackground;
#else
    private global::Avalonia.Media.IBrush? _originalBackground;
#endif

    protected override void OnAttached()
    {
        AttachedCount++;
        CaptureBackground();
        UpdateBackground();
        DiagnosticsChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnDetached()
    {
        RestoreBackground();
        _originalBackground = null;
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
            if (isFocused)
            {
                control.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGreen);
            }
            else
            {
                RestoreBackground();
            }
        }
#elif ANDROID
        if (Control is Android.Views.View view)
        {
            if (isFocused)
            {
                view.SetBackgroundColor(Android.Graphics.Color.LightGreen);
            }
            else
            {
                RestoreBackground();
            }
        }
#elif IOS || MACCATALYST
        if (Control is UIKit.UIView view)
        {
            view.BackgroundColor = isFocused ? UIKit.UIColor.FromRGB(220, 245, 220) : _originalBackground;
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

    private void CaptureBackground()
    {
#if WINDOWS
        if (Control is Microsoft.UI.Xaml.Controls.Control control)
        {
            _originalBackground = control.Background;
        }
#elif ANDROID
        if (Control is Android.Views.View view)
        {
            _originalBackground = view.Background;
        }
#elif IOS || MACCATALYST
        if (Control is UIKit.UIView view)
        {
            _originalBackground = view.BackgroundColor;
        }
#else
        if (Control is global::Avalonia.Controls.TextBox textBox)
        {
            _originalBackground = textBox.Background;
        }
#endif
    }

    private void RestoreBackground()
    {
#if WINDOWS
        if (Control is Microsoft.UI.Xaml.Controls.Control control)
        {
            if (_originalBackground is null)
            {
                control.ClearValue(Microsoft.UI.Xaml.Controls.Control.BackgroundProperty);
            }
            else
            {
                control.Background = _originalBackground;
            }
        }
#elif ANDROID
        if (Control is Android.Views.View view)
        {
            view.SetBackground(_originalBackground);
        }
#elif IOS || MACCATALYST
        if (Control is UIKit.UIView view)
        {
            view.BackgroundColor = _originalBackground;
        }
#else
        if (Control is global::Avalonia.Controls.TextBox textBox)
        {
            textBox.Background = _originalBackground;
        }
#endif
    }
}
