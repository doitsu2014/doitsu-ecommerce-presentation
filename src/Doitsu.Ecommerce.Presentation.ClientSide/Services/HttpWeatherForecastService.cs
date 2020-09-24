using Doitsu.Ecommerce.Presentation.Shared;
using Doitsu.Ecommerce.Presentation.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Doitsu.Ecommerce.Presentation.ClientSide.Services
{
    public class HttpWeatherForecastService : IWeatherForecastService
    {
        private readonly HttpClient client;

        public HttpWeatherForecastService(HttpClient client)
        {
            this.client = client;
        }

        public async Task<WeatherForecast[]> GetForecastAsync()
        {
            return await client.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json");
        }

    }
}
