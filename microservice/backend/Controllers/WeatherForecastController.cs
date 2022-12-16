using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly string connectionString;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
    {
        _logger = logger;
        connectionString = configuration.GetConnectionString("redis");
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        List<WeatherForecast> result = new();

        using (var redisConnection = ConnectionMultiplexer.Connect(connectionString))
        {
            var redis = redisConnection.GetDatabase();

            List<RedisValue>? list = redis.ListRange("weather", 0, 9).ToList();

            result.AddRange(list.Select(jsonString => JsonSerializer.Deserialize<WeatherForecast>(jsonString)!));
        }

        return result;
    }

    [HttpPost(Name = "PostWeatherForecast")]
    public async Task<WeatherForecast> Post()
    {
        using (var redisConnection = ConnectionMultiplexer.Connect(connectionString))
        {
            var redis = redisConnection.GetDatabase();

            var weatherForecast = new WeatherForecast
            {
                Date = DateTime.Now.AddDays(1),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };

            var data = JsonSerializer.Serialize(weatherForecast);

            _logger.LogInformation($"pushing forecast {data}");

            await redis.ListRightPushAsync("weather", data);

            await redis.ListTrimAsync("weather", 0, 9);

            return weatherForecast;
        }
    }
}