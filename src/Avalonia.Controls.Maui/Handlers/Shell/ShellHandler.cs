using System;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
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
            [nameof(MauiShell.FlyoutBackground)] = MapFlyoutBackground,
            [nameof(MauiShell.FlyoutBackgroundColor)] = MapFlyoutBackgroundColor,
            [nameof(MauiShell.FlyoutContent)] = MapFlyoutContent,
            [nameof(MauiShell.FlyoutHeader)] = MapFlyoutHeader,
            [nameof(MauiShell.FlyoutHeaderTemplate)] = MapFlyoutHeader,
            [nameof(MauiShell.FlyoutFooter)] = MapFlyoutFooter,
            [nameof(MauiShell.FlyoutFooterTemplate)] = MapFlyoutFooter,
            [nameof(MauiShell.Items)] = MapItems,
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
    Avalonia.Controls.Button? _hamburgerButton;
    Avalonia.Controls.Button? _backButton;
    TextBlock? _titleTextBlock;
    ContentControl? _mainContentControl;
    ShellItemHandler? _currentItemHandler;

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
            FlyoutBehavior = Platform.FlyoutBehavior.Flyout,
            IsFlyoutOpen = false
        };

        // Create flyout pane structure
        var flyoutPaneContainer = new DockPanel
        {
            LastChildFill = true,
            MinWidth = 300
        };

        flyoutPaneContainer.Background = null; // Inherit from theme

        // Flyout header - docked to top
        _flyoutHeaderControl = new ContentControl
        {
            [DockPanel.DockProperty] = Dock.Top
        };
        flyoutPaneContainer.Children.Add(_flyoutHeaderControl);

        // Flyout footer - docked to bottom
        _flyoutFooterControl = new ContentControl
        {
            [DockPanel.DockProperty] = Dock.Bottom
        };
        flyoutPaneContainer.Children.Add(_flyoutFooterControl);

        // Flyout items panel - fills remaining space
        _flyoutPanel = new StackPanel
        {
            Spacing = 4
        };
        var flyoutScrollViewer = new ScrollViewer
        {
            Content = _flyoutPanel,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        flyoutPaneContainer.Children.Add(flyoutScrollViewer);

        // Set up flyout content control
        _flyoutContentControl = new ContentControl
        {
            Content = flyoutPaneContainer
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
                handler._flyoutContainer.FlyoutBehavior = Platform.FlyoutBehavior.Flyout;
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

        _flyoutPanel.Children.Clear();

        foreach (var item in VirtualView.Items)
        {
            if (!item.IsVisible)
                continue;

            // Create flyout item button
            var button = new Avalonia.Controls.Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        CreateIcon(item.Icon),
                        new TextBlock
                        {
                            Text = item.Title ?? string.Empty,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                        }
                    }
                },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                Padding = new Thickness(16, 12)
            };

            button.Click += (s, e) => OnFlyoutItemSelected(item);

            _flyoutPanel.Children.Add(button);
        }
    }

    private AvaloniaControl CreateIcon(ImageSource? imageSource)
    {
        if (imageSource == null)
            return new Avalonia.Controls.Border { Width = 24, Height = 24 };

        // TODO: Implement proper icon rendering
        return new Avalonia.Controls.Border
        {
            Width = 24,
            Height = 24,
            Background = Avalonia.Media.Brushes.Gray
        };
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
}
