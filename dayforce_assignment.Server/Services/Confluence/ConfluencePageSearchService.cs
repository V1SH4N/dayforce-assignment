using dayforce_assignment.Server.Interfaces.Confluence;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageSearchService : IConfluencePageSearchService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfluencePageSearchService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<JsonElement> SearchConfluencePageAsync(string cql)
        {
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            var response = await httpClient.GetAsync(new Uri(new Uri("https://dayforce.atlassian.net/"), $"wiki/rest/api/content/search?cql={cql}"));

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

            return jsonResponse;
        }
    }
}
