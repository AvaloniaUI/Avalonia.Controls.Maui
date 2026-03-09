namespace MauiConverter;

class Meters : UnitOfMeasurementModel, ISingleton<Meters>
{
	readonly static Lazy<Meters> _instanceHolder = new(() => new Meters());

	Meters() : base(UnitOfMeasurement.Length)
	{
	}

	public static Meters Instance => _instanceHolder.Value;

	public override double ConvertFromBaseUnits(double valueInMeters) => valueInMeters;

	public override double ConvertToBaseUnits(double valueInMeters) => valueInMeters;
}