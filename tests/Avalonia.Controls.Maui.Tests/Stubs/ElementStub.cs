using Microsoft.Maui;
using System.Collections.Generic;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class ElementStub : IElement
{
    public IElementHandler? Handler { get; set; }

    public IElement? Parent { get; set; }

    public IReadOnlyList<IElement> LogicalChildren { get; set; } = new List<IElement>();
}
