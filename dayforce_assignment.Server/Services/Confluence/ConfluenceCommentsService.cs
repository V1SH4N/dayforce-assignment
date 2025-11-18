using dayforce_assignment.Server.Exceptions.ApiExceptions;
using dayforce_assignment.Server.Interfaces.Confluence;
using System.Net;
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
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Confluence base URL is required",
                    internalMessage: "baseUrl was null or empty in GetConfluenceCommentsAsync");
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Confluence page ID is required",
                    internalMessage: "id was null or empty in GetConfluenceCommentsAsync");
            }

            var client = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            var requestUri = new Uri(new Uri(baseUrl), $"wiki/api/v2/pages/{id}/footer-comments?body-format=storage");

            try
            {
                var response = await client.GetAsync(requestUri);

                // Known Confluence error responses
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw response.StatusCode switch
                    {
                        HttpStatusCode.BadRequest => new ApiException(
                            StatusCodes.Status400BadRequest,
                            "Invalid request sent to Confluence API",
                            internalMessage: $"400 from Confluence when requesting comments for id '{id}'"),

                        HttpStatusCode.Unauthorized => new ApiException(
                            StatusCodes.Status401Unauthorized,
                            "Invalid or missing Confluence credentials",
                            internalMessage: "401 Unauthorized from Confluence"),

                        HttpStatusCode.NotFound => new ApiException(
                            StatusCodes.Status404NotFound,
                            "The requested Confluence page could not be found.",
                            internalMessage: $"404 Not Found for Confluence comments page id '{id}'"),

                        _ => new ApiException(
                            StatusCodes.Status502BadGateway,
                            $"Unexpected status code {(int)response.StatusCode} from Confluence API",
                            internalMessage: $"Unexpected Confluence response while requesting comments for id '{id}'")
                    };
                }

                // Parse JSON
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                if (json.ValueKind == JsonValueKind.Undefined)
                {
                    throw new ApiException(
                        StatusCodes.Status502BadGateway,
                        "Confluence API returned empty response for comments",
                        internalMessage: "Received undefined JSON for Confluence comments");
                }

                return json;
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
                    "Confluence comments response had an unsupported content type.",
                    internalMessage: ex.ToString());
            }
            catch (JsonException ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Error parsing Confluence comments JSON",
                    internalMessage: ex.ToString());
            }
        }
    }
}
