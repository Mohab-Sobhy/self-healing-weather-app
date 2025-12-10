
using Microsoft.EntityFrameworkCore;
using SelfHealingWeatherApp.Infrastructure.Persistence;
using SelfHealingWeatherApp.Domain.Repositories;
using SelfHealingWeatherApp.Infrastructure.Repositories;
using SelfHealingWeatherApp.Application.Services;
using SelfHealingWeatherApp.Infrastructure.Providers;
using SelfHealingWeatherApp.Domain.Providers;
using Microsoft.Extensions.Options;

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
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<WeatherAppDbContext>();

        // Database and caching DI
        builder.Services.AddDbContext<WeatherAppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddScoped<IWeatherCacheRepository, WeatherCacheRepository>();
        builder.Services.AddScoped<IWeatherCacheService, WeatherCacheService>();
        builder.Services.AddScoped<IWeatherDescriptionService, TemplateWeatherDescriptionService>();
        builder.Services.AddScoped<IWeatherProviderStrategy, WeatherProviderStrategy>();
        builder.Services.AddScoped<IWeatherService, WeatherService>();

        // Weather providers + strategy
        builder.Services.Configure<WeatherApiOptions>(builder.Configuration.GetSection("WeatherApi"));

        builder.Services.AddHttpClient<OpenMeteoWeatherProvider>();
        builder.Services.AddScoped<IWeatherProvider>(sp => sp.GetRequiredService<OpenMeteoWeatherProvider>());

        var weatherApiKey = builder.Configuration["WeatherApi:ApiKey"];
        if (!string.IsNullOrWhiteSpace(weatherApiKey))
        {
            builder.Services.AddHttpClient<WeatherApiComProvider>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<WeatherApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });
            builder.Services.AddScoped<IWeatherProvider>(sp => sp.GetRequiredService<WeatherApiComProvider>());
        }

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        // Ensure database is created (for demo). Use migrations in real deployments.
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WeatherAppDbContext>();
            db.Database.EnsureCreated();
        }

        app.Run();
    }
}
