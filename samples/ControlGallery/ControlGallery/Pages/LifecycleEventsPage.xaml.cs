using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Dispatching;

namespace ControlGallery.Pages;

public partial class LifecycleEventsPage : ContentPage
{
    private const int MaxLogEntries = 200;
    private int _sequence;
    private int _bindingContextVersion;
    private bool _isActive;
    private readonly Dictionary<Element, string> _labels = new();
    private readonly HashSet<string> _propertyFilter = new(StringComparer.Ordinal)
    {
        nameof(VisualElement.Width),
        nameof(VisualElement.Height),
        nameof(VisualElement.IsVisible),
        nameof(VisualElement.Opacity),
        nameof(VisualElement.IsFocused),
        nameof(Element.Parent),
        nameof(Element.BindingContext)
    };

    public ObservableCollection<string> EventLog { get; } = new();

    public LifecycleEventsPage()
    {
        InitializeComponent();
        BindingContext = this;

        RegisterElement(TrackedBorder, "Tracked");
        RegisterElement(FocusEntry, "Entry");
        RegisterElement(ParentA, "ParentA");
        RegisterElement(ParentB, "ParentB");

        HookElementEvents(TrackedBorder, logPropertyChanges: true);
        HookElementEvents(FocusEntry, logPropertyChanges: false);
        HookElementEvents(ParentA, logPropertyChanges: false);
        HookElementEvents(ParentB, logPropertyChanges: false);

        WidthSlider.Value = 160;
        HeightSlider.Value = 80;
        ApplyTrackedSize();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isActive = true;
    }

    protected override void OnDisappearing()
    {
        _isActive = false;
        base.OnDisappearing();
    }

    private void RegisterElement(Element element, string label)
    {
        _labels[element] = label;
    }

    private string GetLabel(Element? element)
    {
        if (element is null)
            return "null";

        return _labels.TryGetValue(element, out var label)
            ? label
            : element.GetType().Name;
    }

    private void HookElementEvents(VisualElement element, bool logPropertyChanges)
    {
        element.Loaded += (_, _) => Log($"{GetLabel(element)}: Loaded");
        element.Unloaded += (_, _) => Log($"{GetLabel(element)}: Unloaded");
        element.SizeChanged += (_, _) =>
            Log($"{GetLabel(element)}: SizeChanged {element.Width:0}x{element.Height:0}");
        element.Focused += (_, _) => Log($"{GetLabel(element)}: Focused");
        element.Unfocused += (_, _) => Log($"{GetLabel(element)}: Unfocused");
        element.BindingContextChanged += (_, _) => Log($"{GetLabel(element)}: BindingContextChanged");
        element.ParentChanging += (_, e) =>
            Log($"{GetLabel(element)}: ParentChanging {GetLabel(e.OldParent)} -> {GetLabel(e.NewParent)}");
        element.ParentChanged += (_, _) =>
            Log($"{GetLabel(element)}: ParentChanged {GetLabel(element.Parent)}");
        element.HandlerChanging += (_, _) => Log($"{GetLabel(element)}: HandlerChanging");
        element.HandlerChanged += (_, _) => Log($"{GetLabel(element)}: HandlerChanged");
        element.ChildAdded += (_, e) => Log($"{GetLabel(element)}: ChildAdded {GetLabel(e.Element)}");
        element.ChildRemoved += (_, e) => Log($"{GetLabel(element)}: ChildRemoved {GetLabel(e.Element)}");
        element.ChildrenReordered += (_, _) => Log($"{GetLabel(element)}: ChildrenReordered");
        element.DescendantAdded += (_, e) => Log($"{GetLabel(element)}: DescendantAdded {GetLabel(e.Element)}");
        element.DescendantRemoved += (_, e) => Log($"{GetLabel(element)}: DescendantRemoved {GetLabel(e.Element)}");

        if (logPropertyChanges)
        {
            element.PropertyChanged += OnElementPropertyChanged;
            element.PropertyChanging += OnElementPropertyChanging;
        }
    }

