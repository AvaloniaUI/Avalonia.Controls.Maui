using System.Diagnostics;
using System.Text.Json;

namespace Weather;

public class RestService : IWeatherService
{
    HttpClient _client;

    public RestService()
    {
        _client = new HttpClient();
    }

    public async Task<WeatherData?> GetWeatherData(string query)
    {
        WeatherData? weatherData = null;
        try
        {
            var response = await _client.GetAsync(query);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                weatherData = JsonSerializer.Deserialize(content, WeatherJsonContext.Default.WeatherData);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("\t\tERROR {0}", ex.Message);
        }

        return weatherData;
    }
}
