using System.Linq;

namespace BadNews.Repositories.Weather
{
    public class WeatherForecast
    {
        private const string DefaultWeatherImageUrl = "/images/cloudy.png";

        public int TemperatureInCelsius { get; init; }
        public string IconUrl { get; init; }

        public static WeatherForecast CreateFrom(OpenWeatherForecast forecast)
        {
            return new WeatherForecast
            {
                TemperatureInCelsius = (int)forecast.Main.Temp,
                IconUrl = forecast.Weather.FirstOrDefault()?.IconUrl ?? DefaultWeatherImageUrl
            };
        }
    }
}
