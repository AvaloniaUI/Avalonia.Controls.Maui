using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Maui.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Avalonia.Controls.Maui.Platform;

public class StackNavigationManager
{
    private readonly IMauiContext _mauiContext;
    private IView? _currentPage;
    private Page? _currentMauiPage;
    private NavigationView? _navigationView = null!;
    private IStackNavigation? _stackNavigation;
    private bool _connected;
    private ILogger? _logger;

    public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

    public IView CurrentPage => _currentPage ?? throw new InvalidOperationException("CurrentPage cannot be null");

    public IMauiContext MauiContext => _mauiContext;

    public NavigationView NavigationView => _navigationView ?? throw new InvalidOperationException("NavigationView is null");

    public StackNavigationManager(IMauiContext mauiContext)
    {
        _mauiContext = mauiContext;
        _logger = mauiContext.Services?.GetService(typeof(ILogger<StackNavigationManager>)) as ILogger<StackNavigationManager>;
    }

    /// <summary>
    /// Connects the navigation manager to the navigation view and virtual view.
    /// </summary>
    public virtual void Connect(IStackNavigation stackNavigation, NavigationView navigationView)
    {
        _connected = true;
        _stackNavigation = stackNavigation;
        _navigationView = navigationView;

        // Subscribe to property changes on NavigationPage to track TitleView changes
        if (stackNavigation is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnNavigationPagePropertyChanged;
        }

        // Wire up back button click
        _navigationView.BackButton.Click += OnBackButtonClicked;

        _logger?.LogDebug("StackNavigationManager connected");
    }

    /// <summary>
    /// Disconnects the navigation manager.
    /// </summary>
    public virtual void Disconnect(IStackNavigation stackNavigation, NavigationView navigationView)
    {
        if (stackNavigation is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged -= OnNavigationPagePropertyChanged;
        }

        if (_currentMauiPage is INotifyPropertyChanged currentPageNotify)
        {
            currentPageNotify.PropertyChanged -= OnCurrentPagePropertyChanged;
        }

        if (_navigationView != null)
        {
            _navigationView.BackButton.Click -= OnBackButtonClicked;
        }

        _connected = false;
        _stackNavigation = null;
        _navigationView = null;
        _currentMauiPage = null;

        _logger?.LogDebug("StackNavigationManager disconnected");
    }

