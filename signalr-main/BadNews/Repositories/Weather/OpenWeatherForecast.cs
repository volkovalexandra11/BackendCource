namespace BadNews.Repositories.Weather
{
    public class OpenWeatherForecast
    {
        public WeatherInfo[] Weather { get; set; }
        public MainInfo Main { get; set; }

        public class WeatherInfo
        {
            public int Id { get; set; }
            public string Main { get; set; }
            public string Description { get; set; }
            public string Icon { get; set; }
            public string IconUrl => Icon != null
                ? $"http://openweathermap.org/img/wn/{Icon}@2x.png"
                : null;
        }

        public class MainInfo
        {
            public int Temp { get; set; }
            public decimal FeelsLike { get; set; }
            public int TempMin { get; set; }
            public int TempMax { get; set; }
            public int Pressure { get; set; }
            public int Humidity { get; set; }
        }
    }
}
