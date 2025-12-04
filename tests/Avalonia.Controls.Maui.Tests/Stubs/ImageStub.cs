using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class ImageStub : StubBase, Microsoft.Maui.IImage
{
    public Aspect Aspect { get; set; } = Aspect.AspectFit;

    public bool IsAnimationPlaying { get; set; }

    public IImageSource? Source { get; set; }

    public bool IsOpaque { get; set; }

    public List<bool> LoadingStates { get; } = new();

    public bool? LastIsLoading => LoadingStates.Count > 0 ? LoadingStates[^1] : null;

    public void UpdateIsLoading(bool isLoading)
    {
        LoadingStates.Add(isLoading);
    }
}
