namespace MauiConverter;

class Kilometers : UnitOfMeasurementModel, ISingleton<Kilometers>
{
	readonly static Lazy<Kilometers> _instanceHolder = new(() => new Kilometers());

	Kilometers() : base(UnitOfMeasurement.Length)
	{
	}

	public static Kilometers Instance => _instanceHolder.Value;

	public override double ConvertFromBaseUnits(double valueInMeters) => valueInMeters / 1000;

	public override double ConvertToBaseUnits(double valueInKilometers) => valueInKilometers * 1000;
}