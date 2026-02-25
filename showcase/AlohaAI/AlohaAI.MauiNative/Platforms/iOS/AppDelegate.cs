using Foundation;
using UIKit;

namespace AlohaAI;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
	{
		var result = base.FinishedLaunching(application, launchOptions);

		SetupWindowBackground();

		return result;
	}

	private void SetupWindowBackground()
	{
		var scene = UIApplication.SharedApplication.ConnectedScenes
			?.ToArray()
			?.OfType<UIWindowScene>()
			.FirstOrDefault();

		if (scene == null) return;

		foreach (var window in scene.Windows)
		{
			window.BackgroundColor = UIColor.FromRGB(25, 15, 50);

			var image = UIImage.FromBundle("bg_sunset");
			if (image == null) continue;

			var bgView = new UIImageView(window.Bounds)
			{
				Image = image,
				ContentMode = UIViewContentMode.ScaleAspectFill,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				ClipsToBounds = true,
				Tag = 9999
			};
			window.InsertSubview(bgView, 0);

			// Also make root view controller's view transparent
			if (window.RootViewController?.View != null)
			{
				window.RootViewController.View.BackgroundColor = UIColor.Clear;
			}
		}
	}
}
