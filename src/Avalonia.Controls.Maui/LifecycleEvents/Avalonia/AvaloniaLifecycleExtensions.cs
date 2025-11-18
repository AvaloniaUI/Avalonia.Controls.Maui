using Microsoft.Maui.LifecycleEvents;

namespace Avalonia.Controls.Maui.LifecycleEvents;

/// <summary>
/// Provides extension methods for configuring Avalonia lifecycle events in a Maui application.
/// </summary>
public static class AvaloniaLifecycleExtensions
{
	/// <summary>
	/// Adds Avalonia-specific lifecycle events for Windows to the Maui lifecycle builder.
	/// </summary>
	/// <param name="builder">The Maui lifecycle builder.</param>
	/// <param name="configureDelegate">A delegate to configure Avalonia lifecycle events.</param>
	/// <returns>The original lifecycle builder.</returns>
	public static ILifecycleBuilder AddWindows(this ILifecycleBuilder builder, Action<IAvaloniaLifecycleBuilder> configureDelegate)
	{
		var windows = new LifecycleBuilder(builder);

		configureDelegate?.Invoke(windows);

		return builder;
	}

	/// <summary>
	/// Implements the <see cref="IAvaloniaLifecycleBuilder"/> interface for adding lifecycle events.
	/// </summary>
	class LifecycleBuilder : IAvaloniaLifecycleBuilder
	{
		readonly ILifecycleBuilder _builder;

		/// <summary>
		/// Initializes a new instance of the <see cref="LifecycleBuilder"/> class.
		/// </summary>
		/// <param name="builder">The Maui lifecycle builder.</param>
		public LifecycleBuilder(ILifecycleBuilder builder)
		{
			_builder = builder;
		}

		/// <summary>
		/// Adds a lifecycle event to the builder.
		/// </summary>
		/// <typeparam name="TDelegate">The delegate type for the event.</typeparam>
		/// <param name="eventName">The name of the event.</param>
		/// <param name="action">The delegate to invoke for the event.</param>
		public void AddEvent<TDelegate>(string eventName, TDelegate action)
			where TDelegate : Delegate
		{
			_builder.AddEvent(eventName, action);
		}
	}
}