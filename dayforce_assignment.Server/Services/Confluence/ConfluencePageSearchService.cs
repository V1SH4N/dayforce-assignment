using dayforce_assignment.Server.Exceptions.ApiExceptions;
using dayforce_assignment.Server.Interfaces.Confluence;
using System.Net;
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
            if (string.IsNullOrWhiteSpace(cql))
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Invalid search query",
                    "CQL query cannot be null or empty",
                    internalMessage: "Received empty CQL in SearchConfluencePageAsync");

            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            // Ensure type=page and URL-encode the CQL
            string finalCql = $"type=page AND ({cql})";
            string encodedCql = Uri.EscapeDataString(finalCql);

            var baseUri = new Uri("https://dayforce.atlassian.net/");
            var url = $"wiki/rest/api/content/search?cql={encodedCql}";

            try
            {
                var response = await httpClient.GetAsync(new Uri(baseUri, url));

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                        if (json.ValueKind == JsonValueKind.Undefined)
                            throw new ApiException(
                                StatusCodes.Status502BadGateway,
                                "Confluence API returned empty response",
                                internalMessage: "Received empty JSON from Confluence API");
                        return json;

                    case HttpStatusCode.BadRequest:
                        throw new ApiException(
                            StatusCodes.Status400BadRequest,
                            "Invalid request to Confluence API",
                            internalMessage: $"Bad request for CQL: {cql}");

                    case HttpStatusCode.Unauthorized:
                        throw new ApiException(
                            StatusCodes.Status401Unauthorized,
                            "Invalid Confluence credentials",
                            internalMessage: "Unauthorized access to Confluence API");

                    case HttpStatusCode.NotFound:
                        throw new ApiException(
                            StatusCodes.Status404NotFound,
                            "Confluence page search endpoint not found",
                            internalMessage: $"Endpoint not found for CQL: {cql}");

                    default:
                        throw new ApiException(
                            (int)response.StatusCode,
                            $"Unexpected status code {(int)response.StatusCode} from Confluence API",
                            internalMessage: $"Unexpected response for CQL: {cql}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Failed to call Confluence API",
                    internalMessage: ex.ToString());
            }
            catch (NotSupportedException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "The content type from Confluence API is not supported",
                    internalMessage: ex.ToString());
            }
            catch (JsonException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Error parsing Confluence response JSON",
                    internalMessage: ex.ToString());
            }
        }
    }
}
