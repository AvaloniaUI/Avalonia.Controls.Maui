using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.LifecycleEvents;

/// <summary>
/// Defines lifecycle event delegates for Avalonia applications.
/// </summary>
public static class AvaloniaLifecycle
{
	/// <summary>
	/// Represents a method that handles the window activated event.
	/// </summary>
	/// <param name="window">The window that was activated.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void OnActivated(Window window, EventArgs args);
	
	/// <summary>
	/// Represents a method that handles the window closed event.
	/// </summary>
	/// <param name="window">The window that was closed.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void OnClosed(Window window, EventArgs args);
	
	/// <summary>
	/// Represents a method that handles the application launched event.
	/// </summary>
	/// <param name="application">The application that was launched.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void OnLaunched(Application application, EventArgs args);
	
	/// <summary>
	/// Represents a method that handles the application launching event.
	/// </summary>
	/// <param name="application">The application that is launching.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void OnLaunching(Application application, EventArgs args);
	
	/// <summary>
	/// Represents a method that handles the window visibility changed event.
	/// </summary>
	/// <param name="window">The window whose visibility changed.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void OnVisibilityChanged(Window window, EventArgs args);
	
	/// <summary>
	/// Represents a method that handles platform messages for a window.
	/// </summary>
	/// <param name="window">The window receiving the platform message.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void OnPlatformMessage(Window window, EventArgs args);
	
	/// <summary>
	/// Represents a method that handles the window created event.
	/// </summary>
	/// <param name="window">The window that was created.</param>
	public delegate void OnWindowCreated(Window window);
	
	/// <summary>
	/// Represents a method that handles the window resumed event.
	/// </summary>
	/// <param name="window">The window that was resumed.</param>
	public delegate void OnResumed(Window window);
	
	/// <summary>
	/// Represents a method that handles the MAUI context created event.
	/// </summary>
	/// <param name="mauiContext">The MAUI context that was created.</param>
	internal delegate void OnMauiContextCreated(IMauiContext mauiContext);
}