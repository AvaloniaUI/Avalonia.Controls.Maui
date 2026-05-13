using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace MauiSandboxApp;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear();

		using var paint = new SKPaint { Style = SKPaintStyle.Fill };

		if (sender == CvFill)
			paint.Color = SKColors.Purple;
		else if (sender == CvStart)
			paint.Color = SKColors.Red;
		else if (sender == CvFixed)
			paint.Color = SKColors.Green;
		else
			paint.Color = SKColors.Gray;

		canvas.DrawRect(e.Info.Rect, paint);
	}
}
