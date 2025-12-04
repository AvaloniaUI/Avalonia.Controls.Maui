namespace Weather;

public class FakeWeatherService : IWeatherService
{
    private readonly Dictionary<string, WeatherData> _fakeData;

    public FakeWeatherService()
    {
        // Initialize fake weather data for various cities
        _fakeData = new Dictionary<string, WeatherData>(StringComparer.OrdinalIgnoreCase)
        {
            ["seattle"] = CreateWeatherData("Seattle", 52.5, 5.7, 76, "Clouds", "Partly cloudy", 47.60, -122.33),
            ["new york"] = CreateWeatherData("New York", 68.2, 8.3, 65, "Clear", "Clear sky", 40.71, -74.01),
            ["london"] = CreateWeatherData("London", 59.4, 12.4, 82, "Rain", "Light rain", 51.51, -0.13),
            ["tokyo"] = CreateWeatherData("Tokyo", 73.8, 6.2, 58, "Clear", "Clear sky", 35.68, 139.69),
            ["paris"] = CreateWeatherData("Paris", 64.3, 9.8, 71, "Clouds", "Few clouds", 48.86, 2.35),
            ["sydney"] = CreateWeatherData("Sydney", 78.1, 11.2, 62, "Clear", "Sunny", -33.87, 151.21),
            ["toronto"] = CreateWeatherData("Toronto", 55.7, 7.1, 68, "Clouds", "Overcast", 43.65, -79.38),
            ["berlin"] = CreateWeatherData("Berlin", 61.2, 10.5, 74, "Rain", "Light rain", 52.52, 13.40),
            ["madrid"] = CreateWeatherData("Madrid", 82.4, 5.3, 42, "Clear", "Clear sky", 40.42, -3.70),
            ["singapore"] = CreateWeatherData("Singapore", 88.7, 4.8, 85, "Rain", "Tropical rain", 1.35, 103.82)
        };
    }

    public Task<WeatherData?> GetWeatherData(string query)
    {
        // Simulate network delay
        return Task.Run(async () =>
        {
            await Task.Delay(500); // Simulate API call delay

            // Extract city name from query (simple parsing)
            string cityName = ExtractCityFromQuery(query);

            // Return fake data if available, otherwise return default data
            if (_fakeData.TryGetValue(cityName, out var weatherData))
            {
                return weatherData;
            }

            // Return default fake data for unknown cities
            return CreateWeatherData(cityName, 72.0, 8.5, 70, "Clear", "Clear sky", 0.0, 0.0);
        });
    }

    private string ExtractCityFromQuery(string query)
    {
        // Simple extraction: look for q= parameter
        if (query.Contains("q="))
        {
            var startIndex = query.IndexOf("q=") + 2;
            var endIndex = query.IndexOf("&", startIndex);
            if (endIndex == -1)
                endIndex = query.Length;

            return query.Substring(startIndex, endIndex - startIndex);
        }

        return "Unknown";
    }

    private WeatherData CreateWeatherData(
        string cityName,
        double temperature,
        double windSpeed,
        long humidity,
        string weatherMain,
        string description,
        double lat,
        double lon)
    {
        var now = DateTimeOffset.UtcNow;
        var sunrise = new DateTimeOffset(now.Date.AddHours(6), TimeSpan.Zero).ToUnixTimeSeconds();
        var sunset = new DateTimeOffset(now.Date.AddHours(20), TimeSpan.Zero).ToUnixTimeSeconds();

        return new WeatherData
        {
            Title = cityName,
            Coord = new Coord
            {
                Lat = lat,
                Lon = lon
            },
            Weather = new[]
            {
                new Weather
                {
                    Id = 800,
                    Visibility = weatherMain,
                    Description = description,
                    Icon = "01d"
                }
            },
            Base = "stations",
            Main = new Main
            {
                Temperature = temperature,
                Pressure = 1013,
                Humidity = humidity,
                TempMin = temperature - 5,
                TempMax = temperature + 5
            },
            Visibility = 10000,
            Wind = new Wind
            {
                Speed = windSpeed,
                Deg = 180
            },
            Clouds = new Clouds
            {
                All = 20
            },
            Dt = now.ToUnixTimeSeconds(),
            Sys = new Sys
            {
                Type = 1,
                Id = 5091,
                Message = 0.0,
                Country = "US",
                Sunrise = sunrise,
                Sunset = sunset
            },
            Id = 5809844,
            Cod = 200
        };
    }
}
