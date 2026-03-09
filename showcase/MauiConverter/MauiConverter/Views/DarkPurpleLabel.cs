namespace MauiConverter;

class DarkPurpleLabel : Label
{
	public DarkPurpleLabel(in string? text = null)
	{
		Text = text;
		TextColor = ColorConstants.DarkestPurple;
		VerticalTextAlignment = TextAlignment.Center;
	}
}