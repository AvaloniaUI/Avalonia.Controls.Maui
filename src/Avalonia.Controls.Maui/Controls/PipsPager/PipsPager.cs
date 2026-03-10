using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

// Clone of https://github.com/AvaloniaUI/Avalonia/pull/20660
// When Avalonia.Controls.PipsPager ships, remove this file and update:
//   using PlatformView = Avalonia.Controls.PipsPager;
namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// Represents a control that lets the user navigate through a paginated collection using a set of pips.
/// </summary>
[TemplatePart("PART_PreviousButton", typeof(Button))]
[TemplatePart("PART_NextButton", typeof(Button))]
[TemplatePart("PART_PipsPagerList", typeof(ItemsControl))]
[PseudoClasses(":first-page", ":last-page", ":vertical", ":horizontal")]
public class PipsPager : TemplatedControl
{
    private const string PART_PreviousButton = "PART_PreviousButton";
    private const string PART_NextButton = "PART_NextButton";
    private const string PART_PipsPagerList = "PART_PipsPagerList";

    internal const string PipsPagerListPartName = PART_PipsPagerList;

    private Button? _previousButton;
    private Button? _nextButton;
    private ItemsControl? _pipsPagerList;
    private ListBox? _pipsList;
    private bool _scrollPending;
    private bool _updatingPagerSize;
    private bool _isInitialLoad = true;
    private int _lastSelectedPageIndex = -1;
    private CancellationTokenSource? _scrollAnimationCts;
    private double _containerPipSize = 12.0;
    private PipsPagerTemplateSettings _templateSettings = new PipsPagerTemplateSettings();

    /// <summary>Defines the <see cref="MaxVisiblePips"/> property.</summary>
    public static readonly StyledProperty<int> MaxVisiblePipsProperty =
        AvaloniaProperty.Register<PipsPager, int>(nameof(MaxVisiblePips), 5);

    /// <summary>Defines the <see cref="IsNextButtonVisible"/> property.</summary>
    public static readonly StyledProperty<bool> IsNextButtonVisibleProperty =
        AvaloniaProperty.Register<PipsPager, bool>(nameof(IsNextButtonVisible), true);

    /// <summary>Defines the <see cref="NumberOfPages"/> property.</summary>
    public static readonly StyledProperty<int> NumberOfPagesProperty =
        AvaloniaProperty.Register<PipsPager, int>(nameof(NumberOfPages));

    /// <summary>Defines the <see cref="Orientation"/> property.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<PipsPager, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <summary>Defines the <see cref="IsPreviousButtonVisible"/> property.</summary>
    public static readonly StyledProperty<bool> IsPreviousButtonVisibleProperty =
        AvaloniaProperty.Register<PipsPager, bool>(nameof(IsPreviousButtonVisible), true);

