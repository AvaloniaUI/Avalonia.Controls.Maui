namespace ControlGallery.Pages.ShellSamples;

public static class ShellHelper
{
    public static Shell? GetShell(this BindableObject? bindable)
    {
        if (Shell.Current != null)
            return Shell.Current;

        // Drill down into FlyoutPage if necessary (used in Control Gallery)
        if (Application.Current?.MainPage is FlyoutPage flyout)
        {
            if (flyout.Detail is Shell shell)
                return shell;
            
            if (flyout.Detail is NavigationPage nav && nav.CurrentPage is Shell navShell)
                return navShell;
        }

        if (Application.Current?.MainPage is Shell mainShell)
            return mainShell;
            
        // If it's an Element, walk up the parent hierarchy
        if (bindable is Element element)
        {
            Element? current = element;
            while (current != null)
            {
                if (current is Shell shell)
                    return shell;
                current = current.Parent;
            }
        }

        return null;
    }
}
