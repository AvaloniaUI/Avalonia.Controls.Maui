namespace MauiConverter;

class Feet : UnitOfMeasurementModel, ISingleton<Feet>
{
	readonly static Lazy<Feet> _instanceHolder = new(() => new Feet());

	Feet() : base(UnitOfMeasurement.Length)
	{
	}

	public static Feet Instance => _instanceHolder.Value;

	public override double ConvertFromBaseUnits(double valueInMeters) => UnitConverters.MetersToInternationalFeet(valueInMeters);

	public override double ConvertToBaseUnits(double valueInFeet) => UnitConverters.InternationalFeetToMeters(valueInFeet);
}