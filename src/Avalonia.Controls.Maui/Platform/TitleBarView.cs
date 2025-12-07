using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Microsoft.Maui;
using System.Collections.Generic;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Avalonia platform control for rendering MAUI TitleBar.
/// Supports window dragging on non-passthrough areas.
/// Automatically adjusts for platform-specific window control positions.
/// </summary>
public class TitleBarView : MauiView
{
    private readonly DockPanel _rootPanel;
    private readonly Image _iconImage;
    private readonly ContentControl _leadingContentContainer;
    private readonly StackPanel _titleStack;
    private readonly TextBlock _titleTextBlock;
    private readonly TextBlock _subtitleTextBlock;
    private readonly ContentControl _contentContainer;
    private readonly ContentControl _trailingContentContainer;

    private IMauiContext? _mauiContext;
    private ITitleBar? _titleBar;
    private readonly HashSet<Control> _passthroughControls = new();
    private Window? _attachedWindow;
    private Thickness _windowControlsMargin;

    /// <summary>
    /// Gets or sets the MAUI context for converting views.
    /// </summary>
    public IMauiContext? MauiContext
    {
        get => _mauiContext;
        set => _mauiContext = value;
    }

    /// <summary>
    /// Gets or sets the TitleBar virtual view.
    /// </summary>
    public ITitleBar? TitleBar
    {
        get => _titleBar;
        set
        {
            _titleBar = value;
            UpdateTitleBar();
        }
    }

    public TitleBarView()
    {
        Height = 32; // Standard title bar height
        MinHeight = 32;

        _rootPanel = new DockPanel
        {
            LastChildFill = true,
            Background = Brushes.Transparent
        };

        // Icon (16x16)
        _iconImage = new Image
        {
            Width = 16,
            Height = 16,
            IsVisible = false,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16, 0, 0, 0)
        };
        DockPanel.SetDock(_iconImage, Dock.Left);
        _rootPanel.Children.Add(_iconImage);

        // Leading content
        _leadingContentContainer = new ContentControl
        {
            IsVisible = false,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        DockPanel.SetDock(_leadingContentContainer, Dock.Left);
        _rootPanel.Children.Add(_leadingContentContainer);

        // Trailing content
        _trailingContentContainer = new ContentControl
        {
            IsVisible = false,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
            // Note: Margin is dynamically set by UpdateMarginForWindowControls based on platform
        };
        DockPanel.SetDock(_trailingContentContainer, Dock.Right);
        _rootPanel.Children.Add(_trailingContentContainer);

        // Title and subtitle stack (left aligned)
        _titleStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Margin = new Thickness(16, 0, 0, 0)
        };

        _titleTextBlock = new TextBlock
        {
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = false
        };
        _titleStack.Children.Add(_titleTextBlock);

        _subtitleTextBlock = new TextBlock
        {
            FontSize = 12,
            Opacity = 0.7,
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = false
        };
        _titleStack.Children.Add(_subtitleTextBlock);

        DockPanel.SetDock(_titleStack, Dock.Left);
        _rootPanel.Children.Add(_titleStack);

        // Center content (fills remaining space)
        _contentContainer = new ContentControl
        {
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        _rootPanel.Children.Add(_contentContainer);

        Children.Add(_rootPanel);

        // Enable pointer events for drag handling
        PointerPressed += OnPointerPressed;
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (VisualRoot is Window window)
        {
            _attachedWindow = window;
            _attachedWindow.PropertyChanged += OnWindowPropertyChanged;

            // Calculate and apply initial margin
            UpdateWindowControlsMargin(window);
            ApplyMarginToChildren();
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_attachedWindow != null)
        {
            _attachedWindow.PropertyChanged -= OnWindowPropertyChanged;
            _attachedWindow = null;
        }
    }

