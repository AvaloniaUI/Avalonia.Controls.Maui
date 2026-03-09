namespace MauiConverter;

class AppShell : Shell
{
	public AppShell(ConversionPage conversionPage)
	{
		Items.Add(conversionPage);
	}
}