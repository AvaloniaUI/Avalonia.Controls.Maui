using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using AvaloniaControl = Avalonia.Controls.Control;
using SolidColorBrush = Avalonia.Media.SolidColorBrush;
using AvaloniaGrid = Avalonia.Controls.Grid;
using MauiShell = Microsoft.Maui.Controls.Shell;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using MauiPage = Microsoft.Maui.Controls.Page;

namespace Avalonia.Controls.Maui.Handlers.Shell;

/// <summary>Avalonia handler for <see cref="MauiShell"/>.</summary>
public partial class ShellHandler : ViewHandler<MauiShell, AvaloniaControl>
{
    /// <summary>Default height for the navigation bar.</summary>
    internal const double DefaultBarHeight = 48;

    /// <summary>Default spacing between flyout items.</summary>
    internal const double DefaultFlyoutSpacing = 4.0;

    /// <summary>Default selection highlight color for flyout items.</summary>
    internal static readonly Color DefaultSelectionColor = Color.FromArgb(60, 0, 120, 215);

    /// <summary>Selection color used during pointer interaction states.</summary>
    internal static readonly Color SelectionInteractionColor = Color.FromArgb(90, 0, 120, 215);

    /// <summary>Default flyout background color for dark theme.</summary>
    internal static readonly Color DarkThemeFlyoutBackground = Color.Parse("#1F1F1F");

    /// <summary>Default flyout background color for light theme.</summary>
    internal static readonly Color LightThemeFlyoutBackground = Colors.White;

    /// <summary>Default duration for content transitions.</summary>
    internal static readonly TimeSpan DefaultTransitionDuration = TimeSpan.FromMilliseconds(250);

    /// <summary>Property mapper for <see cref="ShellHandler"/>.</summary>
    public static IPropertyMapper<MauiShell, ShellHandler> Mapper =
        new PropertyMapper<MauiShell, ShellHandler>(ViewHandler.ViewMapper)
        {
            [nameof(MauiShell.CurrentItem)] = MapCurrentItem,
            [nameof(MauiShell.FlyoutBehavior)] = MapFlyoutBehavior,
            [nameof(MauiShell.FlyoutIsPresented)] = MapFlyoutIsPresented,
            [nameof(MauiShell.FlyoutIcon)] = MapFlyoutIcon,
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
            [nameof(VisualElement.BackgroundColor)] = MapBackgroundColor,
            [nameof(MauiShell.TitleView)] = MapTitleView,
            [MauiShell.SearchHandlerProperty.PropertyName] = MapSearchHandler,
            [MauiShell.ForegroundColorProperty.PropertyName] = MapForegroundColor,
            [MauiShell.TitleColorProperty.PropertyName] = MapTitleColor,
            [MauiShell.DisabledColorProperty.PropertyName] = MapDisabledColor,
            [MauiShell.UnselectedColorProperty.PropertyName] = MapUnselectedColor,
            [MauiShell.NavBarIsVisibleProperty.PropertyName] = MapNavBarIsVisible,
            [MauiShell.NavBarHasShadowProperty.PropertyName] = MapNavBarHasShadow,
            [MauiShell.BackButtonBehaviorProperty.PropertyName] = MapBackButtonBehavior,
            [MauiShell.TabBarIsVisibleProperty.PropertyName] = MapTabBarIsVisible,
            [MauiShell.TabBarBackgroundColorProperty.PropertyName] = MapTabBarBackgroundColor,
            [MauiShell.TabBarForegroundColorProperty.PropertyName] = MapTabBarForegroundColor,
            [MauiShell.TabBarTitleColorProperty.PropertyName] = MapTabBarTitleColor,
            [MauiShell.TabBarDisabledColorProperty.PropertyName] = MapTabBarDisabledColor,
            [MauiShell.TabBarUnselectedColorProperty.PropertyName] = MapTabBarUnselectedColor,
        };

    /// <summary>Command mapper for <see cref="ShellHandler"/>.</summary>
    public static CommandMapper<MauiShell, ShellHandler> CommandMapper =
        new CommandMapper<MauiShell, ShellHandler>(ViewHandler.ViewCommandMapper);

    /// <summary>Strongly-typed accessor for the MAUI Shell virtual view.</summary>
    internal new MauiShell? VirtualView => ((IElementHandler)this).VirtualView as MauiShell;

    /// <summary>The flyout container that manages flyout open/close behavior.</summary>
    internal DrawerPage? _flyoutContainer;

    /// <summary>Style that prevents drawer content reflow during open/close animation.</summary>
    internal Avalonia.Styling.Style? _paneMinWidthStyle;

    /// <summary>Content control wrapping the flyout panel content.</summary>
    internal ContentControl? _flyoutContentControl;

    /// <summary>Stack panel containing flyout item buttons.</summary>
    internal StackPanel? _flyoutPanel;