    /// <summary>
    /// Called when children are added or removed. Applies margin to new children.
    /// </summary>
    protected override void ChildrenChanged(object? sender, global::System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.ChildrenChanged(sender, e);

        // Apply current margin to any new children
        ApplyMarginToChildren();
    }

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_attachedWindow == null)
            return;

        // Update margins when decoration state or offscreen margins change
        if (e.Property == Window.IsExtendedIntoWindowDecorationsProperty ||
            e.Property == Window.OffScreenMarginProperty)
        {
            UpdateWindowControlsMargin(_attachedWindow);
            ApplyMarginToChildren();
        }
    }

    /// <summary>
    /// Calculates the margin needed for platform-specific window controls.
    /// Uses Avalonia's OffScreenMargin when available, with fallback defaults for system chrome.
    /// </summary>
    private void UpdateWindowControlsMargin(Window window)
    {
        // Only add margin when the window is extended into decorations
        if (!window.IsExtendedIntoWindowDecorations)
        {
            _windowControlsMargin = new Thickness(0);
            return;
        }

        var offScreenMargin = window.OffScreenMargin;

        // If Avalonia provides OffScreenMargin values, use them
        if (offScreenMargin.Left > 0 || offScreenMargin.Right > 0)
        {
            _windowControlsMargin = new Thickness(
                offScreenMargin.Left > 0 ? offScreenMargin.Left + 8 : 0,
                0,
                offScreenMargin.Right > 0 ? offScreenMargin.Right + 8 : 0,
                0);
            return;
        }

        // Fallback: When using PreferSystemChrome, system buttons are rendered by the OS
        // but Avalonia doesn't report their position in OffScreenMargin.
        // Use platform-specific defaults matching MAUI's approach.
        if (OperatingSystem.IsMacOS())
        {
            // macOS: traffic light buttons are on the left (80px as used by MAUI)
            _windowControlsMargin = new Thickness(80, 0, 0, 0);
        }
        else
        {
            // Windows/Linux: window buttons are typically on the right (150px as used by MAUI)
            // Note: Some Linux window managers put buttons on the left, but right is most common
            _windowControlsMargin = new Thickness(0, 0, 150, 0);
        }
    }

    /// <summary>
    /// Applies the current window controls margin to all children.
    /// </summary>
    private void ApplyMarginToChildren()
    {
        foreach (var child in Children)
        {
            if (child is Control control)
            {
                control.Margin = _windowControlsMargin;
            }
        }
    }

    /// <summary>
    /// Handles pointer pressed events to enable window dragging.
    /// </summary>
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Only handle left button press for dragging
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        // Check if the pointer is over a passthrough element
        var hitControl = this.InputHitTest(e.GetPosition(this)) as Control;
        if (hitControl != null && IsPassthroughElement(hitControl))
        {
            // Let the passthrough element handle the input
            return;
        }

        // Start window drag
        var window = this.FindAncestorOfType<Window>();
        if (window != null)
        {
            window.BeginMoveDrag(e);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Checks if a control is a passthrough element (should not trigger window drag).
    /// </summary>
    private bool IsPassthroughElement(Control control)
    {
        // Check if this control or any of its ancestors is in the passthrough set
        var current = control;
        while (current != null && current != this)
        {
            if (_passthroughControls.Contains(current))
                return true;

            current = current.Parent as Control;
        }

        return false;
    }

    /// <summary>
    /// Adds a control to the passthrough elements (won't trigger window drag).
    /// </summary>
    public void AddPassthroughElement(Control control)
    {
        _passthroughControls.Add(control);
    }

    /// <summary>
    /// Removes a control from the passthrough elements.
    /// </summary>
    public void RemovePassthroughElement(Control control)
    {
        _passthroughControls.Remove(control);
    }

    /// <summary>
    /// Clears all passthrough elements.
    /// </summary>
    public void ClearPassthroughElements()
    {
        _passthroughControls.Clear();
    }

    private void UpdateTitleBar()
    {
        if (_titleBar == null)
        {
            IsVisible = false;
            return;
        }

        IsVisible = true;
        UpdateIcon();
        UpdateTitle();
        UpdateSubtitle();
        UpdateLeadingContent();
        UpdateContent();
        UpdateTrailingContent();
        UpdateForegroundColor();
    }

    private void UpdateIcon()
    {
        // Icon is handled through the IContentView pattern
        // The actual icon rendering is done by the TitleBar's TemplatedView
        _iconImage.IsVisible = false;
    }

    private void UpdateTitle()
    {
        var title = _titleBar?.Title;
        _titleTextBlock.Text = title ?? string.Empty;
        _titleTextBlock.IsVisible = !string.IsNullOrEmpty(title);
    }

    private void UpdateSubtitle()
    {
        var subtitle = _titleBar?.Subtitle;
        _subtitleTextBlock.Text = subtitle ?? string.Empty;
        _subtitleTextBlock.IsVisible = !string.IsNullOrEmpty(subtitle);
    }

    private void UpdateLeadingContent()
    {
        // Leading content is handled through the IContentView pattern
        _leadingContentContainer.IsVisible = false;
    }

    private void UpdateContent()
    {
        // Main content is handled through the IContentView pattern
        _contentContainer.IsVisible = false;
    }

    private void UpdateTrailingContent()
    {
        // Trailing content is handled through the IContentView pattern
        _trailingContentContainer.IsVisible = false;
    }

    private void UpdateForegroundColor()
    {
        // ForegroundColor is accessed via the IView interface
        // We'll use default colors for now
        _titleTextBlock.Foreground = Brushes.Black;
        _subtitleTextBlock.Foreground = Brushes.Black;
    }

    /// <summary>
    /// Updates the active/inactive state of the title bar.
    /// </summary>
    public void SetActiveState(bool isActive)
    {
        _titleTextBlock.Opacity = isActive ? 1.0 : 0.7;
        _subtitleTextBlock.Opacity = isActive ? 0.7 : 0.5;
    }
}
