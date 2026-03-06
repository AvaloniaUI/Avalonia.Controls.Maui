using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Extensions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using AvaloniaContentPage = Avalonia.Controls.ContentPage;
using AvaloniaNavigationPage = Avalonia.Controls.NavigationPage;
using MauiPage = Microsoft.Maui.Controls.Page;
using MauiElement = Microsoft.Maui.Controls.Element;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Manages stack-based navigation for MAUI NavigationPage, coordinating page transitions and toolbar updates within an Avalonia <see cref="AvaloniaNavigationPage"/>.
/// </summary>
public class StackNavigationManager
{
    private readonly IMauiContext _mauiContext;
    private IView? _currentPage;
    private MauiPage? _currentMauiPage;
    private AvaloniaNavigationPage? _navigationPage;
    private IStackNavigation? _stackNavigation;
    private bool _connected;
    private bool _isNavigatingFromMaui;
    private ILogger? _logger;
    private FlyoutPage? _parentFlyoutPage;

    /// <summary>
    /// Gets the current page navigation stack.
    /// </summary>
    public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

    /// <summary>
    /// Gets the currently displayed page.
    /// </summary>
    public IView CurrentPage => _currentPage ?? throw new InvalidOperationException("CurrentPage cannot be null");

    /// <summary>
    /// Gets the MAUI context used for handler resolution and service access.
    /// </summary>
    public IMauiContext MauiContext => _mauiContext;

    /// <summary>
    /// Gets the Avalonia <see cref="AvaloniaNavigationPage"/> control that hosts the navigation UI.
    /// </summary>
    public AvaloniaNavigationPage NavigationPage => _navigationPage ?? throw new InvalidOperationException("NavigationPage is null");

    /// <summary>
    /// Initializes a new instance of the <see cref="StackNavigationManager"/> class.
    /// </summary>
    /// <param name="mauiContext">The MAUI context used for handler resolution and service access.</param>
    public StackNavigationManager(IMauiContext mauiContext)
    {
        _mauiContext = mauiContext;
        _logger = mauiContext.Services?.GetService(typeof(ILogger<StackNavigationManager>)) as ILogger<StackNavigationManager>;
    }

    /// <summary>
    /// Connects the navigation manager to the navigation page and virtual view.
    /// </summary>
    public virtual void Connect(IStackNavigation stackNavigation, AvaloniaNavigationPage navigationPage)
    {
        _connected = true;
        _stackNavigation = stackNavigation;
        _navigationPage = navigationPage;

        // Subscribe to Avalonia NavigationPage pop events to sync back to MAUI
        _navigationPage.Popped += OnAvaloniaPoppedPage;

        // Subscribe to property changes on NavigationPage to track TitleView changes
        if (stackNavigation is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnNavigationPagePropertyChanged;
        }

        // Detect FlyoutPage parent for hamburger button visibility
        if (stackNavigation is Microsoft.Maui.Controls.NavigationPage navPage)
        {
            _parentFlyoutPage = FindParentFlyoutPage(navPage);
            if (_parentFlyoutPage != null)
            {
                _parentFlyoutPage.PropertyChanged += OnParentFlyoutPagePropertyChanged;
            }
            else
            {
                // Parent may not be set yet; listen for late parenting
                navPage.ParentChanged += OnNavigationPageParentChanged;
            }
        }

        _logger?.LogDebug("StackNavigationManager connected");
    }