    private void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not VisualElement element)
            return;

        if (string.IsNullOrWhiteSpace(e.PropertyName) || !_propertyFilter.Contains(e.PropertyName))
            return;

        Log($"{GetLabel(element)}: PropertyChanged {e.PropertyName}");
    }

    private void OnElementPropertyChanging(object? sender, Microsoft.Maui.Controls.PropertyChangingEventArgs e)
    {
        if (sender is not VisualElement element)
            return;

        if (string.IsNullOrWhiteSpace(e.PropertyName) || !_propertyFilter.Contains(e.PropertyName))
            return;

        Log($"{GetLabel(element)}: PropertyChanging {e.PropertyName}");
    }

    private void Log(string message)
    {
        var entry = $"{++_sequence:000} {message}";
        Debug.WriteLine(entry);
        Console.WriteLine(entry);

        if (!_isActive)
            return;

        if (Dispatcher.IsDispatchRequired)
        {
            Dispatcher.Dispatch(() =>
            {
                if (_isActive)
                    AppendLogEntry(entry);
            });
        }
        else
        {
            AppendLogEntry(entry);
        }
    }

    private void AppendLogEntry(string entry)
    {
        EventLog.Insert(0, entry);

        if (EventLog.Count > MaxLogEntries)
        {
            EventLog.RemoveAt(EventLog.Count - 1);
        }
    }

    private void OnToggleChildClicked(object sender, EventArgs e)
    {
        if (TrackedBorder.Parent is Layout parent)
        {
            parent.Children.Remove(TrackedBorder);
            Log("Action: Tracked element removed");
            return;
        }

        InsertTracked(ParentA);
        Log("Action: Tracked element added to ParentA");
    }

    private void OnMoveChildClicked(object sender, EventArgs e)
    {
        if (TrackedBorder.Parent == ParentA)
        {
            ParentA.Children.Remove(TrackedBorder);
            InsertTracked(ParentB);
            Log("Action: Tracked element moved to ParentB");
        }
        else if (TrackedBorder.Parent == ParentB)
        {
            ParentB.Children.Remove(TrackedBorder);
            InsertTracked(ParentA);
            Log("Action: Tracked element moved to ParentA");
        }
        else
        {
            InsertTracked(ParentA);
            Log("Action: Tracked element added to ParentA");
        }
    }

    private void OnReorderChildrenClicked(object sender, EventArgs e)
    {
        if (TrackedBorder.Parent is not Layout parent)
            return;

        var trackedIndex = parent.Children.IndexOf(TrackedBorder);
        if (trackedIndex < 0 || parent.Children.Count < 2)
            return;

        var targetIndex = trackedIndex == parent.Children.Count - 1 ? 1 : trackedIndex + 1;

        if (parent.Children is ObservableCollection<View> collection)
        {
            collection.Move(trackedIndex, targetIndex);
        }
        else
        {
            parent.Children.RemoveAt(trackedIndex);
            parent.Children.Insert(targetIndex, TrackedBorder);
        }

        Log($"Action: Reordered children in {GetLabel(parent)}");
    }

    private void OnBindingContextClicked(object sender, EventArgs e)
    {
        _bindingContextVersion++;
        TrackedBorder.BindingContext = new { Version = _bindingContextVersion };
        Log("Action: Tracked BindingContext updated");
    }

    private void OnClearLogClicked(object sender, EventArgs e)
    {
        EventLog.Clear();
        _sequence = 0;
    }

    private void OnFocusEntryClicked(object sender, EventArgs e)
    {
        FocusEntry.Focus();
        Log("Action: Focus entry");
    }

    private void OnUnfocusEntryClicked(object sender, EventArgs e)
    {
        FocusEntry.Unfocus();
        Log("Action: Unfocus entry");
    }

    private void OnSizeSliderChanged(object sender, ValueChangedEventArgs e)
    {
        ApplyTrackedSize();
    }

    private void ApplyTrackedSize()
    {
        TrackedBorder.WidthRequest = WidthSlider.Value;
        TrackedBorder.HeightRequest = HeightSlider.Value;

        WidthValueLabel.Text = $"{WidthSlider.Value:0}";
        HeightValueLabel.Text = $"{HeightSlider.Value:0}";
    }

    private void InsertTracked(Layout parent)
    {
        var insertIndex = Math.Min(1, parent.Children.Count);
        parent.Children.Insert(insertIndex, TrackedBorder);
    }
}
