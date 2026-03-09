using System.Windows.Input;

namespace MauiConverter;

class UnitsPicker : Picker
{
	public static readonly BindableProperty SelectedIndexChangedCommandProperty =
		BindableProperty.Create(nameof(SelectedIndexChangedCommand), typeof(ICommand), typeof(UnitsPicker), null);

	public static readonly BindableProperty SelectedIndexChangedCommandParameterProperty =
		BindableProperty.Create(nameof(SelectedIndexChangedCommandParameter), typeof(object), typeof(UnitsPicker), null);

	public UnitsPicker(string title)
	{
		Title = title;
		TextColor = Colors.Black;
		BackgroundColor = ColorConstants.LightestPurple;
		SelectedIndexChanged += HandleSelectedIndexChanged;
	}

	public ICommand? SelectedIndexChangedCommand
	{
		get => (ICommand?)GetValue(SelectedIndexChangedCommandProperty);
		set => SetValue(SelectedIndexChangedCommandProperty, value);
	}

	public object? SelectedIndexChangedCommandParameter
	{
		get => GetValue(SelectedIndexChangedCommandParameterProperty);
		set => SetValue(SelectedIndexChangedCommandParameterProperty, value);
	}

	void HandleSelectedIndexChanged(object? sender, EventArgs e)
	{
		if (SelectedIndexChangedCommand?.CanExecute(SelectedIndexChangedCommandParameter) is true)
			SelectedIndexChangedCommand.Execute(SelectedIndexChangedCommandParameter);
	}
}