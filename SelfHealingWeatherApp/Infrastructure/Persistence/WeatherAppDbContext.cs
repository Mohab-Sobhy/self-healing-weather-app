using Microsoft.EntityFrameworkCore;
using SelfHealingWeatherApp.Domain.Entities;

namespace SelfHealingWeatherApp.Infrastructure.Persistence;

public class WeatherAppDbContext : DbContext
{
    public WeatherAppDbContext(DbContextOptions<WeatherAppDbContext> options) : base(options)
    {
    }

    public DbSet<WeatherCacheEntry> WeatherCacheEntries => Set<WeatherCacheEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherCacheEntry>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CityNormalized).IsRequired();
            entity.Property(x => x.DataJson).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.ExpiresAtUtc).IsRequired();
            entity.HasIndex(x => x.CityNormalized).IsUnique();
        });
    }
}


