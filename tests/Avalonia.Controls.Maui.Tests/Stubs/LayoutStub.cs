using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using System.Collections;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class LayoutStub : StubBase, ILayout, ICrossPlatformLayout
{
    private readonly List<IView> _children = new();

    public bool ClipsToBounds { get; set; }

    public new Paint? Background { get; set; }

    public new bool InputTransparent { get; set; }

    public bool IgnoreSafeArea { get; set; }
    
    public Microsoft.Maui.Thickness Padding { get; set; }

    public IReadOnlyList<IView> this[Range range] => _children.ToArray()[range];

    public int Count => _children.Count;

    public IView this[int index]
    {
        get => _children[index];
        set
        {
            _children[index] = value;
        }
    }

    public void Add(IView child)
    {
        _children.Add(child);
    }

    public void Clear()
    {
        _children.Clear();
    }

    public bool Contains(IView item) => _children.Contains(item);

    public void CopyTo(IView[] array, int arrayIndex) => _children.CopyTo(array, arrayIndex);

    public IEnumerator<IView> GetEnumerator() => _children.GetEnumerator();

    public int IndexOf(IView item) => _children.IndexOf(item);

    public void Insert(int index, IView child)
    {
        _children.Insert(index, child);
    }

    public bool Remove(IView child)
    {
        return _children.Remove(child);
    }

    public void RemoveAt(int index)
    {
        _children.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool IsReadOnly => false;
    
    public Microsoft.Maui.Graphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
    {
        return new Microsoft.Maui.Graphics.Size(widthConstraint, heightConstraint);
    }

    public Microsoft.Maui.Graphics.Size CrossPlatformArrange(Microsoft.Maui.Graphics.Rect bounds)
    {
        return bounds.Size;
    }
}
