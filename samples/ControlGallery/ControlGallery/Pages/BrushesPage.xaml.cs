namespace ControlGallery.Pages;

public partial class BrushesPage : ContentPage
{
	public BrushesPage()
	{
		InitializeComponent();
	}

    private async void OnAnimateColorsClicked(object sender, EventArgs e)
    {
        var duration = 2000u;
        
        // Simple color animation loop
        var stop1Start = Colors.Red;
        var stop1End = Colors.Yellow;
        var stop2Start = Colors.Blue;
        var stop2End = Colors.Green;

        var anim1 = new Animation(v => Stop1.Color = GetColor(stop1Start, stop1End, v), 0, 1);
        var anim2 = new Animation(v => Stop2.Color = GetColor(stop2Start, stop2End, v), 0, 1);
        
        var parentAnim = new Animation();
        parentAnim.Add(0, 1, anim1);
        parentAnim.Add(0, 1, anim2);

        parentAnim.Commit(this, "ColorAnimation", 16, duration, Easing.Linear, (v, c) => 
        {
            // Reverse
            (stop1Start, stop1End) = (stop1End, stop1Start);
            (stop2Start, stop2End) = (stop2End, stop2Start);
        });
    }
    
    // Helper to interpolate colors
    private Color GetColor(Color start, Color end, double progress)
    {
        return new Color(
            (float)(start.Red + (end.Red - start.Red) * progress),
            (float)(start.Green + (end.Green - start.Green) * progress),
            (float)(start.Blue + (end.Blue - start.Blue) * progress),
            (float)(start.Alpha + (end.Alpha - start.Alpha) * progress));
    }
}
