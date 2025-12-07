using ScrollBarVisibility = Microsoft.Maui.ScrollBarVisibility;

namespace ControlGallery.Pages;

public partial class ScrollViewPage : ContentPage
{
    public ScrollViewPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        DetectScrollView.SizeChanged += OnDetectScrollViewSizeChanged;
        HorizontalDetectScrollView.SizeChanged += OnHorizontalDetectScrollViewSizeChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        DetectScrollView.SizeChanged -= OnDetectScrollViewSizeChanged;
        HorizontalDetectScrollView.SizeChanged -= OnHorizontalDetectScrollViewSizeChanged;
    }

    private void OnDetectScrollViewScrolled(object? sender, ScrolledEventArgs e)
    {
        VerticalScrollStatusLabel.Text = $"Scrolled to: ({Math.Round(e.ScrollX, 1)}, {Math.Round(e.ScrollY, 1)})";
        VerticalScrollXYLabel.Text = $"ScrollX: {Math.Round(DetectScrollView.ScrollX, 1)}, ScrollY: {Math.Round(DetectScrollView.ScrollY, 1)}";
        UpdateVerticalContentSizeLabel();
    }

    private void OnHorizontalDetectScrollViewScrolled(object? sender, ScrolledEventArgs e)
    {
        HorizontalStatusLabel.Text = $"Scrolled to: ({Math.Round(e.ScrollX, 1)}, {Math.Round(e.ScrollY, 1)})";
        HorizontalScrollXYLabel.Text = $"ScrollX: {Math.Round(HorizontalDetectScrollView.ScrollX, 1)}, ScrollY: {Math.Round(HorizontalDetectScrollView.ScrollY, 1)}";
        UpdateHorizontalContentSizeLabel();
    }

    private void OnDetectScrollViewSizeChanged(object? sender, EventArgs e)
    {
        UpdateVerticalContentSizeLabel();
    }

    private void OnHorizontalDetectScrollViewSizeChanged(object? sender, EventArgs e)
    {
        UpdateHorizontalContentSizeLabel();
    }

    private void UpdateVerticalContentSizeLabel()
    {
        VerticalContentSizeLabel.Text = $"Content size: ({Math.Round(DetectScrollView.ContentSize.Width, 1)}, {Math.Round(DetectScrollView.ContentSize.Height, 1)})";
    }

    private void UpdateHorizontalContentSizeLabel()
    {
        HorizontalContentSizeLabel.Text = $"Content size: ({Math.Round(HorizontalDetectScrollView.ContentSize.Width, 1)}, {Math.Round(HorizontalDetectScrollView.ContentSize.Height, 1)})";
    }

    private async void OnScrollToTopClicked(object? sender, EventArgs e)
    {
        await ProgrammaticScrollView.ScrollToAsync(0, 0, true);
    }

    private async void OnScrollToBottomClicked(object? sender, EventArgs e)
    {
        await ProgrammaticScrollView.ScrollToAsync(ProgrammaticBottomMarker, ScrollToPosition.End, true);
    }

    private void OnHorizontalVisibilityClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        VisibilityScrollView.HorizontalScrollBarVisibility = ToVisibility(button.Text);
    }

    private void OnVerticalVisibilityClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        VisibilityScrollView.VerticalScrollBarVisibility = ToVisibility(button.Text);
    }

    private void OnVisibilityOrientationClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        var orientation = ToOrientation(button.Text);
        VisibilityScrollView.Orientation = orientation;

        // Adjust padding when horizontal scrolling is in play to avoid clipping the last item
        if (orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both)
        {
            if (VisibilityScrollView.Content is Layout layout)
            {
                layout.Padding = new Thickness(0, 0, 20, 0);
            }
        }
        else if (VisibilityScrollView.Content is Layout layoutReset)
        {
            layoutReset.Padding = new Thickness(0);
        }
    }

    private void OnOrientationButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        var orientation = ToOrientation(button.Text);
        DynamicOrientationScrollView.Orientation = orientation;

        switch (orientation)
        {
            case ScrollOrientation.Horizontal:
                DynamicOrientationStack.Orientation = StackOrientation.Horizontal;
                DynamicOrientationScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Always;
                DynamicOrientationScrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
                break;
            case ScrollOrientation.Both:
                DynamicOrientationStack.Orientation = StackOrientation.Horizontal;
                DynamicOrientationScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Always;
                DynamicOrientationScrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
                break;
            case ScrollOrientation.Neither:
                DynamicOrientationStack.Orientation = StackOrientation.Vertical;
                DynamicOrientationScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
                DynamicOrientationScrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
                break;
            default:
                DynamicOrientationStack.Orientation = StackOrientation.Vertical;
                DynamicOrientationScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Default;
                DynamicOrientationScrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
                break;
        }
    }

    private static ScrollBarVisibility ToVisibility(string? value)
    {
        return value switch
        {
            "Always" or "H Always" or "V Always" => ScrollBarVisibility.Always,
            "Never" or "H Never" or "V Never" => ScrollBarVisibility.Never,
            _ => ScrollBarVisibility.Default
        };
    }

    private static ScrollOrientation ToOrientation(string? value)
    {
        return value switch
        {
            "Horizontal" => ScrollOrientation.Horizontal,
            "Both" => ScrollOrientation.Both,
            "Neither" => ScrollOrientation.Neither,
            _ => ScrollOrientation.Vertical
        };
    }
}