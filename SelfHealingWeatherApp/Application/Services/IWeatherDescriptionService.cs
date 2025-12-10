using SelfHealingWeatherApp.Domain.Entities;

namespace SelfHealingWeatherApp.Application.Services;

public interface IWeatherDescriptionService
{
    Task<string> DescribeAsync(WeatherSnapshot snapshot, CancellationToken cancellationToken);
}


