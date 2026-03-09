namespace MauiConverter;

class Fahrenheit : UnitOfMeasurementModel, ISingleton<Fahrenheit>
{
	readonly static Lazy<Fahrenheit> _instanceHolder = new(() => new Fahrenheit());

	Fahrenheit() : base(UnitOfMeasurement.Temperature)
	{
	}

	public static Fahrenheit Instance => _instanceHolder.Value;

	public override double ConvertFromBaseUnits(double unitsInKelvin) => UnitConverters.CelsiusToFahrenheit(UnitConverters.KelvinToCelsius(unitsInKelvin));

	public override double ConvertToBaseUnits(double unitsInFahrenheit) => UnitConverters.CelsiusToKelvin(UnitConverters.FahrenheitToCelsius(unitsInFahrenheit));
}