    /// <summary>Defines the <see cref="SelectedPageIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedPageIndexProperty =
        AvaloniaProperty.Register<PipsPager, int>(nameof(SelectedPageIndex),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the <see cref="TemplateSettings"/> property.</summary>
    public static readonly DirectProperty<PipsPager, PipsPagerTemplateSettings> TemplateSettingsProperty =
        AvaloniaProperty.RegisterDirect<PipsPager, PipsPagerTemplateSettings>(nameof(TemplateSettings),
            x => x.TemplateSettings);

    /// <summary>Defines the <see cref="PreviousButtonStyle"/> property.</summary>
    public static readonly StyledProperty<ControlTheme?> PreviousButtonStyleProperty =
        AvaloniaProperty.Register<PipsPager, ControlTheme?>(nameof(PreviousButtonStyle));

    /// <summary>Defines the <see cref="NextButtonStyle"/> property.</summary>
    public static readonly StyledProperty<ControlTheme?> NextButtonStyleProperty =
        AvaloniaProperty.Register<PipsPager, ControlTheme?>(nameof(NextButtonStyle));

    /// <summary>Defines the <see cref="IndicatorTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> IndicatorTemplateProperty =
        AvaloniaProperty.Register<PipsPager, IDataTemplate?>(nameof(IndicatorTemplate));

    /// <summary>Defines the <see cref="SelectedIndexChanged"/> event.</summary>
    public static readonly RoutedEvent<PipsPagerSelectedIndexChangedEventArgs> SelectedIndexChangedEvent =
        RoutedEvent.Register<PipsPager, PipsPagerSelectedIndexChangedEventArgs>(
            nameof(SelectedIndexChanged), RoutingStrategies.Bubble);

    /// <summary>Occurs when the selected index has changed.</summary>
    public event EventHandler<PipsPagerSelectedIndexChangedEventArgs>? SelectedIndexChanged
    {
        add => AddHandler(SelectedIndexChangedEvent, value);
        remove => RemoveHandler(SelectedIndexChangedEvent, value);
    }

    static PipsPager()
    {
        SelectedPageIndexProperty.Changed.AddClassHandler<PipsPager>((x, e) => x.OnSelectedPageIndexChanged(e));
        NumberOfPagesProperty.Changed.AddClassHandler<PipsPager>((x, e) => x.OnNumberOfPagesChanged(e));
        IsPreviousButtonVisibleProperty.Changed.AddClassHandler<PipsPager>((x, e) => x.OnIsPreviousButtonVisibleChanged(e));
        IsNextButtonVisibleProperty.Changed.AddClassHandler<PipsPager>((x, e) => x.OnIsNextButtonVisibleChanged(e));
        OrientationProperty.Changed.AddClassHandler<PipsPager>((x, e) => x.OnOrientationChanged(e));
        MaxVisiblePipsProperty.Changed.AddClassHandler<PipsPager>((x, e) => x.OnMaxVisiblePipsChanged(e));
        IndicatorTemplateProperty.Changed.AddClassHandler<PipsPager>((x, _) => x.ApplyIndicatorTemplate());
    }

    /// <summary>Initializes a new instance of <see cref="PipsPager"/>.</summary>
    public PipsPager()
    {
        UpdatePseudoClasses();
    }

    /// <summary>Gets or sets the maximum number of visible pips.</summary>
    public int MaxVisiblePips
    {
        get => GetValue(MaxVisiblePipsProperty);
        set => SetValue(MaxVisiblePipsProperty, value);
    }

    /// <summary>Gets or sets the visibility of the next button.</summary>
    public bool IsNextButtonVisible
    {
        get => GetValue(IsNextButtonVisibleProperty);
        set => SetValue(IsNextButtonVisibleProperty, value);
    }

    /// <summary>Gets or sets the number of pages.</summary>
    public int NumberOfPages
    {
        get => GetValue(NumberOfPagesProperty);
        set => SetValue(NumberOfPagesProperty, value);
    }

    /// <summary>Gets or sets the orientation of the pips.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>Gets or sets the visibility of the previous button.</summary>
    public bool IsPreviousButtonVisible
    {
        get => GetValue(IsPreviousButtonVisibleProperty);
        set => SetValue(IsPreviousButtonVisibleProperty, value);
    }

    /// <summary>Gets or sets the current selected page index.</summary>
    public int SelectedPageIndex
    {
        get => GetValue(SelectedPageIndexProperty);
        set => SetValue(SelectedPageIndexProperty, value);
    }

    /// <summary>Gets the template settings.</summary>
    public PipsPagerTemplateSettings TemplateSettings
    {
        get => _templateSettings;
        private set => SetAndRaise(TemplateSettingsProperty, ref _templateSettings, value);
    }

    /// <summary>Gets or sets the style for the previous button.</summary>
    public ControlTheme? PreviousButtonStyle
    {
        get => GetValue(PreviousButtonStyleProperty);
        set => SetValue(PreviousButtonStyleProperty, value);
    }

    /// <summary>Gets or sets the style for the next button.</summary>
    public ControlTheme? NextButtonStyle
    {
        get => GetValue(NextButtonStyleProperty);
        set => SetValue(NextButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets a custom <see cref="IDataTemplate"/> used to render each indicator item.
    /// When <c>null</c> the default pip dots are shown.
    /// </summary>
    public IDataTemplate? IndicatorTemplate
    {
        get => GetValue(IndicatorTemplateProperty);
        set => SetValue(IndicatorTemplateProperty, value);
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer()
        => new PipsPagerAutomationPeer(this);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Skip animation when restoring scroll position after template re-apply.
        _isInitialLoad = true;

        _scrollAnimationCts?.Cancel();
        _scrollAnimationCts?.Dispose();
        _scrollAnimationCts = null;

        if (_previousButton != null)
            _previousButton.Click -= PreviousButton_Click;

        if (_nextButton != null)
            _nextButton.Click -= NextButton_Click;

        if (_pipsPagerList != null)
        {
            _pipsPagerList.SizeChanged -= OnPipsPagerListSizeChanged;
            _pipsPagerList.ContainerPrepared -= OnContainerPrepared;
            _pipsPagerList.ContainerIndexChanged -= OnContainerIndexChanged;
        }

        _previousButton = e.NameScope.Find<Button>(PART_PreviousButton);
        _nextButton = e.NameScope.Find<Button>(PART_NextButton);
        _pipsPagerList = e.NameScope.Find<ItemsControl>(PART_PipsPagerList);
        _pipsList = _pipsPagerList as ListBox;

        if (_previousButton != null)
        {
            _previousButton.Click += PreviousButton_Click;
            AutomationProperties.SetName(_previousButton, "Previous page");
        }

        if (_nextButton != null)
        {
            _nextButton.Click += NextButton_Click;
            AutomationProperties.SetName(_nextButton, "Next page");
        }

        if (_pipsPagerList != null)
        {
            _pipsPagerList.SizeChanged += OnPipsPagerListSizeChanged;
            _pipsPagerList.ContainerPrepared += OnContainerPrepared;
            _pipsPagerList.ContainerIndexChanged += OnContainerIndexChanged;
        }

        UpdateButtonsState();
        UpdatePseudoClasses();
        UpdatePagerSize();
        RequestScrollToSelectedPip();
        ApplyIndicatorTemplate();
    }

    private void ApplyIndicatorTemplate()
    {
        if (_pipsPagerList == null)
            return;

        if (IndicatorTemplate != null)
        {
            _pipsPagerList.ItemTemplate = IndicatorTemplate;
            // Allow container to auto-size to the template content by removing fixed dimensions.
            Resources["PipsPagerPipContainerMinorSize"] = double.NaN;
            Resources["PipsPagerPipContainerMajorSize"] = double.NaN;
        }
        else
        {
            _pipsPagerList.ItemTemplate = null;
            // Restore default fixed pip container dimensions.
            Resources.Remove("PipsPagerPipContainerMinorSize");
            Resources.Remove("PipsPagerPipContainerMajorSize");
        }

        // Avalonia does not automatically recreate existing containers when ItemTemplate changes.
        // Force recreation by clearing and re-populating the pips list.
        var pips = TemplateSettings.Pips;
        var count = pips.Count;
        if (count > 0)
        {
            var savedIndex = SelectedPageIndex;
            pips.Clear();
            for (var i = 1; i <= count; i++)
                pips.Add(i);
            // count > 0 here, so count - 1 >= 0 and Clamp is safe.
            SetCurrentValue(SelectedPageIndexProperty, Math.Clamp(savedIndex, 0, count - 1));
        }
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
            return;

        var isHorizontal = Orientation == Orientation.Horizontal;

        switch (e.Key)
        {
            case Key.Left when isHorizontal:
            case Key.Up when !isHorizontal:
                if (SelectedPageIndex > 0)
                {
                    SetCurrentValue(SelectedPageIndexProperty, SelectedPageIndex - 1);
                    e.Handled = true;
                }
                break;
            case Key.Right when isHorizontal:
            case Key.Down when !isHorizontal:
                if (SelectedPageIndex < NumberOfPages - 1)
                {
                    SetCurrentValue(SelectedPageIndexProperty, SelectedPageIndex + 1);
                    e.Handled = true;
                }
                break;
            case Key.Home:
                if (NumberOfPages > 0)
                {
                    SetCurrentValue(SelectedPageIndexProperty, 0);
                    e.Handled = true;
                }
                break;
            case Key.End:
                if (NumberOfPages > 0)
                {
                    SetCurrentValue(SelectedPageIndexProperty, NumberOfPages - 1);
                    e.Handled = true;
                }
                break;
        }
    }

    private void OnSelectedPageIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var newIndex = e.GetNewValue<int>();
        var oldIndex = e.GetOldValue<int>();

        if (newIndex < 0)
        {
            SetCurrentValue(SelectedPageIndexProperty, 0);
            return;
        }

        if (NumberOfPages > 0)
        {
            if (newIndex >= NumberOfPages)
            {
                SetCurrentValue(SelectedPageIndexProperty, NumberOfPages - 1);
                return;
            }
        }
        else
        {
            if (newIndex > 0)
            {
                SetCurrentValue(SelectedPageIndexProperty, 0);
                return;
            }
        }

        UpdateButtonsState();
        UpdatePseudoClasses();
        RequestScrollToSelectedPip();

        RaiseEvent(new PipsPagerSelectedIndexChangedEventArgs(oldIndex, newIndex));
    }

    private void OnNumberOfPagesChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var newValue = e.GetNewValue<int>();

        if (newValue < 0)
        {
            SetCurrentValue(NumberOfPagesProperty, 0);
            return;
        }

        var pips = TemplateSettings.Pips;

        if (pips.Count < newValue)
        {
            var start = pips.Count + 1;
            for (var i = start; i <= newValue; i++)
                pips.Add(i);
        }
        else if (pips.Count > newValue)
        {
            pips.RemoveRange(newValue, pips.Count - newValue);
        }

        if (newValue > 0 && SelectedPageIndex >= newValue)
            SetCurrentValue(SelectedPageIndexProperty, newValue - 1);
        else if (newValue == 0 && SelectedPageIndex > 0)
            SetCurrentValue(SelectedPageIndexProperty, 0);

        // The ListBox SelectedIndex binding may have evaluated before items were present.
        // Re-sync so the selected pip gets the :selected pseudo-class.
        SyncListBoxSelection();

        UpdateButtonsState();
        UpdatePseudoClasses();
        UpdatePagerSize();
        RequestScrollToSelectedPip();
    }

    private void OnIsPreviousButtonVisibleChanged(AvaloniaPropertyChangedEventArgs e) => UpdateButtonsState();
    private void OnIsNextButtonVisibleChanged(AvaloniaPropertyChangedEventArgs e) => UpdateButtonsState();

    private void OnOrientationChanged(AvaloniaPropertyChangedEventArgs e)
    {
        UpdatePseudoClasses();
        UpdatePagerSize();
    }

    private void OnMaxVisiblePipsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<int>() < 1)
        {
            SetCurrentValue(MaxVisiblePipsProperty, 1);
            return;
        }

