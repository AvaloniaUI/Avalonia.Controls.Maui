using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Avalonia.Controls.Maui;

/// <summary>
/// An Avalonia templated control that implements MAUI's Stepper with increment and decrement buttons.
/// </summary>
public class Stepper : TemplatedControl
{
    private Button? _plusButton;
    private Button? _minusButton;

    /// <summary>
    /// Gets or sets the increment/decrement value.
    /// </summary>
    public double Increment { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public double Maximum { get; set; } = 100.0;

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public double Minimum { get; set; } = 0.0;

    private double _value;

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public double Value
    {
        get => _value;
        set
        {
            var clampedValue = Math.Clamp(value, Minimum, Maximum);
            if (_value != clampedValue)
            {
                _value = clampedValue;
                UpdateButtonStates();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Occurs when the value changes.
    /// </summary>
    public event EventHandler? ValueChanged;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Disconnect old buttons
        if (_plusButton != null)
            _plusButton.Click -= OnPlusClicked;
        if (_minusButton != null)
            _minusButton.Click -= OnMinusClicked;

        // Get new buttons from template
        _plusButton = e.NameScope.Find<Button>("PART_PlusButton");
        _minusButton = e.NameScope.Find<Button>("PART_MinusButton");

        // Connect new buttons
        if (_plusButton != null)
            _plusButton.Click += OnPlusClicked;
        if (_minusButton != null)
            _minusButton.Click += OnMinusClicked;

        UpdateButtonStates();
    }

    private void OnPlusClicked(object? sender, RoutedEventArgs e)
    {
        Value = Math.Min(Value + Increment, Maximum);
    }

    private void OnMinusClicked(object? sender, RoutedEventArgs e)
    {
        Value = Math.Max(Value - Increment, Minimum);
    }

    private void UpdateButtonStates()
    {
        if (_plusButton != null)
            _plusButton.IsEnabled = Value + Increment <= Maximum;

        if (_minusButton != null)
            _minusButton.IsEnabled = Value - Increment >= Minimum;
    }
}