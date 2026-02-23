using AlohaAI.Services;
using AlohaAI.Views;

namespace AlohaAI;

public partial class App : Application
{
	private readonly IProgressService _progressService;

	public App(IProgressService progressService)
	{
		InitializeComponent();
		_progressService = progressService;

		// Force dark mode as default for the tropical sunset theme
		UserAppTheme = AppTheme.Dark;

		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			System.Diagnostics.Debug.WriteLine($"Unhandled: {e.ExceptionObject}");

		TaskScheduler.UnobservedTaskException += (s, e) =>
		{
			System.Diagnostics.Debug.WriteLine($"Unobserved task: {e.Exception}");
			e.SetObserved();
		};
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new LoadingPage(_progressService));
	}
}