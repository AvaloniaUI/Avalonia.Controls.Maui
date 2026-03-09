namespace MauiConverter;

class BounceButton : Button
{
	public BounceButton() => Clicked += HandleButtonClick;

	async void HandleButtonClick(object? sender, EventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);

		var bounceButton = (BounceButton)sender;
		await Dispatcher.DispatchAsync(async () =>
		{
			Unfocus();
			await bounceButton.ScaleTo(1.05, 100);
			await bounceButton.ScaleTo(1, 100);
		});
	}
}