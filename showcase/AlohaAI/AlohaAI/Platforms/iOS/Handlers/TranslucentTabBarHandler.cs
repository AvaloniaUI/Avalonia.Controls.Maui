using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using UIKit;

namespace AlohaAI.Platforms.iOS.Handlers;

public class TranslucentShellRenderer : ShellRenderer
{
    protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
    {
        return new DefaultTabBarAppearanceTracker();
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        ClearBackgrounds();
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);
        ClearBackgrounds();
    }

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        ClearBackgrounds();
    }

    private void ClearBackgrounds()
    {
        if (View == null) return;

        View.BackgroundColor = UIColor.Clear;
        View.Opaque = false;

        var current = View.Superview;
        while (current != null && current is not UIWindow)
        {
            current.BackgroundColor = UIColor.Clear;
            current.Opaque = false;
            current = current.Superview;
        }

        ClearChildBackgrounds(View);
    }

    private static void ClearChildBackgrounds(UIView view)
    {
        foreach (var child in view.Subviews)
        {
            if (child is UITabBar) continue;

            child.BackgroundColor = UIColor.Clear;
            child.Opaque = false;
            ClearChildBackgrounds(child);
        }
    }
}

public class DefaultTabBarAppearanceTracker : IShellTabBarAppearanceTracker
{
    public void SetAppearance(UITabBarController controller, ShellAppearance appearance)
    {
        var tabBar = controller.TabBar;
        var barAppearance = new UITabBarAppearance();
        barAppearance.ConfigureWithDefaultBackground();

        barAppearance.BackgroundEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.SystemUltraThinMaterialDark);
        barAppearance.BackgroundColor = UIColor.FromRGBA(30, 20, 52, 40);
        barAppearance.ShadowColor = UIColor.FromRGBA(255, 255, 255, 12);

        tabBar.StandardAppearance = barAppearance;
        tabBar.ScrollEdgeAppearance = barAppearance;
        tabBar.Translucent = true;
        tabBar.TintColor = UIColor.FromRGB(232, 139, 191);
        tabBar.UnselectedItemTintColor = UIColor.FromRGBA(184, 176, 194, 180);
    }

    public void ResetAppearance(UITabBarController controller)
    {
        var tabBar = controller.TabBar;
        var barAppearance = new UITabBarAppearance();
        barAppearance.ConfigureWithDefaultBackground();
        tabBar.StandardAppearance = barAppearance;
        tabBar.ScrollEdgeAppearance = barAppearance;
    }

    public void UpdateLayout(UITabBarController controller) { }

    public void Dispose() { }
}
