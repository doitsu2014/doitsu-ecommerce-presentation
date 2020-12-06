using System;
using System.Linq;
using System.Threading.Tasks;
using Doitsu.Ecommerce.Presentation.Shared;
using Doitsu.Ecommerce.Presentation.Shared.Interfaces;

namespace Doitsu.Ecommerce.Presentation.Server.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public Task<WeatherForecast[]> GetForecastAsync()
        {
            var random = new Random();
            return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = random.Next(-20, 55),
                    Summary = Summaries[random.Next(Summaries.Length)]
                })
                .ToArray());
        }
    }
}