using Microsoft.AspNetCore.Mvc;
using SelfHealingWeatherApp.Application.Services;

namespace SelfHealingWeatherApp.API;

[ApiController]
[Route("api/cache")]
public class CacheController : ControllerBase
{
    private readonly IWeatherCacheService _cacheService;

    public CacheController(IWeatherCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> Get(string city, CancellationToken cancellationToken)
    {
        var value = await _cacheService.GetCachedAsync(city, cancellationToken);
        if (value is null)
        {
            return NotFound();
        }
        return Content(value, "application/json");
    }

    [HttpPost("{city}")]
    public async Task<IActionResult> Set(string city, [FromBody] object payload, CancellationToken cancellationToken)
    {
        // Store raw JSON body as cache value with 1 hour TTL
        var json = payload.ToString() ?? "{}";
        await _cacheService.SetAsync(city, json, TimeSpan.FromHours(1), cancellationToken);
        return Ok();
    }
}


