using NUnit.Framework.Interfaces;
using System.Text.Json;
using Weather.API.Models;
using Weather.API.Tests.Infrastructure;

namespace Weather.API.Tests.TestBase
{
    public class ApiTestBase
    {
        protected static readonly JsonSerializerOptions caseInsensitiveOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        protected static HttpClient BasicClient;
        protected static City RandomCity;

        [OneTimeSetUp]
        public void Setup()
        {
            string apiKey = TestContext.Parameters["ApiKey"];
            string baseUrl = TestContext.Parameters["BaseUrl"];

            BasicClient = CreateBasicClient(apiKey, baseUrl);
            RandomCity = GetRandomCity();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            BasicClient.Dispose();
        }

        [SetUp]
        public void BeforeEach()
        {
            ExtentReportManager.test = ExtentReportManager.extent.CreateTest(TestContext.CurrentContext.Test.Name);
        }

        [TearDown]
        public void AfterEach()
        {
            var status = TestContext.CurrentContext.Result.Outcome.Status;
            if (status == TestStatus.Failed)
                ExtentReportManager.test.Fail("Failed: " + TestContext.CurrentContext.Result.Message);
            else
                ExtentReportManager.test.Pass("Passed");
        }

        protected static HttpClient CreateBasicClient(string apiKey, string baseUrl)
        {
            var handler = new ApiKeyHandler(apiKey)
            {
                InnerHandler = new HttpClientHandler()
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl)
            };

            return client;
        }

        private static City GetRandomCity()
        {
            var jsonPath = Path.Combine(Environment.CurrentDirectory, "TestData", "city.list.json");
            var json = File.ReadAllText(jsonPath);
            List<City> cities = JsonSerializer.Deserialize<List<City>>(json, caseInsensitiveOptions);

            var random = new Random();
            var randomCity = cities[random.Next(cities.Count)];

            return randomCity;
        }
    }
}