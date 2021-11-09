namespace BadNews.Repositories.Weather
{
    public abstract class OpenWeatherForecast
    {
        public WeatherInfo[] Weather { get; set; }
        public MainInfo Main { get; set; }

        public abstract class WeatherInfo
        {
            protected WeatherInfo(string icon)
            {
                Icon = icon;
            }

            public int Id { get; set; }
            public string Main { get; set; }
            public string Description { get; set; }
            private string Icon { get; set; }
            public string IconUrl => Icon != null
                ? $"http://openweathermap.org/img/wn/{Icon}@2x.png"
                : null;
        }

        public abstract class MainInfo
        {
            public double Temp { get; set; }
            public double FeelsLike { get; set; }
            public double TempMin { get; set; }
            public double TempMax { get; set; }
            public int Pressure { get; set; }
            public int Humidity { get; set; }
        }
    }
}
