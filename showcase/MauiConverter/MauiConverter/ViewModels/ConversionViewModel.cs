using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiConverter;

partial class ConversionViewModel : BaseViewModel
{
	readonly WeakEventManager _conversionErrorEventManager = new();
	readonly IReadOnlyDictionary<string, UnitOfMeasurementModel> _unitOfMeasurementDictionary;

	[ObservableProperty]
	int _unitTypePickerSelectedIndex;

	[ObservableProperty]
	string _titleText = string.Empty,
			_numberToConvertEntryText = "0",
			_convertedNumberLabelText = string.Empty,
			_originalUnitsPickerSelectedItem = string.Empty,
			_convertedUnitsPickerSelectedItem = string.Empty;

	[ObservableProperty]
	IReadOnlyList<string> _originalUnitsPickerList = Enumerable.Empty<string>().ToList(),
							_convertedUnitsPickerList = Enumerable.Empty<string>().ToList();

	public ConversionViewModel(Feet feet,
								Miles miles,
								Yards yards,
								Kelvin kelvin,
								Inches inches,
								Meters meters,
								Ounces ounces,
								Pounds pounds,
								Celsius celsius,
								Kilograms kilograms,
								Fahrenheit fahrenheit,
								Kilometers kilometers)
	{
		_unitOfMeasurementDictionary = new Dictionary<string, UnitOfMeasurementModel>
		{
			{ nameof(Celsius), celsius },
			{ nameof(Fahrenheit), fahrenheit},
			{ nameof(Feet), feet },
			{ nameof(Inches), inches },
			{ nameof(Kelvin), kelvin },
			{ nameof(Kilograms), kilograms },
			{ nameof(Kilometers), kilometers },
			{ nameof(Meters), meters },
			{ nameof(Ounces), ounces },
			{ nameof(Miles), miles },
			{ nameof(Pounds), pounds },
			{ nameof(Yards), yards },
		};

		UnitOfMeasurement initialUnitOfMeasurement = 0;

		PopulateUnitsPickerLists(initialUnitOfMeasurement);
		SetTitleText(initialUnitOfMeasurement);
	}

	public event EventHandler<string> ConversionError
	{
		add => _conversionErrorEventManager.AddEventHandler(value);
		remove => _conversionErrorEventManager.RemoveEventHandler(value);
	}

	public IReadOnlyList<string> UnitTypePickerList { get; } = Enum.GetNames(typeof(UnitOfMeasurement));

	void PopulateUnitsPickerLists(UnitOfMeasurement unitOfMeasurement)
	{
		var originalUnitsPickerList = new List<string>();
		var convertedUnitsPickerList = new List<string>();

		var listOfSelectedUnits = _unitOfMeasurementDictionary.Where(x => x.Value.MeasurementType.Equals(unitOfMeasurement));
		foreach (var item in listOfSelectedUnits)
		{
			originalUnitsPickerList.Add(item.Key);
			convertedUnitsPickerList.Add(item.Key);
		}

		OriginalUnitsPickerList = originalUnitsPickerList;
		ConvertedUnitsPickerList = convertedUnitsPickerList;
	}

	[RelayCommand]
	void OriginalUnitsPickerSelectedIndexChanged()
	{
		if (IsConvertedUnitsPickerSelectedItemValid()
			&& IsNumberToConvertEntryTextValid())
		{
			ConvertUnits();
		}
	}

	[RelayCommand]
	void ConvertedUnitsPickerSelectedIndexChanged()
	{
		if (IsOriginalUnitsPickerSelectedItemValid()
			&& IsNumberToConvertEntryTextValid())
		{
			ConvertUnits();
		}
	}

	[RelayCommand]
	void UnitTypePickerSelectedIndexChanged()
	{
		var selectedUnitOfMeasurement = (UnitOfMeasurement)UnitTypePickerSelectedIndex;
		PopulateUnitsPickerLists(selectedUnitOfMeasurement);

		SetTitleText(selectedUnitOfMeasurement);
	}

	void SetTitleText(UnitOfMeasurement unitOfMeasurement) => TitleText = $"Convert {unitOfMeasurement}";

	[RelayCommand]
	void ConvertButton()
	{
		if (!IsOriginalUnitsPickerSelectedItemValid()
			|| !IsConvertedUnitsPickerSelectedItemValid()
			|| !IsNumberToConvertEntryTextValid())
		{
			var errorStringBuilder = new StringBuilder();

			if (!IsNumberToConvertEntryTextValid())
				errorStringBuilder.AppendLine("Number to Convert Invalid");
			if (!IsOriginalUnitsPickerSelectedItemValid())
				errorStringBuilder.AppendLine("Original Unit Not Selected");
			if (!IsConvertedUnitsPickerSelectedItemValid())
				errorStringBuilder.AppendLine("Converted Unit Not Selected");

			errorStringBuilder.Remove(errorStringBuilder.Length - 1, 1);

			OnConversionError(errorStringBuilder.ToString());
		}
		else
		{
			ConvertUnits();
		}
	}

	void ConvertUnits()
	{
		try
		{
			var numberToConvert = double.Parse(NumberToConvertEntryText, CultureInfo.CurrentCulture);

			var firstItemSelectedType = _unitOfMeasurementDictionary[OriginalUnitsPickerSelectedItem];
			var secondItemSelectedType = _unitOfMeasurementDictionary[ConvertedUnitsPickerSelectedItem];

			var inputAsBaseUnits = firstItemSelectedType.ConvertToBaseUnits(numberToConvert);

			var inputAsConvertedUnits = secondItemSelectedType.ConvertFromBaseUnits(inputAsBaseUnits);

			ConvertedNumberLabelText = $"{NumberToConvertEntryText} {OriginalUnitsPickerSelectedItem} = {inputAsConvertedUnits:N3} {ConvertedUnitsPickerSelectedItem}";
		}
		catch
		{
			ConvertedNumberLabelText = string.Empty;
		}
	}

	bool IsNumberToConvertEntryTextValid() => double.TryParse(NumberToConvertEntryText, out _);
	bool IsOriginalUnitsPickerSelectedItemValid() => !string.IsNullOrWhiteSpace(OriginalUnitsPickerSelectedItem);
	bool IsConvertedUnitsPickerSelectedItemValid() => !string.IsNullOrWhiteSpace(ConvertedUnitsPickerSelectedItem);

	void OnConversionError(string message) => _conversionErrorEventManager.HandleEvent(this, message, nameof(ConversionError));

	partial void OnNumberToConvertEntryTextChanged(string value)
	{
		if (!string.IsNullOrWhiteSpace(OriginalUnitsPickerSelectedItem)
			&& !string.IsNullOrWhiteSpace(ConvertedUnitsPickerSelectedItem)
			&& !string.IsNullOrWhiteSpace(NumberToConvertEntryText))
		{
			ConvertUnits();
		}
	}
}