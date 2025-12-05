using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Confluence;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluenceHttpClientService: IConfluenceHttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfluenceHttpClientService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        // Gets confluence page json. Throws exception if not found.
        public async Task<JsonElement> GetPageAsync(string baseUrl, string id)
        {
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(new Uri(baseUrl), $"wiki/api/v2/pages/{id}?body-format=storage"));

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (jsonResponse.ValueKind == JsonValueKind.Undefined)
                {
                    throw new ConfluenceApiException((int)response.StatusCode, $"Confluence returned an empty response for page {id}.");
                }
                return jsonResponse;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ConfluenceUnauthorizedException();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new ConfluencePageNotFoundException(id);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new ConfluenceBadRequestException(id);
            }

            throw new ConfluenceApiException((int)response.StatusCode, $"Unexpected Confluence API error.");
        }


        // Get confluence page attachments json (includes download link, media type & filename). Throws exception if not found.
        public async Task<JsonElement> GetAttachmentsAsync(string baseUrl, string id)
        {
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(new Uri(baseUrl), $"wiki/api/v2/pages/{id}/attachments"));

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (jsonResponse.ValueKind == JsonValueKind.Undefined)
                    throw new ConfluenceApiException((int)response.StatusCode, $"Confluence returned an empty response for attachments of page {id}.");

                return jsonResponse;
            }


            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ConfluenceUnauthorizedException();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new ConfluenceAttachmentsNotFoundException(id);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new ConfluenceBadRequestException(id);
            
            throw new ConfluenceApiException((int)response.StatusCode, $"Unexpected Confluence API error.");
        }


        // Get json confluence page comments json. Throws exception if not found.
        public async Task<JsonElement> GetCommentsAsync(string baseUrl, string id)
        {
            var client = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");
            HttpResponseMessage response = await client.GetAsync(new Uri(new Uri(baseUrl), $"wiki/api/v2/pages/{id}/footer-comments?body-format=storage"));

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (jsonResponse.ValueKind == JsonValueKind.Undefined)
                    throw new ConfluenceApiException((int)response.StatusCode, $"Confluence returned an empty response for comments on page {id}.");

                return jsonResponse;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ConfluenceUnauthorizedException();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new ConfluenceCommentsNotFoundException(id);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new ConfluenceBadRequestException(id);

            throw new ConfluenceApiException((int)response.StatusCode, $"Unexpected Confluence API error.");
        }



    }
}
