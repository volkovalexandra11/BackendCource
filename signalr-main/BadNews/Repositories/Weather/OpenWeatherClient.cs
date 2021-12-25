using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BadNews.Repositories.Weather
{
    public class OpenWeatherClient
    {
        private readonly string apiKey;

        public OpenWeatherClient(string apiKey)
        {
            this.apiKey = apiKey;
        }

        private readonly HttpClient httpClient = new HttpClient();

        public async Task<OpenWeatherForecast> GetWeatherFromApiAsync()
        {
            if (string.IsNullOrEmpty(apiKey))
                return null;
                
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"http://api.openweathermap.org/data/2.5/weather?q=Yekaterinburg,ru&units=metric&appid={apiKey}");
            var response = await httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var forecast = JsonConvert.DeserializeObject<OpenWeatherForecast>(content,
                new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                });
            return forecast;
        }
    }
}
