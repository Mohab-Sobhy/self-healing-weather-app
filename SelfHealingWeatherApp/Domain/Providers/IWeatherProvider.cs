using SelfHealingWeatherApp.Domain.Entities;

namespace SelfHealingWeatherApp.Domain.Providers;

/// <summary>
/// Strategy interface for pulling weather data from an external provider.
/// </summary>
public interface IWeatherProvider
{
    Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken);
}

