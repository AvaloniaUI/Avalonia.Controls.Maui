using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Avalonia.Controls.Maui;

public partial class ControlStyles : Styles
{
    public ControlStyles()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
