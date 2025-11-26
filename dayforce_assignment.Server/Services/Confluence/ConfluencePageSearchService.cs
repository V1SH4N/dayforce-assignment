using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Confluence;
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
            _baseUrl = configuration["Atlassian:BaseUrl"] ?? throw new AtlassianConfigurationException("Default Atlassian base URL is not configured.");
            _space = configuration["Atlassian:DefaultConfluenceSpace"] ?? throw new AtlassianConfigurationException("Default Atlassian Confluence space is not configured.");
        }

        public async Task<JsonElement> SearchPageAsync(string cql)
        {
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            string finalCql = $"type = page AND space = \"{_space}\" AND ({cql})";
            string encodedCql = Uri.EscapeDataString(finalCql);

            var baseUri = new Uri(_baseUrl);
            var url = $"wiki/rest/api/content/search?cql={encodedCql}&limit=10";

            HttpResponseMessage response;
            try
            {
                response = await httpClient.GetAsync(new Uri(baseUri, url));
            }
            catch (HttpRequestException ex)
            {
                throw new ConfluenceException($"Failed to connect to Confluence API: {ex.Message}");
            }

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (jsonResponse.ValueKind == JsonValueKind.Undefined)
                    throw new ConfluenceApiException(200, $"Confluence search returned an empty response for CQL: {cql}.");
                return jsonResponse;
            }

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.BadRequest: // 400
                    var badContent = await response.Content.ReadAsStringAsync();
                    throw new ConfluenceSearchBadRequestException($"Bad request to Confluence search API: {badContent}");

                case System.Net.HttpStatusCode.Unauthorized: // 401
                    throw new ConfluenceUnauthorizedException();

                default:
                    var content = await response.Content.ReadAsStringAsync();
                    throw new ConfluenceApiException((int)response.StatusCode, $"Confluence search API error ({(int)response.StatusCode}): {content}");
            }
        }
    }
}
