using Microsoft.Maui;
using Microsoft.Maui.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class StubBase : ElementStub, IView, IVisualTreeElement, IPropertyMapperView
{
    IElementHandler? IElement.Handler
    {
        get => Handler;
        set => Handler = (IViewHandler?)value;
    }

    public bool IsEnabled { get; set; } = true;

    public bool IsFocused { get; set; }

    public IList<IView> Children { get; set; } = new List<IView>();

    public Visibility Visibility { get; set; } = Visibility.Visible;

    public double Opacity { get; set; } = 1.0d;

    public MauiGraphics.Paint? Background { get; set; }

    public MauiGraphics.Rect Frame { get; set; }

    public new IViewHandler? Handler
    {
        get => (IViewHandler?)base.Handler;
        set => base.Handler = value;
    }

    public MauiGraphics.IShape? Clip { get; set; }

    public IShadow? Shadow { get; set; }

    public MauiGraphics.Size DesiredSize { get; set; } = new MauiGraphics.Size(50, 50);

    public double Width { get; set; } = 50;

    public double Height { get; set; } = 50;

    public double MaximumWidth { get; set; } = Dimension.Maximum;

    public double MaximumHeight { get; set; } = Dimension.Maximum;

    public double MinimumWidth { get; set; } = Dimension.Minimum;

    public double MinimumHeight { get; set; } = Dimension.Minimum;

    public double TranslationX { get; set; }

    public double TranslationY { get; set; }

    public double Scale { get; set; } = 1d;

    public double ScaleX { get; set; } = 1d;

    public double ScaleY { get; set; } = 1d;

    public double Rotation { get; set; }

    public double RotationX { get; set; }

    public double RotationY { get; set; }

    public double AnchorX { get; set; } = .5d;

    public double AnchorY { get; set; } = .5d;

    public Microsoft.Maui.Thickness Margin { get; set; }

    public string AutomationId { get; set; } = string.Empty;

    public FlowDirection FlowDirection { get; set; } = FlowDirection.LeftToRight;

    public LayoutAlignment HorizontalLayoutAlignment { get; set; }

    public LayoutAlignment VerticalLayoutAlignment { get; set; }

    public Semantics? Semantics { get; set; } = new Semantics();

    public int ZIndex { get; set; }

    public bool InputTransparent { get; set; }

    public MauiGraphics.Size Arrange(MauiGraphics.Rect bounds)
    {
        Frame = bounds;
        DesiredSize = bounds.Size;

        // If this view is attached to the visual tree then let's arrange it
        if (IsLoaded)
            Handler?.PlatformArrange(Frame);

        return DesiredSize;
    }

    protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string propertyName = "",
        Action<T, T>? onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        var oldValue = backingStore;
        backingStore = value;
        Handler?.UpdateValue(propertyName);
        onChanged?.Invoke(oldValue, value);
        return true;
    }

    public void InvalidateArrange()
    {
    }

    public void InvalidateMeasure()
    {
    }

    public bool Focus()
    {
        if (Handler == null)
            return false;

        FocusRequest focusRequest = new FocusRequest();
        Handler.Invoke(nameof(IView.Focus), focusRequest);
        IsFocused = true;
        return true;
    }

    public void Unfocus()
    {
        IsFocused = false;
    }

    public MauiGraphics.Size Measure(double widthConstraint, double heightConstraint)
    {
        if (Handler != null)
        {
            DesiredSize = Handler.GetDesiredSize(widthConstraint, heightConstraint);
            return DesiredSize;
        }

        return new MauiGraphics.Size(widthConstraint, heightConstraint);
    }

    public IReadOnlyList<IVisualTreeElement> GetVisualChildren() =>
        this.Children.Cast<IVisualTreeElement>().ToList().AsReadOnly();

    public IVisualTreeElement? GetVisualParent() =>
        this.Parent as IVisualTreeElement;

    public PropertyMapper GetPropertyMapperOverrides() =>
        PropertyMapperOverrides ?? new PropertyMapper<IView>();

    public PropertyMapper? PropertyMapperOverrides { get; set; }

    public bool IsLoaded => Handler?.PlatformView is global::Avalonia.Controls.Control control && control.IsLoaded;
}
