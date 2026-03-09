namespace MauiConverter;

class Miles : UnitOfMeasurementModel, ISingleton<Miles>
{
	readonly static Lazy<Miles> _instanceHolder = new(() => new Miles());

	Miles() : base(UnitOfMeasurement.Length)
	{
	}

	public static Miles Instance => _instanceHolder.Value;

	public override double ConvertFromBaseUnits(double valueInMeters) => UnitConverters.KilometersToMiles(valueInMeters / 1000);

	public override double ConvertToBaseUnits(double valueInMilies) => UnitConverters.MilesToKilometers(valueInMilies) * 1000;
}