using Microsoft.Extensions.DependencyInjection;

namespace MyConference;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

#if DEBUG
		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			Console.Error.WriteLine($"UNHANDLED: {e.ExceptionObject}");
		TaskScheduler.UnobservedTaskException += (s, e) =>
			Console.Error.WriteLine($"UNOBSERVED: {e.Exception}");
#endif
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}