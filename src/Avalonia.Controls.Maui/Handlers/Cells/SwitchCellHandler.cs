using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using System;

namespace Avalonia.Controls.Maui.Handlers.Cells;

/// <summary>
/// Handler for MAUI SwitchCell
/// </summary>
public class SwitchCellHandler : ElementHandler<SwitchCell, global::Avalonia.Controls.Border>
{
    public static IPropertyMapper<SwitchCell, SwitchCellHandler> Mapper =
        new PropertyMapper<SwitchCell, SwitchCellHandler>(ElementMapper)
        {
            [nameof(SwitchCell.Text)] = MapText,
            [nameof(SwitchCell.On)] = MapOn,
            [nameof(SwitchCell.OnColor)] = MapOnColor,
        };

    public static CommandMapper<SwitchCell, SwitchCellHandler> CommandMapper =
        new(ElementCommandMapper);

    private TextBlock? _textBlock;
    private ToggleSwitch? _switch;
    private bool _isUpdating;

    public SwitchCellHandler() : base(Mapper, CommandMapper)
    {
    }

    public SwitchCellHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public SwitchCellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override global::Avalonia.Controls.Border CreatePlatformElement()
    {
        _textBlock = new TextBlock
        {
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center
        };

        _switch = new ToggleSwitch
        {
            VerticalAlignment = VerticalAlignment.Center
        };

        _switch.IsCheckedChanged += (s, e) =>
        {
            if (_isUpdating || VirtualView is null)
                return;

            _isUpdating = true;
            VirtualView.On = _switch.IsChecked ?? false;
            _isUpdating = false;
        };

        var grid = new global::Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(16, 8)
        };

        global::Avalonia.Controls.Grid.SetColumn(_textBlock, 0);
        global::Avalonia.Controls.Grid.SetColumn(_switch, 1);

        grid.Children.Add(_textBlock);
        grid.Children.Add(_switch);

        return new global::Avalonia.Controls.Border
        {
            Child = grid,
            MinHeight = 44
        };
    }

    public static void MapText(SwitchCellHandler handler, SwitchCell switchCell)
    {
        if (handler._textBlock is null)
            return;

        handler._textBlock.Text = switchCell.Text ?? string.Empty;
    }

    public static void MapOn(SwitchCellHandler handler, SwitchCell switchCell)
    {
        if (handler._switch is null || handler._isUpdating)
            return;

        handler._isUpdating = true;
        handler._switch.IsChecked = switchCell.On;
        handler._isUpdating = false;
    }

    public static void MapOnColor(SwitchCellHandler handler, SwitchCell switchCell)
    {
        if (handler._switch is null)
            return;

        // OnColor could be used to customize the switch appearance
        // For now, we'll use the default Avalonia styling
    }
}
