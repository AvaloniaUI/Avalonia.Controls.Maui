namespace MauiConverter;

abstract class UnitOfMeasurementModel
{
	protected UnitOfMeasurementModel(UnitOfMeasurement measurementType) =>
		MeasurementType = measurementType;

	public UnitOfMeasurement MeasurementType { get; }

	public abstract double ConvertToBaseUnits(double unit);

	public abstract double ConvertFromBaseUnits(double baseUnit);
}


enum UnitOfMeasurement { Length, Mass, Temperature };