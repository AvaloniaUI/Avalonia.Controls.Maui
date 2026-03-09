using CommunityToolkit.Maui.Markup;

namespace MauiConverter;

class App : Application
{
	readonly AppShell _appShell;

	public App(AppShell appShell)
	{
		_appShell = appShell;

		Resources = new ResourceDictionary()
		{
			new Style<Shell>(
				(Shell.NavBarHasShadowProperty, true),
				(Shell.TitleColorProperty, Colors.White),
				(Shell.DisabledColorProperty, Colors.White),
				(Shell.UnselectedColorProperty, Colors.White),
				(Shell.ForegroundColorProperty, Colors.White),
				(Shell.BackgroundColorProperty, ColorConstants.DarkPurple)).ApplyToDerivedTypes(true),

			new Style<NavigationPage>(
				(NavigationPage.BarTextColorProperty, Colors.White),
				(NavigationPage.BarBackgroundColorProperty, ColorConstants.DarkPurple)).ApplyToDerivedTypes(true)
		};

		MainPage = _appShell;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = base.CreateWindow(activationState);
		window.TitleBar = new TitleBar
		{
			Title = "MauiConverter",
			BackgroundColor = ColorConstants.DarkPurple,
			ForegroundColor = Colors.White,
		};

		#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
		window.Width = 400;
		window.Height = 500;
		#endif

		// TitleBar extends into client area which isn't supported on Linux
		// On Catalyst we don't need the custom window titlebar.
		if (OperatingSystem.IsLinux() || OperatingSystem.IsMacCatalyst())
		{
			window.TitleBar = null;
		}

		return window;
	}
}