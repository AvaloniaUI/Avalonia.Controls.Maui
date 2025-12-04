namespace Weather;

public interface IWeatherService
{
    Task<WeatherData?> GetWeatherData(string query);
}
