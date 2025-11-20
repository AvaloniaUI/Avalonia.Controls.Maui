using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class CheckBoxStub : StubBase, ICheckBox
{
    public bool IsChecked { get; set; }
    
    public Paint? Foreground { get; set; }
}