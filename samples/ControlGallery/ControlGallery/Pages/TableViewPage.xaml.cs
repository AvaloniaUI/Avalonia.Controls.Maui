using System.Windows.Input;

namespace ControlGallery.Pages;

public partial class TableViewPage : ContentPage
{
    public ICommand TapCommand { get; }
    public ICommand ContextActionCommand { get; }
    private int _tapCount;

    public TableViewPage()
    {
        InitializeComponent();
        TapCommand = new Command(() =>
        {
            _tapCount++;
            CommandStatusLabel.Text = $"Command executed! (Tap #{_tapCount})";
            CommandStatusLabel.TextColor = Colors.Green;
        });

        ContextActionCommand = new Command<string>((actionName) =>
        {
            ContextStatusLabel.Text = $"Context Action: {actionName}";
            ContextStatusLabel.TextColor = actionName == "Delete" ? Colors.Red : Colors.Blue;
        });
        BindingContext = this;
    }

    private void OnFlowDirectionToggled(object sender, ToggledEventArgs e)
    {
        if (RtlTableView != null && FlowDirectionLabel != null)
        {
            RtlTableView.FlowDirection = e.Value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            FlowDirectionLabel.Text = $"FlowDirection: {RtlTableView.FlowDirection}";
        }
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        if (sender is EntryCell cell)
        {
            CommandStatusLabel.Text = $"Entry completed! Text: {cell.Text}";
            CommandStatusLabel.TextColor = Colors.Blue;
        }
    }

    private int _appearingCount;
    private int _disappearingCount;

    private void OnCellAppearing(object sender, EventArgs e)
    {
        _appearingCount++;
        if (sender is TextCell cell)
        {
            LifecycleStatusLabel.Text = $"Appearing: {cell.Text} (Total: {_appearingCount} appearing, {_disappearingCount} disappearing)";
            LifecycleStatusLabel.TextColor = Colors.Green;
        }
    }

    private void OnCellDisappearing(object sender, EventArgs e)
    {
        _disappearingCount++;
        if (sender is TextCell cell)
        {
            LifecycleStatusLabel.Text = $"Disappearing: {cell.Text} (Total: {_appearingCount} appearing, {_disappearingCount} disappearing)";
            LifecycleStatusLabel.TextColor = Colors.Orange;
        }
    }

    private readonly Random _random = new();

    private void OnRandomizeHeightsClicked(object sender, EventArgs e)
    {
        var height1 = _random.Next(48, 120);
        var height2 = _random.Next(48, 120);
        var height3 = _random.Next(48, 120);

        DynamicCell1.Height = height1;
        DynamicCell1.Detail = $"Height: {height1}";

        DynamicCell2.Height = height2;
        DynamicCell2.Detail = $"Height: {height2}";

        DynamicCell3.Height = height3;
        DynamicCell3.Detail = $"Height: {height3}";

        HeightStatusLabel.Text = $"Heights: {height1}, {height2}, {height3}";
        HeightStatusLabel.TextColor = Colors.Purple;
    }
}
