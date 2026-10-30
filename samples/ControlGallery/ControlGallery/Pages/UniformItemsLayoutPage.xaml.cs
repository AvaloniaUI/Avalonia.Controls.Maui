namespace ControlGallery.Pages;

public partial class UniformItemsLayoutPage : ContentPage
{
    private static readonly Color[] TileColors =
    [
        Color.FromArgb("#E57373"),
        Color.FromArgb("#81C784"),
        Color.FromArgb("#64B5F6"),
        Color.FromArgb("#FFD54F"),
        Color.FromArgb("#BA68C8"),
        Color.FromArgb("#4DD0E1"),
    ];

    public UniformItemsLayoutPage()
    {
        InitializeComponent();
    }

    private void OnAddTileClicked(object? sender, EventArgs e)
    {
        TilesLayout.Add(new BoxView
        {
            WidthRequest = 60,
            HeightRequest = 60,
            Margin = 2,
            Color = TileColors[TilesLayout.Count % TileColors.Length],
        });
    }

    private void OnRemoveTileClicked(object? sender, EventArgs e)
    {
        if (TilesLayout.Count > 0)
            TilesLayout.RemoveAt(TilesLayout.Count - 1);
    }
}
