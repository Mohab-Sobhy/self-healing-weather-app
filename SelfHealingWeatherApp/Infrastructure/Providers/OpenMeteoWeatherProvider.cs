using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SelfHealingWeatherApp.Domain.Entities;
using SelfHealingWeatherApp.Domain.Providers;

namespace SelfHealingWeatherApp.Infrastructure.Providers;

/// <summary>
/// Open-Meteo provider – no API key required and suitable for demos.
/// </summary>
public class OpenMeteoWeatherProvider : IWeatherProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenMeteoWeatherProvider> _logger;

    public OpenMeteoWeatherProvider(HttpClient httpClient, ILogger<OpenMeteoWeatherProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken)
    {
        var geocodeUri =
            $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1";
        var geocode = await GetAsync<GeocodeResponse>(geocodeUri, cancellationToken);
        var location = geocode?.Results?.FirstOrDefault()
                       ?? throw new InvalidOperationException($"Could not resolve coordinates for '{city}'.");

        var forecastUri =
            $"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}&current_weather=true&hourly=relativehumidity_2m,wind_speed_10m&timezone=UTC";
        var forecast = await GetAsync<ForecastResponse>(forecastUri, cancellationToken);
        if (forecast?.CurrentWeather is null)
        {
            throw new InvalidOperationException("Provider did not return current weather.");
        }

        var retrievedAtUtc = WeatherTimeParser.ToUtc(forecast.CurrentWeather.Time);
        var description = WeatherCodeMapper.MapCode(forecast.CurrentWeather.WeatherCode);
        var humidity = forecast.Hourly?.RelativeHumidity2M?.FirstOrDefault();
        var windSpeed = forecast.CurrentWeather.Windspeed;

        return new WeatherSnapshot
        {
            City = city,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            TemperatureC = forecast.CurrentWeather.Temperature,
            HumidityPercent = humidity,
            WindSpeedKph = windSpeed,
            Description = description,
            Provider = "Open-Meteo",
            RetrievedAtUtc = retrievedAtUtc
        };
    }

    private async Task<T?> GetAsync<T>(string uri, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
    }

    private sealed record GeocodeResponse
    {
        [JsonPropertyName("results")] public List<GeocodeResult>? Results { get; init; }
    }

    private sealed record GeocodeResult
    {
        [JsonPropertyName("latitude")] public double Latitude { get; init; }

        [JsonPropertyName("longitude")] public double Longitude { get; init; }
    }

    private sealed record ForecastResponse
    {
        [JsonPropertyName("current_weather")] public CurrentWeatherPayload? CurrentWeather { get; init; }

        [JsonPropertyName("hourly")] public HourlyPayload? Hourly { get; init; }
    }

    private sealed record CurrentWeatherPayload
    {
        [JsonPropertyName("temperature")] public double Temperature { get; init; }

        [JsonPropertyName("windspeed")] public double Windspeed { get; init; }

        [JsonPropertyName("weathercode")] public int WeatherCode { get; init; }

        [JsonPropertyName("time")] public string Time { get; init; } = string.Empty;
    }

    private sealed record HourlyPayload
    {
        [JsonPropertyName("relativehumidity_2m")] public List<double>? RelativeHumidity2M { get; init; }

        [JsonPropertyName("wind_speed_10m")] public List<double>? WindSpeed10M { get; init; }
    }
}

internal static class WeatherCodeMapper
{
    private static readonly Dictionary<int, string> Codes = new()
    {
        { 0, "Clear sky" },
        { 1, "Mainly clear" },
        { 2, "Partly cloudy" },
        { 3, "Overcast" },
        { 45, "Fog" },
        { 48, "Depositing rime fog" },
        { 51, "Light drizzle" },
        { 53, "Moderate drizzle" },
        { 55, "Dense drizzle" },
        { 61, "Slight rain" },
        { 63, "Moderate rain" },
        { 65, "Heavy rain" },
        { 71, "Slight snow fall" },
        { 73, "Moderate snow fall" },
        { 75, "Heavy snow fall" },
        { 95, "Thunderstorm" },
        { 96, "Thunderstorm with slight hail" },
        { 99, "Thunderstorm with heavy hail" }
    };

    public static string MapCode(int code)
    {
        return Codes.TryGetValue(code, out var label) ? label : $"Weather code {code}";
    }
}

internal static class WeatherTimeParser
{
    public static DateTime ToUtc(string providerTime)
    {
        if (DateTime.TryParse(providerTime, out var parsed))
        {
            return parsed.Kind == DateTimeKind.Utc ? parsed : parsed.ToUniversalTime();
        }
        return DateTime.UtcNow;
    }
}

