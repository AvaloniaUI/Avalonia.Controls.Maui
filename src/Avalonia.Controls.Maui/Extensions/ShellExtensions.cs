using Avalonia.Media;
using Avalonia.Layout;
using Avalonia.Controls.Maui.Services;
using Avalonia.Controls.Maui.Handlers.Shell;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using MauiShell = Microsoft.Maui.Controls.Shell;
using AvaloniaControl = Avalonia.Controls.Control;
using SolidColorBrush = Avalonia.Media.SolidColorBrush;
using Avalonia.Controls.Primitives;
using ScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility;
using Stretch = Avalonia.Media.Stretch;
using Avalonia.Animation;
using Avalonia.Styling;
using Avalonia.Controls.Maui.Controls.Shell;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping Microsoft.Maui.Controls.Shell properties to ShellHandler and Avalonia controls.
/// </summary>
public static class ShellExtensions
{
    /// <summary>
    /// Updates the background color of the main container.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateBackgroundColor(this ShellHandler handler, MauiShell shell)
    {
        if (handler._mainContainer == null || shell == null)
            return;

        var color = (shell.CurrentPage != null ? MauiShell.GetBackgroundColor(shell.CurrentPage) : null)
            ?? MauiShell.GetBackgroundColor(shell);
        if (color != null)
        {
            handler._mainContainer.Background = color.ToPlatform();

            if (handler._topBar != null)
                handler._topBar.Background = color.ToPlatform();
        }
        else
        {
            handler._mainContainer.ClearValue(DockPanel.BackgroundProperty);
            if (handler._topBar != null)
                handler._topBar.ClearValue(DockPanel.BackgroundProperty);
        }
    }

    /// <summary>
    /// Updates the background color and brush of the flyout pane.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutBackground(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutGrid == null || shell == null)
            return;
        
