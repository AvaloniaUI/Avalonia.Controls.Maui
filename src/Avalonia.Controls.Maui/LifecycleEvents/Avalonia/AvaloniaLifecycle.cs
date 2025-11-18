using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.LifecycleEvents;

public static class AvaloniaLifecycle
{
	public delegate void OnActivated(Window window, EventArgs args);
	public delegate void OnClosed(Window window, EventArgs args);
	public delegate void OnLaunched(Application application, EventArgs args);
	public delegate void OnLaunching(Application application, EventArgs args);
	public delegate void OnVisibilityChanged(Window window, EventArgs args);
	public delegate void OnPlatformMessage(Window window, EventArgs args);
	public delegate void OnWindowCreated(Window window);
	public delegate void OnResumed(Window window);
	internal delegate void OnMauiContextCreated(IMauiContext mauiContext);
}
