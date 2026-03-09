using Avalonia.Controls.Maui.Handlers;
using ControlGallery.Controls;
using ControlGallery.Views;
using Microsoft.Maui;

namespace ControlGallery.Handlers;

public class CounterViewHandler : AvaloniaControlHandler<CounterView, CounterControl>
{
    public static new IPropertyMapper<CounterView, CounterViewHandler> Mapper =
        new PropertyMapper<CounterView, CounterViewHandler>(
            AvaloniaControlHandler<CounterView, CounterControl>.Mapper)
        {
            [nameof(CounterView.Count)] = MapCount,
        };

    public CounterViewHandler() : base(Mapper)
    {
    }

    public static void MapCount(CounterViewHandler handler, CounterView view)
    {
        if (handler.AvaloniaControl is not null)
        {
            handler.AvaloniaControl.Count = view.Count;
        }
    }
}
