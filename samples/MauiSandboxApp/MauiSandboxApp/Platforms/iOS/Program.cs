using System.Diagnostics.CodeAnalysis;
using ObjCRuntime;
using UIKit;

namespace MauiSandboxApp;

public class Program
{
	// This is the main entry point of the application.
	// SceneDelegate is only referenced by name from Info.plist, so keep the trimmer from removing it.
	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SceneDelegate))]
	static void Main(string[] args)
	{
		// if you want to use a different Application Delegate class from "AppDelegate"
		// you can specify it here.
		UIApplication.Main(args, null, typeof(AppDelegate));
	}
}