    /// <summary>Scroll viewer wrapping the flyout items panel.</summary>
    internal ScrollViewer? _flyoutScrollViewer;

    /// <summary>Dock panel container for the flyout pane layout.</summary>
    internal DockPanel? _flyoutPaneContainer;

    /// <summary>Grid used as the root layout for the flyout pane.</summary>
    internal Grid? _flyoutGrid;

    /// <summary>Content control for the flyout header.</summary>
    internal ContentControl? _flyoutHeaderControl;

    /// <summary>Content control for the flyout footer.</summary>
    internal ContentControl? _flyoutFooterControl;

    /// <summary>Main container dock panel holding the top bar and content area.</summary>
    internal DockPanel? _mainContainer;

    /// <summary>DockPanel for the top navigation bar layout (matching DrawerPage inner DockPanel).</summary>
    internal DockPanel? _topBar;

    /// <summary>Border wrapping the top bar that uses the NavigationBarBackground resource.</summary>
    internal Border? _topBarBorder;

    /// <summary>Right-aligned container for toolbar items in the top bar.</summary>
    internal Panel? _topBarRightHost;

    /// <summary>Content host for the full-width search control row.</summary>
    internal ContentControl? _searchHostControl;

    /// <summary>Border used as the shadow below the top navigation bar.</summary>
    internal Border? _topBarShadow;

    /// <summary>Hamburger menu button to toggle the flyout.</summary>
    internal Button? _hamburgerButton;

    /// <summary>Back navigation button.</summary>
    internal Button? _backButton;

    /// <summary>Text block displaying the page title.</summary>
    internal TextBlock? _titleTextBlock;

    /// <summary>Content control for the custom title view.</summary>
    internal ContentControl? _titleViewControl;

    /// <summary>Transitioning content control for the main page content.</summary>
    internal TransitioningContentControl? _mainContentControl;

    /// <summary>Handler for the currently displayed shell item.</summary>
    internal ShellItemHandler? _currentItemHandler;

    /// <summary>Index of the previously selected shell item for transition direction.</summary>
    internal int _previousItemIndex = -1;

    /// <summary>Dictionary mapping shell items to their flyout buttons.</summary>
    internal Dictionary<ShellItem, Button> _flyoutItemButtons = new();

    /// <summary>Search control displayed in the navigation bar.</summary>
    internal ShellSearchControl? _searchControl;

    /// <summary>Currently active search handler.</summary>
    internal SearchHandler? _currentSearchHandler;

    /// <summary>Currently tracked page for property change notifications.</summary>
    internal MauiPage? _trackedPage;

    /// <summary>Currently tracked shell section for property change notifications.</summary>
    internal ShellSection? _trackedSection;

    /// <summary>Transitioning content control for modal page presentation.</summary>
    internal TransitioningContentControl? _modalContainer;

    /// <summary>Currently displayed modal page.</summary>
    internal MauiPage? _currentModalPage;

