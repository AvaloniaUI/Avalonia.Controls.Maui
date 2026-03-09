namespace MauiConverter;

class Pounds : UnitOfMeasurementModel, ISingleton<Pounds>
{
	readonly static Lazy<Pounds> _instanceHolder = new(() => new Pounds());

	Pounds() : base(UnitOfMeasurement.Mass)
	{
	}

	public static Pounds Instance => _instanceHolder.Value;

	public override double ConvertFromBaseUnits(double unitsInGrams) => UnitConverters.KilogramsToPounds(unitsInGrams / 1000);

	public override double ConvertToBaseUnits(double unitsInPounds) => UnitConverters.PoundsToKilograms(unitsInPounds) * 1000;
}