using CommunityToolkit.Maui.Views;

namespace ControlGallery.Pages;

public partial class LazyViewPage : ContentPage
{
    private readonly LazyView<LazyDetailsView> _onDemandView = new();
    private readonly SlowLazyView _slowView = new();

    public LazyViewPage()
    {
        InitializeComponent();
        OnDemandHost.Content = _onDemandView;
        SlowHost.Content = _slowView;
    }

    private async void OnLoadViewClicked(object? sender, EventArgs e)
    {
        await _onDemandView.LoadViewAsync();
        OnDemandStatusLabel.Text = $"HasLazyViewLoaded: {_onDemandView.HasLazyViewLoaded}";
    }

    private async void OnLoadSlowViewClicked(object? sender, EventArgs e)
    {
        if (_slowView.HasLazyViewLoaded)
            return;

        SlowStatusLabel.Text = "Loading...";
        await _slowView.LoadViewAsync();
        SlowStatusLabel.Text = $"HasLazyViewLoaded: {_slowView.HasLazyViewLoaded}";
    }
}

/// <summary>
/// The view that the LazyView examples create when they load.
/// </summary>
public class LazyDetailsView : VerticalStackLayout
{
    public LazyDetailsView()
    {
        Spacing = 4;
        Padding = 10;
        BackgroundColor = Color.FromArgb("#E8EAF6");
        Children.Add(new Label { Text = "This view was created when it was loaded.", TextColor = Colors.Black });
        Children.Add(new Label { Text = $"Created at {DateTime.Now:T}", TextColor = Colors.Black });
    }
}

/// <summary>
/// A LazyView that shows an activity indicator while it prepares its content.
/// </summary>
public class SlowLazyView : LazyView
{
    public override async ValueTask LoadViewAsync(CancellationToken token = default)
    {
        Content = new ActivityIndicator { IsRunning = true, HorizontalOptions = LayoutOptions.Start };
        await Task.Delay(1500, token);
        Content = new LazyDetailsView();
        SetHasLazyViewLoaded(true);
    }
}
