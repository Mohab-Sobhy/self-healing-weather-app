
using Microsoft.EntityFrameworkCore;
using SelfHealingWeatherApp.Infrastructure.Persistence;
using SelfHealingWeatherApp.Domain.Repositories;
using SelfHealingWeatherApp.Infrastructure.Repositories;
using SelfHealingWeatherApp.Application.Services;

namespace SelfHealingWeatherApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Database and caching DI
        builder.Services.AddDbContext<WeatherAppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddScoped<IWeatherCacheRepository, WeatherCacheRepository>();
        builder.Services.AddScoped<IWeatherCacheService, WeatherCacheService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();


        app.MapControllers();

        // Ensure database is created (for demo). Use migrations in real deployments.
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WeatherAppDbContext>();
            db.Database.EnsureCreated();
        }

        app.Run();
    }
}
