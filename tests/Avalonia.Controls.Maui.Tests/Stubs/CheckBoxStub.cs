using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class CheckBoxStub : StubBase, ICheckBox
{
    private bool _isChecked;
    
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
    
    public event EventHandler<CheckedChangedEventArgs>? CheckedChanged;
}