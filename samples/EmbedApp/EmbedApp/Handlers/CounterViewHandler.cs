using Avalonia.Controls.Maui.Handlers;
using EmbedApp.Controls;
using EmbedApp.Views;
using Microsoft.Maui;

namespace EmbedApp.Handlers;

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
