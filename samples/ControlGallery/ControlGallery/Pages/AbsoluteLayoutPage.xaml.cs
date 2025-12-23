namespace ControlGallery.Pages;

public partial class AbsoluteLayoutPage : ContentPage
{
    public AbsoluteLayoutPage()
    {
        InitializeComponent();
    }

    private void OnChessboardSizeChanged(object? sender, EventArgs e)
    {
        if (sender is ContentView contentView)
        {
            // Make the chessboard square based on the smaller dimension
            double size = Math.Min(contentView.Width, 240);
            if (size > 0)
            {
                ChessboardLayout.WidthRequest = size;
                ChessboardLayout.HeightRequest = size;
            }
        }
    }
}
