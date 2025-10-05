using SelfHealingWeatherApp.Domain.Entities;

namespace SelfHealingWeatherApp.Domain.Repositories;

public interface IWeatherCacheRepository
{
    Task<WeatherCacheEntry?> GetValidByCityAsync(string cityNormalized, CancellationToken cancellationToken);
    Task<WeatherCacheEntry> UpsertAsync(string cityNormalized, string dataJson, TimeSpan ttl, CancellationToken cancellationToken);
    Task<int> PurgeExpiredAsync(CancellationToken cancellationToken);
}


