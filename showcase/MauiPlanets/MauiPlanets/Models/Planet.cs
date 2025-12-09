namespace MauiPlanets.Models;

public class Planet
{
    public required string Name { get; set; }
    public required string Subtitle { get; set; }
    public required string HeroImage { get; set; }
    public required string Description { get; set; }
    public required Color AccentColorStart { get; set; }
    public required Color AccentColorEnd { get; set; }
    public required List<string> Images { get; set; }

    // Background
    public Brush Background
    {
        get
        {
            var gradientStops = new GradientStopCollection();
            gradientStops.Add(new GradientStop(AccentColorStart, 0.0f));
            gradientStops.Add(new GradientStop(AccentColorEnd, 1.0f));

            var bgBrush = new LinearGradientBrush(
                gradientStops,
                new Point(0.0, 0.0),
                new Point(1.0, 1.0));

            return bgBrush;
        }
    }    
}