    /// <summary>
    /// Disconnects the navigation manager.
    /// </summary>
    public virtual void Disconnect(IStackNavigation stackNavigation, AvaloniaNavigationPage navigationPage)
    {
        navigationPage.Popped -= OnAvaloniaPoppedPage;

        if (stackNavigation is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged -= OnNavigationPagePropertyChanged;
        }

        if (_currentMauiPage != null)
        {
            if (_currentMauiPage is INotifyPropertyChanged currentPageNotify)
            {
                currentPageNotify.PropertyChanged -= OnCurrentPagePropertyChanged;
            }

            if (_currentMauiPage.ToolbarItems is INotifyCollectionChanged toolbarItemsNotify)
            {
                toolbarItemsNotify.CollectionChanged -= OnToolbarItemsCollectionChanged;
            }
        }

        if (_parentFlyoutPage != null)
        {
            _parentFlyoutPage.PropertyChanged -= OnParentFlyoutPagePropertyChanged;
            _parentFlyoutPage = null;
        }

        if (stackNavigation is Microsoft.Maui.Controls.NavigationPage navPage)
        {
            navPage.ParentChanged -= OnNavigationPageParentChanged;
        }

        _connected = false;
        _stackNavigation = null;
        _navigationPage = null;
        _currentPage = null;
        _currentMauiPage = null;

        _logger?.LogDebug("StackNavigationManager disconnected");
    }

