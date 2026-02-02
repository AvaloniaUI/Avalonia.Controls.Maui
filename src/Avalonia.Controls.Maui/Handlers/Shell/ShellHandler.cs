using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Maui.Services;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AvaloniaControl = Avalonia.Controls.Control;
using AvaloniaGrid = Avalonia.Controls.Grid;
using MauiShell = Microsoft.Maui.Controls.Shell;
using Avalonia.Controls.Maui.Platform;

namespace Avalonia.Controls.Maui.Handlers.Shell;

public partial class ShellHandler : ViewHandler<MauiShell, AvaloniaControl>
{
    public static IPropertyMapper<MauiShell, ShellHandler> Mapper =
        new PropertyMapper<MauiShell, ShellHandler>(ViewHandler.ViewMapper)
        {
            [nameof(MauiShell.CurrentItem)] = MapCurrentItem,
            [nameof(MauiShell.FlyoutBehavior)] = MapFlyoutBehavior,
            [nameof(MauiShell.FlyoutIsPresented)] = MapFlyoutIsPresented,
            [nameof(MauiShell.FlyoutWidth)] = MapFlyoutWidth,
            [nameof(MauiShell.FlyoutHeight)] = MapFlyoutHeight,
            [nameof(MauiShell.FlyoutBackground)] = MapFlyoutBackground,
            [nameof(MauiShell.FlyoutBackgroundColor)] = MapFlyoutBackgroundColor,
            [nameof(MauiShell.FlyoutBackgroundImage)] = MapFlyoutBackgroundImage,
            [nameof(MauiShell.FlyoutBackgroundImageAspect)] = MapFlyoutBackgroundImage,
            [nameof(MauiShell.FlyoutBackdrop)] = MapFlyoutBackdrop,
            [nameof(MauiShell.FlyoutContent)] = MapFlyoutContent,
            [nameof(MauiShell.FlyoutContentTemplate)] = MapFlyoutContentTemplate,
            [nameof(MauiShell.FlyoutHeader)] = MapFlyoutHeader,
            [nameof(MauiShell.FlyoutHeaderTemplate)] = MapFlyoutHeader,
            [nameof(MauiShell.FlyoutHeaderBehavior)] = MapFlyoutHeaderBehavior,
            [nameof(MauiShell.FlyoutFooter)] = MapFlyoutFooter,
            [nameof(MauiShell.FlyoutFooterTemplate)] = MapFlyoutFooter,
            [nameof(MauiShell.FlyoutVerticalScrollMode)] = MapFlyoutVerticalScrollMode,
            [nameof(MauiShell.Items)] = MapItems,
            [nameof(MauiShell.ItemTemplate)] = MapItemTemplate,
            [nameof(MauiShell.MenuItemTemplate)] = MapMenuItemTemplate,
            // Attached properties use string literals for property names
            ["BackgroundColor"] = MapBackgroundColor,
            ["ForegroundColor"] = MapForegroundColor,
            ["TitleColor"] = MapTitleColor,
            ["DisabledColor"] = MapDisabledColor,
            ["UnselectedColor"] = MapUnselectedColor,
            ["NavBarIsVisible"] = MapNavBarIsVisible,
            ["NavBarHasShadow"] = MapNavBarHasShadow,
            ["TitleView"] = MapTitleView,
            ["TabBarIsVisible"] = MapTabBarIsVisible,
            ["TabBarBackgroundColor"] = MapTabBarBackgroundColor,
            ["TabBarForegroundColor"] = MapTabBarForegroundColor,
            ["TabBarTitleColor"] = MapTabBarTitleColor,
            ["TabBarDisabledColor"] = MapTabBarDisabledColor,
            ["TabBarUnselectedColor"] = MapTabBarUnselectedColor,
        };

    public static CommandMapper<MauiShell, ShellHandler> CommandMapper =
        new CommandMapper<MauiShell, ShellHandler>(ViewHandler.ViewCommandMapper);

    FlyoutContainer? _flyoutContainer;
    ContentControl? _flyoutContentControl;
    StackPanel? _flyoutPanel;
    ContentControl? _flyoutHeaderControl;
    ContentControl? _flyoutFooterControl;
    DockPanel? _mainContainer;
    DockPanel? _topBar;
    Border? _topBarShadow;
    Avalonia.Controls.Button? _hamburgerButton;
    Avalonia.Controls.Button? _backButton;
    TextBlock? _titleTextBlock;
    ContentControl? _titleViewControl;
    ContentControl? _mainContentControl;
    ShellItemHandler? _currentItemHandler;
    Image? _flyoutBackgroundImage;
    ScrollViewer? _flyoutScrollViewer;
    Dictionary<ShellItem, Avalonia.Controls.Button> _flyoutItemButtons = new();

