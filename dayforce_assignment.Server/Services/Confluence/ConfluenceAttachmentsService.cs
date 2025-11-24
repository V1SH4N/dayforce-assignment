using dayforce_assignment.Server.Interfaces.Confluence;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluenceAttachmentsService : IConfluenceAttachmentsService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfluenceAttachmentsService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<JsonElement> GetConfluenceAttachmentsAsync(string baseUrl, string id)
        {
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            var response = await httpClient.GetAsync(new Uri(new Uri(baseUrl), $"wiki/api/v2/pages/{id}/attachments"));                    

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

            return jsonResponse;
           
        }
    }
}
