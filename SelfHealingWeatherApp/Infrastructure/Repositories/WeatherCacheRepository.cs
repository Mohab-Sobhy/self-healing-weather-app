using Microsoft.EntityFrameworkCore;
using SelfHealingWeatherApp.Domain.Entities;
using SelfHealingWeatherApp.Domain.Repositories;
using SelfHealingWeatherApp.Infrastructure.Persistence;

namespace SelfHealingWeatherApp.Infrastructure.Repositories;

public class WeatherCacheRepository : IWeatherCacheRepository
{
    private readonly WeatherAppDbContext _dbContext;

    public WeatherCacheRepository(WeatherAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WeatherCacheEntry?> GetValidByCityAsync(string cityNormalized, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.WeatherCacheEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CityNormalized == cityNormalized && x.ExpiresAtUtc > now, cancellationToken);
    }

    public async Task<WeatherCacheEntry> UpsertAsync(string cityNormalized, string dataJson, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var existing = await _dbContext.WeatherCacheEntries
            .FirstOrDefaultAsync(x => x.CityNormalized == cityNormalized, cancellationToken);

        if (existing is null)
        {
            existing = new WeatherCacheEntry
            {
                CityNormalized = cityNormalized,
                DataJson = dataJson,
                CreatedAtUtc = now,
                ExpiresAtUtc = now.Add(ttl)
            };
            _dbContext.WeatherCacheEntries.Add(existing);
        }
        else
        {
            existing.DataJson = dataJson;
            existing.CreatedAtUtc = now;
            existing.ExpiresAtUtc = now.Add(ttl);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<int> PurgeExpiredAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expired = await _dbContext.WeatherCacheEntries
            .Where(x => x.ExpiresAtUtc <= now)
            .ToListAsync(cancellationToken);
        if (expired.Count == 0)
        {
            return 0;
        }
        _dbContext.WeatherCacheEntries.RemoveRange(expired);
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}


