using dayforce_assignment.Server.Interfaces;

namespace dayforce_assignment.Server.Services
{
    public class ConfluencePageService : IConfluencePageService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfluencePageService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<string> GetConfluencePageAsync(string id)
        {
            var httpClient = _httpClientFactory.CreateClient("dayforce");

            var httpResponseMessage = await httpClient.GetAsync($"wiki/api/v2/pages/{id}?body-format=storage");
            //httpResponseMessage.EnsureSuccessStatusCode();
            var json = await httpResponseMessage.Content.ReadAsStringAsync();
            return json;
        }
    }
}
