using SelfHealingWeatherApp.Domain.Entities;

namespace SelfHealingWeatherApp.Application.Services;

public interface IWeatherService
{
    Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken);
}

