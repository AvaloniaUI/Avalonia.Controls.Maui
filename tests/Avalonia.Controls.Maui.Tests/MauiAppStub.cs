using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Tests;

/// <summary>
/// Stub MAUI Application for tests. Creates an empty window for testing purposes.
/// </summary>
public class MauiAppStub : Microsoft.Maui.Controls.Application
{
    protected override Microsoft.Maui.Controls.Window CreateWindow(IActivationState? activationState)
    {
        return new Microsoft.Maui.Controls.Window(new Microsoft.Maui.Controls.ContentPage());
    }
}
