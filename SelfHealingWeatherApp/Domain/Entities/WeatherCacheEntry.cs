namespace SelfHealingWeatherApp.Domain.Entities;

public class WeatherCacheEntry
{
    public int Id { get; set; }
    public string CityNormalized { get; set; } = string.Empty;
    public string DataJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}


