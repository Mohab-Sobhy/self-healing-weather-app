using Microsoft.Extensions.Logging;
using SelfHealingWeatherApp.Domain.Entities;
using SelfHealingWeatherApp.Domain.Providers;

namespace SelfHealingWeatherApp.Application.Services;

/// <summary>
/// Tries registered providers in order until one succeeds.
/// </summary>
public class WeatherProviderStrategy : IWeatherProviderStrategy
{
    private readonly IReadOnlyList<IWeatherProvider> _providers;
    private readonly ILogger<WeatherProviderStrategy> _logger;

    public WeatherProviderStrategy(
        IEnumerable<IWeatherProvider> providers,
        ILogger<WeatherProviderStrategy> logger)
    {
        _providers = providers?.ToList() ?? throw new ArgumentNullException(nameof(providers));
        _logger = logger;
    }

    public async Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken)
    {
        if (_providers.Count == 0)
        {
            throw new InvalidOperationException("No weather providers are registered.");
        }

        List<Exception> failures = new();

        foreach (var provider in _providers)
        {
            try
            {
                _logger.LogInformation("Fetching weather for {City} via {Provider}", city, provider.GetType().Name);
                return await provider.GetCurrentAsync(city, cancellationToken);
            }
            catch (Exception ex)
            {
                failures.Add(ex);
                _logger.LogWarning(ex, "Provider {Provider} failed for city {City}", provider.GetType().Name, city);
            }
        }

        throw new AggregateException($"All weather providers failed for '{city}'.", failures);
    }
}