        UpdatePagerSize();
    }

    private void PreviousButton_Click(object? sender, RoutedEventArgs e)
    {
        if (SelectedPageIndex > 0)
            SetCurrentValue(SelectedPageIndexProperty, SelectedPageIndex - 1);
    }

    private void NextButton_Click(object? sender, RoutedEventArgs e)
    {
        if (SelectedPageIndex < NumberOfPages - 1)
            SetCurrentValue(SelectedPageIndexProperty, SelectedPageIndex + 1);
    }

    private void UpdateButtonsState()
    {
        if (_previousButton != null)
            _previousButton.IsEnabled = SelectedPageIndex > 0;

        if (_nextButton != null)
            _nextButton.IsEnabled = SelectedPageIndex < NumberOfPages - 1;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":first-page", SelectedPageIndex == 0);
        PseudoClasses.Set(":last-page", SelectedPageIndex >= NumberOfPages - 1);
        PseudoClasses.Set(":vertical", Orientation == Orientation.Vertical);
        PseudoClasses.Set(":horizontal", Orientation == Orientation.Horizontal);
    }

    private void OnPipsPagerListSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (!_updatingPagerSize)
            UpdatePagerSize();
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        UpdateContainerAutomationProperties(e.Container, e.Index);

        if (e.Container is ListBoxItem item)
        {
            // Apply size directly: DynamicResource does not reliably update already-prepared containers.
            if (_containerPipSize != 12.0)
                ApplyContainerSize(item);

            item.IsSelected = e.Index == SelectedPageIndex;
        }
    }

    private void OnContainerIndexChanged(object? sender, ContainerIndexChangedEventArgs e)
        => UpdateContainerAutomationProperties(e.Container, e.NewIndex);

    private void UpdateContainerAutomationProperties(Control container, int index)
    {
        AutomationProperties.SetName(container, $"Page {index + 1}");
        AutomationProperties.SetPositionInSet(container, index + 1);
        AutomationProperties.SetSizeOfSet(container, NumberOfPages);
    }

    private void SyncListBoxSelection()
    {
        if (_pipsList != null && _pipsList.SelectedIndex != SelectedPageIndex)
            _pipsList.SelectedIndex = SelectedPageIndex;
    }

    private void RequestScrollToSelectedPip()
    {
        if (_scrollPending)
            return;

        _scrollPending = true;
        Dispatcher.UIThread.Post(() =>
        {
            _scrollPending = false;
            ScrollToSelectedPip();
        }, DispatcherPriority.Input);
    }

    private void ScrollToSelectedPip()
    {
        if (_pipsPagerList == null)
            return;

        // Only scroll when pips overflow MaxVisiblePips.
        if (NumberOfPages <= MaxVisiblePips)
            return;

        var scrollViewer = _pipsPagerList.FindDescendantOfType<ScrollViewer>();
        if (scrollViewer == null)
            return;

        var pipSize = GetContainerPipSize();
        if (double.IsNaN(pipSize))
            return;

        var spacing = (_pipsPagerList.ItemsPanelRoot as StackPanel)?.Spacing ?? 0.0;
        var stepSize = pipSize + spacing;

        var maxVisiblePips = MaxVisiblePips;
        var selectedIndex = SelectedPageIndex;

        // Compensate for even MaxVisiblePips when navigating forward to keep the selected
        // pip centred in the visible window.
        var evenOffset = (maxVisiblePips % 2 == 0 && selectedIndex > _lastSelectedPageIndex) ? 1 : 0;
        var offsetElements = selectedIndex + evenOffset - maxVisiblePips / 2;

        var maxOffset = Orientation == Orientation.Horizontal
            ? scrollViewer.Extent.Width - scrollViewer.Viewport.Width
            : scrollViewer.Extent.Height - scrollViewer.Viewport.Height;

        var targetOffset = Math.Clamp(offsetElements * stepSize, 0, maxOffset);
        _lastSelectedPageIndex = selectedIndex;

        if (_isInitialLoad)
        {
            // On first render, jump directly without animation.
            _isInitialLoad = false;
            scrollViewer.Offset = Orientation == Orientation.Horizontal
                ? new Vector(targetOffset, 0)
                : new Vector(0, targetOffset);
            return;
        }

        AnimateScrollOffset(scrollViewer, targetOffset);
    }

    private void AnimateScrollOffset(ScrollViewer scrollViewer, double targetOffset)
    {
        var oldCts = _scrollAnimationCts;
        _scrollAnimationCts = new CancellationTokenSource();
        var token = _scrollAnimationCts.Token;
        oldCts?.Cancel();
        oldCts?.Dispose();

        var startOffset = Orientation == Orientation.Horizontal
            ? scrollViewer.Offset.X
            : scrollViewer.Offset.Y;

        var delta = targetOffset - startOffset;
        if (Math.Abs(delta) < 0.5)
            return;

        const double duration = 200.0;
        var startTicks = Environment.TickCount64;

        void AnimateStep()
        {
            if (token.IsCancellationRequested)
                return;

            var elapsed = Environment.TickCount64 - startTicks;
            var t = Math.Min(elapsed / duration, 1.0);

            // Cubic ease-out.
            var eased = 1.0 - Math.Pow(1.0 - t, 3);
            var current = startOffset + delta * eased;

            var maxOffset = Orientation == Orientation.Horizontal
                ? scrollViewer.Extent.Width - scrollViewer.Viewport.Width
                : scrollViewer.Extent.Height - scrollViewer.Viewport.Height;
            current = Math.Clamp(current, 0, Math.Max(0, maxOffset));

            scrollViewer.Offset = Orientation == Orientation.Horizontal
                ? new Vector(current, scrollViewer.Offset.Y)
                : new Vector(scrollViewer.Offset.X, current);

            if (t < 1.0)
                DispatcherTimer.RunOnce(AnimateStep, TimeSpan.FromMilliseconds(16), DispatcherPriority.Render);
        }

        DispatcherTimer.RunOnce(AnimateStep, TimeSpan.FromMilliseconds(16), DispatcherPriority.Render);
    }

    internal void InvalidatePagerSize(double containerSize)
    {
        _containerPipSize = containerSize;
        UpdateContainerSizes();
        UpdatePagerSize();
    }

    private void UpdateContainerSizes()
    {
        if (_pipsPagerList == null)
            return;

        for (var i = 0; i < _pipsPagerList.Items.Count; i++)
        {
            if (_pipsPagerList.ContainerFromIndex(i) is ListBoxItem item)
                ApplyContainerSize(item);
        }
    }

    private void ApplyContainerSize(ListBoxItem item)
    {
        var minor = _containerPipSize;
        var major = Math.Max(minor, 24.0);

        if (Orientation == Orientation.Horizontal)
        {
            item.Width = minor;
            item.Height = major;
        }
        else
        {
            item.Width = major;
            item.Height = minor;
        }
    }

    private void UpdatePagerSize()
    {
        if (_pipsPagerList == null)
            return;

        _updatingPagerSize = true;

        try
        {
            var pipSize = GetContainerPipSize();

            if (double.IsNaN(pipSize))
            {
                _pipsPagerList.Width = double.NaN;
                _pipsPagerList.Height = double.NaN;
                return;
            }

            double spacing = 0.0;

            if (_pipsPagerList.ItemsPanelRoot is StackPanel itemsPanel)
                spacing = itemsPanel.Spacing;

            var visibleCount = Math.Min(NumberOfPages, MaxVisiblePips);

            if (visibleCount <= 0)
            {
                _pipsPagerList.Width = 0;
                _pipsPagerList.Height = 0;
                return;
            }

            var extent = (visibleCount * pipSize) + ((visibleCount - 1) * spacing);

            if (Orientation == Orientation.Horizontal)
            {
                _pipsPagerList.Width = extent;
                _pipsPagerList.Height = double.NaN;
            }
            else
            {
                _pipsPagerList.Height = extent;
                _pipsPagerList.Width = double.NaN;
            }

            RequestScrollToSelectedPip();
        }
        finally
        {
            _updatingPagerSize = false;
        }
    }

    private double GetContainerPipSize()
    {
        if (IndicatorTemplate != null && _pipsPagerList != null)
        {
            var container = (_pipsPagerList.ContainerFromIndex(SelectedPageIndex)
                             ?? (_pipsPagerList.Items.Count > 0 ? _pipsPagerList.ContainerFromIndex(0) : null))
                            as Layoutable;
            if (container != null)
            {
                var size = Orientation == Orientation.Horizontal
                    ? container.Bounds.Width
                    : container.Bounds.Height;
                if (size > 0)
                    return size;
            }
            return double.NaN;
        }

        return _containerPipSize;
    }
}
