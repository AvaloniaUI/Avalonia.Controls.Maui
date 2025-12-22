namespace ControlGallery.Pages;

public partial class DatePickerPage : ContentPage
{
    public DatePickerPage()
    {
        InitializeComponent();
    }

    private void OnBasicDateSelected(object? sender, DateChangedEventArgs e)
    {
        SelectedDateLabel.Text = $"Selected date: {e.NewDate:D}";
    }

    private void OnRangeDateSelected(object? sender, DateChangedEventArgs e)
    {
        RangeDateLabel.Text = $"Date: {e.NewDate:MMMM d, yyyy}";
    }

    private void OnSetTodayClicked(object? sender, EventArgs e)
    {
        ProgrammaticDatePicker.Date = DateTime.Today;
    }

    private void OnClearDateClicked(object? sender, EventArgs e)
    {
        // Note: MAUI DatePicker doesn't support null dates in all versions
        // Setting to MinimumDate as a way to "clear"
        ProgrammaticDatePicker.Date = ProgrammaticDatePicker.MinimumDate;
    }

    private void OnSetTo2025Clicked(object? sender, EventArgs e)
    {
        ProgrammaticDatePicker.Date = new DateTime(2025, 1, 1);
    }
}
