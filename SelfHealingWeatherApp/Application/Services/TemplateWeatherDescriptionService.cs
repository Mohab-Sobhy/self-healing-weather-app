using System.Globalization;
using System.Text;
using SelfHealingWeatherApp.Domain.Entities;

namespace SelfHealingWeatherApp.Application.Services;

/// <summary>
/// Simple, deterministic description generator; can be swapped for an LLM later.
/// </summary>
public class TemplateWeatherDescriptionService : IWeatherDescriptionService
{
    public Task<string> DescribeAsync(WeatherSnapshot snapshot, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        var temp = snapshot.TemperatureC.ToString("0.#", CultureInfo.InvariantCulture);
        var humidity = snapshot.HumidityPercent.HasValue
            ? $"{snapshot.HumidityPercent.Value:0.#}% humidity"
            : "humidity data unavailable";
        var wind = snapshot.WindSpeedKph.HasValue
            ? $"{snapshot.WindSpeedKph.Value:0.#} km/h wind"
            : "wind data unavailable";

        sb.Append($"In {snapshot.City}, it feels {snapshot.Description.ToLowerInvariant()} at {temp}°C");
        sb.Append($", with {humidity} and {wind}.");

        return Task.FromResult(sb.ToString());
    }
}


