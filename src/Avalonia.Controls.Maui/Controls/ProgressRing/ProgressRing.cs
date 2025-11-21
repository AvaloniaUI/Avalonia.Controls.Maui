using System.Globalization;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Represents a control that indicates progress.
/// Supports both determinate and indeterminate modes.
/// </summary>
[PseudoClasses(":active", ":indeterminate", ":determinate")]
internal class ProgressRing : RangeBase
{
    static ProgressRing()
    {
        MinimumProperty.OverrideDefaultValue<ProgressRing>(0.0);
        MaximumProperty.OverrideDefaultValue<ProgressRing>(100.0);
        IsHitTestVisibleProperty.OverrideDefaultValue<ProgressRing>(false);
        FocusableProperty.OverrideDefaultValue<ProgressRing>(false);
        
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<ProgressRing>(
            AutomationControlType.ProgressBar);
    }

    /// <summary>
    /// Defines the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsActive), defaultValue: true);

    /// <summary>
    /// Defines the <see cref="IsIndeterminate"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsIndeterminate), defaultValue: true);

    /// <summary>
    /// Gets or sets a value that indicates whether the ProgressRing is showing progress.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the progress ring reports generic progress 
    /// with a repeating pattern or reports progress based on the Value property.
    /// </summary>
    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
        UpdateAccessibility();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty ||
            change.Property == IsIndeterminateProperty)
        {
            UpdatePseudoClasses();
            UpdateAccessibility();
        }
        else if (change.Property == ValueProperty ||
                 change.Property == MinimumProperty ||
                 change.Property == MaximumProperty)
        {
            UpdateAccessibility();
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":active", IsActive);
        PseudoClasses.Set(":indeterminate", IsActive && IsIndeterminate);
        PseudoClasses.Set(":determinate", IsActive && !IsIndeterminate);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ProgressRingAutomationPeer(this);
    }
    
    /// <summary>
    /// Gets the accessibility text for screen readers.
    /// Override to provide localized strings.
    /// </summary>
    /// <returns>
    /// A descriptive string for the current state, or null/empty
    /// to fall back to AutomationProperties.Name.
    /// </returns>
    public virtual string? GetAccessibilityText()
    {
        // If the app has explicitly set a Name, prefer that.
        var explicitName = AutomationProperties.GetName(this);
        
        if (!string.IsNullOrEmpty(explicitName))
            return explicitName;

        if (!IsActive)
            return null;

        if (IsIndeterminate)
            return null;

        // Calculate percentage: (Value - Minimum) / (Maximum - Minimum) * 100
        double percentage;
        if (Maximum <= Minimum)
        {
            percentage = 0;
        }
        else
        {
            var value = Math.Clamp(Value, Minimum, Maximum);
            percentage = ((value - Minimum) / (Maximum - Minimum)) * 100.0;
        }

        // Represent as a localized percentage number.
        // E.g. "75%" in the current UI culture.
        var culture = CultureInfo.CurrentUICulture;
        return percentage.ToString("P0", culture);
    }

    private void UpdateAccessibility()
    {
        var text = GetAccessibilityText();

        if (!string.IsNullOrEmpty(text))
        {
            if (string.IsNullOrEmpty(AutomationProperties.GetName(this)))
                AutomationProperties.SetName(this, text);

            if (string.IsNullOrEmpty(AutomationProperties.GetHelpText(this)))
                AutomationProperties.SetHelpText(this, text);
        }
    }
}