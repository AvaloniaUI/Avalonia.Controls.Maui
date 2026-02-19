namespace ControlGallery.Pages.ShellSamples;

public class Animal
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    public override string ToString() => Name;
}