    /// <summary>Initializes a new instance of <see cref="ShellHandler"/>.</summary>
    public ShellHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ShellHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    /// <param name="commandMapper">The command mapper to use.</param>
    public ShellHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override AvaloniaControl CreatePlatformView()
    {
        _flyoutContainer = new DrawerPage
        {
            ContentTemplate = null,
            DrawerTemplate = null,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        // Hide DrawerPage's built-in top bar since Shell has its own
        _flyoutContainer.Styles.Add(new Avalonia.Styling.Style(x => x.OfType<DrawerPage>().Template().OfType<Border>().Name("PART_TopBar"))
        {
            Setters = { new Avalonia.Styling.Setter(Visual.IsVisibleProperty, false) }
        });

        // Hide DrawerPage's built-in pane toggle button since Shell has its own hamburger button
        _flyoutContainer.Styles.Add(new Avalonia.Styling.Style(x => x.OfType<DrawerPage>().Template().OfType<Avalonia.Controls.Primitives.ToggleButton>().Name("PART_PaneButton"))
        {
            Setters = { new Avalonia.Styling.Setter(Visual.IsVisibleProperty, false) }
        });

        _flyoutGrid = new AvaloniaGrid();

        _flyoutPaneContainer = new DockPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            LastChildFill = true,
            Background = null
        };
        _flyoutGrid.Children.Add(_flyoutPaneContainer);

        // Flyout header docked to top
        _flyoutHeaderControl = new ContentControl
        {
            [DockPanel.DockProperty] = Dock.Top,
            IsVisible = false,
            Background = null
        };
        _flyoutPaneContainer.Children.Add(_flyoutHeaderControl);

        // Flyout footer docked to bottom
        _flyoutFooterControl = new ContentControl
        {
            [DockPanel.DockProperty] = Dock.Bottom,
            IsVisible = false,
            Background = null
        };
        _flyoutPaneContainer.Children.Add(_flyoutFooterControl);

        // Flyout items panel fills remaining space
        _flyoutPanel = new StackPanel
        {
            Spacing = DefaultFlyoutSpacing,
            Background = null
        };
        ApplyFlyoutStyles(_flyoutPanel);

        _flyoutScrollViewer = new ScrollViewer
        {
            Content = _flyoutPanel,
            HorizontalScrollBarVisibility = Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Primitives.ScrollBarVisibility.Auto,
            Background = null
        };
        _flyoutPaneContainer.Children.Add(_flyoutScrollViewer);

        // Set up flyout content control
        _flyoutContentControl = new ContentControl
        {
            Content = _flyoutGrid,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            Background = null
        };

        _flyoutContainer.Drawer = _flyoutContentControl;

        // Ensure selection is updated when flyout content is attached to visual tree
        _flyoutContentControl.AttachedToVisualTree += (s, e) =>
        {
            if (VirtualView != null)
            {
                Threading.Dispatcher.UIThread.Post(() =>
                {
                    this.UpdateFlyoutItemsAppearance(VirtualView);
                }, Threading.DispatcherPriority.Render);
            }
        };


        _mainContainer = new DockPanel
        {
            LastChildFill = true
        };

        // Inner DockPanel matching DrawerPage's inner DockPanel inside PART_TopBar.
        _topBar = new DockPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        // Border matching DrawerPage PART_TopBar: Height=48, Padding=5.
        // Content area inside is 38px tall; buttons stretch to fill it.
        _topBarBorder = new Border
        {
            Child = _topBar,
            Padding = new Thickness(5),
            Height = DefaultBarHeight,
            [DockPanel.DockProperty] = Dock.Top,
            ZIndex = 1
        };
        // Bind background to NavigationBarBackground DynamicResource
        _topBarBorder.Styles.Add(new Avalonia.Styling.Style(x => x.OfType<Border>())
        {
            Setters = { new Avalonia.Styling.Setter(Border.BackgroundProperty, new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension("NavigationBarBackground")) }
        });

        _topBarShadow = new Border
        {
            Height = 4,
            IsVisible = false,
            IsHitTestVisible = false,
            [DockPanel.DockProperty] = Dock.Top
        };
        _topBarShadow.Background = new Avalonia.Media.LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new Avalonia.Media.GradientStop(Color.FromArgb(12, 0, 0, 0), 0),
                new Avalonia.Media.GradientStop(Color.FromArgb(0, 0, 0, 0), 1)
            }
        };

        // Back button: Button > Panel > PathIcon — exactly matching DrawerPage PART_PaneButton structure
        _backButton = new Button
        {
            Content = new Panel
            {
                Children =
                {
                    new PathIcon
                    {
                        Data = Avalonia.Media.StreamGeometry.Parse("M12.7347,4.20949 C13.0332,3.92233 13.508,3.93153 13.7952,4.23005 C14.0823,4.52857 14.0731,5.00335 13.7746,5.29051 L5.50039,13.25 L24.2532,13.25 C24.6674,13.25 25.0032,13.5858 25.0032,13.9999982 C25.0032,14.4142 24.6674,14.75 24.2532,14.75 L5.50137,14.75 L13.7746,22.7085 C14.0731,22.9957 14.0823,23.4705 13.7952,23.769 C13.508,24.0675 13.0332,24.0767 12.7347,23.7896 L3.30673,14.7202 C2.89776,14.3268 2.89776,13.6723 3.30673,13.2788 L12.7347,4.20949 Z"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            },
            Background = Brushes.Transparent,
            [DockPanel.DockProperty] = Dock.Left,
            IsVisible = false
        };
        _backButton.Click += OnBackButtonClick;

        // Hamburger button: Button > Panel > PathIcon — exactly matching DrawerPage PART_PaneButton structure
        _hamburgerButton = new Button
        {
            Content = new Panel
            {
                Children =
                {
                    new PathIcon
                    {
                        Data = Avalonia.Media.StreamGeometry.Parse("M3 17h18a1 1 0 0 1 .117 1.993L21 19H3a1 1 0 0 1-.117-1.993L3 17h18H3Zm0-6 18-.002a1 1 0 0 1 .117 1.993l-.117.007L3 13a1 1 0 0 1-.117-1.993L3 11l18-.002L3 11Zm0-6h18a1 1 0 0 1 .117 1.993L21 7H3a1 1 0 0 1-.117-1.993L3 5h18H3Z"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            },
            Background = Brushes.Transparent,
            [DockPanel.DockProperty] = Dock.Left
        };
        _hamburgerButton.Click += OnHamburgerButtonClick;

        // Disabled button style matching DrawerPage
        var disabledButtonBg = new Avalonia.Styling.Style(x => x.OfType<Button>().Class(":disabled").Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters = { new Avalonia.Styling.Setter(Avalonia.Controls.Presenters.ContentPresenter.BackgroundProperty, Brushes.Transparent) }
        };
        // Apply NavigationBarForeground to PathIcon via style
        var pathIconForegroundStyle = new Avalonia.Styling.Style(x => x.OfType<PathIcon>())
        {
            Setters = { new Avalonia.Styling.Setter(Avalonia.Controls.Primitives.TemplatedControl.ForegroundProperty, new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension("NavigationBarForeground")) }
        };

        _titleViewControl = new ContentControl
        {
            IsVisible = false,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        // Title matching DrawerPage PART_TitlePresenter
        _titleTextBlock = new TextBlock
        {
            FontSize = 16,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(8, 0, 0, 0)
        };
        // Bind title foreground to NavigationBarForeground resource
        _titleTextBlock.Styles.Add(new Avalonia.Styling.Style(x => x.OfType<TextBlock>())
        {
            Setters = { new Avalonia.Styling.Setter(TextBlock.ForegroundProperty, new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension("NavigationBarForeground")) }
        });

        _searchHostControl = new ContentControl
        {
            [DockPanel.DockProperty] = Dock.Top,
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            ClipToBounds = false,
            ZIndex = 2
        };

        var centerHost = new Panel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ClipToBounds = true
        };
        centerHost.Children.Add(_titleTextBlock);
        centerHost.Children.Add(_titleViewControl);

        // Right host for toolbar items
        _topBarRightHost = new Panel
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Right,
            [DockPanel.DockProperty] = Dock.Right
        };

        _topBar.Children.Add(_backButton);
        _topBar.Children.Add(_hamburgerButton);
        _topBar.Children.Add(_topBarRightHost);
        _topBar.Children.Add(centerHost);

        // Apply styles to the top bar
        _topBar.Styles.Add(disabledButtonBg);
        _topBar.Styles.Add(pathIconForegroundStyle);

        _mainContentControl = new TransitioningContentControl
        {
            PageTransition = new Avalonia.Animation.CrossFade(ShellHandler.DefaultTransitionDuration),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };

        _mainContainer.Children.Add(_topBarBorder);
        _mainContainer.Children.Add(_topBarShadow);
        _mainContainer.Children.Add(_searchHostControl);
        _mainContainer.Children.Add(_mainContentControl);


        _flyoutContainer.Content = _mainContainer;

        // Return a ContentPage hosting the DrawerPage so it can be placed
        // inside an Avalonia NavigationPage (StackNavigationManager expects ContentPage).
        return new Avalonia.Controls.ContentPage
        {
            Content = _flyoutContainer,
            ContentTemplate = null,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(AvaloniaControl platformView)
    {
        base.ConnectHandler(platformView);

        if (_flyoutContainer != null)
        {
            _flyoutContainer.Opened += OnFlyoutOpened;
            _flyoutContainer.Closed += OnFlyoutClosed;
        }

        if (VirtualView != null)
        {
            VirtualView.PropertyChanged += OnShellPropertyChanged;

            this.UpdateItemCheckedStates(VirtualView);
            this.UpdateFlyoutItems(VirtualView);
            this.UpdateCurrentItem(VirtualView);
            this.UpdateSearchHandler(VirtualView);
            this.UpdateFlyoutHeader(VirtualView);
            this.UpdateFlyoutFooter(VirtualView);
            this.UpdateFlyoutItemsAppearance(VirtualView);
            TrackCurrentPage();


            Threading.Dispatcher.UIThread.Post(() =>
            {
                if (VirtualView != null)
                {
                    this.UpdateFlyoutItemsAppearance(VirtualView);
                }
            }, Threading.DispatcherPriority.Loaded);

            if (VirtualView.FlyoutBackground != null && !VirtualView.FlyoutBackground.IsEmpty)
            {
                MapFlyoutBackground(this, VirtualView);
            }
            else if (VirtualView.FlyoutBackgroundColor != null)
            {
                MapFlyoutBackgroundColor(this, VirtualView);
            }
            else if (VirtualView.FlyoutBackgroundImage != null)
            {
                this.UpdateFlyoutBackground(VirtualView);
            }
            else
            {
                _flyoutPaneContainer?.ApplyDefaultFlyoutBackground();
            }
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(AvaloniaControl platformView)
    {
        if (_flyoutContainer != null)
        {
            _flyoutContainer.Opened -= OnFlyoutOpened;
            _flyoutContainer.Closed -= OnFlyoutClosed;
        }

        if (VirtualView != null)
        {
            VirtualView.PropertyChanged -= OnShellPropertyChanged;
        }

        if (_trackedPage != null)
        {
            _trackedPage.PropertyChanged -= OnCurrentPagePropertyChanged;
            _trackedPage = null;
        }

        // Clean up item/section PropertyChanged subscriptions
        if (_currentItemHandler?.VirtualView != null)
        {
            _currentItemHandler.VirtualView.PropertyChanged -= OnCurrentItemPropertyChanged;
        }

        if (_trackedSection != null)
        {
            _trackedSection.PropertyChanged -= OnCurrentSectionPropertyChanged;
            _trackedSection = null;
        }

        // Disconnect the current item handler (cascades to ShellSectionHandler)
        _currentItemHandler?.VirtualView?.Handler?.DisconnectHandler();
        _currentItemHandler = null;

        // Unsubscribe flyout item PropertyChanged events and release button references
        foreach (var kvp in _flyoutItemButtons)
        {
            kvp.Key.PropertyChanged -= OnFlyoutItemPropertyChanged;
        }
        _flyoutItemButtons.Clear();

        // Clean up search handler subscription
        if (_currentSearchHandler != null)
        {
            _currentSearchHandler.PropertyChanged -= OnSearchHandlerPropertyChanged;
            _currentSearchHandler = null;
        }
        _searchControl = null;

        // Unsubscribe button Click events
        if (_backButton != null)
            _backButton.Click -= OnBackButtonClick;
        if (_hamburgerButton != null)
            _hamburgerButton.Click -= OnHamburgerButtonClick;

        // Clean up modal container
        if (_modalContainer != null)
        {
            _modalContainer.PageTransition = null;
            _modalContainer.Content = null;
        }
        _currentModalPage = null;

        // Clear flyout panel children to release flyout item buttons from visual tree
        _flyoutPanel?.Children.Clear();

        // Clear the main content control without animation to release any
        // in-flight transition's hidden presenter content. Without this,
        // cancelled CrossFade transitions retain old control trees and their
        // associated native render resources (textures, surfaces, etc.).
        if (_mainContentControl != null)
        {
            _mainContentControl.PageTransition = null;
            _mainContentControl.Content = null;
        }

        base.DisconnectHandler(platformView);
    }

    private void OnFlyoutOpened(object? sender, RoutedEventArgs e)
    {
        if (e.Source != _flyoutContainer)
            return;

        if (VirtualView != null)
        {
            VirtualView.FlyoutIsPresented = true;
            Threading.Dispatcher.UIThread.Post(() =>
            {
                if (VirtualView != null)
                {
                    this.UpdateFlyoutItemsAppearance(VirtualView);
                }
            }, Threading.DispatcherPriority.Loaded);
        }
    }

    private void OnFlyoutClosed(object? sender, RoutedEventArgs e)
    {
        if (e.Source != _flyoutContainer)
            return;

        if (VirtualView != null)
        {
            VirtualView.FlyoutIsPresented = false;
        }
    }

    private void OnShellPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MauiShell.CurrentItem) ||
            e.PropertyName == MauiShell.CurrentStateProperty.PropertyName ||
            e.PropertyName == nameof(MauiShell.CurrentPage))
        {
            if (VirtualView != null)
            {
                this.UpdateTitle(VirtualView);
                this.UpdateSearchHandler(VirtualView);
                this.UpdateBackButtonBehavior(VirtualView);
                this.UpdateFlyoutItemsAppearance(VirtualView);
                this.UpdateBackgroundColor(VirtualView);
            }

            TrackCurrentPage();
            if (VirtualView?.CurrentItem != null)
                _currentItemHandler?.UpdateTabBarVisibility(VirtualView.CurrentItem);

            // Force focus clear on page change to ensure navigation settles cleanly
            Threading.Dispatcher.UIThread.Post(() => 
            {
                var topLevel = _mainContainer != null ? TopLevel.GetTopLevel(_mainContainer) : null;
                topLevel?.FocusManager?.ClearFocus();
            });
        }
        else if (e.PropertyName == nameof(VisualElement.BackgroundColor))
        {
            MapBackgroundColor(this, VirtualView!);
        }
        else if (e.PropertyName == MauiShell.ForegroundColorProperty.PropertyName)
        {
            MapForegroundColor(this, VirtualView!);
        }
        else if (e.PropertyName == MauiShell.TitleColorProperty.PropertyName)
        {
            MapTitleColor(this, VirtualView!);
        }
        else if (e.PropertyName == MauiShell.FlyoutIconProperty.PropertyName)
        {
            MapFlyoutIcon(this, VirtualView!);
        }
        else if (e.PropertyName == MauiShell.FlyoutBackgroundImageProperty.PropertyName ||
                 e.PropertyName == MauiShell.FlyoutBackgroundImageAspectProperty.PropertyName)
        {
            MapFlyoutBackgroundImage(this, VirtualView!);
        }
        else if (e.PropertyName == MauiShell.TabBarBackgroundColorProperty.PropertyName)
        {
            MapTabBarBackgroundColor(this, VirtualView!);
        }
        else if (e.PropertyName == MauiShell.TabBarForegroundColorProperty.PropertyName)
        {
            MapTabBarForegroundColor(this, VirtualView!);
        }
        else if (e.PropertyName == MauiShell.TabBarTitleColorProperty.PropertyName)
        {
            MapTabBarTitleColor(this, VirtualView!);
        }
        else if (e.PropertyName == MauiShell.TabBarDisabledColorProperty.PropertyName)
        {
            MapTabBarDisabledColor(this, VirtualView!);
        }
        else if (e.PropertyName == MauiShell.TabBarUnselectedColorProperty.PropertyName)
        {
            MapTabBarUnselectedColor(this, VirtualView!);
        }
        else if (e.PropertyName == MauiShell.SearchHandlerProperty.PropertyName)
        {
            MapSearchHandler(this, VirtualView!);
        }
    }

    private void TrackCurrentPage()
    {
        var newPage = VirtualView?.CurrentPage;
        if (_trackedPage == newPage)
            return;

        if (_trackedPage != null)
            _trackedPage.PropertyChanged -= OnCurrentPagePropertyChanged;

        _trackedPage = newPage;

        if (_trackedPage != null)
            _trackedPage.PropertyChanged += OnCurrentPagePropertyChanged;
    }

    private void OnCurrentPagePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (VirtualView == null)
            return;

        if (e.PropertyName == MauiShell.TabBarIsVisibleProperty.PropertyName)
        {
            if (VirtualView.CurrentItem != null)
                _currentItemHandler?.UpdateTabBarVisibility(VirtualView.CurrentItem);
        }
        else if (e.PropertyName == MauiShell.NavBarIsVisibleProperty.PropertyName)
        {
            this.UpdateNavBarIsVisible(VirtualView);
            this.UpdateNavBarHasShadow(VirtualView);
        }
        else if (e.PropertyName == MauiShell.NavBarHasShadowProperty.PropertyName)
            this.UpdateNavBarHasShadow(VirtualView);
        else if (e.PropertyName == nameof(MauiShell.TitleView))
            this.UpdateTitleView(VirtualView);
        else if (e.PropertyName == MauiShell.BackButtonBehaviorProperty.PropertyName)
            this.UpdateBackButtonBehavior(VirtualView);
        else if (e.PropertyName == MauiShell.BackgroundColorProperty.PropertyName)
            this.UpdateBackgroundColor(VirtualView);
        else if (e.PropertyName == Microsoft.Maui.Controls.Page.TitleProperty.PropertyName)
            this.UpdateTitle(VirtualView);
    }

    /// <summary>Maps the CurrentItem property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapCurrentItem(ShellHandler handler, MauiShell shell)
    {
        if (handler.MauiContext != null)
        {
            handler.UpdateCurrentItem(shell);
        }
    }

    /// <summary>Maps the FlyoutBehavior property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutBehavior(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutBehavior(shell);
    }

    /// <summary>Maps the FlyoutIcon property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutIcon(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutIcon(shell);
    }

    /// <summary>Maps the FlyoutIsPresented property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutIsPresented(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutIsPresented(shell);
    }

    /// <summary>Maps the FlyoutWidth property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutWidth(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutWidth(shell);
    }

    /// <summary>Maps the FlyoutBackground property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutBackground(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutBackground(shell);
    }

    /// <summary>Maps the FlyoutBackgroundColor property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutBackgroundColor(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutBackground(shell);
    }

    /// <summary>Maps the FlyoutContent property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutContent(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutContent(shell);
    }

    /// <summary>Maps the FlyoutHeader property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutHeader(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutHeader(shell);
    }

    /// <summary>Maps the FlyoutFooter property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutFooter(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutFooter(shell);
    }

    /// <summary>Maps the Items property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapItems(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutItems(shell);
    }

    /// <summary>Maps the ItemTemplate property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapItemTemplate(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutItems(shell);
    }

    /// <summary>Maps the FlyoutHeight property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutHeight(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutHeight(shell);
    }

    /// <summary>Maps the FlyoutBackgroundImage property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutBackgroundImage(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutBackgroundImage(shell);
        handler.UpdateFlyoutBackground(shell);
    }

    /// <summary>Maps the FlyoutBackdrop property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutBackdrop(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutBackdrop(shell);
    }

    /// <summary>Maps the FlyoutContentTemplate property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutContentTemplate(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutContent(shell);
    }

    /// <summary>Maps the FlyoutHeaderBehavior property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutHeaderBehavior(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutHeaderBehavior(shell);
    }

    /// <summary>Maps the FlyoutVerticalScrollMode property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapFlyoutVerticalScrollMode(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutVerticalScrollMode(shell);
    }

    /// <summary>Maps the MenuItemTemplate property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapMenuItemTemplate(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutItems(shell);
    }

    /// <summary>Maps the BackgroundColor property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapBackgroundColor(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateBackgroundColor(shell);
    }

    /// <summary>Maps the ForegroundColor property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapForegroundColor(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateForegroundColor(shell);
    }

    /// <summary>Maps the TitleColor property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapTitleColor(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateTitleColor(shell);
    }

    /// <summary>Maps the DisabledColor property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapDisabledColor(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutItemsAppearance(shell);
    }

    /// <summary>Maps the UnselectedColor property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapUnselectedColor(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateFlyoutItemsAppearance(shell);
    }

    /// <summary>Maps the NavBarIsVisible property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapNavBarIsVisible(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateNavBarIsVisible(shell);
        handler.UpdateNavBarHasShadow(shell);
    }

    /// <summary>Maps the NavBarHasShadow property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapNavBarHasShadow(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateNavBarHasShadow(shell);
    }

    /// <summary>Maps the BackButtonBehavior property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapBackButtonBehavior(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateBackButtonBehavior(shell);
    }

    /// <summary>Maps the TitleView property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapTitleView(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateTitleView(shell);
    }

    /// <summary>Maps the TabBarIsVisible property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapTabBarIsVisible(ShellHandler handler, MauiShell shell)
    {
        if (shell.CurrentItem != null)
            handler._currentItemHandler?.UpdateTabBarVisibility(shell.CurrentItem);
    }

    /// <summary>Maps the TabBarBackgroundColor property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapTabBarBackgroundColor(ShellHandler handler, MauiShell shell)
    {
        if (shell.CurrentItem != null)
            handler._currentItemHandler?.UpdateTabBarBackgroundColor(shell.CurrentItem);
    }

    /// <summary>Maps the TabBarForegroundColor property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapTabBarForegroundColor(ShellHandler handler, MauiShell shell)
    {
        if (shell.CurrentItem != null)
            handler._currentItemHandler?.UpdateTabBarForegroundColor(shell.CurrentItem);
    }

    /// <summary>Maps the TabBarTitleColor property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapTabBarTitleColor(ShellHandler handler, MauiShell shell)
    {
        if (shell.CurrentItem != null)
            handler._currentItemHandler?.UpdateTabBarTitleColor(shell.CurrentItem);
    }

    /// <summary>Maps the TabBarDisabledColor property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapTabBarDisabledColor(ShellHandler handler, MauiShell shell)
    {
        if (shell.CurrentItem != null)
            handler._currentItemHandler?.UpdateTabBarDisabledColor(shell.CurrentItem);
    }

    /// <summary>Maps the TabBarUnselectedColor property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapTabBarUnselectedColor(ShellHandler handler, MauiShell shell)
    {
        if (shell.CurrentItem != null)
            handler._currentItemHandler?.UpdateTabBarUnselectedColor(shell.CurrentItem);
    }

    /// <summary>Maps the SearchHandler property to the platform view.</summary>
    /// <param name="handler">The shell handler.</param>
    /// <param name="shell">The MAUI Shell virtual view.</param>
    public static void MapSearchHandler(ShellHandler handler, MauiShell shell)
    {
        handler.UpdateSearchHandler(shell);
    }

    internal void OnCurrentItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShellItem.CurrentItem))
        {
            if (sender is ShellItem item)
            {
                // Unsubscribe from previous section before subscribing to new one
                // to prevent accumulating subscriptions and leaking handlers
                if (_trackedSection != null)
                {
                    _trackedSection.PropertyChanged -= OnCurrentSectionPropertyChanged;
                }

                _trackedSection = item.CurrentItem;

                if (_trackedSection != null)
                {
                    _trackedSection.PropertyChanged += OnCurrentSectionPropertyChanged;
                }
            }

            if (VirtualView != null)
            {
                this.UpdateTitle(VirtualView);
                this.UpdateSearchHandler(VirtualView);
                this.UpdateBackButtonBehavior(VirtualView);
            }
        }
    }

    internal void OnCurrentSectionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShellSection.CurrentItem))
        {
            if (VirtualView != null)
            {
                this.UpdateTitle(VirtualView);
                this.UpdateSearchHandler(VirtualView);
                this.UpdateBackButtonBehavior(VirtualView);
            }
        }
    }

    internal Button CreateFlyoutItemButton(ShellItem item)
    {
        var button = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(16, 12)
        };

        if (VirtualView?.ItemTemplate != null && MauiContext != null)
        {
            var templateContent = VirtualView.ItemTemplate.CreateContent();
            if (templateContent is View mauiView)
            {
                mauiView.BindingContext = item;
                var handler = mauiView.ToHandler(MauiContext);
                if (handler?.PlatformView is AvaloniaControl avaloniaControl)
                {
                    button.Content = avaloniaControl;
                }
            }
        }
        else
        {
            var contentPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            bool hasText = !string.IsNullOrEmpty(item.Title);
            var icon = item.FlyoutIcon ?? item.Icon;

            if (icon != null)
            {
                var image = new Image
                {
                    Width = 24,
                    Height = 24,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = hasText ? new Thickness(0, 0, 8, 0) : new Thickness(0)
                };
                contentPanel.Children.Add(image);
            }

            if (hasText)
            {
                var textBlock = new TextBlock
                {
                    Text = item.Title ?? string.Empty,
                    VerticalAlignment = VerticalAlignment.Center
                };
                contentPanel.Children.Add(textBlock);
            }

            // Only set content if we have something to show
            if (contentPanel.Children.Count > 0)
            {
                button.Content = contentPanel;
            }
            else
            {
                button.IsVisible = false;
            }
        }

        return button;
    }

    internal void OnFlyoutItemSelected(ShellItem item)
    {
        if (VirtualView == null)
            return;

        ((IShellController)VirtualView).OnFlyoutItemSelected(item);

        if (((Microsoft.Maui.IFlyoutView)VirtualView).FlyoutBehavior == Microsoft.Maui.FlyoutBehavior.Flyout && _flyoutContainer != null)
        {
            _flyoutContainer.IsOpen = false;
            VirtualView.FlyoutIsPresented = false;
        }
    }

    private void OnHamburgerButtonClick(object? sender, Interactivity.RoutedEventArgs e)
    {
        if (VirtualView != null && _flyoutContainer != null)
        {
            _flyoutContainer.IsOpen = !_flyoutContainer.IsOpen;
            VirtualView.FlyoutIsPresented = _flyoutContainer.IsOpen;
        }
    }

    private async void OnBackButtonClick(object? sender, Interactivity.RoutedEventArgs e)
    {
        var topLevel = _topBarBorder != null ? TopLevel.GetTopLevel(_topBarBorder) : null;
        
        if (topLevel != null)
        {
            topLevel.FocusManager?.ClearFocus();
        }

        if (VirtualView == null)
            return;

        // Check for BackButtonBehavior with custom Command
        var behavior = VirtualView.CurrentPage != null
            ? MauiShell.GetBackButtonBehavior(VirtualView.CurrentPage)
            : null;

        if (behavior?.Command != null && behavior.Command.CanExecute(behavior.CommandParameter))
        {
            behavior.Command.Execute(behavior.CommandParameter);
            e.Handled = true;
            return;
        }

        // Navigate back
        if (VirtualView.CurrentItem?.CurrentItem is ShellSection section)
        {
            if (section.Navigation?.NavigationStack?.Count > 1)
            {
                await VirtualView.GoToAsync("..");
                e.Handled = true;
            }
        }
    }

    internal void OnFlyoutItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not ShellItem item)
            return;

        if (e.PropertyName == nameof(BaseShellItem.FlyoutIcon))
        {
            if (_flyoutItemButtons.TryGetValue(item, out var button))
            {
                this.UpdateFlyoutItemIcon(button, item);
            }
        }
    }

    internal void OnSearchHandlerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == SearchHandler.SearchBoxVisibilityProperty.PropertyName)
        {
            Threading.Dispatcher.UIThread.Post(() => 
            {
                if (VirtualView != null)
                {
                    this.UpdateSearchHandler(VirtualView);
                }
            });
        }
    }

    private void ApplyFlyoutStyles(StackPanel panel)
    {
        var baseButtonStyle = new Styling.Style(x => x.OfType<Button>());
        baseButtonStyle.Setters.Add(new Styling.Setter { Property = Button.BackgroundProperty, Value = Brushes.Transparent });
        baseButtonStyle.Setters.Add(new Styling.Setter { Property = Button.BorderBrushProperty, Value = Brushes.Transparent });
        baseButtonStyle.Setters.Add(new Styling.Setter { Property = Button.BorderThicknessProperty, Value = new Thickness(0) });
        baseButtonStyle.Setters.Add(new Styling.Setter { Property = Button.PaddingProperty, Value = new Thickness(12, 8) });
        panel.Styles.Add(baseButtonStyle);

        var selectedButtonStyle = new Styling.Style(x => x.OfType<Button>().Class("selected"));
        selectedButtonStyle.Setters.Add(new Styling.Setter
        {
            Property = Button.BackgroundProperty,
            Value = new SolidColorBrush(DefaultSelectionColor)
        });
        panel.Styles.Add(selectedButtonStyle);

        var interactionStates = new[] { ":pointerover", ":pressed" };
        foreach (var state in interactionStates)
        {
            var selectedStateStyle = new Styling.Style(x => x.OfType<Button>().Class("selected").Class(state).Template().Name("PART_Border"));
            selectedStateStyle.Setters.Add(new Styling.Setter { Property = Border.BackgroundProperty, Value = new SolidColorBrush(SelectionInteractionColor) });
            panel.Styles.Add(selectedStateStyle);

            var unselectedStateStyle = new Styling.Style(x => x.OfType<Button>().Class(":not(.selected)").Class(state).Template().Name("PART_Border"));
            unselectedStateStyle.Setters.Add(new Styling.Setter { Property = Border.BackgroundProperty, Value = Brushes.Transparent });
            unselectedStateStyle.Setters.Add(new Styling.Setter { Property = Border.BorderBrushProperty, Value = Brushes.Transparent });
            panel.Styles.Add(unselectedStateStyle);
        }
    }
}