using System;
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
public partial class ProgressRing : ProgressBar
{
    static ProgressRing()
    {
        MinimumProperty.OverrideDefaultValue<ProgressRing>(0.0);
        MaximumProperty.OverrideDefaultValue<ProgressRing>(100.0);
        IsHitTestVisibleProperty.OverrideDefaultValue<ProgressRing>(false);
        FocusableProperty.OverrideDefaultValue<ProgressRing>(false);
        IsIndeterminateProperty.OverrideDefaultValue<ProgressRing>(true);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<ProgressRing>(AutomationControlType.ProgressBar);
    }

    /// <summary>
    /// Defines the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsActive), defaultValue: true);

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="ProgressRing"/> is showing progress.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public ProgressRing()
    {
        InitializeComponent();
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
        UpdateAccessibility();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsActiveProperty || change.Property == IsIndeterminateProperty)
        {
            UpdatePseudoClasses();
            UpdateAccessibility();
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":active", IsActive);
        PseudoClasses.Set(":indeterminate", IsActive && IsIndeterminate);
        PseudoClasses.Set(":determinate", IsActive && !IsIndeterminate);
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ProgressRingAutomationPeer(this);
    }

    /// <summary>
    /// Gets the accessibility text (e.g. "75%") used by the AutomationPeer.
    /// </summary>
    /// <returns>A string representing the current progress percentage, or null if indeterminate/inactive.</returns>
    public virtual string? GetAccessibilityText()
    {
        var explicitName = AutomationProperties.GetName(this);
        if (!string.IsNullOrEmpty(explicitName))
            return explicitName;

        if (!IsActive || IsIndeterminate)
            return null;

        double percentage = 0;
        if (Maximum > Minimum)
        {
            var val = Math.Clamp(Value, Minimum, Maximum);
            percentage = (val - Minimum) / (Maximum - Minimum) * 100.0;
        }

        return percentage.ToString("P0", CultureInfo.CurrentUICulture);
    }

    private void UpdateAccessibility()
    {
        var text = GetAccessibilityText();
        if (!string.IsNullOrEmpty(text))
        {
            if (string.IsNullOrEmpty(AutomationProperties.GetName(this)))
                AutomationProperties.SetName(this, text);
        }
    }
}
