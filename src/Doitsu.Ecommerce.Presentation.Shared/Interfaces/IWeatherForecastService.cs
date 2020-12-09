using System.Threading.Tasks;

namespace Doitsu.Ecommerce.Presentation.Shared.Interfaces
{
    public interface IWeatherForecastService
    {
        Task<WeatherForecast[]> GetForecastAsync();
    }
}