using Microsoft.Maui;
using Avalonia.Controls.Maui.Platform;
using PlatformView = Avalonia.Controls.CheckBox;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="ICheckBox"/>.</summary>
public class CheckBoxHandler : ViewHandler<ICheckBox, PlatformView>
{
    /// <summary>Property mapper for <see cref="CheckBoxHandler"/>.</summary>
    public static IPropertyMapper<ICheckBox, CheckBoxHandler> Mapper =
        new PropertyMapper<ICheckBox, CheckBoxHandler>(ViewHandler.ViewMapper)
        {
            [nameof(ICheckBox.Background)] = MapBackground,
            [nameof(ICheckBox.IsChecked)] = MapIsChecked,
            [nameof(ICheckBox.Foreground)] = MapForeground,
            ["Color"] = MapColor, // Color is on CheckBox class, not ICheckBox interface
        };

    /// <summary>Command mapper for <see cref="CheckBoxHandler"/>.</summary>
    public static CommandMapper<ICheckBox, CheckBoxHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="CheckBoxHandler"/>.</summary>
    public CheckBoxHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="CheckBoxHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public CheckBoxHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="CheckBoxHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public CheckBoxHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.IsCheckedChanged += OnIsCheckedChanged;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        base.DisconnectHandler(platformView);
        platformView.IsCheckedChanged -= OnIsCheckedChanged;
    }

    void OnIsCheckedChanged(object? sender, Interactivity.RoutedEventArgs e)
    {
        if (sender is PlatformView platformView && VirtualView != null)
        {
            VirtualView.IsChecked = platformView.IsChecked ?? false;
        }
    }

    /// <summary>Gets a value indicating whether this handler requires a container view.</summary>
    public override bool NeedsContainer => false;

    /// <summary>Maps the Background property to the platform view.</summary>
    /// <param name="handler">The handler for the check box.</param>
    /// <param name="checkBox">The virtual view.</param>
    public static void MapBackground(CheckBoxHandler handler, ICheckBox checkBox)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        ((PlatformView)handler.PlatformView)?.UpdateBackground(checkBox);
    }

    /// <summary>Maps the IsChecked property to the platform view.</summary>
    /// <param name="handler">The handler for the check box.</param>
    /// <param name="checkBox">The virtual view.</param>
    public static void MapIsChecked(CheckBoxHandler handler, ICheckBox checkBox) =>
        ((PlatformView)handler.PlatformView)?.UpdateIsChecked(checkBox);

    /// <summary>Maps the Foreground property to the platform view.</summary>
    /// <param name="handler">The handler for the check box.</param>
    /// <param name="checkBox">The virtual view.</param>
    public static void MapForeground(CheckBoxHandler handler, ICheckBox checkBox) =>
        ((PlatformView)handler.PlatformView)?.UpdateForeground(checkBox);

    /// <summary>Maps the Color property to the platform view.</summary>
    /// <param name="handler">The handler for the check box.</param>
    /// <param name="checkBox">The virtual view.</param>
    public static void MapColor(CheckBoxHandler handler, ICheckBox checkBox) =>
        handler.UpdateValue(nameof(ICheckBox.Foreground));
}