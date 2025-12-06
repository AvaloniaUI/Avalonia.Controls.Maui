using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Platform;

public class NavigationView : DockPanel
{
    private readonly TransitioningContentControl _contentControl;
    private readonly ContentControl _titleViewContainer;
    private readonly TextBlock _titleTextBlock;
    private readonly Button _backButton;
    private readonly DockPanel _navigationBar;
    private readonly Image _titleIconImage;
    private readonly StackPanel _titleStack;

    /// <summary>
    /// Gets or sets the current page being displayed in the navigation view.
    /// </summary>
    public IView? CurrentPage { get; set; }

    /// <summary>
    /// Gets or sets the cross-platform layout interface for the navigation view.
    /// </summary>
    public IStackNavigationView? CrossPlatformLayout { get; set; }

    /// <summary>
    /// Gets the container for the custom title view.
    /// </summary>
    public ContentControl TitleViewContainer => _titleViewContainer;

    /// <summary>
    /// Gets the text block for displaying the page title.
    /// </summary>
    public TextBlock TitleTextBlock => _titleTextBlock;

    /// <summary>
    /// Gets the back button.
    /// </summary>
    public Button BackButton => _backButton;

    /// <summary>
    /// Gets the title icon image control.
    /// </summary>
    public Image TitleIconImage => _titleIconImage;

    /// <summary>
    /// Gets or sets the navigation bar visibility.
    /// </summary>
    public bool IsNavigationBarVisible
    {
        get => _navigationBar.IsVisible;
        set => _navigationBar.IsVisible = value;
    }

    /// <summary>
    /// Gets or sets the navigation bar background brush.
    /// </summary>
    public IBrush? NavigationBarBackground
    {
        get => _navigationBar.Background;
        set => _navigationBar.Background = value;
    }

    public NavigationView()
    {
        LastChildFill = true;

        _navigationBar = new DockPanel
        {
            Height = 44,
            Background = Brushes.Transparent, // Will be set by UpdateNavigationBarColors
            LastChildFill = true
        };
        DockPanel.SetDock(_navigationBar, Dock.Top);

        _backButton = new Button
        {
            Content = "←",
            FontSize = 16,
            MinWidth = 40,
            Height = 40,
            IsVisible = false,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(8, 0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        DockPanel.SetDock(_backButton, Dock.Left);
        _navigationBar.Children.Add(_backButton);

        _titleViewContainer = new ContentControl
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            IsVisible = false
        };

        _titleIconImage = new Image
        {
            Width = 24,
            Height = 24,
            IsVisible = false,
            VerticalAlignment = VerticalAlignment.Center
        };

        _titleTextBlock = new TextBlock
        {
            FontSize = 16,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = Avalonia.Media.TextAlignment.Center
        };

        // Stack for title icon and text
        _titleStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8
        };
        _titleStack.Children.Add(_titleIconImage);
        _titleStack.Children.Add(_titleTextBlock);

        var centerStack = new Panel();
        centerStack.Children.Add(_titleStack);
        centerStack.Children.Add(_titleViewContainer);

        _navigationBar.Children.Add(centerStack);

        _contentControl = new TransitioningContentControl
        {
            PageTransition = new CrossFade(TimeSpan.FromMilliseconds(200))
        };

        Children.Add(_navigationBar);
        Children.Add(_contentControl);
    }

    /// <summary>
    /// Navigates to a new page with optional animation.
    /// </summary>
    /// <param name="page">The page to navigate to.</param>
    /// <param name="isBackNavigation">True if this is a back navigation (will reverse the transition).</param>
    public void NavigateToPage(Control page, bool isBackNavigation)
    {
        _contentControl.IsTransitionReversed = isBackNavigation;
        _contentControl.Content = page;
    }
}
