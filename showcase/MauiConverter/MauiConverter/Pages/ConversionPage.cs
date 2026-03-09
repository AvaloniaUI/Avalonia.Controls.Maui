using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace MauiConverter;

class ConversionPage : BaseContentPage<ConversionViewModel>
{
	readonly Border _resultBorder;
	string _previousResult = string.Empty;

	public ConversionPage(ConversionViewModel conversionViewModel) : base(conversionViewModel)
	{
		BackgroundColor = ColorConstants.LightPurple;
		BindingContext.ConversionError += HandleConversionError;
		BindingContext.PropertyChanged += OnViewModelPropertyChanged;

		this.Bind(TitleProperty, nameof(ConversionViewModel.TitleText));

		_resultBorder = new Border
		{
			StrokeShape = new RoundRectangle { CornerRadius = 8 },
			Stroke = ColorConstants.DarkPurple,
			BackgroundColor = Colors.White,
			Padding = new Thickness(16, 12),
			IsVisible = false,
			Opacity = 0,
			Scale = 0.8,
			Content = new Label
			{
				TextColor = ColorConstants.DarkestPurple,
				FontSize = 18,
				FontAttributes = FontAttributes.Bold,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
			}.Bind(Label.TextProperty,
				static (ConversionViewModel vm) => vm.ConvertedNumberLabelText)
		};

		Content = new Grid
		{
			RowSpacing = 8,
			ColumnSpacing = 20,

			RowDefinitions = Rows.Define(
				(Row.UnitType, Star),
				(Row.NumberToConvert, Star),
				(Row.OriginalUnits, Star),
				(Row.ConvertedUnits, Star),
				(Row.ConvertButton, Stars(2)),
				(Row.ConvertResult, Star)),

			ColumnDefinitions = Columns.Define(
				(Column.Label, Star),
				(Column.Input, Star)),

			Children =
			{
				new DarkPurpleLabel("Unit Type")
				   .Row(Row.UnitType).Column(Column.Label),

				new UnitsPicker("Unit Type")
				   .Row(Row.UnitType).Column(Column.Input)
				   .Bind(Picker.ItemsSourceProperty,
							static (ConversionViewModel vm) => vm.UnitTypePickerList,
							mode: BindingMode.OneTime)
				   .Bind(Picker.SelectedIndexProperty,
							static (ConversionViewModel vm) => vm.UnitTypePickerSelectedIndex,
							static (ConversionViewModel vm, int index) => vm.UnitTypePickerSelectedIndex = index)
				   .Bind(UnitsPicker.SelectedIndexChangedCommandProperty,
							static (ConversionViewModel vm) => vm.UnitTypePickerSelectedIndexChangedCommand,
							mode: BindingMode.OneTime),

				new DarkPurpleLabel("Number to Convert")
				   .Row(Row.NumberToConvert).Column(Column.Label),

				new Entry { Keyboard = Keyboard.Numeric }
				   .Row(Row.NumberToConvert).Column(Column.Input)
				   .Placeholder("Enter Number")
				   .TextColor(Colors.Black)
				   .BackgroundColor(ColorConstants.LightestPurple)
				   .Bind(Entry.TextProperty,
							static (ConversionViewModel vm) => vm.NumberToConvertEntryText,
							static (ConversionViewModel vm,string text) => vm.NumberToConvertEntryText = text),

				new DarkPurpleLabel("Original Units")
				   .Row(Row.OriginalUnits).Column(Column.Label),

				new UnitsPicker("Original Units")
				   .Row(Row.OriginalUnits).Column(Column.Input)
				   .Bind(Picker.ItemsSourceProperty,
							static (ConversionViewModel vm) => vm.OriginalUnitsPickerList)
				   .Bind(Picker.SelectedItemProperty,
							static (ConversionViewModel vm) => vm.OriginalUnitsPickerSelectedItem,
							static (ConversionViewModel vm, string selectedUnit) => vm.OriginalUnitsPickerSelectedItem = selectedUnit)
				   .Bind(UnitsPicker.SelectedIndexChangedCommandProperty,
							static (ConversionViewModel vm) => vm.OriginalUnitsPickerSelectedIndexChangedCommand,
							mode: BindingMode.OneTime),

				new DarkPurpleLabel("Converted Units")
				   .Row(Row.ConvertedUnits).Column(Column.Label),

				new UnitsPicker("Converted Units")
				   .Row(Row.ConvertedUnits).Column(Column.Input)
				   .Bind(Picker.ItemsSourceProperty,
							static (ConversionViewModel vm) => vm.ConvertedUnitsPickerList)
				   .Bind(Picker.SelectedItemProperty,
							static (ConversionViewModel vm) => vm.ConvertedUnitsPickerSelectedItem,
							static (ConversionViewModel vm, string selectedUnit) => vm.ConvertedUnitsPickerSelectedItem = selectedUnit)
				   .Bind(UnitsPicker.SelectedIndexChangedCommandProperty,
							static (ConversionViewModel vm) => vm.ConvertedUnitsPickerSelectedIndexChangedCommand,
							mode:BindingMode.OneTime),

				_resultBorder
				   .Row(Row.ConvertResult).ColumnSpan(All<Column>()).Margins(left: 20, right: 20),

				new BounceButton()
				   .Row(Row.ConvertButton).ColumnSpan(All<Column>()).FillHorizontal()
				   .BackgroundColor(ColorConstants.DarkPurple)
				   .Text("Convert", Colors.White)
				   .Margin(20)
				   .Bind(Button.CommandProperty,
							static (ConversionViewModel vm) => vm.ConvertButtonCommand,
							mode: BindingMode.OneTime)
			}
		}.FillHorizontal().CenterVertical().Paddings(left: 20, right: 20);
	}

	enum Row { UnitType, NumberToConvert, OriginalUnits, ConvertedUnits, ConvertButton, ConvertResult };
	enum Column { Label, Input };

	void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(ConversionViewModel.ConvertedNumberLabelText))
			return;

		var newResult = BindingContext.ConvertedNumberLabelText;
		var hadResult = !string.IsNullOrEmpty(_previousResult);
		var hasResult = !string.IsNullOrEmpty(newResult);
		_previousResult = newResult;

		if (hasResult && !hadResult)
			AnimateResultIn();
		else if (!hasResult && hadResult)
			AnimateResultOut();
	}

	async void AnimateResultIn()
	{
		_resultBorder.IsVisible = true;
		await Task.WhenAll(
			_resultBorder.FadeTo(1, 250, Easing.CubicOut),
			_resultBorder.ScaleTo(1, 250, Easing.CubicOut));
	}

	async void AnimateResultOut()
	{
		await Task.WhenAll(
			_resultBorder.FadeTo(0, 200, Easing.CubicIn),
			_resultBorder.ScaleTo(0.8, 200, Easing.CubicIn));
		_resultBorder.IsVisible = false;
	}

	async void HandleConversionError(object? sender, string message) =>
		await Dispatcher.DispatchAsync(() => DisplayAlert("Conversion Error", message, "OK"));
}