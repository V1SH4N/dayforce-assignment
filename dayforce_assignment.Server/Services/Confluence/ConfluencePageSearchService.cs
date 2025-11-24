using dayforce_assignment.Server.Interfaces.Confluence;
using System.Net;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageSearchService : IConfluencePageSearchService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl;
        private readonly string _space;

        public ConfluencePageSearchService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _baseUrl = configuration["Atlassian:BaseUrl"];
            _space = configuration["Atlassian:DefaultConfluenceSpace"];
        }

        public async Task<JsonElement> SearchConfluencePageAsync(string cql)
        {
          
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            //string finalCql = $"type = page AND ({cql})";
            string finalCql = $"type = page AND space = \"{_space}\" AND ({cql})";


            string encodedCql = Uri.EscapeDataString(finalCql);

            var baseUri = new Uri(_baseUrl);
            var url = $"wiki/rest/api/content/search?cql={encodedCql}&limit=10";


            var response = await httpClient.GetAsync(new Uri(baseUri, url));
            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

            return jsonResponse;
        }
    }
}
