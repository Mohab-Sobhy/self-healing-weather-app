using SelfHealingWeatherApp.Domain.Entities;

namespace SelfHealingWeatherApp.Domain.Providers;

/// <summary>
/// Strategy that orchestrates multiple weather providers.
/// </summary>
public interface IWeatherProviderStrategy
{
    Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken);
}


