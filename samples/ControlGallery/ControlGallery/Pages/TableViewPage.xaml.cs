using System.Windows.Input;

namespace ControlGallery.Pages;

public partial class TableViewPage : ContentPage
{
    public ICommand TapCommand { get; }
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
}
