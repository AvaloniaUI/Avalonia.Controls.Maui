namespace ControlGallery.Pages;

public partial class TimePickerPage : ContentPage
{
	public TimePickerPage()
	{
		InitializeComponent();
	}

    private void OnSetTime8Clicked(object sender, EventArgs e)
    {
        ProgrammaticTimePicker.Time = new TimeSpan(8, 0, 0);
    }

    private void OnSetTime12Clicked(object sender, EventArgs e)
    {
        ProgrammaticTimePicker.Time = new TimeSpan(12, 0, 0);
    }

    private void OnSetTime17Clicked(object sender, EventArgs e)
    {
        ProgrammaticTimePicker.Time = new TimeSpan(17, 0, 0);
    }
}