namespace SelfHealingWeatherApp.Domain.Entities;

/// <summary>
/// Unified weather payload exposed by the API regardless of upstream provider.
/// </summary>
public class WeatherSnapshot
{
    public string City { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double TemperatureC { get; set; }
    public double? HumidityPercent { get; set; }
    public double? WindSpeedKph { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? HumanizedSummary { get; set; }
    public string Provider { get; set; } = string.Empty;
    public DateTime RetrievedAtUtc { get; set; }
}

