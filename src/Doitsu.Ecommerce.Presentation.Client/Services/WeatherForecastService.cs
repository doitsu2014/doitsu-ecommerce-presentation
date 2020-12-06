using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Doitsu.Ecommerce.Presentation.Shared.Interfaces;

namespace Doitsu.Ecommerce.Presentation.Shared.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly HttpClient _client;

        public WeatherForecastService(HttpClient client)
        {
            _client = client;
        }
        
        public async Task<WeatherForecast[]> GetForecastAsync()
        {
            return await _client.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
        }
    }
}