    private void OnNavigationPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // When CurrentPage or navigation bar properties change, update the display
        if (e.PropertyName == nameof(NavigationPage.CurrentPage) ||
            e.PropertyName == nameof(NavigationPage.BarBackgroundColor) ||
            e.PropertyName == nameof(NavigationPage.BarTextColor))
        {
            UpdateTitleView();
        }
    }

    private void OnCurrentPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // When TitleView property changes on the current page, update the display
        if (e.PropertyName == "TitleView" || e.PropertyName == "Title")
        {
            UpdateTitleView();
        }
    }

    private async void OnBackButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_stackNavigation is NavigationPage navigationPage)
        {
            _logger?.LogDebug("Back button clicked, popping page");
            await navigationPage.PopAsync();
        }
    }

    /// <summary>
    /// Navigates to the requested navigation stack state.
    /// </summary>
    public virtual void NavigateTo(NavigationRequest request)
    {
        if (!_connected || _navigationView == null)
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

        // Determine if this is a back navigation
        bool isBackNavigation = false;
        if (!initialNavigation && previousNavigationStackCount > newPageStack.Count)
        {
            isBackNavigation = true;
        }

        // Unsubscribe from previous page's property changes
        if (_currentMauiPage is INotifyPropertyChanged oldPageNotify)
        {
            oldPageNotify.PropertyChanged -= OnCurrentPagePropertyChanged;
        }

        // Get the target page (top of the new stack)
        _currentPage = newPageStack[newPageStack.Count - 1];
        if (_currentPage == null)
        {
            throw new InvalidOperationException("Navigation Request Contains Null Elements");
        }

        // Subscribe to new page's property changes
        _currentMauiPage = _currentPage as Page;
        if (_currentMauiPage is INotifyPropertyChanged newPageNotify)
        {
            newPageNotify.PropertyChanged += OnCurrentPagePropertyChanged;
        }

        // Convert the MAUI IView to an Avalonia Control
        var platformPage = _currentPage.ToPlatform(MauiContext);
        if (platformPage is not Control control)
        {
            throw new InvalidOperationException($"Page must be converted to Avalonia Control, got {platformPage?.GetType().Name}");
        }

        // Update the navigation stack
        NavigationStack = newPageStack;

        // Navigate to the new page
        try
        {
            _navigationView.NavigateToPage(control, isBackNavigation);
            _navigationView.CurrentPage = _currentPage;

            // Update TitleView for the current page
            UpdateTitleView();

            // Fire navigation finished after the transition
            // In Avalonia, we can listen to TransitionCompleted event
            // but for now we'll fire immediately
            FireNavigationFinished();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during navigation");
            FireNavigationFinished();
            throw;
        }
    }

    private void UpdateTitleView()
    {
        if (_navigationView == null || _stackNavigation == null)
            return;

        // Get the TitleView from the current page or NavigationPage
        View? titleView = null;
        string? title = null;
        bool hasNavigationBar = true;
        ImageSource? titleIconImageSource = null;

        if (_stackNavigation is NavigationPage navigationPage)
        {
            // Check current page first
            if (navigationPage.CurrentPage is Page currentPage)
            {
                titleView = NavigationPage.GetTitleView(currentPage);
                title = currentPage.Title;
                hasNavigationBar = NavigationPage.GetHasNavigationBar(currentPage);
                titleIconImageSource = NavigationPage.GetTitleIconImageSource(currentPage);
            }

            // Fall back to NavigationPage itself if no page-specific TitleView
            if (titleView == null && title == null)
            {
                titleView = NavigationPage.GetTitleView(navigationPage);
                title = navigationPage.Title;
            }

            // Update navigation bar colors from MAUI theme
            UpdateNavigationBarColors(navigationPage);
        }

        // Update navigation bar visibility
        _navigationView.IsNavigationBarVisible = hasNavigationBar;

        // Convert and set the TitleView or Title text
        if (titleView != null)
        {
            var platformTitleView = titleView.ToPlatform(MauiContext);
            _navigationView.TitleViewContainer.Content = platformTitleView;
            _navigationView.TitleViewContainer.IsVisible = true;
            _navigationView.TitleTextBlock.IsVisible = false;
            _navigationView.TitleIconImage.IsVisible = false;
            _logger?.LogDebug("Updated TitleView: {TitleViewType}", titleView.GetType().Name);
        }
        else
        {
            _navigationView.TitleViewContainer.Content = null;
            _navigationView.TitleViewContainer.IsVisible = false;
            _navigationView.TitleTextBlock.Text = title ?? string.Empty;
            _navigationView.TitleTextBlock.IsVisible = !string.IsNullOrEmpty(title);

            // Update title icon
            UpdateTitleIcon(titleIconImageSource);

            _logger?.LogDebug("Updated Title: {Title}", title);
        }

        // Update back button visibility
        UpdateBackButton();
    }

    private void UpdateNavigationBarColors(NavigationPage navigationPage)
    {
        // Update bar background - prefer BarBackground (Brush) over BarBackgroundColor
        if (navigationPage.BarBackground != null && !navigationPage.BarBackground.IsEmpty)
        {
            _navigationView!.NavigationBarBackground = navigationPage.BarBackground.ToPlatform();
        }
        else if (navigationPage.BarBackgroundColor != null)
        {
            var color = navigationPage.BarBackgroundColor;
            _navigationView!.NavigationBarBackground = new Avalonia.Media.SolidColorBrush(
                Avalonia.Media.Color.FromArgb(
                    (byte)(color.Alpha * 255),
                    (byte)(color.Red * 255),
                    (byte)(color.Green * 255),
                    (byte)(color.Blue * 255)
                )
            );
        }
        else
        {
            // Default color
            _navigationView!.NavigationBarBackground = new Avalonia.Media.SolidColorBrush(
                Avalonia.Media.Color.Parse("#F0F0F0")
            );
        }

        // Update bar text color
        if (navigationPage.BarTextColor != null)
        {
            var textColor = navigationPage.BarTextColor;
            var avaloniaColor = Avalonia.Media.Color.FromArgb(
                (byte)(textColor.Alpha * 255),
                (byte)(textColor.Red * 255),
                (byte)(textColor.Green * 255),
                (byte)(textColor.Blue * 255)
            );
            _navigationView.TitleTextBlock.Foreground = new Avalonia.Media.SolidColorBrush(avaloniaColor);
            _navigationView.BackButton.Foreground = new Avalonia.Media.SolidColorBrush(avaloniaColor);
        }
        else
        {
            // Default text color (black)
            var defaultBrush = Avalonia.Media.Brushes.Black;
            _navigationView.TitleTextBlock.Foreground = defaultBrush;
            _navigationView.BackButton.Foreground = defaultBrush;
        }
    }

    private async void UpdateTitleIcon(ImageSource? titleIconImageSource)
    {
        if (_navigationView == null)
            return;

        if (titleIconImageSource == null)
        {
            _navigationView.TitleIconImage.Source = null;
            _navigationView.TitleIconImage.IsVisible = false;
            return;
        }

        try
        {
            var imageSourceServiceProvider = MauiContext.Services.GetService<IImageSourceServiceProvider>();
            var service = imageSourceServiceProvider?.GetImageSourceService(titleIconImageSource.GetType());

            if (service is Avalonia.Controls.Maui.Services.IAvaloniaImageSourceService avaloniaService)
            {
                var result = await avaloniaService.GetImageAsync(titleIconImageSource, 1.0f);
                if (result?.Value is Avalonia.Media.Imaging.Bitmap bitmap)
                {
                    _navigationView.TitleIconImage.Source = bitmap;
                    _navigationView.TitleIconImage.IsVisible = true;
                    _logger?.LogDebug("Updated TitleIconImageSource");
                }
                else
                {
                    _navigationView.TitleIconImage.Source = null;
                    _navigationView.TitleIconImage.IsVisible = false;
                }
            }
            else
            {
                _navigationView.TitleIconImage.Source = null;
                _navigationView.TitleIconImage.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading TitleIconImageSource");
            _navigationView.TitleIconImage.Source = null;
            _navigationView.TitleIconImage.IsVisible = false;
        }
    }

    private void UpdateBackButton()
    {
        if (_navigationView == null || _stackNavigation == null)
            return;

        // Show back button if navigation stack has more than 1 page
        bool showBackButton = NavigationStack.Count > 1;
        string? backButtonTitle = null;
        Microsoft.Maui.Graphics.Color? iconColor = null;

        if (_stackNavigation is NavigationPage navigationPage && navigationPage.CurrentPage is Page currentPage)
        {
            // Check if page explicitly sets HasBackButton
            showBackButton = showBackButton && NavigationPage.GetHasBackButton(currentPage);

            // Get the IconColor from the current page
            iconColor = NavigationPage.GetIconColor(currentPage);

            // Get the BackButtonTitle from the previous page (the one we'd go back to)
            if (NavigationStack.Count > 1 && NavigationStack[NavigationStack.Count - 2] is Page previousPage)
            {
                backButtonTitle = NavigationPage.GetBackButtonTitle(previousPage);
            }
        }

        _navigationView.BackButton.IsVisible = showBackButton;

        // Update back button content - show title if available, otherwise show arrow
        if (!string.IsNullOrEmpty(backButtonTitle))
        {
            _navigationView.BackButton.Content = $"← {backButtonTitle}";
        }
        else
        {
            _navigationView.BackButton.Content = "←";
        }

        // Update back button icon color if specified
        if (iconColor != null)
        {
            _navigationView.BackButton.Foreground = new Avalonia.Media.SolidColorBrush(
                Avalonia.Media.Color.FromArgb(
                    (byte)(iconColor.Alpha * 255),
                    (byte)(iconColor.Red * 255),
                    (byte)(iconColor.Green * 255),
                    (byte)(iconColor.Blue * 255)
                )
            );
        }
        // Note: If iconColor is null, the color will be set by UpdateNavigationBarColors based on BarTextColor

        _logger?.LogDebug("Back button visible: {IsVisible}, title: {Title}, iconColor: {IconColor}", showBackButton, backButtonTitle, iconColor);
    }

    private void FireNavigationFinished()
    {
        _logger?.LogDebug("Navigation finished, stack size: {StackSize}", NavigationStack.Count);
        _stackNavigation?.NavigationFinished(NavigationStack);
    }
}
