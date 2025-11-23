using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class CheckBoxStub : StubBase, ICheckBox
{
    private bool _isChecked;
    private bool _colorSet;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private Color _color;
#pragma warning restore CS8618

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked != value)
            {
                _isChecked = value;
                CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(value));
            }
        }
    }

    public Paint? Foreground { get; set; }

    public Color Color
    {
        get
        {
            // If Color is explicitly set, return it
            if (_colorSet)
                return _color;

            // Otherwise, try to get color from Foreground if it's a SolidPaint
            if (Foreground is SolidPaint solidPaint)
                return solidPaint.Color;

            // Return transparent as default if nothing is set
            return Colors.Transparent;
        }
        set
        {
            _color = value;
            _colorSet = true;
            // When Color is set, also update Foreground to maintain consistency
            Foreground = new SolidPaint(value);
        }
    }

    public event EventHandler<CheckedChangedEventArgs>? CheckedChanged;
}