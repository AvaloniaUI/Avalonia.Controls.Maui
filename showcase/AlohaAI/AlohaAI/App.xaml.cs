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

		// Load saved theme preference (async fire-and-forget on startup)
		_ = LoadThemeAsync();

		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			System.Diagnostics.Debug.WriteLine($"Unhandled: {e.ExceptionObject}");

		TaskScheduler.UnobservedTaskException += (s, e) =>
		{
			System.Diagnostics.Debug.WriteLine($"Unobserved task: {e.Exception}");
			e.SetObserved();
		};
	}

	private async Task LoadThemeAsync()
	{
		try
		{
			var saved = await _progressService.GetSettingAsync("app_theme");
			switch (saved)
			{
				case "dark":
					UserAppTheme = AppTheme.Dark;
					break;
				case "light":
					UserAppTheme = AppTheme.Light;
					break;
				// "system" or null => follow system theme (don't set UserAppTheme)
			}
		}
		catch
		{
			// If settings aren't available yet, follow system theme
		}
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new LoadingPage(_progressService)) { Title = "AlohaAI" };
	}
}