    public ShellHandler() : base(Mapper, CommandMapper)
    {
    }

    public ShellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override AvaloniaControl CreatePlatformView()
    {
        // Create the flyout container
        _flyoutContainer = new FlyoutContainer
        {
            FlyoutWidth = 300,
            FlyoutBehavior = Platform.FlyoutBehavior.Popover,
            IsFlyoutOpen = false
        };

        // Create flyout pane structure with background image support
        var flyoutPaneGrid = new AvaloniaGrid();

        // Background image (behind all content)
        _flyoutBackgroundImage = new Image
        {
            Stretch = Avalonia.Media.Stretch.UniformToFill,
            IsVisible = false
        };
        flyoutPaneGrid.Children.Add(_flyoutBackgroundImage);

        // Flyout content container
        var flyoutPaneContainer = new DockPanel
        {
            LastChildFill = true
        };
        flyoutPaneGrid.Children.Add(flyoutPaneContainer);

        flyoutPaneContainer.Background = null; // Inherit from theme

        // Flyout header docked to top
        _flyoutHeaderControl = new ContentControl
        {
            [DockPanel.DockProperty] = Dock.Top
        };
        flyoutPaneContainer.Children.Add(_flyoutHeaderControl);

        // Flyout footer docked to bottom
        _flyoutFooterControl = new ContentControl
        {
            [DockPanel.DockProperty] = Dock.Bottom
        };
        flyoutPaneContainer.Children.Add(_flyoutFooterControl);

        // Flyout items panel fills remaining space
        _flyoutPanel = new StackPanel
        {
            Spacing = 4
        };
        _flyoutScrollViewer = new ScrollViewer
        {
            Content = _flyoutPanel,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        flyoutPaneContainer.Children.Add(_flyoutScrollViewer);

        // Set up flyout content control
        _flyoutContentControl = new ContentControl
        {
            Content = flyoutPaneGrid
        };

        _flyoutContainer.SetFlyoutContent(_flyoutContentControl);

        // Create main container
        _mainContainer = new DockPanel
        {
            LastChildFill = true
        };

        // Create top bar with hamburger button and title
        _topBar = new DockPanel
        {
            [DockPanel.DockProperty] = Dock.Top,
            Height = 48
        };

        _topBar.Background = null; // Inherit from theme

        // Shadow border under the nav bar (initially hidden)
        _topBarShadow = new Border
        {
            Height = 1,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(50, 0, 0, 0)),
            IsVisible = false,
            [DockPanel.DockProperty] = Dock.Top
        };

        // Back button on the left (hidden by default)
        _backButton = new Avalonia.Controls.Button
        {
            Content = "←",
            FontSize = 20,
            Width = 48,
            Height = 48,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Background = Brushes.Transparent,
            IsVisible = false,
            [DockPanel.DockProperty] = Dock.Left
        };
        _backButton.Click += OnBackButtonClick;
        _topBar.Children.Add(_backButton);

        // Hamburger button next to back button
        _hamburgerButton = new Avalonia.Controls.Button
        {
            Content = "☰",
            FontSize = 20,
            Width = 48,
            Height = 48,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Background = Brushes.Transparent,
            [DockPanel.DockProperty] = Dock.Left
        };
        _hamburgerButton.Click += OnHamburgerButtonClick;
        _topBar.Children.Add(_hamburgerButton);

        // TitleView control (for custom title content)
        _titleViewControl = new ContentControl
        {
            IsVisible = false,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        _topBar.Children.Add(_titleViewControl);

        // Title text in the center
        _titleTextBlock = new TextBlock
        {
            FontSize = 18,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(8, 0)
        };
        _topBar.Children.Add(_titleTextBlock);

        _mainContainer.Children.Add(_topBar);
        _mainContainer.Children.Add(_topBarShadow);

        // Main content area
        _mainContentControl = new ContentControl
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        _mainContainer.Children.Add(_mainContentControl);

        _flyoutContainer.SetDetailContent(_mainContainer);

        return _flyoutContainer;
    }

    protected override void ConnectHandler(AvaloniaControl platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView != null)
        {
            VirtualView.PropertyChanged += OnShellPropertyChanged;

            if (VirtualView is IShellController shellController)
            {
                // Subscribe to flyout item selection
            }

            // Initialize UI now that MauiContext is available
            UpdateFlyoutItems();
            UpdateCurrentItem();
            UpdateFlyoutHeader();
            UpdateFlyoutFooter();
            UpdateItemCheckedStates(); // Initialize checked states

            // Apply flyout background if set
            if (VirtualView.FlyoutBackground != null)
            {
                MapFlyoutBackground(this, VirtualView);
            }
            else if (VirtualView.FlyoutBackgroundColor != null)
            {
                MapFlyoutBackgroundColor(this, VirtualView);
            }
        }
    }

