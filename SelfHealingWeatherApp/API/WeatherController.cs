using Microsoft.AspNetCore.Mvc;
using SelfHealingWeatherApp.Application.Services;

namespace SelfHealingWeatherApp.API;

[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string city, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("City is required.");
        }

        try
        {
            var snapshot = await _weatherService.GetCurrentAsync(city, cancellationToken);
            return Ok(snapshot);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

