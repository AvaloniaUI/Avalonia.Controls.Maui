namespace MauiConverter;

class Inches : UnitOfMeasurementModel, ISingleton<Inches>
{
	readonly static Lazy<Inches> _inchesHolder = new(() => new Inches());

	Inches() : base(UnitOfMeasurement.Length)
	{
	}

	public static Inches Instance => _inchesHolder.Value;

	public override double ConvertFromBaseUnits(double valueInMeters) => UnitConverters.MetersToInternationalFeet(valueInMeters) * 12;

	public override double ConvertToBaseUnits(double valueInInches) => UnitConverters.InternationalFeetToMeters(valueInInches / 12);
}