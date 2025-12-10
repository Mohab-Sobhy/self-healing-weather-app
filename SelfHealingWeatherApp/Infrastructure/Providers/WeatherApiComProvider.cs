using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SelfHealingWeatherApp.Domain.Entities;
using SelfHealingWeatherApp.Domain.Providers;

namespace SelfHealingWeatherApp.Infrastructure.Providers;

/// <summary>
/// WeatherAPI.com provider (requires API key). Acts as a secondary provider.
/// </summary>
public class WeatherApiComProvider : IWeatherProvider
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiOptions _options;
    private readonly ILogger<WeatherApiComProvider> _logger;

    public WeatherApiComProvider(
        HttpClient httpClient,
        IOptions<WeatherApiOptions> options,
        ILogger<WeatherApiComProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("WeatherAPI.com API key is not configured.");
        }

        var requestUri =
            $"current.json?key={_options.ApiKey}&q={Uri.EscapeDataString(city)}&aqi=no";

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload =
            await response.Content.ReadFromJsonAsync<WeatherApiResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Provider returned no payload.");

        var current = payload.Current
                      ?? throw new InvalidOperationException("Provider did not return current weather.");

        return new WeatherSnapshot
        {
            City = payload.Location?.Name ?? city,
            Latitude = payload.Location?.Lat ?? 0,
            Longitude = payload.Location?.Lon ?? 0,
            TemperatureC = current.TempC,
            HumidityPercent = current.Humidity,
            WindSpeedKph = current.WindKph,
            Description = current.Condition?.Text ?? "WeatherAPI.com",
            Provider = "WeatherAPI.com",
            RetrievedAtUtc = current.RetrievedAtUtc,
        };
    }

    private sealed record WeatherApiResponse
    {
        [JsonPropertyName("location")] public LocationPayload? Location { get; init; }
        [JsonPropertyName("current")] public CurrentPayload? Current { get; init; }
    }

    private sealed record LocationPayload
    {
        [JsonPropertyName("name")] public string? Name { get; init; }
        [JsonPropertyName("lat")] public double Lat { get; init; }
        [JsonPropertyName("lon")] public double Lon { get; init; }
    }

    private sealed record CurrentPayload
    {
        [JsonPropertyName("temp_c")] public double TempC { get; init; }
        [JsonPropertyName("humidity")] public double Humidity { get; init; }
        [JsonPropertyName("wind_kph")] public double WindKph { get; init; }
        [JsonPropertyName("last_updated_epoch")] public long LastUpdatedEpoch { get; init; }
        [JsonPropertyName("condition")] public ConditionPayload? Condition { get; init; }

        public DateTime RetrievedAtLocal => DateTimeOffset.FromUnixTimeSeconds(LastUpdatedEpoch).DateTime;
        public DateTime RetrievedAtUtc => DateTimeOffset.FromUnixTimeSeconds(LastUpdatedEpoch).UtcDateTime;
    }

    private sealed record ConditionPayload
    {
        [JsonPropertyName("text")] public string? Text { get; init; }
    }
}


