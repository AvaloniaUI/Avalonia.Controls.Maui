namespace Weather;

public partial class MainPage : ContentPage
{
    private IWeatherService _weatherService;
    private readonly FakeWeatherService _fakeService;
    private readonly RestService _realService;

    // Set to true to use fake data, false to use real API
    private readonly bool _useFakeData = true;

    public MainPage()
    {
        InitializeComponent();
        _fakeService = new FakeWeatherService();
        _realService = new RestService();

        // Select service based on bool
        _weatherService = _useFakeData ? _fakeService : _realService;
    }

    async void OnGetWeatherButtonClicked(object sender, EventArgs e)
    {
        // If using real service, check for API key
        if (!_useFakeData)
        {
            if (string.IsNullOrWhiteSpace(Constants.OpenWeatherMapAPIKey) || Constants.OpenWeatherMapAPIKey == "YOUR_API_KEY")
            {
                await DisplayAlertAsync("API Key Missing", "Please set your OpenWeatherMap API key in Constants.OpenWeatherMapAPIKey.", "OK");
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(_cityEntry.Text))
        {
            string requestUri = GenerateRequestUri(Constants.OpenWeatherMapEndpoint);
            WeatherData? weatherData = await _weatherService.GetWeatherData(requestUri);
            BindingContext = weatherData;
        }
    }

    string GenerateRequestUri(string endpoint)
    {
        string requestUri = endpoint;
        requestUri += $"?q={_cityEntry.Text}";
        requestUri += "&units=imperial"; // or units=metric
        requestUri += $"&appid={Constants.OpenWeatherMapAPIKey}"; // changed to lowercase 'appid'
        return requestUri;
    }
}