    protected override void DisconnectHandler(AvaloniaControl platformView)
    {
        if (VirtualView != null)
        {
            VirtualView.PropertyChanged -= OnShellPropertyChanged;
        }

        _currentItemHandler = null;

        base.DisconnectHandler(platformView);
    }

    private void OnShellPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Update title when navigation changes
        if (e.PropertyName == nameof(MauiShell.CurrentItem) ||
            e.PropertyName == "CurrentState")
        {
            UpdateTitle();
            UpdateBackButtonVisibility();
        }
    }

    public static void MapCurrentItem(ShellHandler handler, MauiShell shell)
    {
        if (handler.MauiContext != null)
        {
            handler.UpdateCurrentItem();
            handler.UpdateItemCheckedStates();
        }
    }

    public static void MapFlyoutBehavior(ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContainer == null)
            return;

        switch (shell.FlyoutBehavior)
        {
            case Microsoft.Maui.FlyoutBehavior.Disabled:
                handler._flyoutContainer.FlyoutBehavior = Platform.FlyoutBehavior.Disabled;
                handler._flyoutContainer.IsFlyoutOpen = false;
                if (handler._hamburgerButton != null)
                    handler._hamburgerButton.IsVisible = false;
                break;
            case Microsoft.Maui.FlyoutBehavior.Flyout:
                handler._flyoutContainer.FlyoutBehavior = Platform.FlyoutBehavior.Popover;
                if (handler._hamburgerButton != null)
                    handler._hamburgerButton.IsVisible = true;
                break;
            case Microsoft.Maui.FlyoutBehavior.Locked:
                handler._flyoutContainer.FlyoutBehavior = Platform.FlyoutBehavior.Locked;
                handler._flyoutContainer.IsFlyoutOpen = true;
                if (handler._hamburgerButton != null)
                    handler._hamburgerButton.IsVisible = false;
                break;
        }
    }

    public static void MapFlyoutIsPresented(ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContainer != null)
        {
            handler._flyoutContainer.IsFlyoutOpen = shell.FlyoutIsPresented;
        }
    }

    public static void MapFlyoutWidth(ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContainer != null && shell.FlyoutWidth > 0)
        {
            handler._flyoutContainer.FlyoutWidth = shell.FlyoutWidth;
        }
    }

    public static void MapFlyoutBackground(ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContentControl != null && shell.FlyoutBackground != null)
        {
            handler._flyoutContentControl.Background = shell.FlyoutBackground.ToPlatform();
        }
    }

    public static void MapFlyoutBackgroundColor(ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContentControl != null && shell.FlyoutBackgroundColor != null)
        {
            handler._flyoutContentControl.Background = new Avalonia.Media.SolidColorBrush(
                Avalonia.Media.Color.FromArgb(
                    (byte)(shell.FlyoutBackgroundColor.Alpha * 255),
                    (byte)(shell.FlyoutBackgroundColor.Red * 255),
                    (byte)(shell.FlyoutBackgroundColor.Green * 255),
                    (byte)(shell.FlyoutBackgroundColor.Blue * 255)
                )
            );
        }
    }

    public static void MapFlyoutContent(ShellHandler handler, MauiShell shell)
    {
        if (handler.MauiContext != null)
        {
            handler.UpdateFlyoutContent();
        }
    }

    public static void MapFlyoutHeader(ShellHandler handler, MauiShell shell)
    {
        if (handler.MauiContext != null)
        {
            handler.UpdateFlyoutHeader();
        }
    }

    public static void MapFlyoutFooter(ShellHandler handler, MauiShell shell)
    {
        if (handler.MauiContext != null)
        {
            handler.UpdateFlyoutFooter();
        }
    }

    public static void MapItems(ShellHandler handler, MauiShell shell)
    {
        if (handler.MauiContext != null)
        {
            handler.UpdateFlyoutItems();
        }
    }

    public static void MapItemTemplate(ShellHandler handler, MauiShell shell)
    {
        if (handler.MauiContext != null)
        {
            handler.UpdateFlyoutItems();
        }
    }


    public static void MapFlyoutHeight(ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContainer != null && shell.FlyoutHeight > 0)
        {
            handler._flyoutContainer.FlyoutHeight = shell.FlyoutHeight;
        }
    }

    public static void MapFlyoutBackgroundImage(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutBackgroundImage();
    }

    public static void MapFlyoutBackdrop(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutBackdrop();
    }

    public static void MapFlyoutContentTemplate(ShellHandler handler, MauiShell shell)
    {
        if (handler.MauiContext != null)
        {
            handler.UpdateFlyoutContent();
        }
    }

    public static void MapFlyoutHeaderBehavior(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutHeaderBehavior();
    }

    public static void MapFlyoutVerticalScrollMode(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutVerticalScrollMode();
    }

    public static void MapMenuItemTemplate(ShellHandler handler, MauiShell shell)
    {
        if (handler.MauiContext != null)
        {
            handler.UpdateFlyoutItems();
        }
    }

    public static void MapBackgroundColor(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateBackgroundColor();
    }

    public static void MapForegroundColor(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateForegroundColor();
    }

    public static void MapTitleColor(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateTitleColor();
    }

    public static void MapDisabledColor(ShellHandler handler, MauiShell shell)
    {
        // DisabledColor is used for styling disabled flyout items
        handler.UpdateFlyoutItems();
    }

    public static void MapUnselectedColor(ShellHandler handler, MauiShell shell)
    {
        // UnselectedColor is used for styling unselected flyout items
        handler.UpdateFlyoutItems();
    }

    public static void MapNavBarIsVisible(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateNavBarVisibility();
    }

    public static void MapNavBarHasShadow(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateNavBarShadow();
    }

    public static void MapTitleView(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateTitleView();
    }

    public static void MapTabBarIsVisible(ShellHandler handler, MauiShell shell)
    {
        // TabBar visibility is handled by ShellItemHandler
        if (handler._currentItemHandler != null)
        {
            handler._currentItemHandler.UpdateTabBarVisibility();
        }
    }

    public static void MapTabBarBackgroundColor(ShellHandler handler, MauiShell shell)
    {
        if (handler._currentItemHandler != null)
        {
            handler._currentItemHandler.UpdateTabBarBackgroundColor();
        }
    }

    public static void MapTabBarForegroundColor(ShellHandler handler, MauiShell shell)
    {
        if (handler._currentItemHandler != null)
        {
            handler._currentItemHandler.UpdateTabBarForegroundColor();
        }
    }

    public static void MapTabBarTitleColor(ShellHandler handler, MauiShell shell)
    {
        if (handler._currentItemHandler != null)
        {
            handler._currentItemHandler.UpdateTabBarTitleColor();
        }
    }

    public static void MapTabBarDisabledColor(ShellHandler handler, MauiShell shell)
    {
        if (handler._currentItemHandler != null)
        {
            handler._currentItemHandler.UpdateTabBarDisabledColor();
        }
    }

    public static void MapTabBarUnselectedColor(ShellHandler handler, MauiShell shell)
    {
        if (handler._currentItemHandler != null)
        {
            handler._currentItemHandler.UpdateTabBarUnselectedColor();
        }
    }

    private void UpdateCurrentItem()
    {
        if (VirtualView?.CurrentItem == null || _mainContentControl == null || MauiContext == null)
            return;

        // Unsubscribe from previous item
        if (_currentItemHandler?.VirtualView != null)
        {
            _currentItemHandler.VirtualView.PropertyChanged -= OnCurrentItemPropertyChanged;
            if (_currentItemHandler.VirtualView.CurrentItem != null)
            {
                _currentItemHandler.VirtualView.CurrentItem.PropertyChanged -= OnCurrentSectionPropertyChanged;
            }
        }

        // Create new handler for current item
        var handler = VirtualView.CurrentItem.ToHandler(MauiContext);
        _currentItemHandler = handler as ShellItemHandler;

        if (handler?.PlatformView is AvaloniaControl control)
        {
            _mainContentControl.Content = control;
        }

        // Subscribe to current item changes for title updates
        if (_currentItemHandler?.VirtualView != null)
        {
            _currentItemHandler.VirtualView.PropertyChanged += OnCurrentItemPropertyChanged;
            if (_currentItemHandler.VirtualView.CurrentItem != null)
            {
                _currentItemHandler.VirtualView.CurrentItem.PropertyChanged += OnCurrentSectionPropertyChanged;
            }
        }

        // Update title and back button
        UpdateTitle();
        UpdateBackButtonVisibility();
    }

    private void OnCurrentItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShellItem.CurrentItem))
        {
            // Subscribe to new section
            if (sender is ShellItem item && item.CurrentItem != null)
            {
                item.CurrentItem.PropertyChanged += OnCurrentSectionPropertyChanged;
            }
            UpdateTitle();
            UpdateBackButtonVisibility();
        }
    }

    private void OnCurrentSectionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShellSection.CurrentItem))
        {
            UpdateTitle();
            UpdateBackButtonVisibility();
        }
    }

    private void UpdateTitle()
    {
        if (_titleTextBlock == null || VirtualView == null)
            return;

        // Get the current title from the navigation hierarchy
        string? title = null;

        // Try to get title from current content
        if (VirtualView.CurrentItem?.CurrentItem?.CurrentItem is ShellContent content)
        {
            title = content.Title;
        }
        // Fallback to current section title
        else if (VirtualView.CurrentItem?.CurrentItem is ShellSection section)
        {
            title = section.Title;
        }
        // Fallback to current item title
        else if (VirtualView.CurrentItem is ShellItem item)
        {
            title = item.Title;
        }

        _titleTextBlock.Text = title ?? string.Empty;
    }

    private void UpdateFlyoutContent()
    {
        if (_flyoutContentControl == null || MauiContext == null)
            return;

        if (VirtualView?.FlyoutContent != null && VirtualView.FlyoutContent is IElement element)
        {
            // Use custom flyout content
            var contentHandler = element.ToHandler(MauiContext);
            if (contentHandler?.PlatformView is AvaloniaControl control)
            {
                _flyoutContentControl.Content = control;
            }
        }
        else
        {
            // Use default flyout structure with items
            UpdateFlyoutItems();
        }
    }

    private void UpdateFlyoutHeader()
    {
        if (_flyoutHeaderControl == null || VirtualView == null || MauiContext == null)
            return;

        // Check if there's a template first
        if (VirtualView.FlyoutHeaderTemplate != null)
        {
            var templateContent = VirtualView.FlyoutHeaderTemplate.CreateContent();
            if (templateContent is Microsoft.Maui.Controls.View templateView)
            {
                // Set binding context if FlyoutHeader is set (acts as data context for template)
                if (VirtualView.FlyoutHeader != null)
                {
                    templateView.BindingContext = VirtualView.FlyoutHeader;
                }

                var handler = templateView.ToHandler(MauiContext);
                if (handler?.PlatformView is AvaloniaControl control)
                {
                    _flyoutHeaderControl.Content = control;
                }
            }
            return;
        }

        // Fallback to direct content
        object? header = VirtualView.FlyoutHeader;

        if (header is Microsoft.Maui.Controls.View headerView)
        {
            var headerHandler = headerView.ToHandler(MauiContext);
            if (headerHandler?.PlatformView is AvaloniaControl control)
            {
                _flyoutHeaderControl.Content = control;
            }
        }
        else if (header != null)
        {
            _flyoutHeaderControl.Content = new TextBlock { Text = header.ToString() };
        }
        else
        {
            _flyoutHeaderControl.Content = null;
        }
    }

    private void UpdateFlyoutFooter()
    {
        if (_flyoutFooterControl == null || VirtualView == null || MauiContext == null)
            return;

        // Check if there's a template first
        if (VirtualView.FlyoutFooterTemplate != null)
        {
            var templateContent = VirtualView.FlyoutFooterTemplate.CreateContent();
            if (templateContent is Microsoft.Maui.Controls.View templateView)
            {
                // Set binding context if FlyoutFooter is set (acts as data context for template)
                if (VirtualView.FlyoutFooter != null)
                {
                    templateView.BindingContext = VirtualView.FlyoutFooter;
                }

                var handler = templateView.ToHandler(MauiContext);
                if (handler?.PlatformView is AvaloniaControl control)
                {
                    _flyoutFooterControl.Content = control;
                }
            }
            return;
        }

        // Fallback to direct content
        object? footer = VirtualView.FlyoutFooter;

        if (footer is Microsoft.Maui.Controls.View footerView)
        {
            var footerHandler = footerView.ToHandler(MauiContext);
            if (footerHandler?.PlatformView is AvaloniaControl control)
            {
                _flyoutFooterControl.Content = control;
            }
        }
        else if (footer != null)
        {
            _flyoutFooterControl.Content = new TextBlock { Text = footer.ToString() };
        }
        else
        {
            _flyoutFooterControl.Content = null;
        }
    }

    private void UpdateFlyoutItems()
    {
        if (_flyoutPanel == null || VirtualView == null)
            return;

        // Unsubscribe from old items
        foreach (var kvp in _flyoutItemButtons)
        {
            kvp.Key.PropertyChanged -= OnFlyoutItemPropertyChanged;
        }

        _flyoutPanel.Children.Clear();
        _flyoutItemButtons.Clear();

        foreach (var item in VirtualView.Items)
        {
            // Check both IsVisible and FlyoutItemIsVisible
            if (!item.IsVisible || !item.FlyoutItemIsVisible)
                continue;

            // Create flyout item button with dynamic content
            var button = CreateFlyoutItemButton(item);
            button.Click += (s, e) => OnFlyoutItemSelected(item);

            _flyoutPanel.Children.Add(button);
            _flyoutItemButtons[item] = button;

            // Subscribe to property changes for this item
            item.PropertyChanged += OnFlyoutItemPropertyChanged;

            // Load initial icon based on IsChecked state
            UpdateFlyoutItemIcon(button, item);
        }
    }

    private Avalonia.Controls.Button CreateFlyoutItemButton(ShellItem item)
    {
        var button = new Avalonia.Controls.Button
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Padding = new Thickness(16, 12),
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent
        };

        // Check if there's a custom ItemTemplate defined
        if (VirtualView?.ItemTemplate != null && MauiContext != null)
        {
            // Create content from the DataTemplate
            var templateContent = VirtualView.ItemTemplate.CreateContent();
            if (templateContent is Microsoft.Maui.Controls.View mauiView)
            {
                // Set the binding context to the ShellItem
                mauiView.BindingContext = item;

                // Convert the MAUI view to an Avalonia control
                var handler = mauiView.ToHandler(MauiContext);
                if (handler?.PlatformView is AvaloniaControl avaloniaControl)
                {
                    button.Content = avaloniaControl;
                }
            }
        }
        else
        {
            // Use default layout
            var contentPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            bool hasText = !string.IsNullOrEmpty(item.Title);

            // Add image placeholder that will be populated asynchronously
            var icon = item.FlyoutIcon ?? item.Icon;
            if (icon != null)
            {
                var image = new Image
                {
                    Width = 24,
                    Height = 24,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    // Add margin only when both image and text are present
                    Margin = hasText ? new Thickness(0, 0, 8, 0) : new Thickness(0)
                };
                contentPanel.Children.Add(image);
            }

            // Add text if present
            if (hasText)
            {
                var textBlock = new TextBlock
                {
                    Text = item.Title ?? string.Empty,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                contentPanel.Children.Add(textBlock);
            }

            button.Content = contentPanel;
        }

        return button;
    }

    private async Task LoadFlyoutItemIconAsync(Avalonia.Controls.Button button, ImageSource imageSource)
    {
        if (MauiContext == null || button.Content is not StackPanel contentPanel)
            return;

        // Find the image control in the content panel
        var image = contentPanel.Children.OfType<Image>().FirstOrDefault();
        if (image == null)
            return;

        try
        {
            var imageSourceServiceProvider = this.GetRequiredService<IImageSourceServiceProvider>();
            var serviceSource = imageSourceServiceProvider.GetImageSourceService(imageSource.GetType());

            if (serviceSource is IAvaloniaImageSourceService avaloniaService)
            {
                var result = await avaloniaService.GetImageAsync(imageSource, 1.0f);
                if (result?.Value is global::Avalonia.Media.Imaging.Bitmap bitmap)
                {
                    image.Source = bitmap;
                }
            }
        }
        catch
        {
            // If image loading fails, the placeholder remains empty
        }
    }

    private void OnFlyoutItemSelected(ShellItem item)
    {
        if (VirtualView == null)
            return;

        VirtualView.CurrentItem = item;

        // Close flyout on item selection
        if (VirtualView.FlyoutBehavior == Microsoft.Maui.FlyoutBehavior.Flyout && _flyoutContainer != null)
        {
            _flyoutContainer.IsFlyoutOpen = false;
            VirtualView.FlyoutIsPresented = false;
        }
    }

    private void OnHamburgerButtonClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (VirtualView != null && _flyoutContainer != null)
        {
            _flyoutContainer.IsFlyoutOpen = !_flyoutContainer.IsFlyoutOpen;
            VirtualView.FlyoutIsPresented = _flyoutContainer.IsFlyoutOpen;
        }
    }

    private async void OnBackButtonClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (VirtualView?.CurrentItem?.CurrentItem is ShellSection section)
        {
            // Try to pop from the navigation stack
            await section.Navigation.PopAsync();

            // Force UI update by triggering the section handler to sync
            if (_currentItemHandler?.VirtualView?.CurrentItem is ShellSection currentSection &&
                currentSection.Handler is ShellSectionHandler sectionHandler)
            {
                sectionHandler.SyncNavigationStack();
            }

            UpdateBackButtonVisibility();
            UpdateTitle();
        }
    }

    private void UpdateBackButtonVisibility()
    {
        if (_backButton == null)
            return;

        // Check if there's a navigation stack with more than one page
        bool canGoBack = false;

        if (VirtualView?.CurrentItem?.CurrentItem is ShellSection section)
        {
            canGoBack = section.Navigation?.NavigationStack?.Count > 1;
        }

        _backButton.IsVisible = canGoBack;
    }

    /// <summary>
    /// Updates the IsChecked state of all shell items based on the current selection.
    /// This method uses reflection to access the internal IsCheckedPropertyKey from BaseShellItem.
    /// </summary>
    private void UpdateItemCheckedStates()
    {
        if (VirtualView == null)
            return;

        // Get the IsCheckedPropertyKey using reflection
        var baseShellItemType = typeof(BaseShellItem);
        var isCheckedPropertyKeyField = baseShellItemType.GetField("IsCheckedPropertyKey",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (isCheckedPropertyKeyField == null)
            return;

        var isCheckedPropertyKey = isCheckedPropertyKeyField.GetValue(null) as BindablePropertyKey;
        if (isCheckedPropertyKey == null)
            return;

        // Update all items
        foreach (var item in VirtualView.Items)
        {
            bool isChecked = item == VirtualView.CurrentItem;
            item.SetValue(isCheckedPropertyKey, isChecked);
        }
    }

    /// <summary>
    /// Handles property changes on flyout items, particularly the IsChecked and FlyoutIcon properties.
    /// </summary>
    private void OnFlyoutItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not ShellItem item)
            return;

        if (e.PropertyName == nameof(BaseShellItem.FlyoutIcon))
        {
            if (_flyoutItemButtons.TryGetValue(item, out var button))
            {
                UpdateFlyoutItemIcon(button, item);
            }
        }
    }

    /// <summary>
    /// Updates the flyout item button's icon based on the item's IsChecked state.
    /// </summary>
    private void UpdateFlyoutItemIcon(Avalonia.Controls.Button button, ShellItem item)
    {
        // If using ItemTemplate, the binding context will automatically update the content
        // through MAUI's binding system, so we only need to handle the default case
        if (VirtualView?.ItemTemplate == null)
        {
            var icon = item.FlyoutIcon ?? item.Icon;
            if (icon != null && MauiContext != null)
            {
                LoadFlyoutItemIconAsync(button, icon).ConfigureAwait(false);
            }
        }
        // For templates, the MAUI binding system handles updates automatically
    }

    private async void UpdateFlyoutBackgroundImage()
    {
        if (_flyoutBackgroundImage == null || VirtualView == null || MauiContext == null)
            return;

        var imageSource = VirtualView.FlyoutBackgroundImage;
        if (imageSource == null)
        {
            _flyoutBackgroundImage.Source = null;
            _flyoutBackgroundImage.IsVisible = false;
            return;
        }

        try
        {
            var imageSourceServiceProvider = this.GetRequiredService<IImageSourceServiceProvider>();
            var serviceSource = imageSourceServiceProvider.GetImageSourceService(imageSource.GetType());

            if (serviceSource is IAvaloniaImageSourceService avaloniaService)
            {
                var result = await avaloniaService.GetImageAsync(imageSource, 1.0f);
                if (result?.Value is global::Avalonia.Media.Imaging.Bitmap bitmap)
                {
                    _flyoutBackgroundImage.Source = bitmap;
                    _flyoutBackgroundImage.IsVisible = true;

                    // Apply aspect
                    _flyoutBackgroundImage.Stretch = VirtualView.FlyoutBackgroundImageAspect switch
                    {
                        Microsoft.Maui.Aspect.AspectFill => Avalonia.Media.Stretch.UniformToFill,
                        Microsoft.Maui.Aspect.AspectFit => Avalonia.Media.Stretch.Uniform,
                        Microsoft.Maui.Aspect.Fill => Avalonia.Media.Stretch.Fill,
                        Microsoft.Maui.Aspect.Center => Avalonia.Media.Stretch.None,
                        _ => Avalonia.Media.Stretch.UniformToFill
                    };
                }
            }
        }
        catch
        {
            _flyoutBackgroundImage.IsVisible = false;
        }
    }

    private void UpdateFlyoutBackdrop()
    {
        if (_flyoutContainer == null || VirtualView == null)
            return;

        var backdrop = VirtualView.FlyoutBackdrop;
        if (backdrop != null)
        {
            _flyoutContainer.FlyoutBackdrop = backdrop.ToPlatform();
        }
    }

    private void UpdateFlyoutHeaderBehavior()
    {
        if (_flyoutHeaderControl == null || _flyoutScrollViewer == null || VirtualView == null)
            return;

        // FlyoutHeaderBehavior controls how the header scrolls with the flyout content
        switch (VirtualView.FlyoutHeaderBehavior)
        {
            case FlyoutHeaderBehavior.Default:
            case FlyoutHeaderBehavior.Fixed:
                // Header stays fixed at top, only items scroll
                _flyoutHeaderControl.SetValue(DockPanel.DockProperty, Dock.Top);
                break;
            case FlyoutHeaderBehavior.Scroll:
            case FlyoutHeaderBehavior.CollapseOnScroll:
                // In a more complete implementation, header would be inside the scroll viewer
                // For now, treat similar to Fixed
                _flyoutHeaderControl.SetValue(DockPanel.DockProperty, Dock.Top);
                break;
        }
    }

    private void UpdateFlyoutVerticalScrollMode()
    {
        if (_flyoutScrollViewer == null || VirtualView == null)
            return;

        _flyoutScrollViewer.VerticalScrollBarVisibility = VirtualView.FlyoutVerticalScrollMode switch
        {
            Microsoft.Maui.Controls.ScrollMode.Auto => Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Microsoft.Maui.Controls.ScrollMode.Enabled => Avalonia.Controls.Primitives.ScrollBarVisibility.Visible,
            Microsoft.Maui.Controls.ScrollMode.Disabled => Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            _ => Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
    }

    private void UpdateBackgroundColor()
    {
        if (_mainContainer == null || VirtualView == null)
            return;

        var color = VirtualView.BackgroundColor;
        if (color != null)
        {
            _mainContainer.Background = color.ToPlatform();
        }
        else
        {
            _mainContainer.ClearValue(DockPanel.BackgroundProperty);
        }
    }

    private void UpdateForegroundColor()
    {
        // ForegroundColor affects text in the shell chrome (hamburger button, etc.)
        // For now, we'll leave the default theme colors
    }

    private void UpdateTitleColor()
    {
        if (_titleTextBlock == null || VirtualView == null)
            return;

        // TitleColor is an attached property
        var color = MauiShell.GetTitleColor(VirtualView);
        if (color != null)
        {
            _titleTextBlock.Foreground = color.ToPlatform();
        }
        else
        {
            _titleTextBlock.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    private void UpdateNavBarVisibility()
    {
        if (_topBar == null || _topBarShadow == null || VirtualView == null)
            return;

        // Check current page's NavBarIsVisible attached property
        var isVisible = MauiShell.GetNavBarIsVisible(VirtualView.CurrentPage ?? VirtualView);

        _topBar.IsVisible = isVisible;
        // Shadow visibility follows nav bar visibility (and NavBarHasShadow property)
        if (!isVisible)
        {
            _topBarShadow.IsVisible = false;
        }
    }

    private void UpdateNavBarShadow()
    {
        if (_topBarShadow == null || VirtualView == null)
            return;

        // Check current page's NavBarHasShadow attached property
        var hasShadow = MauiShell.GetNavBarHasShadow(VirtualView.CurrentPage ?? VirtualView);
        var navBarVisible = _topBar?.IsVisible ?? true;

        _topBarShadow.IsVisible = hasShadow && navBarVisible;
    }

    private void UpdateTitleView()
    {
        if (_titleViewControl == null || _titleTextBlock == null || VirtualView == null || MauiContext == null)
            return;

        // Get TitleView from current page or shell
        var titleView = MauiShell.GetTitleView(VirtualView.CurrentPage ?? VirtualView);

        if (titleView != null)
        {
            var handler = ((IElement)titleView).ToHandler(MauiContext);
            if (handler?.PlatformView is AvaloniaControl control)
            {
                _titleViewControl.Content = control;
                _titleViewControl.IsVisible = true;
                _titleTextBlock.IsVisible = false;
            }
        }
        else
        {
            _titleViewControl.Content = null;
            _titleViewControl.IsVisible = false;
            _titleTextBlock.IsVisible = true;
        }
    }
}