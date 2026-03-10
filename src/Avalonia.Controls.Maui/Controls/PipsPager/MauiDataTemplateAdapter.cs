using Avalonia.Controls.Templates;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// Bridges a MAUI <see cref="DataTemplate"/> to Avalonia's <see cref="IDataTemplate"/> for use as a
/// <see cref="PipsPager.IndicatorTemplate"/>. The binding context passed to each item is the 1-based
/// page number (<see cref="int"/>) from <see cref="PipsPagerTemplateSettings.Pips"/>.
/// </summary>
/// <remarks>
/// Each <see cref="Build"/> call creates a MAUI handler via <c>ToHandler</c>. Because
/// <see cref="IDataTemplate"/> has no disposal lifecycle, handlers are not disconnected when pip
/// containers are recycled. The impact is negligible given the small number of pips.
/// </remarks>
internal sealed class MauiDataTemplateAdapter : IDataTemplate
{
    private readonly DataTemplate _mauiTemplate;
    private readonly IMauiContext _context;

    public MauiDataTemplateAdapter(DataTemplate mauiTemplate, IMauiContext context)
    {
        _mauiTemplate = mauiTemplate;
        _context = context;
    }

    public Control? Build(object? param)
    {
        var content = _mauiTemplate.CreateContent();
        if (content is not View view)
            return new Panel();

        var handler = view.ToHandler(_context);
        // BindingContext must be set after ToHandler; handler init overwrites it if set before.
        view.BindingContext = param;

        var control = (Control?)((IViewHandler)handler).ContainerView
                      ?? (Control?)handler.PlatformView;

        // Disable hit-testing so pointer events reach the ListBoxItem and trigger selection.
        if (control != null)
            control.IsHitTestVisible = false;

        return control;
    }

    public bool Match(object? data) => true;
}
