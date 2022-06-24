namespace WebApplication1.Services
{
    public class TestService : HttpService, ITestService
    {
        private readonly HttpClient _httpClient;
        public override HttpClient HttpClient { get => _httpClient; init => _httpClient = value; }

        public TestService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetData()
        {
            var response = await _httpClient.GetAsync("/api/data").ConfigureAwait(false);

            return await response.Content.ReadAsStringAsync();
        }
    }
}
