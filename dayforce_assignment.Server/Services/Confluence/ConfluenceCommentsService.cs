using dayforce_assignment.Server.Exceptions;
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
            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync(new Uri(new Uri(baseUrl), $"wiki/api/v2/pages/{id}/footer-comments?body-format=storage"));
            }
            catch (HttpRequestException ex)
            {
                throw new ConfluenceException($"Failed to connect to Confluence API: {ex.Message}");
            }

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (jsonResponse.ValueKind == JsonValueKind.Undefined)
                    throw new ConfluenceApiException(200, $"Confluence returned an empty response for comments on page {id}.");

                return jsonResponse;
            }

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.BadRequest: // 400
                    var badContent = await response.Content.ReadAsStringAsync();
                    throw new ConfluenceBadRequestException($"Bad request to Confluence API: {badContent}");

                case System.Net.HttpStatusCode.Unauthorized: // 401
                    throw new ConfluenceUnauthorizedException();

                case System.Net.HttpStatusCode.NotFound: // 404
                    throw new ConfluenceCommentsNotFoundException(id);

                default:
                    var content = await response.Content.ReadAsStringAsync();
                    throw new ConfluenceApiException((int)response.StatusCode, $"Confluence API error ({(int)response.StatusCode}): {content}");
            }
        }
    }
}
