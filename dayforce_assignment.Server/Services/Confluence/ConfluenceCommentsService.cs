using dayforce_assignment.Server.Interfaces.Confluence;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluenceCommentsService : IConfluenceCommentsService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfluenceCommentsService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<JsonElement> GetConfluenceCommentsAsync(string baseUrl, string id)
        {
            var client = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            var response = await client.GetAsync(new Uri(new Uri(baseUrl), $"wiki/api/v2/pages/{id}/footer-comments?body-format=storage"));

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

            return jsonResponse;
        }
    }
}
