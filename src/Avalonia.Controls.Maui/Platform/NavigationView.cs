using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Avalonia control that provides MAUI-style stack navigation with a navigation bar containing a back button, title, and toolbar items.
/// </summary>
public class NavigationView : DockPanel
{
    private readonly TransitioningContentControl _contentControl;
    private readonly ContentControl _titleViewContainer;
    private readonly TextBlock _titleTextBlock;
    private readonly Button _hamburgerButton;
    private readonly Button _backButton;
    private readonly NavigationBarPanel _navigationBar;
    private readonly Image _titleIconImage;
    private readonly StackPanel _titleStack;
    private readonly StackPanel _toolbarItemsContainer;
    private readonly Button _toolbarOverflowButton;
    private readonly ContextMenu _toolbarOverflowMenu;
    
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
    /// Gets the hamburger menu button.
    /// </summary>
    public Button HamburgerButton => _hamburgerButton;

    /// <summary>
    /// Gets the back button.
    /// </summary>
    public Button BackButton => _backButton;

    /// <summary>
    /// Gets the title icon image control.
    /// </summary>
    public Image TitleIconImage => _titleIconImage;
    
    /// <summary>
    /// Gets the container for toolbar items.
    /// </summary>
    public StackPanel ToolbarItemsContainer => _toolbarItemsContainer;

    /// <summary>
    /// Gets the toolbar overflow button.
    /// </summary>
    public Button ToolbarOverflowButton => _toolbarOverflowButton;
    
    /// <summary>
    /// Gets the toolbar overflow menu.
    /// </summary>
    public ContextMenu ToolbarOverflowMenu => _toolbarOverflowMenu;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationView"/> class, constructing the navigation bar, back button, title area, toolbar, and content host.
    /// </summary>
    public NavigationView()
    {
        LastChildFill = true;

        _navigationBar = new NavigationBarPanel
        {
            Height = 44,
            Background = Brushes.Transparent // Will be set by UpdateNavigationBarColors
        };
        DockPanel.SetDock(_navigationBar, Dock.Top);

        _hamburgerButton = new Button
        {
            Content = "\u2630",
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

        // Left buttons
        var leftButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        leftButtons.Children.Add(_hamburgerButton);
        leftButtons.Children.Add(_backButton);

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
            FontWeight = Media.FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = Media.TextAlignment.Center,
            TextTrimming = Media.TextTrimming.CharacterEllipsis
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

        var centerStack = new Panel
        {
            VerticalAlignment = VerticalAlignment.Center,
            ClipToBounds = true
        };
        centerStack.Children.Add(_titleStack);
        centerStack.Children.Add(_titleViewContainer);

        _toolbarItemsContainer = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Stretch,
            Spacing = 8,
            Margin = new Thickness(0, 0, 8, 0)
        };

        _toolbarOverflowMenu = new ContextMenu();

        _toolbarOverflowButton = new Button
        {
            Content = "...",
            Width = 30,
            Height = 30,
            VerticalAlignment = VerticalAlignment.Center,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            IsVisible = false // Hidden by default
        };

        _toolbarOverflowButton.Click += (s, e) => _toolbarOverflowMenu.Open(_toolbarOverflowButton);

        // Add overflow button to the container (at the end)
        _toolbarItemsContainer.Children.Add(_toolbarOverflowButton);

        // Children order matters: left (0), center (1), right (2)
        _navigationBar.Children.Add(leftButtons);
        _navigationBar.Children.Add(centerStack);
        _navigationBar.Children.Add(_toolbarItemsContainer);

        _contentControl = new TransitioningContentControl
        {
            PageTransition = new PageSlide(TimeSpan.FromMilliseconds(300), PageSlide.SlideAxis.Horizontal)
        };

        Children.Add(_navigationBar);
        Children.Add(_contentControl);
    }

    /// <summary>
    /// Clears the content control without animation, ensuring any in-flight transition's
    /// old presenter content is released so that the referenced Avalonia controls (and their
    /// MAUI handler back-references) can be garbage collected.
    /// </summary>
    public void ClearContent()
    {
        var savedTransition = _contentControl.PageTransition;
        _contentControl.PageTransition = null;
        _contentControl.Content = null;
        _contentControl.PageTransition = savedTransition;
        CurrentPage = null;
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        // When the NavigationView is removed from the visual tree (e.g. window.Page
        // is replaced), immediately clear the TransitioningContentControl's content.
        // The control uses two alternating presenters for animated transitions; if
        // the NavigationView is detached mid-transition, the hidden presenter still
        // holds a reference to the previous Avalonia control tree, whose child
        // handlers keep the corresponding MAUI controls alive.
        ClearContent();
    }

    /// <summary>
    /// Navigates to a new page with optional animation.
    /// </summary>
    /// <param name="page">The page to navigate to.</param>
    /// <param name="isBackNavigation">True if this is a back navigation (will reverse the transition).</param>
    /// <param name="animated">Whether to animate the transition.</param>
    public void NavigateToPage(Control page, bool isBackNavigation, bool animated = true)
    {
        _contentControl.IsTransitionReversed = isBackNavigation;

        if (!animated)
        {
            // Temporarily disable the transition
            var savedTransition = _contentControl.PageTransition;
            _contentControl.PageTransition = null;
            _contentControl.Content = page;
            _contentControl.PageTransition = savedTransition;
        }
        else
        {
            _contentControl.Content = page;
        }
    }

    /// <summary>
    /// Panel that lays out three children (left, center, right) where the center child is
    /// page-centered but clamped so it never overlaps the left or right children.
    /// </summary>
    private sealed class NavigationBarPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Children.Count < 3)
            {
                return base.MeasureOverride(availableSize);
            }

            var left = Children[0];
            var center = Children[1];
            var right = Children[2];

            // Measure left and right first to determine available space for center
            left.Measure(availableSize);
            right.Measure(availableSize);

            double remainingWidth = Math.Max(0, availableSize.Width - left.DesiredSize.Width - right.DesiredSize.Width);
            center.Measure(new Size(remainingWidth, availableSize.Height));

            double height = Math.Max(Math.Max(left.DesiredSize.Height, center.DesiredSize.Height), right.DesiredSize.Height);
            double width = left.DesiredSize.Width + center.DesiredSize.Width + right.DesiredSize.Width;

            return new Size(
                double.IsInfinity(availableSize.Width) ? width : availableSize.Width,
                height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count < 3)
            {
                return base.ArrangeOverride(finalSize);
            }

            var left = Children[0];
            var center = Children[1];
            var right = Children[2];

            double leftWidth = left.DesiredSize.Width;
            double rightWidth = right.DesiredSize.Width;
            double centerWidth = center.DesiredSize.Width;

            // Position left items at the left edge
            left.Arrange(new Rect(0, 0, leftWidth, finalSize.Height));

            // Position right items at the right edge
            right.Arrange(new Rect(finalSize.Width - rightWidth, 0, rightWidth, finalSize.Height));

            // Center title in the full width, but clamp to avoid overlapping left/right
            double availableForCenter = Math.Max(0, finalSize.Width - leftWidth - rightWidth);
            double actualCenterWidth = Math.Min(centerWidth, availableForCenter);
            double idealX = (finalSize.Width - actualCenterWidth) / 2;
            double minX = leftWidth;
            double maxX = Math.Max(minX, finalSize.Width - rightWidth - actualCenterWidth);
            double x = Math.Clamp(idealX, minX, maxX);

            center.Arrange(new Rect(x, 0, actualCenterWidth, finalSize.Height));

            return finalSize;
        }
    }
}