    private void OnNavigationPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Microsoft.Maui.Controls.NavigationPage.CurrentPage))
        {
            UpdateCurrentPageWrapper();
        }
        else if (e.PropertyName == nameof(Microsoft.Maui.Controls.NavigationPage.BarBackgroundColor) ||
                 e.PropertyName == nameof(Microsoft.Maui.Controls.NavigationPage.BarTextColor) ||
                 e.PropertyName == nameof(Microsoft.Maui.Controls.NavigationPage.BarBackground))
        {
            UpdateNavigationBarColors();
        }
    }

    private void OnCurrentPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is "TitleView" or "Title" or "TitleIconImageSource" or "IconColor")
        {
            UpdateCurrentPageWrapper();
        }
    }

    private void OnParentFlyoutPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FlyoutPage.FlyoutLayoutBehavior))
        {
            UpdateBackButton();
        }
    }

    private void OnNavigationPageParentChanged(object? sender, EventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.NavigationPage navPage)
        {
            navPage.ParentChanged -= OnNavigationPageParentChanged;

            _parentFlyoutPage = FindParentFlyoutPage(navPage);
            if (_parentFlyoutPage != null)
            {
                _parentFlyoutPage.PropertyChanged += OnParentFlyoutPagePropertyChanged;
                UpdateBackButton();
            }
        }
    }

    private static FlyoutPage? FindParentFlyoutPage(MauiPage page)
    {
        var parent = page.Parent;
        while (parent != null)
        {
            if (parent is FlyoutPage fp)
                return fp;
            parent = (parent as MauiElement)?.Parent;
        }
        return null;
    }

    private void OnToolbarItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateCurrentPageWrapper();
    }

    private async void OnAvaloniaPoppedPage(object? sender, Avalonia.Controls.NavigationEventArgs e)
    {
        // If this pop was initiated by us (via NavigateTo), MAUI already knows about it.
        if (_isNavigatingFromMaui)
            return;

        // User-initiated pop (back button, swipe gesture, etc.)
        // Route through MAUI so it updates its own navigation stack.
        if (_stackNavigation is Microsoft.Maui.Controls.NavigationPage mauiNavPage)
        {
            _logger?.LogDebug("User-initiated back navigation detected, syncing to MAUI");
            await mauiNavPage.PopAsync();
        }
    }

    /// <summary>
    /// Navigates to the requested navigation stack state.
    /// </summary>
    public virtual async Task NavigateTo(NavigationRequest request)
    {
        if (!_connected || _navigationPage == null)
        {
            _logger?.LogWarning("NavigateTo called but manager is not connected");
            return;
        }

        var newPageStack = new List<IView>(request.NavigationStack);
        var previousNavigationStack = NavigationStack;
        var previousNavigationStackCount = previousNavigationStack.Count;
        bool initialNavigation = NavigationStack.Count == 0;

        _logger?.LogDebug("NavigateTo: stack size {NewSize}, previous size {PreviousSize}, animated: {Animated}",
            newPageStack.Count, previousNavigationStackCount, request.Animated);

        // Disconnect handlers of pages that were removed from the stack
        if (!initialNavigation && previousNavigationStackCount > newPageStack.Count)
        {
            for (int i = newPageStack.Count; i < previousNavigationStackCount; i++)
            {
                if (previousNavigationStack[i] is IView poppedView)
                {
                    poppedView.DisconnectHandlers();
                }
            }
        }

        // Unsubscribe from previous page's property changes
        if (_currentMauiPage is INotifyPropertyChanged oldPageNotify)
        {
            oldPageNotify.PropertyChanged -= OnCurrentPagePropertyChanged;
        }

        if (_currentMauiPage != null && _currentMauiPage.ToolbarItems is INotifyCollectionChanged oldToolbarItemsNotify)
        {
            oldToolbarItemsNotify.CollectionChanged -= OnToolbarItemsCollectionChanged;
        }

        // Get the target page (top of the new stack)
        _currentPage = newPageStack[newPageStack.Count - 1];
        if (_currentPage == null)
        {
            throw new InvalidOperationException("Navigation Request Contains Null Elements");
        }

        // Subscribe to new page's property changes
        _currentMauiPage = _currentPage as MauiPage;
        if (_currentMauiPage is INotifyPropertyChanged newPageNotify)
        {
            newPageNotify.PropertyChanged += OnCurrentPagePropertyChanged;
        }

        if (_currentMauiPage != null && _currentMauiPage.ToolbarItems is INotifyCollectionChanged newToolbarItemsNotify)
        {
            newToolbarItemsNotify.CollectionChanged += OnToolbarItemsCollectionChanged;
        }

        // Wrap the MAUI page in an Avalonia ContentPage
        var wrappedPage = MauiPageWrapper.Wrap(_currentPage, MauiContext);

        // Determine what changed
        IView? previousTopPage = previousNavigationStackCount > 0
            ? previousNavigationStack[previousNavigationStackCount - 1]
            : null;
        bool topPageChanged = !ReferenceEquals(_currentPage, previousTopPage);

        // Update the navigation stack
        NavigationStack = newPageStack;

        _isNavigatingFromMaui = true;
        try
        {
            Avalonia.Animation.IPageTransition? transition = request.Animated
                ? _navigationPage.PageTransition
                : null;

            if (initialNavigation)
            {
                await _navigationPage.PushAsync(wrappedPage, transition);
            }
            else if (!topPageChanged)
            {
                // Stack changed but the visible page is the same (e.g., InsertPageBefore,
                // RemovePage on a non-top page). Silently sync the Avalonia stack so
                // future back-navigation reveals the correct pages.
                SyncNonTopPages(newPageStack);
            }
            else if (previousNavigationStackCount > newPageStack.Count)
            {
                // Back navigation — stack shrunk.
                // First sync pages below the new top so PopToPageAsync can find it.
                SyncNonTopPages(newPageStack);

                if (_navigationPage.NavigationStack.Contains(wrappedPage))
                {
                    await _navigationPage.PopToPageAsync(wrappedPage, transition);
                }
                else
                {
                    // Target page isn't in the Avalonia stack (edge case) — replace instead.
                    await _navigationPage.ReplaceAsync(wrappedPage, transition);
                }
            }
            else if (newPageStack.Count > previousNavigationStackCount)
            {
                // Forward navigation — stack grew.
                await _navigationPage.PushAsync(wrappedPage, transition);
            }
            else
            {
                // Same depth, different page — replace.
                await _navigationPage.ReplaceAsync(wrappedPage, transition);
            }

            UpdateCurrentPageWrapper();
            FireNavigationFinished();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during navigation");
            FireNavigationFinished();
            throw;
        }
        finally
        {
            _isNavigatingFromMaui = false;
        }
    }

    /// <summary>
    /// Silently syncs the Avalonia NavigationPage's non-top pages to match the MAUI stack.
    /// Uses <see cref="AvaloniaNavigationPage.InsertPage"/> and <see cref="AvaloniaNavigationPage.RemovePage"/>
    /// which modify the stack without animation, so future back-navigation reveals the correct pages.
    /// </summary>
    private void SyncNonTopPages(IList<IView> targetMauiStack)
    {
        if (_navigationPage == null) return;

        // Build the target list of wrapped pages
        var targetWrapped = new List<AvaloniaContentPage>(targetMauiStack.Count);
        for (int i = 0; i < targetMauiStack.Count; i++)
            targetWrapped.Add(MauiPageWrapper.Wrap(targetMauiStack[i], MauiContext));

        var avaloniaStack = _navigationPage.NavigationStack;
        var avaloniaTop = avaloniaStack.Count > 0 ? avaloniaStack[avaloniaStack.Count - 1] : null;

        // Remove Avalonia pages (except the current top) that aren't in the target stack
        for (int i = avaloniaStack.Count - 1; i >= 0; i--)
        {
            var page = avaloniaStack[i];
            if (page != avaloniaTop && !targetWrapped.Contains(page))
                _navigationPage.RemovePage(page);
        }

        // Re-read after removals
        avaloniaStack = _navigationPage.NavigationStack;

        // Insert missing pages at their correct positions.
        // Walk the target list and insert any page not yet present,
        // placing it before the next target page that IS in the Avalonia stack.
        for (int i = 0; i < targetWrapped.Count; i++)
        {
            if (avaloniaStack.Contains(targetWrapped[i]))
                continue;

            // Find the next page in the target that already exists in the Avalonia stack
            Avalonia.Controls.Page? insertBefore = null;
            for (int j = i + 1; j < targetWrapped.Count; j++)
            {
                if (avaloniaStack.Contains(targetWrapped[j]))
                {
                    insertBefore = targetWrapped[j];
                    break;
                }
            }

            if (insertBefore != null)
            {
                _navigationPage.InsertPage(targetWrapped[i], insertBefore);
                avaloniaStack = _navigationPage.NavigationStack; // re-read after insert
            }
        }

        _logger?.LogDebug("SyncNonTopPages: Avalonia stack depth now {Depth}", _navigationPage.StackDepth);
    }

    private void UpdateCurrentPageWrapper()
    {
        if (_navigationPage == null || _stackNavigation == null || _currentPage == null)
            return;

        // Update the wrapper's properties
        if (MauiPageWrapper.TryGetWrapper(_currentPage, out var wrapper) && wrapper != null)
        {
            MauiPageWrapper.UpdateProperties(wrapper, _currentPage);
        }

        // Update back button
        UpdateBackButton();

        // Update navigation bar colors
        UpdateNavigationBarColors();
    }

    private void UpdateBackButton()
    {
        if (_navigationPage == null || _stackNavigation == null)
            return;

        bool showBackButton = NavigationStack.Count > 1;
        bool showHamburger = false;

        if (_stackNavigation is Microsoft.Maui.Controls.NavigationPage navigationPage &&
            navigationPage.CurrentPage is MauiPage currentPage)
        {
            showBackButton = showBackButton && Microsoft.Maui.Controls.NavigationPage.GetHasBackButton(currentPage);

            // Get back button title from previous page
            var mauiNavStack = navigationPage.Navigation?.NavigationStack;
            if (mauiNavStack != null && mauiNavStack.Count > 1)
            {
                var previousPage = mauiNavStack[mauiNavStack.Count - 2];
                var backButtonTitle = Microsoft.Maui.Controls.NavigationPage.GetBackButtonTitle(previousPage);
                if (!string.IsNullOrEmpty(backButtonTitle))
                {
                    // Set back button content on the current wrapper page
                    if (MauiPageWrapper.TryGetWrapper(_currentPage!, out var wrapper) && wrapper != null)
                    {
                        AvaloniaNavigationPage.SetBackButtonContent(wrapper, $"\u2190 {backButtonTitle}");
                    }
                }
            }
        }

        // Determine hamburger button visibility
        if (_parentFlyoutPage != null && NavigationStack.Count <= 1)
        {
            showHamburger = _parentFlyoutPage.ShouldShowToolbarButton();
        }

        // At root with flyout parent: show hamburger as back button content
        if (showHamburger && MauiPageWrapper.TryGetWrapper(_currentPage!, out var rootWrapper) && rootWrapper != null)
        {
            AvaloniaNavigationPage.SetBackButtonContent(rootWrapper, "\u2630");
            AvaloniaNavigationPage.SetHasBackButton(rootWrapper, true);
            _navigationPage.IsBackButtonVisible = true;

            // Wire hamburger click behavior via the NavigationPage's back button event
            _navigationPage.PageNavigationSystemBackButtonPressed -= OnHamburgerBackPressed;
            _navigationPage.AddHandler(
                Avalonia.Controls.Page.PageNavigationSystemBackButtonPressedEvent,
                OnHamburgerBackPressed);
        }
        else
        {
            _navigationPage.IsBackButtonVisible = showBackButton;

            if (!showBackButton && MauiPageWrapper.TryGetWrapper(_currentPage!, out var currentWrapper) && currentWrapper != null)
            {
                AvaloniaNavigationPage.SetHasBackButton(currentWrapper, false);
            }
        }

        _logger?.LogDebug("Back button visible: {IsVisible}, hamburger: {HamburgerVisible}",
            showBackButton, showHamburger);
    }

    private void UpdateNavigationBarColors()
    {
        if (_navigationPage == null || _stackNavigation is not Microsoft.Maui.Controls.NavigationPage navigationPage)
            return;

        // Update bar background — prefer BarBackground (Brush) over BarBackgroundColor
        if (navigationPage.BarBackground != null && !navigationPage.BarBackground.IsEmpty)
        {
            _navigationPage.Resources["NavigationBarBackground"] = navigationPage.BarBackground.ToPlatform();
        }
        else if (navigationPage.BarBackgroundColor is { } barBgColor)
        {
            _navigationPage.Resources["NavigationBarBackground"] = new Avalonia.Media.SolidColorBrush(
                barBgColor.ToAvaloniaColor());
        }
        else
        {
            _navigationPage.Resources.Remove("NavigationBarBackground");
        }

        // Update bar foreground color.
        // Per-page IconColor takes highest priority (matches MAUI's NavigationPageToolbar.GetIconColor).
        // Then BarTextColor if explicitly set by the user (not the theme default).
        var iconColor = _currentMauiPage != null
            ? Microsoft.Maui.Controls.NavigationPage.GetIconColor(_currentMauiPage)
            : null;

        if (iconColor != null)
        {
            _navigationPage.Resources["NavigationBarForeground"] = new Avalonia.Media.SolidColorBrush(
                iconColor.ToAvaloniaColor());
        }
        else if (navigationPage.BarTextColor is { } barTextColor)
        {
            _navigationPage.Resources["NavigationBarForeground"] = new Avalonia.Media.SolidColorBrush(
                barTextColor.ToAvaloniaColor());
        }
        else
        {
            _navigationPage.Resources.Remove("NavigationBarForeground");
        }
    }

    private void OnHamburgerBackPressed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Only handle as hamburger when at root of stack
        if (_parentFlyoutPage != null && NavigationStack.Count <= 1)
        {
            _parentFlyoutPage.IsPresented = !_parentFlyoutPage.IsPresented;
            e.Handled = true;
        }
    }

    private void FireNavigationFinished()
    {
        _logger?.LogDebug("Navigation finished, stack size: {StackSize}", NavigationStack.Count);
        _stackNavigation?.NavigationFinished(NavigationStack);
    }
}
