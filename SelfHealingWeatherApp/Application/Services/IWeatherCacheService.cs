namespace SelfHealingWeatherApp.Application.Services;

public interface IWeatherCacheService
{
    Task<string?> GetCachedAsync(string city, CancellationToken cancellationToken);
    Task<string> SetAsync(string city, string dataJson, TimeSpan ttl, CancellationToken cancellationToken);
}


