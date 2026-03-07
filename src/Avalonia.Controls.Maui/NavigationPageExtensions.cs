using Avalonia.Animation;
using Avalonia.Controls.Maui.Animations;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Provides attached properties for configuring Avalonia Navigation Page on a MAUI <see cref="NavigationPage"/>.
/// </summary>
public static class NavigationPageExtensions
{
    /// <summary>
    /// Identifies the PageTransition attached property.
    /// </summary>
    public static readonly BindableProperty PageTransitionProperty =
        BindableProperty.CreateAttached(
            "PageTransition",
            typeof(IPageTransition),
            typeof(NavigationPageExtensions),
            null);

    /// <summary>
    /// Gets the Avalonia page transition for the specified <see cref="NavigationPage"/>.
    /// </summary>
    /// <param name="page">The navigation page.</param>
    /// <returns>The page transition, or <c>null</c> to use the default <see cref="MauiNavigationTransition"/>.</returns>
    public static IPageTransition? GetPageTransition(BindableObject page) =>
        (IPageTransition?)page.GetValue(PageTransitionProperty);

    /// <summary>
    /// Sets the Avalonia page transition for the specified <see cref="NavigationPage"/>.
    /// </summary>
    /// <param name="page">The navigation page.</param>
    /// <param name="value">The page transition to use, or <c>null</c> to use the default <see cref="MauiNavigationTransition"/>.</param>
    public static void SetPageTransition(BindableObject page, IPageTransition? value) =>
        page.SetValue(PageTransitionProperty, value);
}
