namespace MauiConverter;

class Ounces : UnitOfMeasurementModel, ISingleton<Ounces>
{
	readonly static Lazy<Ounces> _instanceHolder = new(() => new Ounces());

	Ounces() : base(UnitOfMeasurement.Mass)
	{
	}

	public static Ounces Instance => _instanceHolder.Value;

	public override double ConvertFromBaseUnits(double numberInGrams) => UnitConverters.KilogramsToPounds(numberInGrams / 1000) * 16;

	public override double ConvertToBaseUnits(double numberInOunces) => UnitConverters.PoundsToKilograms(numberInOunces / 16) * 1000;
}