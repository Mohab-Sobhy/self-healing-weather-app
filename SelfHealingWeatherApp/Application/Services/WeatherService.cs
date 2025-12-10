using System.Text.Json;
using Microsoft.Extensions.Logging;
using SelfHealingWeatherApp.Domain.Entities;
using SelfHealingWeatherApp.Domain.Providers;

namespace SelfHealingWeatherApp.Application.Services;

public class WeatherService : IWeatherService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private readonly IWeatherProviderStrategy _providerStrategy;
    private readonly IWeatherDescriptionService _descriptionService;
    private readonly IWeatherCacheService _cacheService;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IWeatherProviderStrategy providerStrategy,
        IWeatherDescriptionService descriptionService,
        IWeatherCacheService cacheService,
        ILogger<WeatherService> logger)
    {
        _providerStrategy = providerStrategy;
        _descriptionService = descriptionService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken)
    {
        var cachedJson = await _cacheService.GetCachedAsync(city, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedJson))
        {
            try
            {
                var cached = JsonSerializer.Deserialize<WeatherSnapshot>(cachedJson);
                if (cached is not null)
                {
                    cached.HumanizedSummary ??=
                        await _descriptionService.DescribeAsync(cached, cancellationToken);
                    return cached;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize cached weather for {City}", city);
            }
        }

        var snapshot = await _providerStrategy.GetCurrentAsync(city, cancellationToken);
        snapshot.HumanizedSummary ??= await _descriptionService.DescribeAsync(snapshot, cancellationToken);

        var serialized = JsonSerializer.Serialize(snapshot);
        await _cacheService.SetAsync(city, serialized, CacheTtl, cancellationToken);
        return snapshot;
    }
}

