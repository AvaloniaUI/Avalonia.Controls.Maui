using Microsoft.Maui.LifecycleEvents;

namespace Avalonia.Controls.Maui.LifecycleEvents;

public static class AvaloniaLifecycleExtensions
{
	public static ILifecycleBuilder AddWindows(this ILifecycleBuilder builder, Action<IAvaloniaLifecycleBuilder> configureDelegate)
	{
		var windows = new LifecycleBuilder(builder);

		configureDelegate?.Invoke(windows);

		return builder;
	}

	class LifecycleBuilder : Avalonia.Controls.Maui.LifecycleEvents.IAvaloniaLifecycleBuilder
	{
		readonly ILifecycleBuilder _builder;

		public LifecycleBuilder(ILifecycleBuilder builder)
		{
			_builder = builder;
		}

		public void AddEvent<TDelegate>(string eventName, TDelegate action)
			where TDelegate : Delegate
		{
			_builder.AddEvent(eventName, action);
		}
	}
}