using System.Threading.Tasks;

namespace BadNews.Repositories.Weather
{
    public interface IWeatherForecastRepository
    {
        Task<WeatherForecast> GetWeatherForecastAsync();
    }
}
