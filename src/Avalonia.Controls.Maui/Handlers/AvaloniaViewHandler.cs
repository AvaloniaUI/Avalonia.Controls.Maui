using Avalonia.Controls.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Avalonia.Controls.Maui.Handlers
{
    public partial class AvaloniaViewHandler
    {
        public static IPropertyMapper<AvaloniaView, AvaloniaViewHandler> PropertyMapper = new PropertyMapper<AvaloniaView, AvaloniaViewHandler>(Microsoft.Maui.Handlers.ViewHandler.ViewMapper)
        {
            [nameof(AvaloniaView.Content)] = MapContent
        };

        public AvaloniaViewHandler() : base(PropertyMapper)
        {
        }
    }
}