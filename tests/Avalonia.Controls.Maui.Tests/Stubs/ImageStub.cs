using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class ImageStub : StubBase, Microsoft.Maui.IImage, Microsoft.Maui.Controls.IImageController
{
    public Aspect Aspect { get; set; } = Aspect.AspectFit;

    public bool IsAnimationPlaying { get; set; }

    public IImageSource? Source { get; set; }

    public bool IsOpaque { get; set; }

    public List<bool> LoadingStates { get; } = new();

    public bool? LastIsLoading => LoadingStates.Count > 0 ? LoadingStates[^1] : null;

    public bool Batched => throw new NotImplementedException();

    public bool DisableLayout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public EffectiveFlowDirection EffectiveFlowDirection => throw new NotImplementedException();

    public bool IsInPlatformLayout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool IsPlatformStateConsistent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool IsPlatformEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public NavigationProxy NavigationProxy => throw new NotImplementedException();

    public IEffectControlProvider EffectControlProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Element RealParent => throw new NotImplementedException();

    IReadOnlyList<Element> IElementController.LogicalChildren => throw new NotImplementedException();

#pragma warning disable CS0067 // Unused event
    public event EventHandler<EventArg<VisualElement>>? BatchCommitted;
    public event EventHandler<VisualElement.FocusRequestArgs>? FocusChangeRequested;
#pragma warning restore CS0067

    public IEnumerable<Element> Descendants()
    {
        throw new NotImplementedException();
    }

    public bool EffectIsAttached(string name)
    {
        throw new NotImplementedException();
    }

    public bool GetLoadAsAnimation()
    {
        throw new NotImplementedException();
    }

    public void InvalidateMeasure(InvalidationTrigger trigger)
    {
        throw new NotImplementedException();
    }

    public void PlatformSizeChanged()
    {
        throw new NotImplementedException();
    }

    public void SetIsLoading(bool isLoading)
    {
        LoadingStates.Add(isLoading);
    }

    public void SetValueFromRenderer(BindableProperty property, object value)
    {
        throw new NotImplementedException();
    }

    public void SetValueFromRenderer(BindablePropertyKey propertyKey, object value)
    {
        throw new NotImplementedException();
    }

    public void UpdateIsLoading(bool isLoading)
    {
        LoadingStates.Add(isLoading);
    }
}
