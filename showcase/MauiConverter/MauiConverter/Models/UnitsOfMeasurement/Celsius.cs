namespace MauiConverter;

class Celsius : UnitOfMeasurementModel, ISingleton<Celsius>
{
	readonly static Lazy<Celsius> _instanceHolder = new(() => new Celsius());

	Celsius() : base(UnitOfMeasurement.Temperature)
	{
	}

	public static Celsius Instance => _instanceHolder.Value;

	public override double ConvertFromBaseUnits(double unitsInKelvin) => UnitConverters.KelvinToCelsius(unitsInKelvin);

	public override double ConvertToBaseUnits(double unitsInCelsius) => UnitConverters.CelsiusToKelvin(unitsInCelsius);
}