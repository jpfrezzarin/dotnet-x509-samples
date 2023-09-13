using Core;
using Newtonsoft.Json;

const string baseUrl = "https://localhost:5064";

Console.WriteLine("Getting the weather forecast... ");

using var httpClient = new HttpClient();

var httpRequestMsg = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/WeatherForecast");

var httpResponseMsg = await httpClient.SendAsync(httpRequestMsg);

if (!httpResponseMsg.IsSuccessStatusCode)
    throw new Exception("The server didn't like our request :(");

var contentStream = await httpResponseMsg.Content.ReadAsStringAsync();
var response = JsonConvert.DeserializeObject<IEnumerable<WeatherForecast>>(contentStream);

foreach (var weatherForecast in response!)
{
    Console.WriteLine($"Day {weatherForecast.Date:MM/dd/yyyy}: Weather {weatherForecast.Summary}: Temperature {weatherForecast.TemperatureC}º C");
}