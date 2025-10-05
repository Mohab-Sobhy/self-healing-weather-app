using System.Text;
using SelfHealingWeatherApp.Domain.Repositories;

namespace SelfHealingWeatherApp.Application.Services;

public class WeatherCacheService : IWeatherCacheService
{
    private readonly IWeatherCacheRepository _repository;

    public WeatherCacheService(IWeatherCacheRepository repository)
    {
        _repository = repository;
    }

    public async Task<string?> GetCachedAsync(string city, CancellationToken cancellationToken)
    {
        var key = NormalizeCity(city);
        var cached = await _repository.GetValidByCityAsync(key, cancellationToken);
        return cached?.DataJson;
    }

    public async Task<string> SetAsync(string city, string dataJson, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var key = NormalizeCity(city);
        await _repository.UpsertAsync(key, dataJson, ttl, cancellationToken);
        return dataJson;
    }

    private static string NormalizeCity(string city)
    {
        return city.Trim().ToLowerInvariant();
    }
}


