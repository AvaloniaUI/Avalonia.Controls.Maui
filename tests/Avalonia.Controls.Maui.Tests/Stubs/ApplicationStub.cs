using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using System.Collections.Generic;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class ApplicationStub : ElementStub, IApplication
{
    private readonly List<IWindow> _windows = new();

    public IReadOnlyList<IWindow> Windows => _windows.AsReadOnly();

    public AppTheme UserAppTheme { get; set; } = AppTheme.Unspecified;

    public bool ThemeChangedCalled { get; private set; }

    public int OpenWindowCallCount { get; private set; }

    public int CloseWindowCallCount { get; private set; }

    public int ActivateWindowCallCount { get; private set; }

    public IWindow? LastOpenedWindow { get; private set; }

    public IWindow? LastClosedWindow { get; private set; }

    public IWindow? LastActivatedWindow { get; private set; }

    public IWindow CreateWindow(IActivationState? activationState)
    {
        var window = new WindowStub();
        _windows.Add(window);
        return window;
    }

    public void OpenWindow(IWindow window)
    {
        OpenWindowCallCount++;
        LastOpenedWindow = window;
        if (!_windows.Contains(window))
        {
            _windows.Add(window);
        }
    }

    public void CloseWindow(IWindow window)
    {
        CloseWindowCallCount++;
        LastClosedWindow = window;
        _windows.Remove(window);
    }

    public void ActivateWindow(IWindow window)
    {
        ActivateWindowCallCount++;
        LastActivatedWindow = window;
    }

    public void ThemeChanged()
    {
        ThemeChangedCalled = true;
    }

    public void AddWindow(IWindow window)
    {
        if (!_windows.Contains(window))
        {
            _windows.Add(window);
        }
    }
}
