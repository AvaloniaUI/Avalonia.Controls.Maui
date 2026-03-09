using Avalonia.Animation;
using Avalonia.Controls.Maui.Animations;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using AvaloniaContentPage = Avalonia.Controls.ContentPage;
using AvaloniaNavigationPage = Avalonia.Controls.NavigationPage;
using MauiShell = Microsoft.Maui.Controls.Shell;

namespace Avalonia.Controls.Maui.Handlers.Shell;

/// <summary>
/// A <see cref="StackNavigationManager"/> subclass tailored for Shell navigation.
/// Hides the NavigationPage's nav bar (Shell manages its own), resolves transitions
/// from the parent Shell, and supports PresentationMode-based transitions.
/// </summary>
internal class ShellStackNavigationManager : StackNavigationManager
{
    private static readonly TimeSpan ModalTransitionDuration = TimeSpan.FromMilliseconds(400);

    /// <summary>
    /// Initializes a new instance of <see cref="ShellStackNavigationManager"/>.
    /// </summary>
    /// <param name="mauiContext">The MAUI context.</param>
    public ShellStackNavigationManager(IMauiContext mauiContext)
        : base(mauiContext)
    {
    }

    /// <inheritdoc/>
    public override void Connect(IStackNavigation stackNavigation, AvaloniaNavigationPage navigationPage)
    {
        // Shell manages its own navigation bar; hide the NavigationPage's bar.
        AvaloniaNavigationPage.SetHasNavigationBar(navigationPage, false);

        base.Connect(stackNavigation, navigationPage);
    }

    /// <inheritdoc/>
    protected override AvaloniaContentPage WrapPage(IView mauiPage)
    {
        var page = base.WrapPage(mauiPage);
        // Shell manages the nav bar; hide it on individual pages.
        AvaloniaNavigationPage.SetHasNavigationBar(page, false);
        return page;
    }

    /// <inheritdoc/>
    protected override IPageTransition? ResolveTransition(NavigationRequest request)
    {
        if (!request.Animated)
            return null;

        // Check PresentationMode on the top page of the new stack
        var topView = request.NavigationStack.Count > 0
            ? request.NavigationStack[request.NavigationStack.Count - 1]
            : null;

        if (topView is Microsoft.Maui.Controls.Page topPage)
        {
            var mode = MauiShell.GetPresentationMode(topPage);

            if (mode.HasFlag(PresentationMode.NotAnimated))
                return null;

            if (mode.HasFlag(PresentationMode.Modal))
                return new PageSlide(ModalTransitionDuration, PageSlide.SlideAxis.Vertical);
        }

        // Use user-configured transition from the parent Shell, or default.
        var shell = FindParentShell();
        if (shell != null)
        {
            var userTransition = NavigationPageExtensions.GetPageTransition(shell);
            if (userTransition != null)
                return userTransition;
        }

        return new MauiNavigationTransition();
    }

    /// <inheritdoc/>
    protected override void OnNavigationCompleted()
    {
        // Shell manages its own toolbar — skip the base implementation
        // which updates NavigationPage toolbar/back button/colors.
    }

    private MauiShell? FindParentShell()
    {
        Element? current = StackNavigation as Element;
        while (current != null)
        {
            if (current is MauiShell shell)
                return shell;
            current = current.Parent;
        }

        return MauiShell.Current;
    }
}