        if (shell.FlyoutBackground != null && !shell.FlyoutBackground.IsEmpty)
        {
            handler._flyoutGrid.Background = shell.FlyoutBackground.ToPlatform();
        }
        else if (shell.FlyoutBackgroundColor != null)
        {
            handler._flyoutGrid.Background = shell.FlyoutBackgroundColor.ToPlatform();
        }
        else
        {
            handler._flyoutGrid.ApplyDefaultFlyoutBackground();
        }
    }

    /// <summary>
    /// Updates the backdrop (scrim) of the flyout.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutBackdrop(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContainer == null || shell == null)
            return;

        var backdrop = shell.FlyoutBackdrop;
        if (backdrop != null)
        {
            handler._flyoutContainer.FlyoutBackdrop = backdrop.ToPlatform();
        }
        else
        {
            handler._flyoutContainer.FlyoutBackdrop = null;
        }
    }

    /// <summary>
    /// Updates the visibility of the navigation bar.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateNavBarIsVisible(this ShellHandler handler, MauiShell shell)
    {
        if (handler._topBar == null || shell == null)
            return;

        var isVisible = shell.CurrentPage != null && shell.CurrentPage.IsSet(MauiShell.NavBarIsVisibleProperty)
            ? MauiShell.GetNavBarIsVisible(shell.CurrentPage)
            : MauiShell.GetNavBarIsVisible(shell);
        handler._topBar.IsVisible = isVisible;
    }

    /// <summary>
    /// Updates the shadow visibility of the navigation bar.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateNavBarHasShadow(this ShellHandler handler, MauiShell shell)
    {
        if (handler._topBarShadow == null || shell == null)
            return;

        var hasShadow = shell.CurrentPage != null && shell.CurrentPage.IsSet(MauiShell.NavBarHasShadowProperty)
            ? MauiShell.GetNavBarHasShadow(shell.CurrentPage)
            : MauiShell.GetNavBarHasShadow(shell);
        var isVisible = shell.CurrentPage != null && shell.CurrentPage.IsSet(MauiShell.NavBarIsVisibleProperty)
            ? MauiShell.GetNavBarIsVisible(shell.CurrentPage)
            : MauiShell.GetNavBarIsVisible(shell);
        handler._topBarShadow.IsVisible = hasShadow && isVisible;
    }

    /// <summary>
    /// Updates the title color of the navigation bar.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateTitleColor(this ShellHandler handler, MauiShell shell)
    {
        if (handler._titleTextBlock == null || shell == null)
            return;

        var color = (shell.CurrentPage != null ? MauiShell.GetTitleColor(shell.CurrentPage) : null)
            ?? MauiShell.GetTitleColor(shell);
        if (color != null)
        {
            handler._titleTextBlock.Foreground = color.ToPlatform();
        }
        else
        {
            handler._titleTextBlock.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the foreground color (icons and buttons) of the navigation bar.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateForegroundColor(this ShellHandler handler, MauiShell shell)
    {
        if (handler._hamburgerButton == null || shell == null)
            return;

        var color = (shell.CurrentPage != null ? MauiShell.GetForegroundColor(shell.CurrentPage) : null)
            ?? MauiShell.GetForegroundColor(shell);
        if (color != null)
        {
            handler._hamburgerButton.Foreground = color.ToPlatform();
            if (handler._backButton != null)
                handler._backButton.Foreground = color.ToPlatform();
        }
        else
        {
            handler._hamburgerButton.ClearValue(TemplatedControl.ForegroundProperty);
            handler._backButton?.ClearValue(TemplatedControl.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the icon used to open the flyout.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutIcon(this ShellHandler handler, MauiShell shell)
    {
        if (handler._hamburgerButton == null || shell == null)
            return;

        var flyoutIcon = shell.FlyoutIcon;
        if (flyoutIcon != null)
        {
            var imageControl = new Image
            {
                Width = 24,
                Height = 24
            };
            handler._hamburgerButton.Content = imageControl;
            handler.LoadIconAsync(imageControl, flyoutIcon).ConfigureAwait(false);
        }
        else
        {
            handler._hamburgerButton.Content = "☰";
        }
    }

    /// <summary>
    /// Updates the background image of the flyout.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static async void UpdateFlyoutBackgroundImage(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutPaneContainer == null || shell == null || handler.MauiContext == null)
            return;

        var backgroundImage = shell.FlyoutBackgroundImage;

        if (backgroundImage != null)
        {
            var provider = handler.MauiContext.Services.GetService(typeof(IImageSourceServiceProvider)) as IImageSourceServiceProvider;
            if (provider != null)
            {
                var service = provider.GetImageSourceService(backgroundImage.GetType());

                if (service is IAvaloniaImageSourceService avaloniaService)
                {
                    try
                    {
                        var result = await avaloniaService.GetImageAsync(backgroundImage, 1.0f);
                        
                        await Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            if (result != null)
                            {
                                var brush = new Media.ImageBrush(result.Value);
                                
                                switch (shell.FlyoutBackgroundImageAspect)
                                {
                                    case Aspect.AspectFill:
                                        brush.Stretch = Stretch.UniformToFill;
                                        break;
                                    case Aspect.AspectFit:
                                        brush.Stretch = Stretch.Uniform;
                                        break;
                                    case Aspect.Center:
                                        brush.Stretch = Stretch.None;
                                        break;
                                    case Aspect.Fill:
                                        brush.Stretch = Stretch.Fill;
                                        break;
                                }

                                handler._flyoutPaneContainer.Background = brush;
                            }
                            else
                            {
                                handler.UpdateFlyoutBackground(shell);
                            }
                        });

                        if (result != null) return;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading image: {ex}");
                    }
                }
            }
        }

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            handler.UpdateFlyoutBackground(shell);
        });
    }

    /// <summary>
    /// Updates the behavior (Popover, Locked, or Disabled) of the flyout.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutBehavior(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContainer == null || shell == null)
            return;

        switch (shell.FlyoutBehavior)
        {
            case Microsoft.Maui.FlyoutBehavior.Disabled:
                handler._flyoutContainer.FlyoutBehavior = Controls.Shell.FlyoutBehavior.Disabled;
                handler._flyoutContainer.IsFlyoutOpen = false;
                if (handler._hamburgerButton != null)
                    handler._hamburgerButton.IsVisible = false;
                break;
            case Microsoft.Maui.FlyoutBehavior.Flyout:
                handler._flyoutContainer.FlyoutBehavior = Controls.Shell.FlyoutBehavior.Popover;
                if (handler._hamburgerButton != null)
                    handler._hamburgerButton.IsVisible = true;
                break;
            case Microsoft.Maui.FlyoutBehavior.Locked:
                handler._flyoutContainer.FlyoutBehavior = Controls.Shell.FlyoutBehavior.Locked;
                handler._flyoutContainer.IsFlyoutOpen = true;
                if (handler._hamburgerButton != null)
                    handler._hamburgerButton.IsVisible = false;
                break;
        }
    }

    /// <summary>
    /// Updates the width of the flyout.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutWidth(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContainer != null && shell != null && shell.FlyoutWidth > 0)
        {
            handler._flyoutContainer.FlyoutWidth = shell.FlyoutWidth;
        }
    }

    /// <summary>
    /// Updates the height of the flyout.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutHeight(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContainer != null && shell != null && shell.FlyoutHeight > 0)
        {
            handler._flyoutContainer.FlyoutHeight = shell.FlyoutHeight;
        }
    }

    /// <summary>
    /// Updates whether the flyout is currently presented.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutIsPresented(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContainer != null && shell != null)
        {
            handler._flyoutContainer.IsFlyoutOpen = shell.FlyoutIsPresented;
        }
    }

    /// <summary>
    /// Updates the vertical scroll mode of the flyout.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutVerticalScrollMode(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutScrollViewer == null || shell == null)
            return;

        switch (shell.FlyoutVerticalScrollMode)
        {
            case ScrollMode.Auto:
                handler._flyoutScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                break;
            case ScrollMode.Enabled:
                handler._flyoutScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                break;
            case ScrollMode.Disabled:
                handler._flyoutScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                break;
        }
    }

    /// <summary>
    /// Updates the custom content of the flyout.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutContent(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutContentControl == null || handler.MauiContext == null || shell == null)
            return;

        if (shell.FlyoutContent != null && shell.FlyoutContent is IElement element)
        {
            var contentHandler = element.ToHandler(handler.MauiContext);
            if (contentHandler is IViewHandler viewHandler)
            {
                var platformView = (viewHandler.ContainerView as AvaloniaControl) ?? (viewHandler.PlatformView as AvaloniaControl);
                if (platformView != null)
                {
                    platformView.DetachFromVisualTree();
                    handler._flyoutContentControl.Content = platformView;
                }
            }
        }
        else
        {
            handler.UpdateFlyoutItems(shell);
        }
    }

    /// <summary>
    /// Updates the header of the flyout.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutHeader(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutHeaderControl == null || shell == null || handler.MauiContext == null)
            return;

        if (shell.FlyoutHeaderTemplate != null)
        {
            var templateContent = shell.FlyoutHeaderTemplate.CreateContent();
            if (templateContent is View templateView)
            {
                if (shell.FlyoutHeader != null)
                {
                    templateView.BindingContext = shell.FlyoutHeader;
                }

                var templateHandler = templateView.ToHandler(handler.MauiContext);
                if (templateHandler is IViewHandler viewHandler)
                {
                    var platformView = (viewHandler.ContainerView as AvaloniaControl) ?? (viewHandler.PlatformView as AvaloniaControl);
                    if (platformView != null)
                    {
                        platformView.DetachFromVisualTree();
                        handler._flyoutHeaderControl.Content = platformView;
                        handler._flyoutHeaderControl.IsVisible = true;
                    }
                }
            }
            return;
        }

        object? header = shell.FlyoutHeader;

        if (header is View headerView)
        {
            var headerHandler = headerView.ToHandler(handler.MauiContext);
            if (headerHandler is IViewHandler viewHandler)
            {
                var platformView = (viewHandler.ContainerView as AvaloniaControl) ?? (viewHandler.PlatformView as AvaloniaControl);
                if (platformView != null)
                {
                    platformView.DetachFromVisualTree();
                    handler._flyoutHeaderControl.Content = platformView;
                    handler._flyoutHeaderControl.IsVisible = true;
                }
            }
        }
        else if (header != null)
        {
            handler._flyoutHeaderControl.Content = new TextBlock { Text = header.ToString() };
            handler._flyoutHeaderControl.IsVisible = true;
        }
        else
        {
            handler._flyoutHeaderControl.Content = null;
            handler._flyoutHeaderControl.IsVisible = false;
        }
    }

    /// <summary>
    /// Updates the footer of the flyout.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutFooter(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutFooterControl == null || shell == null || handler.MauiContext == null)
            return;

        if (shell.FlyoutFooterTemplate != null)
        {
            var templateContent = shell.FlyoutFooterTemplate.CreateContent();
            if (templateContent is View templateView)
            {
                if (shell.FlyoutFooter != null)
                {
                    templateView.BindingContext = shell.FlyoutFooter;
                }

                var templateHandler = templateView.ToHandler(handler.MauiContext);
                if (templateHandler is IViewHandler viewHandler)
                {
                    var platformView = (viewHandler.ContainerView as AvaloniaControl) ?? (viewHandler.PlatformView as AvaloniaControl);
                    if (platformView != null)
                    {
                        platformView.DetachFromVisualTree();
                        handler._flyoutFooterControl.Content = platformView;
                        handler._flyoutFooterControl.IsVisible = true;
                    }
                }
            }
            return;
        }

        object? footer = shell.FlyoutFooter;

        if (footer is View footerView)
        {
            var footerHandler = footerView.ToHandler(handler.MauiContext);
            if (footerHandler is IViewHandler viewHandler)
            {
                var platformView = (viewHandler.ContainerView as AvaloniaControl) ?? (viewHandler.PlatformView as AvaloniaControl);
                if (platformView != null)
                {
                    platformView.DetachFromVisualTree();
                    handler._flyoutFooterControl.Content = platformView;
                    handler._flyoutFooterControl.IsVisible = true;
                }
            }
        }
        else if (footer != null)
        {
            handler._flyoutFooterControl.Content = new TextBlock { Text = footer.ToString() };
            handler._flyoutFooterControl.IsVisible = true;
        }
        else
        {
            handler._flyoutFooterControl.Content = null;
            handler._flyoutFooterControl.IsVisible = false;
        }
    }

    /// <summary>
    /// Updates the behavior of the flyout header (Fixed, Scroll, or Collapse).
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutHeaderBehavior(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutPaneContainer == null || shell == null || handler._flyoutHeaderControl == null || handler._flyoutScrollViewer == null || handler._flyoutPanel == null)
            return;

        switch (shell.FlyoutHeaderBehavior)
        {
            case FlyoutHeaderBehavior.Default:
            case FlyoutHeaderBehavior.Fixed:
                if (handler._flyoutHeaderControl.Parent != handler._flyoutPaneContainer)
                {
                    handler._flyoutHeaderControl.DetachFromVisualTree();
                    handler._flyoutPaneContainer.Children.Insert(0, handler._flyoutHeaderControl);
                }
                handler._flyoutHeaderControl.SetValue(DockPanel.DockProperty, Dock.Top);
                break;
            case FlyoutHeaderBehavior.Scroll:
            case FlyoutHeaderBehavior.CollapseOnScroll:
                if (handler._flyoutHeaderControl.Parent != handler._flyoutPanel)
                {
                    handler._flyoutHeaderControl.DetachFromVisualTree();
                    handler._flyoutPanel.Children.Insert(0, handler._flyoutHeaderControl);
                }
                break;
        }
    }

    /// <summary>
    /// Updates the title displayed in the navigation bar.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateTitle(this ShellHandler handler, MauiShell shell)
    {
        if (handler._titleTextBlock == null || shell == null)
            return;

        string? title = shell.CurrentPage?.Title;

        if (string.IsNullOrEmpty(title))
        {
            var content = shell.CurrentItem?.CurrentItem?.CurrentItem;
            title = content?.Title;
        }

        if (string.IsNullOrEmpty(title))
        {
            var section = shell.CurrentItem?.CurrentItem;
            title = section?.Title;
        }

        if (string.IsNullOrEmpty(title))
        {
            var item = shell.CurrentItem;
            title = item?.Title;
        }

        if (string.IsNullOrEmpty(title))
        {
            title = shell.Title;
        }

        handler._titleTextBlock.Text = title ?? string.Empty;
    }

    /// <summary>
    /// Updates the current item and its content.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateCurrentItem(this ShellHandler handler, MauiShell shell)
    {
        if (shell?.CurrentItem == null || handler._mainContentControl == null || handler.MauiContext == null)
            return;

        if (handler._currentItemHandler?.VirtualView != null)
        {
            handler._currentItemHandler.VirtualView.PropertyChanged -= handler.OnCurrentItemPropertyChanged;
        }

        // Unsubscribe from previous section via tracked field
        if (handler._trackedSection != null)
        {
            handler._trackedSection.PropertyChanged -= handler.OnCurrentSectionPropertyChanged;
        }

        int currentIndex = shell.Items.IndexOf(shell.CurrentItem);
        handler._previousItemIndex = currentIndex;

        var itemHandler = shell.CurrentItem.ToHandler(handler.MauiContext);
        handler._currentItemHandler = itemHandler as ShellItemHandler;

        if (itemHandler?.PlatformView is AvaloniaControl control)
        {
            // Clear old content without animation first to ensure any in-flight
            // transition's hidden presenter releases its content reference.
            // Without this, cancelled CrossFade transitions skip HideOldPresenter(),
            // leaving old control trees alive and leaking native render resources.
            var savedTransition = handler._mainContentControl.PageTransition;
            handler._mainContentControl.PageTransition = null;
            handler._mainContentControl.Content = null;
            handler._mainContentControl.PageTransition = new CrossFade(ShellHandler.DefaultTransitionDuration);

            handler._mainContentControl.Content = control;
        }

        if (handler._currentItemHandler?.VirtualView != null)
        {
            handler._currentItemHandler.VirtualView.PropertyChanged += handler.OnCurrentItemPropertyChanged;

            // Track the new section
            handler._trackedSection = handler._currentItemHandler.VirtualView.CurrentItem;

            if (handler._trackedSection != null)
            {
                handler._trackedSection.PropertyChanged += handler.OnCurrentSectionPropertyChanged;
            }
        }
        else
        {
            handler._trackedSection = null;
        }

        handler.UpdateTitle(shell);
        handler.UpdateSearchHandler(shell);
        handler.UpdateBackButtonBehavior(shell);

        handler.UpdateItemCheckedStates(shell);
        handler.UpdateFlyoutItemsAppearance(shell);
    }

    /// <summary>
    /// Updates the list of items displayed in the flyout.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateFlyoutItems(this ShellHandler handler, MauiShell shell)
    {
        if (handler._flyoutPanel == null || shell == null)
            return;

        foreach (var kvp in handler._flyoutItemButtons)
        {
            kvp.Key.PropertyChanged -= handler.OnFlyoutItemPropertyChanged;
        }

        // Remove all children except the header control (which may be in the panel for scroll behavior)
        for (int i = handler._flyoutPanel.Children.Count - 1; i >= 0; i--)
        {
            if (handler._flyoutPanel.Children[i] != handler._flyoutHeaderControl)
            {
                handler._flyoutPanel.Children.RemoveAt(i);
            }
        }

        handler._flyoutItemButtons.Clear();

        foreach (var item in shell.Items)
        {
            if (!item.IsVisible || !item.FlyoutItemIsVisible)
                continue;

            // Skip items with no visible content (no title and no icon)
            var icon = item.FlyoutIcon ?? item.Icon;
            if (string.IsNullOrEmpty(item.Title) && icon == null && handler.VirtualView?.ItemTemplate == null)
                continue;

            var button = handler.CreateFlyoutItemButton(item);
            button.Click += (s, e) => handler.OnFlyoutItemSelected(item);

            handler._flyoutPanel.Children.Add(button);
            handler._flyoutItemButtons[item] = button;

            item.PropertyChanged += handler.OnFlyoutItemPropertyChanged;
            handler.UpdateFlyoutItemIcon(button, item);
            button.UpdateFlyoutItemAppearance(item, shell);
        }
    }

    /// <summary>
    /// Updates the back button based on BackButtonBehavior attached property.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateBackButtonBehavior(this ShellHandler handler, MauiShell shell)
    {
        if (handler._backButton == null || shell == null)
            return;

        var behavior = shell.CurrentPage != null
            ? MauiShell.GetBackButtonBehavior(shell.CurrentPage)
            : null;

        // Update IsEnabled
        handler._backButton.IsEnabled = behavior?.IsEnabled ?? true;

        // Update content (IconOverride or TextOverride)
        if (behavior?.IconOverride != null)
        {
            var imageControl = new Image
            {
                Width = 24,
                Height = 24
            };
            handler._backButton.Content = imageControl;
            handler.LoadIconAsync(imageControl, behavior.IconOverride).ConfigureAwait(false);
        }
        else if (!string.IsNullOrEmpty(behavior?.TextOverride))
        {
            handler._backButton.Content = behavior.TextOverride;
        }
        else
        {
            // Reset to default back arrow
            handler._backButton.Content = "←";
        }

        // Update visibility - consider both navigation stack and behavior.IsVisible
        handler.UpdateBackButtonVisibility(shell);
    }

    /// <summary>
    /// Updates the visibility of the back button based on the navigation stack and BackButtonBehavior.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateBackButtonVisibility(this ShellHandler handler, MauiShell shell)
    {
        if (handler._backButton == null || shell == null)
            return;

        bool canGoBack = false;

        if (shell.CurrentItem?.CurrentItem is ShellSection section)
        {
            canGoBack = section.Navigation?.NavigationStack?.Count > 1;
        }

        // Check BackButtonBehavior.IsVisible
        var behavior = shell.CurrentPage != null
            ? MauiShell.GetBackButtonBehavior(shell.CurrentPage)
            : null;

        bool behaviorIsVisible = behavior?.IsVisible ?? true;

        handler._backButton.IsVisible = canGoBack && behaviorIsVisible;
    }

    /// <summary>
    /// Updates the IsChecked state of all items in the shell.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance containing the items.</param>
    public static void UpdateItemCheckedStates(this ShellHandler handler, MauiShell shell)
    {
        if (shell == null)
            return;

        var baseShellItemType = typeof(BaseShellItem);
        var isCheckedPropertyKeyField = baseShellItemType.GetField("IsCheckedPropertyKey",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (isCheckedPropertyKeyField == null)
            return;

        var isCheckedPropertyKey = isCheckedPropertyKeyField.GetValue(null) as BindablePropertyKey;
        if (isCheckedPropertyKey == null)
            return;

        foreach (var item in shell.Items)
        {
            bool isChecked = false;

            if (item == shell.CurrentItem)
            {
                isChecked = true;
            }
            else
            {
                var current = shell.CurrentItem as Element;
                while (current != null)
                {
                    if (current == item)
                    {
                        isChecked = true;
                        break;
                    }
                    current = current.Parent;
                }
            }

            item.SetValue(isCheckedPropertyKey, isChecked);
        }
    }

    /// <summary>
    /// Updates the visual appearance (selection state) of flyout items.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance containing the flyout items.</param>
    public static void UpdateFlyoutItemsAppearance(this ShellHandler handler, MauiShell shell)
    {
        if (shell == null || handler._flyoutItemButtons == null)
            return;

        foreach (var kvp in handler._flyoutItemButtons)
        {
            kvp.Value.UpdateFlyoutItemAppearance(kvp.Key, shell);
        }
    }

    /// <summary>
    /// Updates the icon of a flyout item button.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="button">The button whose icon needs updating.</param>
    /// <param name="item">The <see cref="ShellItem"/> containing the icon source.</param>
    internal static void UpdateFlyoutItemIcon(this ShellHandler handler, Button button, ShellItem item)
    {
        if (handler.VirtualView?.ItemTemplate == null)
        {
            var icon = item.FlyoutIcon ?? item.Icon;
            if (icon != null && button.Content is StackPanel contentPanel)
            {
                var image = contentPanel.Children.OfType<Image>().FirstOrDefault();
                if (image != null)
                {
                    handler.LoadIconAsync(image, icon).ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Asynchronously loads an icon from an <see cref="ImageSource"/> into an <see cref="Image"/> control.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="image">The target <see cref="Image"/> control.</param>
    /// <param name="imageSource">The source <see cref="ImageSource"/> to load.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task LoadIconAsync(this ShellHandler handler, Image image, ImageSource imageSource)
    {
        if (handler.MauiContext == null || imageSource == null || image == null)
            return;

        try
        {
            var imageSourceServiceProvider = handler.GetRequiredService<IImageSourceServiceProvider>();
            var serviceSource = imageSourceServiceProvider.GetImageSourceService(imageSource.GetType());

            if (serviceSource is IAvaloniaImageSourceService avaloniaService)
            {
                var result = await avaloniaService.GetImageAsync(imageSource, 1.0f);
                if (result?.Value is Media.Imaging.Bitmap bitmap)
                {
                    image.Source = bitmap;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading shell icon: {ex}");
        }
    }

    /// <summary>
    /// Updates the TitleView of the current page.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateTitleView(this ShellHandler handler, MauiShell shell)
    {
        if (handler._titleViewControl == null || handler._topBar == null || shell == null || handler.MauiContext == null)
            return;

        if (handler._searchControl != null && handler._searchControl.IsVisible)
        {
            handler._titleViewControl.IsVisible = false;
            if (handler._titleTextBlock != null) handler._titleTextBlock.IsVisible = false;
            return;
        }

        var titleView = (shell.CurrentPage != null ? MauiShell.GetTitleView(shell.CurrentPage) : null)
            ?? MauiShell.GetTitleView(shell);

        if (titleView != null)
        {
            var titleViewHandler = titleView.ToHandler(handler.MauiContext);
            if (titleViewHandler is IViewHandler viewHandler)
            {
                var platformView = (viewHandler.ContainerView as AvaloniaControl) ?? (viewHandler.PlatformView as AvaloniaControl);
                if (platformView != null)
                {
                    platformView.DetachFromVisualTree();
                    handler._titleViewControl.Content = platformView;
                    handler._titleViewControl.IsVisible = true;
                    if (handler._titleTextBlock != null) handler._titleTextBlock.IsVisible = false;
                    return;
                }
            }
        }

        handler._titleViewControl.Content = null;
        handler._titleViewControl.IsVisible = false;
        if (handler._titleTextBlock != null) handler._titleTextBlock.IsVisible = true;
    }

    /// <summary>
    /// Updates the SearchHandler of the current page.
    /// </summary>
    /// <param name="handler">The <see cref="ShellHandler"/> instance.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance to update from.</param>
    public static void UpdateSearchHandler(this ShellHandler handler, MauiShell shell)
    {
        if (handler._topBar == null || shell == null || handler.MauiContext == null)
            return;

        var page = shell.CurrentPage;
        var searchHandler = page != null ? MauiShell.GetSearchHandler(page) : null;

        if (searchHandler == null)
        {
            var shellContent = shell.CurrentItem?.CurrentItem?.CurrentItem;
            if (shellContent is IShellContentController contentController)
            {
                var contentPage = contentController.GetOrCreateContent();
                if (contentPage != null && contentPage != page)
                {
                    searchHandler = MauiShell.GetSearchHandler(contentPage);
                }
            }
        }

        if (searchHandler == null)
        {
            searchHandler = MauiShell.GetSearchHandler(shell);
        }

        if (handler._currentSearchHandler != searchHandler)
        {
            if (handler._currentSearchHandler != null)
            {
                handler._currentSearchHandler.PropertyChanged -= handler.OnSearchHandlerPropertyChanged;
            }

            handler._currentSearchHandler = searchHandler;

            if (handler._currentSearchHandler != null)
            {
                handler._currentSearchHandler.PropertyChanged += handler.OnSearchHandlerPropertyChanged;
            }
        }

        if (searchHandler == null || searchHandler.SearchBoxVisibility == SearchBoxVisibility.Hidden)
        {
            if (handler._searchControl != null)
            {
                handler._searchControl.CleanUp();
                handler._topBar.Children.Remove(handler._searchControl);
                handler._searchControl = null;
            }
                handler.UpdateTitleView(shell);
            return;
        }

        if (handler._searchControl != null && handler._searchControl.SearchHandler == searchHandler)
        {
            return;
        }

        if (handler._searchControl != null)
        {
            handler._searchControl.CleanUp();
            handler._topBar.Children.Remove(handler._searchControl);
            handler._searchControl = null;
        }

        handler._searchControl = new ShellSearchControl(searchHandler, handler.MauiContext);
        handler._searchControl.HorizontalAlignment = HorizontalAlignment.Stretch;

        handler._topBar.Children.Add(handler._searchControl);

        if (handler._titleTextBlock != null) handler._titleTextBlock.IsVisible = false;
        if (handler._titleViewControl != null) handler._titleViewControl.IsVisible = false;
    }

    /// <summary>
    /// Applies the default flyout background based on the current theme.
    /// </summary>
    /// <param name="flyoutPaneContainer">The panel to apply the background to.</param>
    public static void ApplyDefaultFlyoutBackground(this Panel flyoutPaneContainer)
    {
        if (flyoutPaneContainer == null)
            return;

        var theme = Application.Current?.ActualThemeVariant;

        if (theme == ThemeVariant.Dark)
        {
            flyoutPaneContainer.Background = new SolidColorBrush(ShellHandler.DarkThemeFlyoutBackground);
        }
        else
        {
            flyoutPaneContainer.Background = new SolidColorBrush(ShellHandler.LightThemeFlyoutBackground);
        }
    }

    /// <summary>
    /// Updates the appearance (colors and selection state) of a flyout item button.
    /// </summary>
    /// <param name="button">The button to update.</param>
    /// <param name="item">The <see cref="ShellItem"/> associated with the button.</param>
    /// <param name="shell">The <see cref="MauiShell"/> instance containing theme and selection information.</param>
    public static void UpdateFlyoutItemAppearance(this Button button, ShellItem item, MauiShell shell)
    {
        if (button == null || item == null || shell == null)
            return;

        // Check multiple ways to determine if this item is selected
        bool isSelected = item == shell.CurrentItem || item.IsChecked;

        // Also check if the current item is contained within this item (for nested structures)
        if (!isSelected && shell.CurrentItem != null)
        {
            var current = shell.CurrentItem as Element;
            while (current != null && !isSelected)
            {
                if (current == item)
                {
                    isSelected = true;
                }
                current = current.Parent;
            }
        }

        // Check if this item contains the current page (for implicit items)
        if (!isSelected && shell.CurrentPage != null)
        {
            foreach (var section in item.Items)
            {
                foreach (var content in section.Items)
                {
                    if (content.Content == shell.CurrentPage ||
                        (content is IShellContentController controller &&
                         controller.Page == shell.CurrentPage))
                    {
                        isSelected = true;
                        break;
                    }
                }
                if (isSelected) break;
            }
        }

        bool isEnabled = item.IsEnabled;

        var unselectedColor = (MauiShell.GetUnselectedColor(item) ?? MauiShell.GetUnselectedColor(shell))?.ToPlatform();
        var disabledColor = (MauiShell.GetDisabledColor(item) ?? MauiShell.GetDisabledColor(shell))?.ToPlatform();
        var foregroundColor = (MauiShell.GetForegroundColor(item) ?? MauiShell.GetForegroundColor(shell))?.ToPlatform();

        IBrush? targetForeground = null;

        // Set selection state regardless of enabled state
        bool wasSelected = button.Classes.Contains("selected");
        button.Classes.Set("selected", isSelected);
        if (wasSelected != isSelected)
        {
            button.InvalidateVisual();
        }

        if (!isEnabled)
        {
            targetForeground = disabledColor;
        }
        else if (isSelected)
        {
            targetForeground = foregroundColor;
        }
        else
        {
            targetForeground = unselectedColor;
        }

        if (targetForeground != null)
        {
            button.Foreground = targetForeground;

            if (button.Content is StackPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        tb.Foreground = targetForeground;
                    }
                }
            }
        }
        else
        {
            button.ClearValue(TemplatedControl.ForegroundProperty);
            if (button.Content is StackPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        tb.ClearValue(TextBlock.ForegroundProperty);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Detaches a control from its visual parent if it has one.
    /// </summary>
    /// <param name="control">The control to detach.</param>
    public static void DetachFromVisualTree(this AvaloniaControl control)
    {
        if (control.Parent != null)
        {
            if (control.Parent is ContentControl cc)
            {
                if (cc.Content == control) cc.Content = null;
            }
            else if (control.Parent is Panel p)
            {
                p.Children.Remove(control);
            }
            else if (control.Parent is Decorator d)
            {
                if (d.Child == control) d.Child = null;
            }
        }
    }
}