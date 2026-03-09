namespace MauiConverter;

class Kilograms : UnitOfMeasurementModel, ISingleton<Kilograms>
{
	readonly static Lazy<Kilograms> _instanceHolder = new(() => new Kilograms());

	Kilograms() : base(UnitOfMeasurement.Mass)
	{
	}

	public static Kilograms Instance => _instanceHolder.Value;

	public override double ConvertFromBaseUnits(double numberInGrams) => numberInGrams / 1000;

	public override double ConvertToBaseUnits(double numberInKilograms) => numberInKilograms * 1000;
}