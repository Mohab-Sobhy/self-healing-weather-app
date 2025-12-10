namespace SelfHealingWeatherApp.Infrastructure.Providers;

public class WeatherApiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.weatherapi.com/v1/";
}


