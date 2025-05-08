using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Weather.API.Models.WeatherResponse;
using Weather.API.Models;
using Weather.API.Tests.TestBase;
using Weather.API.Tests.TestHelpers;

namespace Weather.API.Tests
{
    [TestFixture]
    public class WeatherTests : ApiTestBase
    {
        [TestCase("metric", TemperatureRanges.MetricMin, TemperatureRanges.MetricMax)]
        [TestCase("imperial", TemperatureRanges.ImperialMin, TemperatureRanges.ImperialMax)]
        [TestCase("standard", TemperatureRanges.StandardMin, TemperatureRanges.StandardMax)]
        public async Task GET_WeatherByCityId_ValidValueByUnits_ReturnsValidData(string units, int minTemp, int maxTemp)
        {
            // Act
            HttpResponseMessage response = await BasicClient.GetAsync($"weather?id={RandomCity.Id}&units={units}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

            var weatherResponse = await response.Content.ReadFromJsonAsync<WeatherResponse>();

            Assert.That(weatherResponse.Name, Is.EqualTo(RandomCity.Name));
            AssertValidResponseData(weatherResponse, minTemp, maxTemp);
        }

        [Test]
        public async Task GET_WeatherByCityLatLon_ValidValue_ReturnsValidDataInStandardUnits()
        {
            // Act
            HttpResponseMessage response = await BasicClient.GetAsync($"weather?lat={RandomCity.Coord.Lat}&lon={RandomCity.Coord.Lon}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var weatherResponse = await response.Content.ReadFromJsonAsync<WeatherResponse>();

            Assert.That(weatherResponse.Name, Is.EqualTo(RandomCity.Name));
            AssertValidResponseData(weatherResponse, TemperatureRanges.StandardMin, TemperatureRanges.StandardMax);
        }


        [TestCase("xml", "application/xml")]
        [TestCase("html", "text/html")]
        public async Task GET_WeatherById_ValidValueByMode_ReturnsValidData(string mode, string modeValue)
        {
            // Act
            HttpResponseMessage response = await BasicClient.GetAsync($"weather?id={RandomCity.Id}&mode={mode}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo(modeValue));
        }

        [Test]
        public async Task GET_WeatherByCityLatLon_ValidValueByUnitsAndLanguage_ReturnsTranslatedDescription()
        {
            // Arrange
            HttpResponseMessage responseEn = await BasicClient.GetAsync($"weather?lat={RandomCity.Coord.Lat}&lon={RandomCity.Coord.Lon}&lang=en");

            // Act
            HttpResponseMessage responseLt = await BasicClient.GetAsync($"weather?lat={RandomCity.Coord.Lat}&lon={RandomCity.Coord.Lon}&units=metric&lang=lt");

            // Assert
            Assert.That(responseEn.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseLt.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var weatherResponseEn = await responseEn.Content.ReadFromJsonAsync<WeatherResponse>();
            var weatherResponseLt = await responseLt.Content.ReadFromJsonAsync<WeatherResponse>();

            Assert.That(weatherResponseLt.Weather[0].Description, Is.Not.EqualTo(weatherResponseEn.Weather[0].Description));
            AssertValidResponseData(weatherResponseLt, TemperatureRanges.MetricMin, TemperatureRanges.MetricMax);
        }

        [Test]
        public async Task GET_WeatherByCityLatLon_MissingLatLonParams_ReturnsBadRequest()
        {
            // Act
            HttpResponseMessage response = await BasicClient.GetAsync($"weather?");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var weatherResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.That(weatherResponse.Cod, Is.EqualTo("400"));
            Assert.That(weatherResponse.Message, Is.EqualTo("Nothing to geocode"));
        }

        [Test]
        public async Task GET_WeatherById_NotExistingCityId_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await BasicClient.GetAsync($"weather?id=1");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var weatherResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.That(weatherResponse.Cod, Is.EqualTo("404"));
            Assert.That(weatherResponse.Message, Is.EqualTo("city not found"));
        }

        [Test]
        public async Task GET_WeatherById_NotExistingPath_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await BasicClient.GetAsync($"weather123?");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var weatherResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.That(weatherResponse.Cod, Is.EqualTo("404"));
            Assert.That(weatherResponse.Message, Is.EqualTo("Internal error"));
        }

        [Test]
        public async Task GET_WeatherById_InvalidApiKey_ReturnsUnauthorized()
        {
            // Arrange
            HttpClient BasicClientWithIncorrectAppKey = CreateBasicClient("someValue", TestContext.Parameters["BaseUrl"]);

            // Act
            HttpResponseMessage response = await BasicClientWithIncorrectAppKey.GetAsync($"weather?id={RandomCity.Id}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

            var weatherResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.That(weatherResponse.Cod, Is.EqualTo("401"));
            Assert.That(weatherResponse.Message, Is.EqualTo("Invalid API key. Please see https://openweathermap.org/faq#error401 for more info."));
        }

        [Test]
        public async Task GET_WeatherById_UsingCallback_ReturnsCorrectJsonpResponse()
        {
            // Arrange
            var expectedCallback = "someTestFunction";

            // Act
            HttpResponseMessage response = await BasicClient.GetAsync($"weather?id={RandomCity.Id}&callback={expectedCallback}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("text/plain"));

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.That(responseBody.StartsWith($"{expectedCallback}("), "Response body does not start with the expected callback function");
            Assert.That(responseBody.EndsWith(")"), "Response body does not end with a closing parenthesis");

            var startIndex = expectedCallback.Length + 1; // Skip the callback name and opening parenthesis
            var endIndex = responseBody.Length - 1; // Remove the closing parenthesis
            var jsonString = responseBody.Substring(startIndex, endIndex - startIndex);
            var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(jsonString, caseInsensitiveOptions);

            AssertValidResponseData(weatherResponse, TemperatureRanges.StandardMin, TemperatureRanges.StandardMax);
        }

        private void AssertValidResponseData(WeatherResponse weatherResponse, int minTemp, int maxTemp)
        {
            Assert.That(weatherResponse.Main.Temp, Is.InRange(minTemp, maxTemp), "Temperature is outside valid range.");
            Assert.That(weatherResponse.Main.Feels_Like, Is.InRange(minTemp, maxTemp), "Temperature is outside valid range.");
            Assert.That(weatherResponse.Main.Temp_Min, Is.InRange(minTemp, maxTemp), "Temperature is outside valid range.");
            Assert.That(weatherResponse.Main.Temp_Max, Is.InRange(minTemp, maxTemp), "Temperature is outside valid range.");
            Assert.That(weatherResponse.Main.Humidity, Is.InRange(0, 100), "Humidity is outside valid range.");
            Assert.That(weatherResponse.Weather[0].Description, Is.Not.Empty, "Weather description should not be empty.");
            Assert.That(weatherResponse.Coord.Lat, Is.Not.EqualTo(0), "Latitude should not be zero.");
            Assert.That(weatherResponse.Coord.Lon, Is.Not.EqualTo(0), "Longitude should not be zero.");
            Assert.That(weatherResponse.Sys.Country, Is.EqualTo(RandomCity.Country), "Country mismatch.");
            Assert.That(weatherResponse.Id, Is.EqualTo(RandomCity.Id), "City Id mismatch.");
            Assert.That(weatherResponse.Coord.Lat, Is.EqualTo(RandomCity.Coord.Lat).Within(0.01), "Latitude mismatch.");
            Assert.That(weatherResponse.Coord.Lon, Is.EqualTo(RandomCity.Coord.Lon).Within(0.01), "Longitude mismatch.");
            Assert.That(weatherResponse.Cod, Is.EqualTo(200));
        }
    }
}
