using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

// When Avalonia.Controls.PipsPager ships, remove this file entirely.
namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// Wraps a MAUI <see cref="DataTemplate"/> as an Avalonia <see cref="IDataTemplate"/>
/// so it can be assigned to an Avalonia <see cref="ItemsControl.ItemTemplate"/>.
/// </summary>
internal sealed class MauiDataTemplateAdapter : IDataTemplate
{
    private readonly DataTemplate _template;
    private readonly IMauiContext _context;

    internal MauiDataTemplateAdapter(DataTemplate template, IMauiContext context)
    {
        _template = template;
        _context = context;
    }

    /// <inheritdoc/>
    public bool Match(object? data) => data is int;

    /// <inheritdoc/>
    public Control? Build(object? param)
    {
        var content = _template.CreateContent();
        if (content is not View mauiView)
            return null;

        mauiView.BindingContext = param;

        var handler = mauiView.ToHandler(_context);
        return handler.PlatformView as Control;
    }
}
