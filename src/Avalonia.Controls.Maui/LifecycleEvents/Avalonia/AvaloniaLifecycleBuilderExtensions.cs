using Microsoft.Maui.LifecycleEvents;

namespace Avalonia.Controls.Maui.LifecycleEvents;

public static class AvaloniaLifecycleBuilderExtensions
{
	public static IAvaloniaLifecycleBuilder OnActivated(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnActivated del) => lifecycle.OnEvent(del);
	public static IAvaloniaLifecycleBuilder OnClosed(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnClosed del) => lifecycle.OnEvent(del);
	public static IAvaloniaLifecycleBuilder OnLaunching(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnLaunching del) => lifecycle.OnEvent(del);
	public static IAvaloniaLifecycleBuilder OnLaunched(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnLaunched del) => lifecycle.OnEvent(del);
	public static IAvaloniaLifecycleBuilder OnVisibilityChanged(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnVisibilityChanged del) => lifecycle.OnEvent(del);
	public static IAvaloniaLifecycleBuilder OnWindowCreated(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnWindowCreated del) => lifecycle.OnEvent(del);
	public static IAvaloniaLifecycleBuilder OnResumed(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnResumed del) => lifecycle.OnEvent(del);
	public static IAvaloniaLifecycleBuilder OnPlatformMessage(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnPlatformMessage del) => lifecycle.OnEvent(del);

	internal static IAvaloniaLifecycleBuilder OnMauiContextCreated(this IAvaloniaLifecycleBuilder lifecycle, AvaloniaLifecycle.OnMauiContextCreated del) => lifecycle.OnEvent(del);
}
